using System.Globalization;
using System.Text;
using PoultryManager.Helpers;
using PoultryManager.Models;

namespace PoultryManager.Services.Export;

/// <summary>
/// Génère le rapport au format PDF (A4), sans bibliothèque externe :
/// en-tête, chiffres clés de la période, puis tableau jour par jour sur autant de pages que nécessaire.
/// </summary>
public static class PdfReportWriter
{
    private const double PageWidth = 595.28;
    private const double PageHeight = 841.89;
    private const double Margin = 40;
    private const double RowHeight = 17;

    // Couleurs de la maquette (RGB 0 à 1)
    private static readonly (double R, double G, double B) Forest = (0x17 / 255.0, 0x4B / 255.0, 0x35 / 255.0);
    private static readonly (double R, double G, double B) Sage = (0xDC / 255.0, 0xE8 / 255.0, 0xDF / 255.0);
    private static readonly (double R, double G, double B) Pale = (0xF2 / 255.0, 0xF7 / 255.0, 0xF3 / 255.0);
    private static readonly (double R, double G, double B) Text = (0x17 / 255.0, 0x39 / 255.0, 0x2C / 255.0);
    private static readonly (double R, double G, double B) Muted = (0x5C / 255.0, 0x70 / 255.0, 0x65 / 255.0);
    private static readonly (double R, double G, double B) White = (1, 1, 1);

    public static byte[] Write(ReportData report)
    {
        var columns = ReportColumns.For(report.Options);
        var pages = new List<PageBuilder>();
        var page = NewPage(pages);

        // ---------- En-tête ----------
        page.FillRect(0, PageHeight - 92, PageWidth, 92, Forest);
        page.Text(Margin, PageHeight - 46, 22, bold: true, "PoultryManager", White);
        page.Text(Margin, PageHeight - 70, 13, bold: false, $"Rapport de suivi · {report.Band.Name}", Sage);

        var y = PageHeight - 120;
        page.Text(Margin, y, 10, false, $"Exploitation : {report.FarmName}   ·   Éleveur : {report.AuthorName}", Text);
        y -= 15;
        page.Text(Margin, y, 10, false,
            $"Période : du {Formats.ShortDate(report.From)} au {Formats.ShortDate(report.To)} ({Formats.Plural(report.DayCount, "jour", "jours")})", Text);
        y -= 15;
        var strain = string.IsNullOrWhiteSpace(report.Band.Strain) ? string.Empty : $"{report.Band.Strain} · ";
        page.Text(Margin, y, 10, false,
            $"Bande : {strain}démarrée le {Formats.ShortDate(report.Band.StartDate)} · effectif initial {Formats.Integer(report.Band.InitialCount)}" +
            (report.Band.IsClosed && report.Band.ClosedDate.HasValue ? $" · clôturée le {Formats.ShortDate(report.Band.ClosedDate.Value)}" : string.Empty),
            Text);
        y -= 15;
        page.Text(Margin, y, 9, false, $"Généré le {Formats.ShortDate(report.GeneratedAt)} à {report.GeneratedAt:HH\\:mm}", Muted);

        // ---------- Chiffres clés ----------
        y -= 28;
        page.Text(Margin, y, 12, true, "Chiffres clés de la période", Text);
        y -= 10;
        var figures = KeyFigures(report);
        const double boxWidth = (PageWidth - 2 * Margin - 2 * 10) / 3;
        const double boxHeight = 44;
        for (var i = 0; i < figures.Count; i++)
        {
            var col = i % 3;
            var row = i / 3;
            var x = Margin + col * (boxWidth + 10);
            var top = y - row * (boxHeight + 8);
            page.FillRect(x, top - boxHeight, boxWidth, boxHeight, Pale);
            page.Text(x + 10, top - 16, 8.5, false, figures[i].Label, Muted);
            page.Text(x + 10, top - 34, 14, true, figures[i].Value, Text);
        }

        y -= ((figures.Count + 2) / 3) * (boxHeight + 8) + 18;

        // ---------- Tableau jour par jour ----------
        page.Text(Margin, y, 12, true, "Détail jour par jour", Text);
        y -= 8;

        var totalWidth = columns.Sum(c => c.Width);
        var scale = (PageWidth - 2 * Margin) / totalWidth;

        void Header()
        {
            page.FillRect(Margin, y - RowHeight, PageWidth - 2 * Margin, RowHeight, Sage);
            DrawRow(page, columns, scale, y, c => c.Header, bold: true, Forest);
            y -= RowHeight;
        }

        Header();
        var index = 0;
        foreach (var day in report.Days.OrderBy(d => d.Date))
        {
            if (y - RowHeight < Margin + 20)
            {
                page = NewPage(pages);
                y = PageHeight - Margin;
                page.Text(Margin, y - 4, 10, true, $"{report.Band.Name} · détail jour par jour (suite)", Text);
                y -= 16;
                Header();
            }

            if (index % 2 == 1)
            {
                page.FillRect(Margin, y - RowHeight, PageWidth - 2 * Margin, RowHeight, Pale);
            }

            DrawRow(page, columns, scale, y, c => c.Value(day), bold: false, Text);
            y -= RowHeight;
            index++;
        }

        if (report.Days.Count == 0)
        {
            page.Text(Margin, y - 14, 10, false, "Aucune saisie sur cette période.", Muted);
        }

        // ---------- Pieds de page ----------
        for (var i = 0; i < pages.Count; i++)
        {
            pages[i].Text(Margin, 22, 8, false, "PoultryManager · rapport généré par l'application", Muted);
            var label = $"Page {i + 1} / {pages.Count}";
            pages[i].Text(PageWidth - Margin - TextWidth(label, 8, false), 22, 8, false, label, Muted);
        }

        return Assemble(pages);
    }

    /// <summary>Chiffres clés affichés selon les données choisies.</summary>
    private static List<(string Label, string Value)> KeyFigures(ReportData report)
    {
        var o = report.Options;
        var figures = new List<(string, string)>();
        var first = report.Days.FirstOrDefault();
        var last = report.Days.LastOrDefault();

        if (o.IncludeCount)
        {
            figures.Add(("Effectif en début de période", Formats.Integer(first?.StartCount)));
            figures.Add(("Effectif en fin de période", Formats.Integer(last?.EndCount)));
        }

        if (o.IncludeEggs) figures.Add(("Œufs collectés", Formats.Integer(report.TotalEggs)));
        if (o.IncludeLayingRate) figures.Add(("Taux moyen de ponte", Formats.Percent(report.AverageLayingRate)));
        if (o.IncludeFeed) figures.Add(("Aliment distribué", Formats.Kilograms(report.TotalFeedKg)));
        if (o.IncludeMortality)
        {
            figures.Add(("Morts sur la période", Formats.Integer(report.TotalDeaths)));
            figures.Add(("Mortalité cumulée (bande)",
                $"{Formats.Integer(report.Summary.CumulativeDeaths)} · {Formats.Percent(report.Summary.MortalityRate)}"));
        }

        return figures;
    }

    private static void DrawRow(PageBuilder page, IReadOnlyList<ReportColumns.Column> columns, double scale, double top,
        Func<ReportColumns.Column, string> text, bool bold, (double R, double G, double B) color)
    {
        var x = Margin;
        const double size = 8.5;
        foreach (var column in columns)
        {
            var width = column.Width * scale;
            var value = text(column);
            var tx = column.AlignRight ? x + width - 6 - TextWidth(value, size, bold) : x + 6;
            page.Text(tx, top - 12, size, bold, value, color);
            x += width;
        }
    }

    // =====================================================================
    // Construction bas niveau du fichier PDF
    // =====================================================================

    private static PageBuilder NewPage(List<PageBuilder> pages)
    {
        var page = new PageBuilder();
        pages.Add(page);
        return page;
    }

    /// <summary>Contenu d'une page (opérateurs PDF).</summary>
    private sealed class PageBuilder
    {
        private readonly StringBuilder _content = new();

        public string Content => _content.ToString();

        public void FillRect(double x, double y, double w, double h, (double R, double G, double B) c)
            => _content.Append(Inv($"{c.R:0.###} {c.G:0.###} {c.B:0.###} rg {x:0.##} {y:0.##} {w:0.##} {h:0.##} re f\n"));

        public void Text(double x, double y, double size, bool bold, string text, (double R, double G, double B) c)
            => _content.Append(Inv($"BT {c.R:0.###} {c.G:0.###} {c.B:0.###} rg /{(bold ? "F2" : "F1")} {size:0.##} Tf {x:0.##} {y:0.##} Td ("))
                .Append(EscapeText(text))
                .Append(") Tj ET\n");
    }

    private static string Inv(FormattableString s) => s.ToString(CultureInfo.InvariantCulture);

    private static string EscapeText(string text)
        => text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    /// <summary>Assemble les objets PDF et la table des références croisées.</summary>
    private static byte[] Assemble(IReadOnlyList<PageBuilder> pages)
    {
        // Objets : 1 catalogue, 2 arbre des pages, 3 police normale, 4 police grasse, puis (page, contenu) × n
        var objects = new List<byte[]>();
        var pageIds = Enumerable.Range(0, pages.Count).Select(i => 5 + i * 2).ToList();

        objects.Add(Ascii("<< /Type /Catalog /Pages 2 0 R >>"));
        objects.Add(Ascii($"<< /Type /Pages /Kids [{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] /Count {pages.Count} >>"));
        objects.Add(Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
        objects.Add(Ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"));

        for (var i = 0; i < pages.Count; i++)
        {
            var contentId = pageIds[i] + 1;
            objects.Add(Ascii(Inv($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth:0.##} {PageHeight:0.##}] ") +
                              $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentId} 0 R >>"));
            var stream = WinAnsi(pages[i].Content);
            objects.Add([.. Ascii($"<< /Length {stream.Length} >>\nstream\n"), .. stream, .. Ascii("\nendstream")]);
        }

        using var output = new MemoryStream();
        void Write(byte[] bytes) => output.Write(bytes, 0, bytes.Length);

        Write(Ascii("%PDF-1.4\n"));
        Write([0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A]); // marque binaire
        var offsets = new List<long>();
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(output.Position);
            Write(Ascii($"{i + 1} 0 obj\n"));
            Write(objects[i]);
            Write(Ascii("\nendobj\n"));
        }

        var xref = output.Position;
        var sb = new StringBuilder();
        sb.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            sb.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }

        sb.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        Write(Ascii(sb.ToString()));
        return output.ToArray();
    }

    private static byte[] Ascii(string s) => Encoding.ASCII.GetBytes(s);

    /// <summary>Convertit le texte en octets WinAnsi (accents français, œ, €, tirets, guillemets).</summary>
    private static byte[] WinAnsi(string text)
    {
        var bytes = new byte[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            bytes[i] = c switch
            {
                ' ' or ' ' => 0xA0,
                '€' => 0x80, '‚' => 0x82, '„' => 0x84, '…' => 0x85, '‰' => 0x89, 'Œ' => 0x8C,
                '‘' => 0x91, '’' => 0x92, '“' => 0x93, '”' => 0x94, '•' => 0x95, '–' => 0x96, '—' => 0x97,
                '™' => 0x99, 'œ' => 0x9C, 'Ÿ' => 0x9F, '−' => (byte)'-',
                _ when c < 0x80 || (c >= 0xA0 && c <= 0xFF) => (byte)c,
                _ => (byte)'?'
            };
        }

        return bytes;
    }

    /// <summary>Largeur approximative d'un texte en Helvetica (pour aligner les nombres à droite).</summary>
    private static double TextWidth(string text, double size, bool bold)
    {
        double units = 0;
        foreach (var c in text)
        {
            units += c switch
            {
                >= '0' and <= '9' => 556,
                ' ' or ' ' or ' ' or ',' or '.' or '/' => 278,
                '%' => 889,
                '—' => 1000,
                '−' or '-' => 333,
                'g' or 'k' => bold ? 611 : 556,
                _ => 600
            };
        }

        return units / 1000 * size;
    }
}
