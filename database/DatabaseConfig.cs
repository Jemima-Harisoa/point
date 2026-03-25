namespace point.Database;

/// <summary>
/// Configuration de la connexion à la base de données PostgreSQL
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "point_game";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = "postgres";

    /// <summary>
    /// Génère la chaîne de connexion PostgreSQL
    /// </summary>
    public string GetConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
    }

    /// <summary>
    /// Configuration par défaut pour le développement local
    /// </summary>
    public static DatabaseConfig GetDefaultConfig()
    {
        return new DatabaseConfig
        {
            Host = "localhost",
            Port = 5432,
            Database = "point_game",
            Username = "postgres",
            Password = "postgres"
        };
    }

    /// <summary>
    /// Configure depuis une chaîne de connexion personnalisée
    /// </summary>
    public static DatabaseConfig FromConnectionString(string connectionString)
    {
        var config = new DatabaseConfig();
        var parts = connectionString.Split(';');

        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length != 2) continue;

            var key = keyValue[0].Trim().ToLower();
            var value = keyValue[1].Trim();

            switch (key)
            {
                case "host":
                    config.Host = value;
                    break;
                case "port":
                    if (int.TryParse(value, out int port))
                        config.Port = port;
                    break;
                case "database":
                    config.Database = value;
                    break;
                case "username":
                    config.Username = value;
                    break;
                case "password":
                    config.Password = value;
                    break;
            }
        }

        return config;
    }
}
