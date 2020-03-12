using System;
using System.Diagnostics;
using Npgsql;

namespace ThrowawayDb.Postgres
{
    public class ThrowawayDatabase : IDisposable
    {
        /// <summary>Returns the connection string of the database that was created</summary>
        public string ConnectionString { get; internal set; } = "";
        /// <summary>Returns the name of the database that was created</summary>
        public string Name { get; internal set; } = "";
        private bool databaseCreated = false;
        private string originalConnectionString = "";

        private ThrowawayDatabase(string originalConnectionString)
        {
            // Default constructor is private
            this.originalConnectionString = originalConnectionString;
            var (derivedConnectionString, databaseName) = DeriveThrowawayConnectionString(originalConnectionString);
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

                }

                using (var connection = new NpgsqlConnection(this.originalConnectionString))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand($"REVOKE CONNECT ON DATABASE {Name} FROM public", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand($"select pg_terminate_backend(pid) from pg_stat_activity where datname='{Name}'", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand($"DROP DATABASE {Name}", connection))
                    {
                        var result = cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private (string connectionString, string databaseName) DeriveThrowawayConnectionString(string originalConnectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);

            var databaseName = $"throwawaydb{Guid.NewGuid().ToString("n").Substring(0, 10).ToLowerInvariant()}";

            if (builder.TryGetValue("Database", out var initialDb))
            {
                builder.Remove("Database");
            }

            builder.Database = databaseName;
            return (builder.ConnectionString, databaseName);
        }

        public static ThrowawayDatabase Create(string username, string password, string host)
        {
            var connectionString = $"Host={host}; Username={username}; Password={password}; Port=5432; Database=postgres";
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

            var database = new ThrowawayDatabase(connectionString);

            if (!database.CreateDatabaseIfDoesNotExist())
            {
                throw new Exception("Could not create the throwaway database");
            }

            return database;
        }

        public static ThrowawayDatabase Create(string connectionString)
        {
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

            var database = new ThrowawayDatabase(connectionString);

            if (!database.CreateDatabaseIfDoesNotExist())
            {
                throw new Exception("Could not create the throwaway database");
            }

            return database;
        }
        private bool CreateDatabaseIfDoesNotExist()
        {
            try
            {
                var databaseName = "";
                var builder = new NpgsqlConnectionStringBuilder(this.ConnectionString);
                var creationResult = false;
                if (builder.TryGetValue("Database", out var database))
                {
                    databaseName = database.ToString();
                    var connectionStringOfMaster = this.ConnectionString.Replace(databaseName, "postgres");
                    using (var otherConnection = new NpgsqlConnection(connectionStringOfMaster))
                    {
                        otherConnection.Open();
                        using (var createCmd = new NpgsqlCommand($"CREATE DATABASE {databaseName}", otherConnection))
                        {
                            var result = createCmd.ExecuteNonQuery();
                            Debug.Print($"Succesfully created database {databaseName}");
                            this.databaseCreated = true;
                            creationResult = true;
                        }
                    }

                    return creationResult;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
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
                        var result = cmd.ExecuteScalar();
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
