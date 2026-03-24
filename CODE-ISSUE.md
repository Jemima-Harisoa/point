# Alleger le code et améliorer les performances
##  Analyse des Bottlenecks : goulots d'étranglement

### **1. PROBLÈME MAJEUR: `getLineList()` - O(n²) ou pire** 
```
Pour 100 points: ~10 000 opérations
Pour 500 points: ~250 000 opérations
```
- Appelle `getSameColor()` à chaque itération 
- Recalcule depuis zéro à chaque fois
- Crée des listes temporaires massives
- `OrderBy()` coûteux appelé en boucle
**Solution:**  stocker dans un `Dictionary<Color, List<Point>>` au lieu de seulement `List<Point>` (On garde les swicth 0 et 1 pour les couleurs, mais on stocke les points dans un dictionnaire pour un accès rapide.)

### **2. `LShapeLine()` - Récursif et coûteux**
- Appelle `Intersect()` (O(n²)) pour chaque intersection
- `Construct()` crée des combinaisons exponentielles
- Appelée plusieurs fois par tour

**Solution:**  
1. Simplifier la logique pour éviter les appels récursifs et limiter les intersections à vérifier (ex: seulement les lignes proches du point actuel). Au lieu d'appeler LShapeLine() - Intersect() va prendre en argumenet une liste de lignes déjà calculées et vérifier seulement celles qui sont pertinentes (ex: celles qui partagent un point avec la ligne actuelle). Donc on garde la logique de Intersect() 

2. Utiliser un cache pour les résultats d'intersection afin d'éviter les recalculs redondants. On peut stocker les résultats d'intersection dans un `Dictionary<(Point, Point), bool>` pour éviter de recalculer les mêmes paires de points.


### **3. `Intersect()` - Comparaison de toutes les paires**
- Compare CHAQUE paire de lignes (O(n²))
- Utilisé par `Suggest()` à chaque suggestion

**Solution:** On ne compare que lorsqu'un point est ajouté ou déplacé, et on ne compare que les lignes qui partagent un point commun avec la ligne modifiée. On peut aussi utiliser un `HashSet<Point>` pour vérifier rapidement si une ligne contient un point donné, au lieu de faire des boucles imbriquées.

### **4. `Suggest()` - Recalcule tout**
- Appelle `LShapeLine()` + `getLineList()` à nouveau
- Boucles imbriquées sur les suggestions
- Réappelle les mêmes fonctions plusieurs fois

**Solution:**  Pré-calculer les suggestions pour chaque point ajouté et stocker ces suggestions dans un cache. Par exemple, on peut avoir un `Dictionary<Point, List<Point>>` qui stocke les suggestions pour chaque point sur le plateau. Lorsqu'un point est ajouté ou déplacé, on met à jour ce cache au lieu de recalculer tout à partir de zéro.

### **5. `paint()` et `Draw()` appelés en boucle**
- À chaque `space.Invalidate()`
- Pas de double buffering
- Recalcule **chaque fois** même si les données n'ont pas changé

**Solution:** Implémenter un système de double buffering pour réduire le flickering et améliorer les performances de rendu. De plus, introduire un drapeau `isDirty` pour ne redessiner que lorsque les données ont réellement changé, au lieu de redessiner à chaque appel d'invalidation. 
---


## Bug identifie : 
- Les ligne en diagonal ne sont pas paint correctement 
- Les suggestions de point ne redconnaissent pas les ligne en L quand elle on une formation U 
- La detection du L prend du retard pour la condition gagnante
- Il y a des intersections ou on a peut pas ajouter de point quand bien meme qu'il y a pas de point dessus 

# Commenter les logiques de jeu sur les suggestion de point et le condition de victoire dans le cas ou un jour alignerait 5 points en L
- [x] Simplifier la logique du jeu victoire si n point aligne en ligne droite
- [x] Commenter les boutons de suggestion
- [x] Parametrer le nombre de ligne et colonne ainsi que le nombre de ligne a aligner pour dire que le joueur a gagner (defaut 5) avec le modal de depart 


# TODO — Feature Missile

## Statut global

| Tâche | Statut |
|---|---|
| Input colonnes/lignes dans ShowStartForm | ✅ Fait |
| Classe `Missile` | ⬜ À faire |
| Intégration dans le tour de jeu | ⬜ À faire |
| UI barre de lancement (`MissileBar`) | ⬜ À faire |
| Gestion des collisions et destruction | ⬜ À faire |

---

## ✅ Ajouter un input pour paramétrer colonnes/lignes — FAIT
> Déjà implémenté dans `ShowStartForm()` dans `Window_Designer.cs`

---

## ✅ 1. Créer `Missile.cs` (nouveau fichier)

Créer le fichier `Missile.cs` dans le projet, namespace `point`.

### ✅ 1.1 Créer l'enum `MissileState`
En dehors de la classe, dans le même fichier :
```csharp
public enum MissileState { Ready, Flying, Destroyed }
```

### ✅ 1.2 Propriétés de la classe `Missile`

| Propriété | Type | Description |
|---|---|---|
| `Position` | `Point` | Case de départ sur la grille (alignée sur `GameConfig.GridSize`) |
| `Power` | `int` | Puissance de 1 à 5 — nombre de cases parcourues |
| `State` | `MissileState` | État courant : `Ready`, `Flying`, `Destroyed` |
| `Direction` | `int` | Direction de vol — reprend les valeurs de `Line.Equation()` : `1`=vertical, `2`=horizontal, `3`=diagonale croissante, `4`=diagonale décroissante |
| `OwnerColor` | `Color` | Couleur du joueur propriétaire, pour le rendu |
| `Trajectory` | `List<Point>` | Liste des cases traversées, calculée au lancement — privée |

### ✅ 1.3 Méthodes

**`void Launch(Point from, int direction, int power)`**
- Assigne `Position`, `Direction`, `Power`
- Calcule `Trajectory` : boucle `power` fois en appelant `Equation(direction, point)` sur chaque point successif
  - `Equation` est `private` dans `Line` → la dupliquer dans `Missile` en `private static Point Step(int dir, Point p, int gridSize)`, même logique
- Met `State = MissileState.Flying`

**`List<Point> GetTrajectory()`**
- Retourne une copie de `Trajectory` (lecture seule pour l'extérieur)

**`bool CheckCollision(List<Point> enemyPoints)`**
- Parcourt `Trajectory`
- Si un point de la trajectoire est dans `enemyPoints` → retourne `true`
- Sinon → retourne `false`
- Ne modifie pas encore la liste (la suppression se fait dans `Window_Designer`)

**`void paint(object sender, PaintEventArgs e)`**
- Si `State == Flying` :
  - Dessiner une ligne de `Position` jusqu'au dernier point de `Trajectory` avec un `Pen` de la couleur `OwnerColor`, épaisseur 3
  - Dessiner une petite flèche ou cercle plein au bout de la trajectoire pour matérialiser l'impact
- Si `State == Destroyed` :
  - Dessiner un cercle orange/rouge centré sur le dernier point de `Trajectory`, rayon ~10px, semi-transparent (`Color.FromArgb(180, 255, 80, 0)`)
- Si `State == Ready` : ne rien dessiner

---

## ✅ 2. Modifier `Player.cs`

### ✅ 2.1 Ajouter les propriétés missiles
```csharp
public List<Missile> Missiles { get; private set; } = new List<Missile>();
public bool HasLaunchedMissileThisTurn { get; set; } = false;
```

### ✅ 2.2 Ajouter la méthode `CreateMissile()`
```csharp
public Missile CreateMissile()
// → instancie un new Missile avec OwnerColor = this.color
// → l'ajoute à Missiles
// → retourne le missile pour que Window_Designer puisse appeler Launch() dessus
```

### ✅ 2.3 Ajouter la méthode `ResetTurn()`
```csharp
public void ResetTurn()
// → remet HasLaunchedMissileThisTurn = false
// → à appeler dans Window_Designer au changement de tour
```

---

## ✅ 3. Modifier `Window_Designer.cs`

### ✅ 3.1 Ajouter la méthode `MissileBar(Player player, Player adversaire)`
Retourne un `Panel` à insérer dans `LeftInterface` (joueur 1) et `RightInterface` (joueur 2).

Contenu du panel :
- `Label` : "Ligne de départ :"
- `NumericUpDown` pour choisir la colonne de départ (min=1, max=`GameConfig.GridColumns - 1`)
- `Label` : "Puissance (1–5) :"
- `TrackBar` slider puissance (min=1, max=5, valeur initiale=3)
- `Label` affichant la valeur du slider en temps réel
- `Label` : "Direction :"
- `ComboBox` avec les options : `Vertical`, `Horizontal`, `Diagonale ↗`, `Diagonale ↘` (mappe sur `1`, `2`, `3`, `4`)
- `Button` "🚀 Lancer !"

**Logique du bouton "Lancer !" :**
1. Vérifier `tour % 2 == player.Order && game` — sinon ignorer
2. Vérifier `!player.HasLaunchedMissileThisTurn` — sinon `MessageBox.Show("Déjà lancé ce tour")`
3. Calculer `Position` : `new Point(colonne * GameConfig.GridSize, 0)` si vertical (à adapter selon direction choisie)
4. Appeler `player.CreateMissile()`
5. Appeler `missile.Launch(position, direction, power)`
6. Appeler `missile.CheckCollision(adversaire.line.getSameColor())` — ⚠️ `getSameColor()` est `private` dans `Line`, voir note ci-dessous
7. Si collision → appeler `RemoveEnemyPoint(point_touché, adversaire)` (voir 3.3)
8. `player.HasLaunchedMissileThisTurn = true`
9. `tour++`
10. `adversaire.ResetTurn()`
11. `_isDirty = true; space.Invalidate()`

> ⚠️ **Note sur `getSameColor()`** : cette méthode est `private` dans `Line`. Il faudra soit la passer en `public`, soit ajouter une méthode publique `GetPlayerPoints()` dans `Line` qui la délègue. Préférer la deuxième option pour ne pas exposer trop l'intérieur de `Line`.

### ✅ 3.2 Brancher `MissileBar` dans `InitializeComponent()`
Remplacer les blocs commentés des boutons Suggest par :
```csharp
LeftInterface.Controls.Add(MissileBar(player1, player2));
RightInterface.Controls.Add(MissileBar(player2, player1));
```

### ✅ 3.3 Ajouter `RemoveEnemyPoint(Point p, Player adversaire)`
Méthode privée dans `Window_Designer` :
- Recherche `p` dans `clickedPoints`
- Trouver son index
- **Problème clé** : supprimer le point casse l'alternance pair/impair qui identifie les joueurs
- **Solution retenue** : remplacer le point par `Point.Empty` (ou `new Point(-1, -1)`) pour le "masquer" sans casser les index → à adapter dans `paint()` pour ignorer les `Point.Empty`

### ✅ 3.4 Mettre à jour `space_MouseClick()`
Ajouter en début de la condition `if (game && hasStarted)` :
```csharp
// Bloquer le clic si le joueur courant a déjà lancé un missile ce tour
Player currentPlayer = (clickedPoints.Count % 2 == 0) ? player1 : player2;
if (currentPlayer.HasLaunchedMissileThisTurn) return;
```

### ✅ 3.5 Mettre à jour `Draw()`
Après les vérifications de victoire, ajouter :
```csharp
foreach (var missile in player1.Missiles)
    missile.paint(sender, e);

foreach (var missile in player2.Missiles)
    missile.paint(sender, e);
```

### ✅ 3.6 Mettre à jour `restart()`
Ajouter le nettoyage des missiles au restart :
```csharp
player1.Missiles.Clear();
player2.Missiles.Clear();
player1.HasLaunchedMissileThisTurn = false;
player2.HasLaunchedMissileThisTurn = false;
```

---

## ✅ 4. Modifier `Line.cs` (modification mineure)

### ✅ 4.1 Exposer les points d'un joueur
Ajouter une méthode publique pour que `MissileBar` puisse récupérer les points ennemis sans accéder à `getSameColor()` directement :
```csharp
public List<Point> GetPlayerPoints()
{
    return getSameColor(); // délègue simplement
}
```

---

## Ordre d'implémentation recommandé

```
1. Missile.cs         → isolé, zéro dépendance UI, testable seul
2. Player.cs          → ajouter Missiles + méthodes, tester que ça compile
3. Line.cs            → ajouter GetPlayerPoints(), 3 lignes
4. Window_Designer    → MissileBar UI, brancher dans InitializeComponent
5. Window_Designer    → RemoveEnemyPoint + mise à jour paint/Draw/restart
```

---

## Points de vigilance

| Risque | Détail |
|---|---|
| Alternance pair/impair cassée | Supprimer un point de `clickedPoints` décale tous les index suivants — utiliser `Point.Empty` en remplacement |
| `getSameColor()` privée | Ne pas la rendre publique directement, ajouter `GetPlayerPoints()` |
| `Equation()` privée dans `Line` | La dupliquer en `private static Step()` dans `Missile` — ne pas créer de couplage fort |
| Missile hors grille | `Launch()` doit vérifier que chaque point de la trajectoire est dans les limites (`> 0` et `< Width/Height`) et stopper si on sort |



