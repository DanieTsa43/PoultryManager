# DESIGN.md — PoultryManager

Application mobile de gestion d'un élevage de poules pondeuses. Ce fichier décrit le système de conception utilisé par les 13 écrans de la maquette. Il peut être fourni tel quel à Google Stitch comme contexte de style.

## 1. Identité

- Ambiance : végétale, vert forêt profond, naturelle et professionnelle.
- Feuillages et illustration de poule : réservés aux écrans 00, 01 et 02.
- Écrans métier (03 à 12) : fond vert très pâle, cartes blanches, aucun décor.
- Langue de l'interface : français. Format des dates : JJ/MM/AAAA. Séparateur décimal : virgule.

## 2. Couleurs

| Rôle | Code | Usage |
|---|---|---|
| Vert forêt principal | `#174B35` | Fond d'accueil, carte mise en avant, élément actif |
| Vert profond secondaire | `#103A2A` | Feuillages sombres, ombres |
| Vert des boutons | `#286B46` | Boutons principaux, liens, bordure de champ actif, graphiques |
| Vert sauge clair | `#DCE8DF` | Pastilles d'icône, badge « Active », onglet actif, bandeau de succès |
| Vert très pâle | `#F2F7F3` | Fond des écrans métier, fond des champs |
| Blanc | `#FFFFFF` | Cartes, fond des écrans d'accès |
| Texte principal | `#17392C` | Titres, valeurs, libellés |
| Texte secondaire | `#7C8D82` | Texte indicatif des champs, bordures discrètes, éléments ≥ 16 px |
| Bordures | `#E1EAE3` | Contours de cartes et de champs |
| Jaune œuf | `#EFCB70` | Accent ponctuel : pastille « œufs », bec, œuf illustré |
| Rouge d'alerte | `#C94D4D` | Bordure de champ invalide, barres de mortalité, bouton sensible |

Variantes de lisibilité (dérivées, non substituables à la palette) :

- `#5C7065` : texte secondaire en petite taille (11 à 14 px). `#7C8D82` sur blanc n'atteint pas le contraste 4,5:1 en petit corps.
- `#B03E3E` : texte des messages d'erreur (même raison pour `#C94D4D`).
- `#F6E3E3` : fond du bandeau d'erreur et de la pastille « mortalité ».
- `#1F5A40`, `#C9A24E` : uniquement dans les illustrations (feuille intermédiaire, nid).

## 3. Typographie

Police unique : **Plus Jakarta Sans** (Google Fonts), graisses 500, 600, 700, 800.

| Style | Taille / interligne | Graisse |
|---|---|---|
| Titre d'accueil | 40 / 46 px | 800 |
| Titre d'écran d'accès | 28 / 34 px | 800 |
| Titre d'écran métier | 20 / 26 px | 800 |
| Valeur d'indicateur | 22 / 28 px | 800 |
| Titre de section | 14 à 15 px | 700 |
| Corps, champs | 15 / 22 px | 500 à 600 |
| Libellé de champ | 13 px | 600 |
| Légende | 12 px | 400 |
| Navigation | 11 px | 600 (800 si actif) |

## 4. Espacements, rayons, ombres

- Grille de 4 px : 4, 8, 12, 16, 24, 28.
- Marge latérale : 16 px (écrans métier), 28 px (accueil, connexion, inscription).
- Écart entre cartes : 10 à 12 px. Marge interne des cartes : 12 à 16 px.
- Rayons : champ 12 · bouton 14 · carte 16 · conteneur de formulaire 20 · badge 999. Écrans d'accès : champ 14 à 16, bouton 16.
- Ombres : aucune sur les cartes (bordure 1 px `#E1EAE3`). Une seule ombre, sur la boîte de dialogue : `0 8px 24px rgba(16,58,42,0.14)`.
- Cible tactile minimale : 44 × 44 px.
- Format d'écran : 390 × 844 px.

## 5. Composants

- **Bouton principal** : hauteur 52 px, fond `#286B46`, texte blanc 16 px / 700.
- **Bouton secondaire** : fond blanc, bordure 1 px `#286B46`, texte `#174B35`.
- **Bouton sensible** (clôturer, supprimer, déconnexion) : fond blanc, bordure `#C94D4D`, texte `#B03E3E`. Dans une confirmation : fond `#B03E3E`, texte blanc.
- **Champ** : hauteur 48 px, fond `#F2F7F3`, bordure 1 px `#E1EAE3`, libellé visible au-dessus. Les champs facultatifs portent la mention « (facultatif) ».
- **Champ d'accès** (écrans 01 et 02) : hauteur 50 à 54 px, icône à gauche, texte indicatif à l'intérieur.
- **Carte d'indicateur** : fond blanc, pastille d'icône 40 px, légende 12 px, valeur 22 px.
- **Carte calculée** : fond `#DCE8DF`, formule en clair à gauche, résultat à droite.
- **Badge** : « Active » fond `#DCE8DF` ; « Clôturée » contour seul.
- **Barre de navigation** : 5 entrées — Accueil, Bandes, Suivi, Rapports, Plus. Entrée active : fond `#DCE8DF`, texte `#174B35`.
- **Graphiques** : courbe `#286B46` sur aplat `#DCE8DF` ; barres `#286B46` (ponte) ou `#C94D4D` (mortalité) ; valeurs affichées directement, sans légende séparée.
- **Icônes** : trait 1,8 px, bouts arrondis, 20 px (24 px dans la navigation).

## 6. États

| État | Rendu |
|---|---|
| Champ normal | Fond `#F2F7F3`, bordure `#E1EAE3` |
| Champ actif | Fond blanc, bordure 2 px `#286B46` |
| Champ invalide | Fond blanc, bordure 2 px `#C94D4D`, message `#B03E3E` sous le champ |
| Chargement | Bouton `#174B35` désactivé avec indicateur circulaire et libellé « … en cours » |
| Liste vide | Encadré en pointillés, icône, phrase d'explication, bouton d'action |
| Enregistrement réussi | Bandeau `#DCE8DF` avec coche |
| Échec | Bandeau `#F6E3E3` avec action « Réessayer » |
| Confirmation de suppression | Boîte de dialogue : conséquence expliquée, « Annuler » / « Supprimer » |
| Calcul impossible | Tiret « — » à la place de la valeur, avec la raison |

## 7. Règles de calcul

- Effectif actuel = effectif initial + entrées − sorties − mortalité cumulée.
- Effectif vivant moyen du jour = (effectif en début de journée + effectif en fin de journée) ÷ 2.
- Taux de ponte (%) = œufs collectés ÷ effectif vivant moyen × 100. Non calculé si l'effectif est nul.
- Consommation par poule (g) = quantité distribuée (kg) × 1 000 ÷ effectif vivant moyen.
- Taux de mortalité cumulé (%) = mortalité cumulée ÷ effectif initial × 100.
- Indice de conversion alimentaire = kg d'aliment distribué ÷ kg d'œufs produits, avec kg d'œufs = œufs collectés × poids moyen d'un œuf (60 g par défaut, réglable dans « Unités de mesure »). Non affiché si l'une des deux données manque.

## 8. Données d'exemple (fictives)

Ferme des Ailes · Jean Dupont · Bande 1, souche légère, démarrée le 01/04/2025 avec 1 800 poules, sans entrée ni sortie.

| Date | Morts | Effectif fin de journée | Effectif moyen | Œufs | Taux de ponte | Aliment | g / poule |
|---|---|---|---|---|---|---|---|
| 12/05 | 1 | 1 759 | 1 759,5 | 1 465 | 83,3 % | — | — |
| 13/05 | 2 | 1 757 | 1 758 | 1 478 | 84,1 % | — | — |
| 14/05 | 1 | 1 756 | 1 756,5 | 1 490 | 84,8 % | — | — |
| 15/05 | 1 | 1 755 | 1 755,5 | 1 498 | 85,3 % | 206 kg | 117,3 |
| 16/05 | 2 | 1 753 | 1 754 | 1 512 | 86,2 % | 205 kg | 116,9 |
| 17/05 | 1 | 1 752 | 1 752,5 | 1 525 | 87,0 % | 206 kg | 117,5 |
| 18/05 | 2 | 1 750 | 1 751 | 1 532 | 87,5 % | 207 kg | 118,2 |

Bilan du 01/04 au 18/05/2025 (48 jours) : mortalité cumulée 50 (2,8 %), 52 850 œufs, 9 550 kg d'aliment, 85 240 poules-jours, taux moyen de ponte 62,0 %. Taux hebdomadaires : 20 · 41 · 59 · 70 · 78 · 84 · 85,8 %. Indice de conversion du 18/05 : 207 ÷ (1 532 × 0,060) = 2,25.

## 9. Écrans et parcours

| N° | Écran | Accès depuis | Mène vers |
|---|---|---|---|
| 00 | Bienvenue | Lancement | « Commencer » → 02 · « J'ai déjà un compte » → 01 |
| 01 | Connexion | 00, 02 | « Se connecter » → 03 · « Créer un compte » → 02 |
| 02 | Inscription | 00, 01 | « Créer mon compte » → 03 · « Se connecter » → 01 |
| 03 | Tableau de bord | 01, 02, navigation | Cartes → 04, 05, 06, 07, 08 · « Bilan » → 10 |
| 04 | Gestion des bandes | Navigation | Carte → 10 · « Nouvelle bande », « Modifier » → 04b |
| 04b | Nouvelle bande | 04 | « Annuler » → 04 |
| 05 | Production d'œufs | 03 | Historique → 09 |
| 06 | Consommation d'aliment | 03 | Historique → 09 |
| 07 | Mortalité | 03 | — |
| 08 | Indicateurs de rendement | 03, 09 | — |
| 09 | Historique des suivis | Navigation « Suivi » | « Modifier » → 05 · « Indicateurs » → 08 |
| 10 | Bilan par bande | 03, 04 | « Voir le détail » → 09 · « Exporter » → 11 |
| 11 | Rapports et exportation | Navigation, 10 | — |
| 12 | Profil et paramètres | Navigation « Plus », avatar | « Déconnexion » → 00 |

## 10. Transfert vers Google Stitch puis Figma

Ce qui n'existe pas : Stitch ne propose pas d'import de fichiers HTML, et Figma n'ouvre pas de fichiers HTML. Le transfert passe donc par les mécanismes ci-dessous.

1. **Dans Stitch** : créer un projet mobile, puis fournir ce fichier `DESIGN.md` comme système de conception (Stitch lit ce format). À défaut, coller les sections 2 à 6 dans la consigne.
2. **Écran par écran** : joindre l'image de l'écran exportée depuis la maquette (PNG) comme référence, avec une consigne courte du type « Reproduis cet écran en respectant DESIGN.md ». Générer les 13 écrans dans l'ordre 00 à 12 pour que le style reste stable.
3. **Vers Figma** : dans Stitch, utiliser l'action d'export « Copier vers Figma », puis coller dans un fichier Figma. Les calques arrivent modifiables ; vérifier l'écran d'accueil, les illustrations et les graphiques, que cet export restitue parfois mal.
4. **Alternative sans Stitch** : importer les écrans dans Figma avec un module de conversion HTML vers Figma (par exemple html.to.design), à partir des écrans de la maquette.
5. **Dans Figma** : créer les styles de couleur et de texte à partir des sections 2 et 3, transformer bouton, champ, carte, badge et barre de navigation en composants, puis relier les écrans selon le tableau de la section 9.

Les fonctions d'export de Stitch évoluent : vérifier le libellé exact du bouton dans la version utilisée.
