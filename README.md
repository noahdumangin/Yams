# Solveur de Sudoku en Assembleur MIPS32

Ce projet propose un **solveur de Sudoku écrit en assembleur MIPS32**. Le programme lit une grille de Sudoku depuis un fichier, la convertit en nombres, résout le puzzle et affiche la solution à l’écran de manière claire et lisible.


## Table des matières
- [Prérequis](#prérequis)  
- [Structure du projet](#structure-du-projet)  
- [Fonctionnement](#fonctionnement)  
- [Fonctions principales](#fonctions-principales)  
- [Exécution](#exécution)  
- [Exemple de sortie](#exemple-de-sortie)  


## Prérequis
- Un **émulateur MIPS** (par exemple MARS ou SPIM).  
- Un **fichier texte** contenant une grille de Sudoku (`sudoku.txt`) au format **81 caractères**, où `0` représente une case vide.


## Structure du projet

### Section données
- **filename** : chemin vers le fichier contenant la grille.  
- **buffer** : zone tampon pour lire les caractères du fichier.  
- **grille** : tableau représentant la grille du Sudoku.  
- **squares** et **offsets** : indices utilisés pour parcourir les sous-grilles (carrés 3x3).

### Section code
Le programme est organisé en plusieurs fonctions, chacune avec un rôle précis :  
- Lecture et transformation de la grille.  
- Vérification de la validité des lignes, colonnes et carrés.  
- Résolution du Sudoku.  
- Affichage formaté de la grille.

---

## Fonctionnement

1. **Lecture du fichier** : `parseValues` lit le fichier caractère par caractère et remplit la grille.  
2. **Conversion ASCII → entier** : `transformAsciiValues` convertit les caractères ASCII en valeurs numériques.  
3. **Résolution** : `solve_sudoku` remplit les cases vides de manière récursive en respectant les règles du Sudoku.  
4. **Affichage** : `displayGrille` ou `displaysudoku` affiche la grille avec des séparateurs `|` et `-` pour délimiter les sous-grilles.


## Fonctions principales

| Fonction | Description |
|----------|-------------|
| `parseValues` | Charge la grille depuis un fichier. |
| `transformAsciiValues` | Convertit les caractères ASCII en entiers. |
| `solve_sudoku` | Résout la grille de Sudoku de manière récursive. |
| `displayGrille` | Affiche la grille avec une mise en forme lisible. |
| `check_n_row / check_n_column / check_n_square` | Vérifie qu’une ligne, colonne ou carré est valide. |
| `check_sudoku` | Vérifie que la grille entière est correcte. |
| `zeroToSpace` | Remplace les `0` par des espaces pour un affichage plus clair. |
| `addPipe / addDash` | Ajoute des séparateurs visuels dans l’affichage. |


## Exécution

1. Vérifier que le chemin du fichier dans `filename` est correct.  
2. Charger le fichier `.asm` dans MARS ou un autre émulateur MIPS.  
3. Exécuter le programme (`main`).  
4. La grille résolue s’affiche automatiquement dans la console.


## Exemple de sortie

```text
5 3 4 | 6 7 8 | 9 1 2
6 7 2 | 1 9 5 | 3 4 8
1 9 8 | 3 4 2 | 5 6 7
---------------------
8 5 9 | 7 6 1 | 4 2 3
4 2 6 | 8 5 3 | 7 9 1
7 1 3 | 9 2 4 | 8 5 6
---------------------
9 6 1 | 5 3 7 | 2 8 4
2 8 7 | 4 1 9 | 6 3 5
3 4 5 | 2 8 6 | 1 7 9
