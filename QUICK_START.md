# Guide de Démarrage Rapide - PostgreSQL

## 🚀 Installation Rapide (5 minutes)

### Étape 1 : Démarrer PostgreSQL avec Docker

```bash
# Méthode 1 : Docker Compose (recommandé)
cd c:\Users\ACER\Documents\L2\Multi-Langage\C#\point
docker-compose up -d

# Méthode 2 : Docker direct
docker run --name point-game-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=point_game \
  -p 5432:5432 \
  -d postgres:16
```

### Étape 2 : Initialiser la Base de Données

```bash
# Si vous utilisez docker-compose, le schéma est automatiquement chargé !
# Sinon, exécutez :
docker cp database/schema.sql point-game-db:/schema.sql
docker exec -it point-game-db psql -U postgres -d point_game -f /schema.sql
```

### Étape 3 : Vérifier l'Installation

```bash
# Se connecter à PostgreSQL
docker exec -it point-game-db psql -U postgres -d point_game

# Lister les tables
\dt

# Vérifier les tables créées
# Vous devriez voir : players, games, game_points, missile_actions
\q  # Pour quitter
```

### Étape 4 : Restaurer les Packages

```bash
cd c:\Users\ACER\Documents\L2\Multi-Langage\C#\point
dotnet restore
```

### Étape 5 : Lancer le Jeu

```bash
dotnet run
```

---

## 📋 Commandes Utiles

### Docker

```bash
# Démarrer PostgreSQL
docker-compose up -d

# Arrêter PostgreSQL
docker-compose down

# Arrêter et supprimer les données
docker-compose down -v

# Voir les logs
docker-compose logs -f postgres

# Redémarrer
docker-compose restart
```

### PostgreSQL

```bash
# Se connecter au shell PostgreSQL
docker exec -it point-game-db psql -U postgres -d point_game

# Une fois connecté :
\dt              # Lister les tables
\d players       # Décrire la table players
\dv              # Lister les vues
\df              # Lister les fonctions

# Requêtes utiles :
SELECT * FROM players;
SELECT * FROM games;
SELECT * FROM active_game_points;  # Points actifs uniquement
SELECT * FROM game_history;        # Historique complet

# Quitter
\q
```

### .NET

```bash
# Restaurer les packages
dotnet restore

# Compiler
dotnet build

# Lancer
dotnet run

# Nettoyer
dotnet clean
```

---

## 🔧 Configuration Personnalisée

### Modifier la Connexion PostgreSQL

Éditez `DatabaseConfig` dans votre code :

```csharp
var config = new DatabaseConfig
{
    Host = "localhost",      // Changer si serveur distant
    Port = 5432,
    Database = "point_game",
    Username = "postgres",
    Password = "votre_mot_de_passe"
};
```

Ou utilisez une chaîne de connexion :

```csharp
var config = DatabaseConfig.FromConnectionString(
    "Host=localhost;Port=5432;Database=point_game;Username=postgres;Password=postgres"
);
```

---

## 🐛 Dépannage

### Problème : PostgreSQL ne démarre pas

```bash
# Vérifier si le port 5432 est déjà utilisé
netstat -ano | findstr :5432

# Arrêter tout conteneur existant
docker stop point-game-db
docker rm point-game-db

# Redémarrer
docker-compose up -d
```

### Problème : Connexion refusée

```bash
# Vérifier que PostgreSQL est démarré
docker ps

# Vérifier les logs
docker logs point-game-db

# Tester la connexion
docker exec -it point-game-db pg_isready -U postgres
```

### Problème : Schéma non chargé

```bash
# Réinitialiser la base
docker-compose down -v
docker-compose up -d

# Attendre quelques secondes, puis vérifier
docker exec -it point-game-db psql -U postgres -d point_game -c "\dt"
```

### Problème : Package Npgsql manquant

```bash
# Installer manuellement
dotnet add package Npgsql

# Ou version spécifique
dotnet add package Npgsql --version 8.0.2
```

---

## 📊 Requêtes SQL Utiles

### Statistiques de Partie

```sql
-- Nombre de parties par joueur
SELECT p.name, COUNT(g.id) as games_played
FROM players p
LEFT JOIN games g ON (p.id = g.player1_id OR p.id = g.player2_id)
GROUP BY p.name;

-- Parties gagnées par joueur
SELECT p.name, COUNT(g.id) as games_won
FROM players p
LEFT JOIN games g ON p.id = g.winner_id
GROUP BY p.name;
```

### Historique d'une Partie

```sql
-- Tous les points d'une partie (actifs + supprimés)
SELECT
    gp.turn_number,
    p.name as player,
    gp.x_coordinate,
    gp.y_coordinate,
    gp.is_deleted,
    gp.deleted_at
FROM game_points gp
JOIN players p ON gp.player_id = p.id
WHERE gp.game_id = 1
ORDER BY gp.turn_number;

-- Tous les missiles d'une partie
SELECT
    ma.turn_number,
    p.name as player,
    ma.impact_x,
    ma.impact_y,
    ma.power,
    ma.hit_target
FROM missile_actions ma
JOIN players p ON ma.player_id = p.id
WHERE ma.game_id = 1
ORDER BY ma.turn_number;
```

### Analyse de Gameplay

```sql
-- Points moyens par partie
SELECT AVG(point_count) as avg_points
FROM (
    SELECT game_id, COUNT(*) as point_count
    FROM game_points
    WHERE is_deleted = FALSE
    GROUP BY game_id
) AS subquery;

-- Missiles avec le plus de succès
SELECT
    p.name,
    COUNT(*) as total_missiles,
    SUM(CASE WHEN hit_target THEN 1 ELSE 0 END) as hits,
    ROUND(100.0 * SUM(CASE WHEN hit_target THEN 1 ELSE 0 END) / COUNT(*), 2) as hit_rate
FROM missile_actions ma
JOIN players p ON ma.player_id = p.id
GROUP BY p.name;
```

---

## 🗄️ Sauvegarde et Restauration

### Sauvegarder la Base

```bash
# Sauvegarder toute la base
docker exec -it point-game-db pg_dump -U postgres point_game > backup.sql

# Sauvegarder uniquement les données
docker exec -it point-game-db pg_dump -U postgres --data-only point_game > data_backup.sql
```

### Restaurer la Base

```bash
# Restaurer depuis un fichier
cat backup.sql | docker exec -i point-game-db psql -U postgres point_game

# Windows PowerShell
Get-Content backup.sql | docker exec -i point-game-db psql -U postgres point_game
```

---

## 📱 Prochaines Étapes

1. ✅ PostgreSQL configuré
2. ✅ Schéma créé
3. ✅ Package Npgsql installé
4. ✅ Gestionnaire de sauvegarde prêt

**Il ne reste plus qu'à :**

- Intégrer `GameSaveManager` dans `Window.cs`
- Ajouter les appels lors des événements (points, missiles)
- Tester le système complet

Consultez `/Examples/WindowIntegrationExample.cs` pour voir comment intégrer le système dans votre code.

---

## 📞 Support

En cas de problème :

1. Vérifiez les logs Docker : `docker logs point-game-db`
2. Consultez `/database/README.md` pour la documentation complète
3. Vérifiez que le port 5432 n'est pas bloqué par le firewall
