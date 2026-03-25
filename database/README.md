# Système de Sauvegarde Hybride - PostgreSQL + Local

Ce projet intègre maintenant un système de sauvegarde hybride qui conserve la logique actuelle (sauvegarde locale) tout en ajoutant une persistance PostgreSQL avec **historisation complète**.

## 🎯 Fonctionnalités

### Sauvegarde Locale (existante)
- Enregistrement des points dans un fichier texte
- Lecture des parties sauvegardées
- Noms des joueurs conservés

### Nouvelle Persistance PostgreSQL
- **Historisation complète** : tous les points sont conservés, même ceux supprimés
- **Soft Delete** : les points détruits ne sont jamais supprimés physiquement, ils sont marqués comme `is_deleted = TRUE`
- **Traçabilité missile** : chaque missile est enregistré avec :
  - Point de lancement
  - Point d'impact (avec marqueur visuel "X")
  - Joueur qui a lancé le missile
  - Si le missile a touché une cible
  - Quelle cible a été détruite
- **Historique complet** : possibilité de rejouer une partie ou analyser toutes les actions

## 📁 Structure de la Base de Données

### Tables principales
1. **players** : joueurs
2. **games** : parties
3. **game_points** : points placés avec historisation (soft delete)
4. **missile_actions** : actions missiles avec points d'impact

### Schéma complet
Voir le fichier `/database/schema.sql` pour le schéma complet avec index et fonctions.

## 🚀 Installation

### 1. Installer PostgreSQL avec Docker

```bash
# Créer un conteneur PostgreSQL
docker run --name point-game-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=point_game \
  -p 5432:5432 \
  -d postgres:16

# Vérifier que le conteneur fonctionne
docker ps
```

### 2. Initialiser le schéma

```bash
# Copier le schéma dans le conteneur
docker cp database/schema.sql point-game-db:/schema.sql

# Exécuter le schéma
docker exec -it point-game-db psql -U postgres -d point_game -f /schema.sql
```

### 3. Restaurer les packages NuGet

```bash
dotnet restore
```

## 💻 Utilisation dans le Code

### Initialisation du gestionnaire de sauvegarde

```csharp
using point;
using point.Database;

// Créer le gestionnaire de sauvegarde hybride
var saveManager = new GameSaveManager(clickedPoints);

// Configuration de la base de données
var dbConfig = DatabaseConfig.GetDefaultConfig();
// ou personnalisé :
var dbConfig = new DatabaseConfig
{
    Host = "localhost",
    Port = 5432,
    Database = "point_game",
    Username = "postgres",
    Password = "postgres"
};

// Activer la persistance PostgreSQL
bool connected = await saveManager.EnableDatabasePersistenceAsync(dbConfig);
if (connected)
{
    Console.WriteLine("✓ PostgreSQL activé");
}

// Démarrer une nouvelle partie
await saveManager.StartNewGameAsync("Joueur1", "Joueur2", 18, 18);
```

### Enregistrer un point

```csharp
// Enregistrer un point placé par un joueur
Point newPoint = new Point(100, 150);
await saveManager.SavePointAsync(newPoint, playerOrder: 0, turnNumber: 1);
```

### Enregistrer une action missile

```csharp
// Missile lancé
Point launchPoint = new Point(50, 100);
Point impactPoint = new Point(200, 100);
bool hitTarget = true;
Point targetPoint = new Point(200, 100);

await saveManager.SaveMissileActionAsync(
    launchPoint,
    impactPoint,
    power: 5,
    direction: 1, // 1 = droite, -1 = gauche
    playerOrder: 0,
    turnNumber: 2,
    hitTarget: hitTarget,
    targetPoint: targetPoint
);

// Si un point est touché, il est automatiquement marqué comme supprimé (soft delete)
// mais reste en base de données avec is_deleted = TRUE
```

### Historisation : Soft Delete

```csharp
// Marquer un point comme supprimé (historisation)
Point pointToDelete = new Point(100, 150);
await saveManager.SoftDeletePointAsync(pointToDelete, missileId: 123);

// Le point n'est PAS supprimé de la base !
// Il est simplement marqué : is_deleted = TRUE, deleted_at = NOW()
```

### Récupérer l'historique complet

```csharp
// Récupérer TOUS les points (actifs + supprimés)
var allPoints = await saveManager.GetGameHistoryAsync();

foreach (var point in allPoints)
{
    if (point.IsDeleted)
    {
        Console.WriteLine($"Point supprimé à {point.DeletedAt} par missile {point.DeletedByMissileId}");
    }
}

// Récupérer tous les missiles
var missiles = await saveManager.GetMissileHistoryAsync();

foreach (var missile in missiles)
{
    Console.WriteLine($"Missile: {missile.ImpactX}, {missile.ImpactY} - Hit: {missile.HitTarget}");
}
```

### Terminer une partie

```csharp
// Terminer la partie et définir le gagnant
await saveManager.EndGameAsync(winnerOrder: 0); // Joueur 1 gagne
```

### Mode Local uniquement

```csharp
// Désactiver PostgreSQL (retour au mode local)
saveManager.DisableDatabasePersistence();

// Sauvegarde locale classique
saveManager.WriteLocalSave("Joueur1", "Joueur2", clickedPoints);

// Chargement local
var points = saveManager.LoadLocalSave();
var players = GameSaveManager.LoadLocalPlayerList();
```

## 🔍 Fonctionnalités Avancées

### Vues SQL disponibles

```sql
-- Points actifs uniquement (non supprimés)
SELECT * FROM active_game_points WHERE game_id = 1;

-- Historique complet d'une partie (points + missiles)
SELECT * FROM game_history WHERE game_id = 1 ORDER BY turn_number;
```

### Fonctions SQL

```sql
-- Marquer un point comme supprimé
SELECT soft_delete_point(point_id, missile_id);

-- Obtenir le score actuel d'un joueur
SELECT get_player_score(game_id, player_id);
```

## 📊 Exemple d'Intégration dans Window.cs

```csharp
// Dans Window.Designer.cs ou Window.cs
private GameSaveManager? _saveManager;

// Au démarrage de la partie
private async void StartGame()
{
    _saveManager = new GameSaveManager(clickedPoints);

    // Demander à l'utilisateur s'il veut utiliser PostgreSQL
    var result = MessageBox.Show(
        "Voulez-vous utiliser la sauvegarde PostgreSQL ?",
        "Mode de sauvegarde",
        MessageBoxButtons.YesNo
    );

    if (result == DialogResult.Yes)
    {
        var config = DatabaseConfig.GetDefaultConfig();
        bool connected = await _saveManager.EnableDatabasePersistenceAsync(config);

        if (connected)
        {
            await _saveManager.StartNewGameAsync(player1.nom, player2.nom,
                                                 GameConfig.GridRows,
                                                 GameConfig.GridColumns);
        }
        else
        {
            MessageBox.Show("Connexion PostgreSQL échouée, mode local activé.");
        }
    }
}

// Lors du placement d'un point
private async void OnPointPlaced(Point newPoint, int playerOrder)
{
    clickedPoints.Add(newPoint);

    if (_saveManager != null)
    {
        await _saveManager.SavePointAsync(newPoint, playerOrder, tour);
    }
}

// Lors du lancement d'un missile
private async void OnMissileLaunched(Missile missile, Player player)
{
    var launchPoint = missile.Position;
    var impactPoint = missile.GetCurrentPosition();
    var hitTarget = missile.HitTarget;

    if (_saveManager != null)
    {
        await _saveManager.SaveMissileActionAsync(
            launchPoint,
            impactPoint,
            missile.Power,
            missile.Direction,
            player.Order,
            tour,
            hitTarget,
            hitTarget ? impactPoint : null
        );
    }
}
```

## 🐳 Docker Compose (optionnel)

Créez un fichier `docker-compose.yml` :

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    container_name: point-game-db
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: point_game
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/schema.sql:/docker-entrypoint-initdb.d/schema.sql

volumes:
  postgres_data:
```

Lancement :
```bash
docker-compose up -d
```

## 📝 Notes Importantes

1. **Soft Delete** : Les points ne sont JAMAIS supprimés physiquement de la base. Ils sont marqués `is_deleted = TRUE`.

2. **Historisation complète** : Tous les points et missiles sont conservés en base, permettant de :
   - Rejouer une partie
   - Analyser les stratégies
   - Détecter les patterns de jeu

3. **Indicateur visuel "X"** : Les missiles marquent leur point d'impact avec un "X" (géré par `Missile.paint()` avec `HitTarget = false`).

4. **Compatibilité** : Le système reste compatible avec la sauvegarde locale (fichier texte) qui continue de fonctionner.

5. **Migration progressive** : Vous pouvez activer/désactiver PostgreSQL à tout moment sans casser le code existant.

## 🛠️ Prochaines Étapes

- [ ] Ajouter un modal dans l'interface pour choisir le mode de sauvegarde
- [ ] Implémenter le chargement d'une partie depuis PostgreSQL
- [ ] Ajouter un viewer d'historique de partie
- [ ] Statistiques joueurs (victoires, défaites, points marqués)
