using System;
using System.Diagnostics;
using Npgsql;

namespace ThrowawayDb.Postgres
{
    public class ThrowawayDatabase : IDisposable
    {
        /// <summary>Returns the connection string of the database that was created</summary>
        public string ConnectionString { get; internal set; }
        /// <summary>Returns the name of the database that was created</summary>
        public string Name { get; internal set; }
        private bool databaseCreated;
        private readonly string originalConnectionString;
        private readonly string defaultDatabaseNamePrefix = "throwawaydb";

        private ThrowawayDatabase(string originalConnectionString, string? databaseNamePrefix)
        {
            // Default constructor is private
            this.originalConnectionString = originalConnectionString;
            var (derivedConnectionString, databaseName) = DeriveThrowawayConnectionString(originalConnectionString, databaseNamePrefix);
            
            ConnectionString = derivedConnectionString;
            Name = databaseName;
        }

        private void DropDatabaseIfCreated()
        {
            if (databaseCreated)
            {
                // Revoke future connections
                using (var connection = new NpgsqlConnection(this.originalConnectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand($@"
                        REVOKE CONNECT ON DATABASE { Name } FROM public;
                        select pg_terminate_backend(pid) from pg_stat_activity where datname='{Name}';
                        DROP DATABASE {Name};", connection))
                    {
                        var result = command.ExecuteNonQuery();
                    }
                }
            }
        }

        private (string connectionString, string databaseName) DeriveThrowawayConnectionString(string originalConnectionString, string? databaseNamePrefix)
        {
            var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);
            var databasePrefix = string.IsNullOrWhiteSpace(databaseNamePrefix) ? defaultDatabaseNamePrefix : databaseNamePrefix;

            var databaseName = databasePrefix + Guid.NewGuid().ToString("n")[..10].ToLowerInvariant();

            builder.Remove("Database");
            builder.Database = databaseName;

            return (builder.ConnectionString, databaseName);
        }

        /// <summary>
        /// Creates a new disposable database, using the <paramref name="template"/> database as a template.
        /// </summary>
        /// <param name="template">the <see cref="ThrowawayDatabase"/> to use as a template</param>
        /// <param name="databaseNamePrefix">a string to prepend to the new database's name</param>
        /// <returns></returns>
        public static ThrowawayDatabase FromTemplate(ThrowawayDatabase template, string? databaseNamePrefix = null)
        {
            var connectionBuilder = new NpgsqlConnectionStringBuilder(template.ConnectionString);
            var (username, password, host) = (connectionBuilder.Username, connectionBuilder.Password, connectionBuilder.Host);

            if (username is null || password is null || host is null)
                throw new ArgumentNullException("One ore more connection string parameters are null");

            return Create(username, password, host, databaseNamePrefix, template);
        }

        public static ThrowawayDatabase Create(string username, string password, string host, string? databaseNamePrefix = null, ThrowawayDatabase? template = null)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Username = username,
                Password = password,
                Host = host,
                Database = "postgres" // we need an existing db to test the connection
            };

            var connectionString = connectionStringBuilder.ConnectionString; // $"Host={host}; Username={username}; Password={password}; Port=5432; Database=postgres";
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

            var database = new ThrowawayDatabase(connectionString, databaseNamePrefix);
            if (!database.CreateDatabaseIfDoesNotExist(template))
            {
                throw new Exception("Could not create the throwaway database");
            }

            return database;
        }

        public static ThrowawayDatabase Create(string connectionString, string? databaseNamePrefix = null)
        {
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

            var database = new ThrowawayDatabase(connectionString, databaseNamePrefix);
            if (!database.CreateDatabaseIfDoesNotExist())
            {
                throw new Exception("Could not create the throwaway database");
            }

            return database;
        }

        private bool CreateDatabaseIfDoesNotExist(ThrowawayDatabase? template = null)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(this.ConnectionString);
                if (builder.TryGetValue("Database", out var database))
                {
                    var databaseName = database.ToString();
                    builder.Database = "postgres";
                    var connectionStringOfMaster = builder.ToString();
                    using (var otherConnection = new NpgsqlConnection(connectionStringOfMaster))
                    {
                        otherConnection.Open();
                        using (var createCmd = new NpgsqlCommand($"CREATE DATABASE {databaseName}", otherConnection))
                        {
                            if (template is { Name: var templateName })
                                createCmd.CommandText += $" TEMPLATE {templateName}";

                            var result = createCmd.ExecuteNonQuery();
                            Debug.Print($"Successfully created database {databaseName}");
                            this.databaseCreated = true;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
        }

        private static bool TryPingDatabase(string originalConnectionString)
        {
            try
            {
                using (var connection = new NpgsqlConnection(originalConnectionString))
                {
                    connection.Open();
                    using (var cmd = new NpgsqlCommand("select now()", connection))
                    {
                        cmd.ExecuteScalar();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error while pinging the Sql server at '{originalConnectionString}'");
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }

        public void Dispose() => DropDatabaseIfCreated();
    }
}
