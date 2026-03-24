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





