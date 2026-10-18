using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PoultryManager.Constants;
using PoultryManager.Data.Repositories;
using PoultryManager.Helpers;
using PoultryManager.Models;
using PoultryManager.Services.Auth;
using PoultryManager.Services.Dialogs;
using PoultryManager.Services.Farm;
using PoultryManager.Services.Navigation;

namespace PoultryManager.ViewModels;

/// <summary>ViewModel de l'écran 04b · Nouvelle bande / Modifier la bande.</summary>
public partial class BandEditViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IAuthService _authService;
    private readonly IFarmService _farmService;
    private readonly IBandRepository _bandRepository;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private Band? _band;
    private BandRecords? _records;
    private int _requestedBandId;
    private bool _loaded;

    public BandEditViewModel(
        IAuthService authService,
        IFarmService farmService,
        IBandRepository bandRepository,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _authService = authService;
        _farmService = farmService;
        _bandRepository = bandRepository;
        _navigationService = navigationService;
        _dialogService = dialogService;
        Title = "Nouvelle bande";
    }

    public Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    /// <summary>Choix proposés pour le type ou la souche.</summary>
    public IReadOnlyList<string> Strains => AppConstants.Strains;

    // ---------- Champs ----------

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? strain = AppConstants.Strains[0];

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private string initialCountText = string.Empty;

    [ObservableProperty]
    private string ageText = string.Empty;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string submitText = "Créer la bande";

    [ObservableProperty]
    private bool isEditMode;

    public DateTime MaximumDate => Clock().Date;

    // ---------- Erreurs ----------

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNameError))]
    private string? nameError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStartDateError))]
    private string? startDateError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInitialCountError))]
    private string? initialCountError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAgeError))]
    private string? ageError;

    public bool HasNameError => NameError is not null;
    public bool HasStartDateError => StartDateError is not null;
    public bool HasInitialCountError => InitialCountError is not null;
    public bool HasAgeError => AgeError is not null;

    partial void OnNameChanged(string value) => NameError = null;
    partial void OnStartDateChanged(DateTime value) => StartDateError = null;
    partial void OnInitialCountTextChanged(string value) => InitialCountError = null;
    partial void OnAgeTextChanged(string value) => AgeError = null;

    // ---------- Chargement ----------

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _requestedBandId = query.TryGetValue(Routes.BandIdParameter, out var id) && id is int bandId ? bandId : 0;
    }

    public override async Task OnAppearingAsync()
    {
        // Le formulaire n'est chargé qu'une fois (pas de rechargement au retour d'un dialogue).
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        if (_requestedBandId == 0)
        {
            await PrepareNewBandAsync();
            return;
        }

        _records = await _farmService.GetBandRecordsAsync(_requestedBandId);
        if (_records is null)
        {
            await _navigationService.GoBackAsync();
            return;
        }

        _band = _records.Band;
        IsEditMode = true;
        Title = "Modifier la bande";
        SubmitText = "Enregistrer";
        Name = _band.Name;
        Strain = string.IsNullOrWhiteSpace(_band.Strain) ? AppConstants.Strains[^1] : _band.Strain;
        StartDate = _band.StartDate;
        InitialCountText = Formats.Integer(_band.InitialCount);
        AgeText = _band.AgeAtArrivalWeeks?.ToString() ?? string.Empty;
        Notes = _band.Notes;
        ClearErrors();
    }

    /// <summary>Propose « Bande N » comme nom par défaut.</summary>
    private async Task PrepareNewBandAsync()
    {
        IsEditMode = false;
        var bands = await _farmService.GetBandsAsync();
        var next = bands.Count + 1;
        while (bands.Any(b => string.Equals(b.Name, $"Bande {next}", StringComparison.CurrentCultureIgnoreCase)))
        {
            next++;
        }

        Name = string.Empty;
        NamePlaceholder = $"Ex. Bande {next}";
        StartDate = Clock().Date;
        ClearErrors();
    }

    [ObservableProperty]
    private string namePlaceholder = "Ex. Bande 1";

    private void ClearErrors() => NameError = StartDateError = InitialCountError = AgeError = null;

    // ---------- Commandes ----------

    /// <summary>« Créer la bande » / « Enregistrer ».</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var user = _authService.CurrentUser;
        if (IsBusy || user is null || !await ValidateAsync(user.Id))
        {
            return;
        }

        try
        {
            IsBusy = true;
            NumberParser.TryParseInt(InitialCountText, out var initialCount);
            int? age = NumberParser.TryParseInt(AgeText, out var weeks) ? weeks : null;
            var strainValue = Strain == AppConstants.Strains[^1] ? string.Empty : Strain ?? string.Empty;

            if (_band is null)
            {
                var band = new Band
                {
                    UserId = user.Id,
                    Name = Name.Trim(),
                    Strain = strainValue,
                    StartDate = StartDate.Date,
                    InitialCount = initialCount,
                    AgeAtArrivalWeeks = age,
                    Notes = Notes.Trim()
                };
                await _bandRepository.AddAsync(band);
            }
            else
            {
                _band.Name = Name.Trim();
                _band.Strain = strainValue;
                _band.StartDate = StartDate.Date;
                _band.InitialCount = initialCount;
                _band.AgeAtArrivalWeeks = age;
                _band.Notes = Notes.Trim();
                await _bandRepository.UpdateAsync(_band);
            }

            await _navigationService.GoBackAsync();
        }
        catch (Exception)
        {
            await _dialogService.ShowAlertAsync("Enregistrement impossible", "La bande n'a pas pu être enregistrée. Veuillez réessayer.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>« Annuler » ou flèche retour.</summary>
    [RelayCommand]
    private Task CancelAsync() => _navigationService.GoBackAsync();

    /// <summary>Vérifie le formulaire ; renvoie vrai s'il est valide.</summary>
    public async Task<bool> ValidateAsync(int userId)
    {
        ClearErrors();

        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Ce champ est obligatoire.";
        }
        else if (await _bandRepository.NameExistsAsync(userId, Name, _band?.Id))
        {
            NameError = "Une autre bande porte déjà ce nom.";
        }

        if (StartDate.Date > Clock().Date)
        {
            StartDateError = "La date de démarrage ne peut pas être dans le futur.";
        }
        else if (_records?.FirstRecordDate is DateTime first && StartDate.Date > first)
        {
            StartDateError = $"Des suivis existent depuis le {Formats.ShortDate(first)} : la date de démarrage doit être antérieure.";
        }

        if (!NumberParser.TryParseInt(InitialCountText, out var count) || count <= 0)
        {
            InitialCountError = "Indiquez un nombre de poules supérieur à 0.";
        }
        else if (_records is not null && count < _records.Mortality.Sum(m => m.Deaths))
        {
            InitialCountError = $"L'effectif initial ne peut pas être inférieur à la mortalité déjà saisie ({Formats.Integer(_records.Mortality.Sum(m => m.Deaths))}).";
        }

        if (!string.IsNullOrWhiteSpace(AgeText) && (!NumberParser.TryParseInt(AgeText, out var age) || age < 0 || age > 150))
        {
            AgeError = "Âge invalide (nombre de semaines).";
        }

        return !(HasNameError || HasStartDateError || HasInitialCountError || HasAgeError);
    }
}
