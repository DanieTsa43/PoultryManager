# PoultryManager

Application mobile de gestion d'un élevage de poules pondeuses : bandes, production d'œufs, alimentation, mortalité, indicateurs et rapports.

Développée avec **.NET MAUI** (C# / XAML) selon l'architecture **MVVM**, dans **Visual Studio Community**.

## Avancement

| Étape | Contenu | État |
|---|---|---|
| 1 | Initialisation du projet : structure MVVM, navigation, thème, polices | ✅ |
| 2 | Écrans Bienvenue et Connexion | ✅ |
| 3 | Base de données locale (SQLite), Inscription, session mémorisée | ✅ |
| 4 | Barre de navigation et écrans métier (03 à 12) | ✅ |
| 5 | Backend ASP.NET Core + SQL Server (`PoultryManager.Api`), application reliée à l'API | ✅ |

## Fonctionnalités clés

| Fonctionnalité | Écrans | Ce que fait l'application |
|---|---|---|
| **Effectif** | 04 Bandes · 04b Nouvelle bande · 03 Accueil | Création / modification / clôture des bandes ; effectif actuel = effectif initial − mortalité cumulée, recalculé à chaque saisie. |
| **Production d'œufs** | 05 Production · 09 Historique | Saisie quotidienne (œufs collectés, cassés, catégorie) ; contrôle « pas plus d'œufs que de poules ». |
| **Consommation d'aliment** | 06 Aliment · 08 Indicateurs | Quantité distribuée et restante ; consommation par poule calculée en direct (g / poule / jour). |
| **Mortalité** | 07 Mortalité · 08 Indicateurs | Morts du jour et cause ; effectif restant affiché ; dernières saisies modifiables ; graphique par jour. |
| **Taux de ponte** | 03 Accueil · 05 · 08 Indicateurs | Œufs ÷ effectif vivant moyen × 100 ; écart avec la veille ; courbe de production ; indice de conversion. |
| **Bilan par bande** | 10 Bilan · 11 Rapports | Effectifs, total d'œufs, aliment, mortalité cumulée (%), taux moyen de ponte, taux par semaine ; export **PDF** et **CSV**. |

Autres écrans : historique filtrable avec modification / suppression d'une journée (09), profil, exploitation,
poids moyen d'un œuf, rappel de saisie, sauvegarde de la base et déconnexion (12).

## Backend (étape 5)

Les données ne sont plus dans SQLite sur le téléphone : elles sont sur un serveur **ASP.NET Core Web API** (`PoultryManager.Api`) avec **SQL Server** (EF Core) et authentification **JWT**. Les écrans et fonctionnalités de l'application sont inchangés ; les calculs (taux de ponte, bilan…) restent dans `FarmCalculator`.

**Démarrer**
1. SQL Server LocalDB (installé avec Visual Studio) ou une autre instance : adapter `ConnectionStrings:Default` dans `PoultryManager.Api/appsettings.json`.
2. Lancer le projet **PoultryManager.Api** (F5, profil `http://localhost:5080`). La base `PoultryManagerDb` et le compte de démonstration sont créés au premier démarrage (en Development).
3. Lancer ensuite l'application MAUI (Windows : `localhost`, émulateur Android : `10.0.2.2`). Téléphone réel : renseigner `ApiSettings.BaseUrlOverride` (ex. `http://192.168.1.20:5080`) et utiliser le profil de lancement « accessible depuis un téléphone ».

**Migrations (recommandé)** : `dotnet ef migrations add InitialCreate --project PoultryManager.Api` — l'API les applique alors au démarrage (sinon elle crée la base directement).

**Avant une mise en production** : clé `Jwt:Key` par variable d'environnement, HTTPS (retirer `usesCleartextTraffic` du manifeste Android), `Seed:DemoData` à false, et remplacer la réinitialisation de mot de passe par e-mail (désactivée par défaut, `Auth:AllowDirectPasswordReset`).

## Prérequis

- **Visual Studio 2026 Community** (gratuit) avec la charge de travail **« Développement .NET Multi-platform App UI »**.
- **SDK .NET 10** (installé avec la charge de travail).
- Pour tester sur Android : un émulateur (créé depuis *Outils › Android › Gestionnaire d'appareils Android*) ou un téléphone en mode développeur.

> Vous utilisez **Visual Studio 2022** ? Ouvrez `PoultryManager/PoultryManager.csproj` et remplacez les deux occurrences de `net10.0` par `net9.0`.

## Compte de démonstration

Au premier démarrage de l'API (Development), un compte de démonstration avec 4 bandes
(3 actives, 1 clôturée) et leurs saisies quotidiennes, calculées par rapport à la date du jour :

| Identifiant | Mot de passe |
|---|---|
| `jean.dupont@exemple.com` ou `jean.dupont` | `Poultry2025` |

Un compte créé avec **Commencer** démarre sans aucune bande.

## Tests de l'étape 4

1. Connexion avec le compte de démonstration : la barre **Accueil · Bandes · Suivi · Rapports · Plus** apparaît.
2. **Accueil** : effectif, œufs, aliment, mortalité et taux de ponte du jour de la Bande 1, courbe des 7 jours.
   Changez de bande dans la liste : tous les chiffres changent.
3. **+ Œufs** : saisissez un nombre supérieur à l'effectif → message d'erreur ; un nombre correct →
   « Suivi enregistré · taux de ponte … % ». Le tableau de bord est mis à jour.
4. **+ Aliment** : la consommation par poule se calcule pendant la saisie.
5. **+ Mortalité** : l'effectif restant se calcule ; la saisie apparaît dans « Dernières saisies » (crayon / corbeille).
6. **Bandes** : **Nouvelle bande** (nom obligatoire et unique, effectif > 0), **Modifier**, **Clôturer**, **Bilan**.
7. **Bilan** : 6 cartes et taux de ponte par semaine ; **Voir le détail** ouvre l'historique filtré ;
   **Exporter** ouvre les rapports avec la bande et sa période.
8. **Suivi** : filtres par bande et par période ; **Modifier** (choix Œufs / Aliment / Mortalité) et **Supprimer**.
   Le lien **Indicateurs** ouvre l'écran 08 (7 j / 30 j / Bande).
9. **Rapports** : **Générer le rapport**, puis **Exporter en PDF** ou **en CSV** (menu de partage du téléphone).
10. **Plus** : chaque rubrique ouvre sa page complète : **Exploitation** (nom de la ferme et nombre de bandes),
    **Profil utilisateur** (nom, téléphone, e-mail, mot de passe), **Unités de mesure** (poids moyen d'un œuf),
    **Notifications et rappels** (activation et heure du rappel), **Sauvegarde des données** (copie de la base) ;
    puis **Déconnexion**.
    Depuis l'écran **Connexion**, **Mot de passe oublié ?** ouvre la réinitialisation : l'application étant locale
    (sans serveur), elle se fait sur l'appareil, avec l'adresse e-mail du compte.
11. Créez un nouveau compte : l'accueil propose de créer la première bande.

## Lancer l'application

1. Ouvrir `PoultryManager.sln` dans Visual Studio.
2. Attendre la fin de la restauration des paquets NuGet (barre d'état en bas).
3. Dans la barre d'outils, choisir la cible :
   - **Windows Machine** : test rapide sur le PC (fenêtre au format téléphone) ;
   - **Android Emulators › …** : test sur l'émulateur Android.
4. Appuyer sur **F5** (ou le bouton ▶).

## Dépannage

| Message | Solution |
|---|---|
| **NETSDK1005** « project.assets.json n'a aucune cible pour net10.0-… » | La restauration NuGet n'a pas abouti. Fermez Visual Studio, supprimez les dossiers `bin` et `obj` du projet, rouvrez la solution, puis clic droit sur la solution › **Restaurer les packages NuGet** et **Régénérer**. Si l'erreur persiste, lancez `dotnet restore` dans le terminal pour voir la cause exacte. |
| **NETSDK1045** (version de .NET non prise en charge) | Le SDK .NET 10 n'est pas installé : mettez Visual Studio à jour, ou remplacez `net10.0` par `net9.0` dans `PoultryManager.csproj`. |
| **NU1100 / NU1101** (paquet introuvable) | La source nuget.org manque : `dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org`. |
| Données de démonstration à réinitialiser | Supprimez la base `PoultryManagerDb` (SQL Server) puis relancez l'API. |
| « Impossible de joindre le serveur » | L'API n'est pas lancée, ou l'adresse est fausse (voir `ApiSettings`). |

## Architecture du projet

```
PoultryManager/                     ← racine du dépôt GitHub
├── PoultryManager.sln              ← solution à ouvrir dans Visual Studio
├── README.md
├── docs/                           ← maquette, arborescence de navigation, licences
└── PoultryManager/                 ← projet .NET MAUI
    ├── App.xaml / App.xaml.cs      ← ressources globales et fenêtre principale
    ├── AppShell.xaml / .cs         ← navigation (routes des écrans)
    ├── MauiProgram.cs              ← démarrage : polices, injection de dépendances
    ├── Constants/                  ← constantes (routes, listes de valeurs…)
    ├── Helpers/                    ← formats français, lecture des nombres, validation
    ├── Models/                     ← données métier (bande, suivi, utilisateur…)
    ├── ViewModels/                 ← logique des écrans (MVVM) ; Items/ : éléments de listes
    ├── Views/                      ← écrans XAML (Views/Base : page reliée à son ViewModel)
    ├── Services/
    │   ├── Auth/                   ← connexion, inscription, session
    │   ├── Farm/                   ← bandes de l'utilisateur et leurs saisies
    │   ├── Statistics/             ← règles de calcul (FarmCalculator)
    │   ├── Export/                 ← rapports PDF et CSV (sans bibliothèque externe)
    │   ├── Settings/ · Session/    ← réglages et état partagé entre onglets
    │   ├── Navigation/ · Dialogs/  ← navigation et boîtes de dialogue
    │   └── Platform/ · Sharing/    ← partage de fichiers (spécifique à MAUI)
    ├── Data/                       ← base SQLite (AppDatabase), dépôts, données de démonstration
    ├── Controls/                   ← graphiques (courbe, barres) dessinés avec Microsoft.Maui.Graphics
    ├── Converters/                 ← convertisseurs de liaison XAML
    ├── Resources/
    │   ├── Styles/                 ← Colors.xaml (palette) et Styles.xaml
    │   ├── Fonts/                  ← Plus Jakarta Sans
    │   ├── Images/                 ← images et icônes
    │   ├── AppIcon/ · Splash/      ← icône et écran de démarrage
    │   └── Raw/                    ← fichiers bruts
    └── Platforms/                  ← code propre à Android et Windows
```

### Principe MVVM appliqué

- **View** (`Views/*.xaml`) : uniquement l'affichage. Le code-behind se limite à `BindingContext = viewModel`.
- **ViewModel** (`ViewModels/*ViewModel.cs`) : état de l'écran et commandes, grâce à `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`).
- **Model / Services** : données et règles métier, injectés dans les ViewModels par le constructeur.
- **Data** : la base SQLite n'est manipulée que par les dépôts (`Data/Repositories`).
- **Règles de calcul** (`Services/Statistics/FarmCalculator.cs`) : effectif, taux de ponte, consommation par poule,
  mortalité, indice de conversion et bilan — conformément à la section 7 de `docs/maquette/DESIGN.md`.
- Chaque nouvel écran est déclaré dans `MauiProgram.cs` (page + ViewModel) et, si besoin, dans `AppShell.xaml.cs` (route).

## Conception

La procédure d'envoi sur GitHub est décrite dans `docs/GUIDE-GITHUB.md`.

La maquette complète est dans `docs/maquette/` (captures des écrans et `DESIGN.md`), et l'arborescence de navigation dans `docs/PoultryManager_Arborescence.pdf`.

Police : *Plus Jakarta Sans*, licence SIL Open Font License (`docs/licences/`).
