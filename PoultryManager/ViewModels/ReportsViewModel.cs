using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Export;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;
using PoultryManager.Services.Session;
using PoultryManager.Services.Settings;
using PoultryManager.Services.Sharing;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 11 · Rapports et exportation (onglet Rapports).</summary>
public partial class ReportsViewModel : BaseViewModel
{
    private readonly IFarmService _farmService;
    private readonly IAuthService _authService;
    private readonly IFileShareService _fileShareService;
    private readonly INavigationService _navigationService;
    private readonly ISettingsService _settings;
    private readonly AppState _appState;
    private bool _isLoading;

    public ReportsViewModel(
        IFarmService farmService,
        IAuthService authService,
        IFileShareService fileShareService,
        INavigationService navigationService,
        ISettingsService settings,
        AppState appState)
    {
        _farmService = farmService;
        _authService = authService;
        _fileShareService = fileShareService;
        _navigationService = navigationService;
        _settings = settings;
        _appState = appState;
        Title = "Rapports";
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    public ObservableCollection<Band> Bands { get; } = [];

    [ObservableProperty]
    private Band? selectedBand;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Today.AddDays(-6);

    [ObservableProperty]
    private DateTime toDate = DateTime.Today;

    // ---------- Données à inclure ----------

    [ObservableProperty]
    private bool includeCount = true;

    [ObservableProperty]
    private bool includeEggs = true;

    [ObservableProperty]
    private bool includeFeed = true;

    [ObservableProperty]
    private bool includeMortality = true;

    [ObservableProperty]
    private bool includeLayingRate = true;

    // ---------- État ----------

    /// <summary>Rapport généré, prêt à être exporté.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReport))]
    private ReportData? report;

    public bool HasReport => Report is not null;

    [ObservableProperty]
    private string reportStatus = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private bool hasBands = true;

    public string GenerateButtonText => IsBusy ? "Génération en cours…" : "Générer le rapport";

    public override async Task OnAppearingAsync()
    {
        _isLoading = true;
        try
        {
            var bands = await _farmService.GetBandsAsync();
            var previous = SelectedBand?.Id;
            Bands.Clear();
            foreach (var band in bands)
            {
                Bands.Add(band);
            }

            HasBands = Bands.Count > 0;
            var requested = _appState.TakeReportRequest();
            var wanted = requested ?? previous ?? _settings.SelectedBandId;
            SelectedBand = Bands.FirstOrDefault(b => b.Id == wanted) ?? Bands.FirstOrDefault();

            // Nouvelle bande demandée (depuis le bilan) ou premier affichage : toute la période de la bande.
            if (SelectedBand is not null && (requested is not null || previous is null))
            {
                SetFullPeriod(SelectedBand);
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    partial void OnSelectedBandChanged(Band? value)
    {
        Report = null;
        if (!_isLoading && value is not null)
        {
            SetFullPeriod(value);
        }
    }

    partial void OnFromDateChanged(DateTime value) => Report = null;
    partial void OnToDateChanged(DateTime value) => Report = null;
    partial void OnIncludeCountChanged(bool value) => Report = null;
    partial void OnIncludeEggsChanged(bool value) => Report = null;
    partial void OnIncludeFeedChanged(bool value) => Report = null;
    partial void OnIncludeMortalityChanged(bool value) => Report = null;
    partial void OnIncludeLayingRateChanged(bool value) => Report = null;

    /// <summary>Période par défaut : du démarrage de la bande à sa clôture (ou à aujourd'hui).</summary>
    private void SetFullPeriod(Band band)
    {
        FromDate = band.StartDate.Date;
        ToDate = band.IsClosed && band.ClosedDate.HasValue ? band.ClosedDate.Value.Date : Clock().Date;
    }

    /// <summary>« Générer le rapport ».</summary>
    [RelayCommand]
    private async Task GenerateAsync()
    {
        ErrorMessage = null;
        Report = null;

        if (SelectedBand is null)
        {
            ErrorMessage = "Choisissez une bande.";
            return;
        }

        if (FromDate.Date > ToDate.Date)
        {
            ErrorMessage = "La date de début doit précéder la date de fin.";
            return;
        }

        if (!(IncludeCount || IncludeEggs || IncludeFeed || IncludeMortality || IncludeLayingRate))
        {
            ErrorMessage = "Cochez au moins une donnée à inclure.";
            return;
        }

        try
        {
            SetBusy(true);
            var records = await _farmService.GetBandRecordsAsync(SelectedBand);
            var from = FromDate.Date < SelectedBand.StartDate ? SelectedBand.StartDate : FromDate.Date;
            var to = ToDate.Date;
            var user = _authService.CurrentUser;

            Report = new ReportData
            {
                Band = SelectedBand,
                FarmName = user?.FarmName ?? string.Empty,
                AuthorName = user?.FullName ?? string.Empty,
                From = from,
                To = to,
                Options = new ReportOptions(IncludeCount, IncludeEggs, IncludeFeed, IncludeMortality, IncludeLayingRate),
                Days = records.DailyStats(from, to),
                Summary = FarmCalculatorSummary(records, to),
                GeneratedAt = Clock()
            };

            ReportStatus = $"Rapport prêt · {SelectedBand.Name} · {Formats.Plural(Report.DayCount, "jour", "jours")}";
        }
        catch (Exception)
        {
            ErrorMessage = "Le rapport n'a pas pu être généré.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static BandSummary FarmCalculatorSummary(BandRecords records, DateTime to)
        => records.Summary(to);

    /// <summary>« Exporter en PDF » : création du fichier puis partage.</summary>
    [RelayCommand]
    private Task ExportPdfAsync() => ExportAsync("pdf", "application/pdf", PdfReportWriter.Write);

    /// <summary>« Exporter en CSV » : tableau lisible dans Excel.</summary>
    [RelayCommand]
    private Task ExportCsvAsync() => ExportAsync("csv", "text/csv", CsvReportWriter.Write);

    private async Task ExportAsync(string extension, string contentType, Func<ReportData, byte[]> writer)
    {
        if (Report is null)
        {
            ErrorMessage = "Générez d'abord le rapport.";
            return;
        }

        try
        {
            ErrorMessage = null;
            var bytes = writer(Report);
            var fileName = FileNameFor(Report, extension);
            await _fileShareService.ShareAsync(fileName, bytes, contentType, $"Rapport {Report.Band.Name}");
        }
        catch (Exception)
        {
            ErrorMessage = "L'export n'a pas pu être réalisé. Réessayez.";
        }
    }

    /// <summary>Nom du fichier, ex. « PoultryManager-Bande-1-2025-04-01-2025-05-18.pdf ».</summary>
    public static string FileNameFor(ReportData report, string extension)
    {
        var safeName = new string(report.Band.Name.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
        return $"PoultryManager-{safeName}-{report.From:yyyy-MM-dd}-{report.To:yyyy-MM-dd}.{extension}";
    }

    /// <summary>Bandeau d'erreur : « Réessayer ».</summary>
    [RelayCommand]
    private Task RetryAsync() => GenerateAsync();

    [RelayCommand]
    private Task GoBackAsync() => _navigationService.GoToAsync(Routes.DashboardPath);

    private void SetBusy(bool value)
    {
        IsBusy = value;
        OnPropertyChanged(nameof(GenerateButtonText));
    }
}
