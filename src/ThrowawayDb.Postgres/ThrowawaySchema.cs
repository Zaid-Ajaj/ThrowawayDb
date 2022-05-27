using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Npgsql;

namespace ThrowawayDb.Postgres
{
    public class ThrowawaySchema : IDisposable
    {
        /// <summary>Returns the connection string of the database that was created</summary>
        public string ConnectionString { get; internal set; }
        /// <summary>Returns the name of the database that was created</summary>
        public string Name { get; internal set; }

        private bool schemaCreated;

        private readonly string databaseName;
        private readonly string originalConnectionString;

        private ThrowawaySchema(ThrowawayDatabase database)
        {
            originalConnectionString = database.ConnectionString;
            databaseName = database.Name;

            var (derivedConnectionString, schemaName) = DeriveThrowawayConnectionString(originalConnectionString);

            ConnectionString = derivedConnectionString;
            Name = schemaName;
        }

        private void DropSchemaIfCreated()
        {
            if (!schemaCreated) return;

            // Revoke future connections
            using var connection = new NpgsqlConnection(originalConnectionString);
            connection.Open();

            using var commands = new NpgsqlCommand($@"
                REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA {Name} FROM PUBLIC;
                DROP SCHEMA {Name} CASCADE;", connection);

            commands.ExecuteNonQuery();
        }

        private static (string connectionString, string schemaName) DeriveThrowawayConnectionString(string originalConnectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(originalConnectionString);
            var schemaName = $"schema_{Guid.NewGuid().ToString("n").Substring(0, 10).ToLowerInvariant()}";

            builder.SearchPath = schemaName;

            return (builder.ConnectionString, schemaName);
        }

        public static async Task<ThrowawaySchema?> TryCreateAsync(ThrowawayDatabase database)
        {
            try
            {
                if (!TryPingDatabase(database.ConnectionString))
                    throw new NpgsqlException("Could not connect to the database");

                ThrowawaySchema schema = new(database);
                await schema.CreateSchemaIfNotExistsAsync();

                return schema;
            }
            catch (Exception ex)
            {
                Debug.Print("Could not create schema: {0}", ex.Message);
                return null;
            }
        }

        private async Task CreateSchemaIfNotExistsAsync()
        {
            using var connection = new NpgsqlConnection(originalConnectionString);
            await connection.OpenAsync();

            using var createCmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {Name};", connection);
            var result = createCmd.ExecuteNonQuery();

            Debug.Print($"Successfully created schema {Name}");
            schemaCreated = true;
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

        public void Dispose() => DropSchemaIfCreated();
    }
}
