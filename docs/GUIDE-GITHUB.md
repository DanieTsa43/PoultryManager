# Procédure GitHub — PoultryManager

Ce guide explique comment créer le dépôt GitHub du projet, puis comment y envoyer le code à chaque étape.
Il se fait entièrement depuis **Visual Studio** (une variante en ligne de commande est donnée à la fin).

---

## A. Préparation (une seule fois)

1. **Créer un compte GitHub** sur <https://github.com> (gratuit), si ce n'est pas déjà fait.
2. **Vérifier Git** : Visual Studio installe Git automatiquement. Pour le contrôler, ouvrez un terminal (*Affichage › Terminal*) et tapez `git --version`.
3. **Connecter GitHub à Visual Studio** : *Fichier › Paramètres du compte… › Ajouter* (sous « Comptes GitHub ») et connectez-vous dans la fenêtre du navigateur.
4. **Renseigner votre identité Git** (elle apparaît dans l'historique) : dans le terminal :
   ```
   git config --global user.name "Votre Nom"
   git config --global user.email "votre-adresse@exemple.com"
   ```

---

## B. Étape 1 — Créer le dépôt et envoyer l'initialisation

1. Créez un dossier de travail, par exemple `C:\Projets\`.
2. Extrayez **PoultryManager-etape1.zip** dans `C:\Projets\`.
   Vous obtenez `C:\Projets\PoultryManager\`, qui contient `PoultryManager.sln`.
3. Double-cliquez sur `PoultryManager.sln` pour ouvrir le projet dans Visual Studio.
4. **Testez** : cible **Windows Machine** puis **F5**. L'écran « Étape 1 · Initialisation du projet » s'affiche ; le bouton « Tester la liaison MVVM » met à jour le message.
5. Arrêtez l'application (bouton ■).
6. Menu **Git › Créer un dépôt Git…**
   - Colonne de gauche : **GitHub**.
   - Compte : le vôtre.
   - Nom du dépôt : `PoultryManager`.
   - Description : `Application .NET MAUI de gestion d'un élevage de poules pondeuses`.
   - Cochez **Dépôt privé** si vous ne voulez pas qu'il soit public.
   - Le modèle `.gitignore` et la licence sont inutiles : ils sont déjà fournis dans le projet (choisir « Aucun » s'ils sont proposés).
7. Cliquez sur **Créer et envoyer (Create and Push)**.
   Visual Studio crée le dépôt sur GitHub, fait le premier *commit* et l'envoie.
8. Vérifiez sur <https://github.com/VOTRE-COMPTE/PoultryManager> que les fichiers sont présents.

> Pour renommer le premier commit (facultatif) : ce n'est pas nécessaire ; les commits suivants porteront des messages clairs.

---

## C. Étapes suivantes (2, 3, 4…) — Mettre à jour et envoyer

Chaque livraison est fournie sous forme d'un zip contenant **le projet complet** à cet état.

1. **Fermez Visual Studio.**
2. Ouvrez `C:\Projets\PoultryManager\` dans l'Explorateur de fichiers.
   Affichez les éléments masqués (*Affichage › Afficher › Éléments masqués*) : vous voyez un dossier **`.git`**.
3. **Supprimez tout le contenu du dossier SAUF le dossier `.git`.**
   (Le dossier `.git` contient l'historique : il ne faut surtout pas le supprimer.)
4. Extrayez le nouveau zip (ex. **PoultryManager-etape2.zip**) dans `C:\Projets\`, en acceptant de **remplacer** les fichiers.
   Le dossier `C:\Projets\PoultryManager\` contient à nouveau `PoultryManager.sln`, à côté de `.git`.
5. Ouvrez `PoultryManager.sln` et **testez** (voir la liste de tests de l'étape dans le README).
6. Ouvrez la fenêtre **Modifications Git** (*Affichage › Modifications Git*, ou menu *Git › Valider ou remiser*).
   Vous voyez les fichiers ajoutés (A), modifiés (M) et supprimés (D).
7. Saisissez un message, par exemple :
   `Étape 2 : écrans Bienvenue et Connexion`
8. Cliquez sur la flèche du bouton **Valider tout** puis **Valider tout et envoyer (Commit All and Push)**.
   (Ou : **Valider tout**, puis le bouton **Envoyer / Push** ↑ en haut de la fenêtre.)
9. Vérifiez sur GitHub que le nouveau commit apparaît.

**Facultatif — marquer chaque étape d'une étiquette :** fenêtre *Git › Afficher l'historique de la branche*, clic droit sur le commit › **Nouvelle étiquette** (`etape-2`), puis *Git › Envoyer* (cocher l'envoi des étiquettes). Ou en ligne de commande : `git tag etape-2` puis `git push --tags`.

---

## D. Variante en ligne de commande

Dans un terminal ouvert dans `C:\Projets\PoultryManager\` :

**Étape 1 (une seule fois)** — créez d'abord sur <https://github.com/new> un dépôt **vide** nommé `PoultryManager` (sans README, sans .gitignore, sans licence), puis :
```
git init
git add .
git commit -m "Étape 1 : initialisation du projet (structure MVVM, navigation, thème)"
git branch -M main
git remote add origin https://github.com/VOTRE-COMPTE/PoultryManager.git
git push -u origin main
```

**Étapes suivantes** (après avoir remplacé les fichiers, sections C.1 à C.5) :
```
git add -A
git commit -m "Étape 2 : écrans Bienvenue et Connexion"
git push
```

`git add -A` prend aussi en compte les fichiers supprimés d'une étape à l'autre.

---

## E. Problèmes fréquents

| Problème | Solution |
|---|---|
| Des dossiers `bin`, `obj` ou `.vs` apparaissent dans les modifications | Le fichier `.gitignore` est absent : vérifiez qu'il est bien à la racine, à côté de `PoultryManager.sln`. |
| « Updates were rejected » lors de l'envoi | Le dépôt GitHub contient un fichier créé en ligne (README…). Faites *Git › Tirer (Pull)* puis renvoyez. |
| Visual Studio demande de se connecter à chaque envoi | Reconnectez le compte dans *Fichier › Paramètres du compte*. |
| Le dossier `.git` a été supprimé par erreur | Clonez à nouveau le dépôt (*Git › Cloner un dépôt*), puis recommencez l'étape C à partir du point 3. |
