using PoultryManager.Helpers;
using PoultryManager.Models;

namespace PoultryManager.Services.Export;

/// <summary>Colonnes du tableau jour par jour, selon les données choisies dans le rapport.</summary>
public static class ReportColumns
{
    /// <summary>Une colonne : titre, largeur relative et valeur d'une journée.</summary>
    public sealed record Column(string Header, double Width, Func<DailyStat, string> Value, bool AlignRight = true);

    public static IReadOnlyList<Column> For(ReportOptions options)
    {
        var columns = new List<Column>
        {
            new("Date", 1.25, d => Formats.ShortDate(d.Date), AlignRight: false)
        };

        if (options.IncludeCount)
        {
            columns.Add(new("Effectif début", 1.15, d => Formats.Integer(d.StartCount)));
            columns.Add(new("Effectif fin", 1.05, d => Formats.Integer(d.EndCount)));
        }

        if (options.IncludeMortality)
        {
            columns.Add(new("Morts", 0.8, d => d.HasMortalityRecord ? Formats.Integer(d.Deaths) : Formats.Dash));
        }

        if (options.IncludeEggs)
        {
            columns.Add(new("Œufs", 0.95, d => Formats.Integer(d.Eggs)));
            columns.Add(new("Cassés", 0.8, d => Formats.Integer(d.BrokenEggs)));
        }

        if (options.IncludeLayingRate)
        {
            columns.Add(new("Ponte", 0.95, d => Formats.Percent(d.LayingRate)));
        }

        if (options.IncludeFeed)
        {
            columns.Add(new("Aliment", 1.05, d => Formats.Kilograms(d.FeedKg)));
            columns.Add(new("g / poule", 1.0, d => Formats.Grams(d.FeedPerHenGrams)));
        }

        return columns;
    }
}
