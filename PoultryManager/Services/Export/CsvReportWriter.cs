using System.Globalization;
using System.Text;
using PoultryManager.Models;

namespace PoultryManager.Services.Export;

/// <summary>
/// Export CSV du rapport : séparateur « ; » et virgule décimale (ouverture directe dans Excel en français),
/// encodage UTF-8 avec BOM pour les accents.
/// </summary>
public static class CsvReportWriter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("fr-FR");

    public static byte[] Write(ReportData report)
    {
        var o = report.Options;
        var headers = new List<string> { "Date", "Bande" };
        if (o.IncludeCount) headers.AddRange(["Effectif début", "Effectif fin", "Effectif moyen"]);
        if (o.IncludeMortality) headers.Add("Morts");
        if (o.IncludeEggs) headers.AddRange(["Œufs collectés", "Œufs cassés"]);
        if (o.IncludeLayingRate) headers.Add("Taux de ponte (%)");
        if (o.IncludeFeed) headers.AddRange(["Aliment (kg)", "Aliment par poule (g)"]);

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", headers.Select(Escape)));

        foreach (var d in report.Days)
        {
            var cells = new List<string> { d.Date.ToString("dd/MM/yyyy", Culture), report.Band.Name };
            if (o.IncludeCount)
            {
                cells.Add(d.StartCount.ToString(Culture));
                cells.Add(d.EndCount.ToString(Culture));
                cells.Add(d.AverageCount.ToString("0.#", Culture));
            }

            if (o.IncludeMortality) cells.Add(d.HasMortalityRecord ? d.Deaths.ToString(Culture) : string.Empty);
            if (o.IncludeEggs)
            {
                cells.Add(d.Eggs?.ToString(Culture) ?? string.Empty);
                cells.Add(d.BrokenEggs?.ToString(Culture) ?? string.Empty);
            }

            if (o.IncludeLayingRate) cells.Add(d.LayingRate?.ToString("0.0", Culture) ?? string.Empty);
            if (o.IncludeFeed)
            {
                cells.Add(d.FeedKg?.ToString("0.##", Culture) ?? string.Empty);
                cells.Add(d.FeedPerHenGrams?.ToString("0.0", Culture) ?? string.Empty);
            }

            sb.AppendLine(string.Join(";", cells.Select(Escape)));
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        return [.. preamble, .. body];
    }

    private static string Escape(string value)
        => value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
