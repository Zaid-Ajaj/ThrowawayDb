using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ThrowawayDb
{
    public class ThrowawayDatabase : IDisposable
    {
        /// <summary>Returns the connection string of the database that was created</summary>
        public string ConnectionString { get; internal set; }
        /// <summary>Returns the name of the database that was created</summary>
        public string Name { get; internal set; }
        private bool databaseCreated;
        private string originalConnectionString;
        private string defaultDatabaseNamePrefix = "ThrowawayDb";

        private ThrowawayDatabase(string originalConnectionString, string databaseNamePrefix)
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
                using (var connection = new SqlConnection(this.originalConnectionString))
                {
                    connection.Open();

                    var resetActiveSessions = $"ALTER DATABASE {Name} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";

                    using (var cmd = new SqlCommand(resetActiveSessions, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand($"DROP DATABASE {Name}", connection))
                    {
                        var result = cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool CreateDatabaseIfDoesNotExist()
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(this.ConnectionString);
                if (builder.TryGetValue("Initial Catalog", out var database))
                {
                    var databaseName = database.ToString();
                    var connectionStringOfMaster = this.ConnectionString.Replace(databaseName, "master");
                    using (var connection = new SqlConnection(connectionStringOfMaster))
                    {
                        connection.Open();
                        var getAllDatabases = "SELECT NAME from sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
                        using (var cmd = new SqlCommand(getAllDatabases, connection))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                var databaseExists = false;
                                while (reader.Read())
                                {
                                    if (reader.GetString(0) == databaseName)
                                    {
                                        databaseExists = true;
                                        this.databaseCreated = true;
                                        break;
                                    }
                                }

                                if (!databaseExists)
                                {
                                    using (var otherConnection = new SqlConnection(connectionStringOfMaster))
                                    {
                                        otherConnection.Open();
                                        using (var createCmd = new SqlCommand($"CREATE DATABASE {databaseName}", otherConnection))
                                        {
                                            var result = createCmd.ExecuteNonQuery();
                                            Debug.Print($"Successfully created database {databaseName}");
                                            this.databaseCreated = true;
                                        }
                                    }
                                }
                                else
                                {
                                    this.databaseCreated = true;
                                }
                            }
                        }
                    }

                    return true;
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
                using (var connection = new SqlConnection(originalConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT GETDATE()", connection))
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

        private (string connectionString, string databaseName) DeriveThrowawayConnectionString(string originalConnectionString, string databaseNamePrefix)
        {
            var builder = new SqlConnectionStringBuilder(originalConnectionString);
            var databasePrefix = string.IsNullOrWhiteSpace(databaseNamePrefix) ? defaultDatabaseNamePrefix : databaseNamePrefix;

            var databaseName = $"{databasePrefix}{Guid.NewGuid().ToString("n").Substring(0, 10)}";

            if (builder.TryGetValue("Initial Catalog", out var initialDb))
            {
                builder.Remove("Initial Catalog");
            }

            builder.InitialCatalog = databaseName;
            return (builder.ConnectionString, databaseName);
        }

        /// <summary>
        /// Uses the given instance as the Data Source of the connection string along with integration security assuming that the current user has direct access to his or her Sql server instance.
        /// </summary>
        public static ThrowawayDatabase FromLocalInstance(string instance, string databaseNamePrefix = null)
        {
            var connectionString = $"Data Source={instance};Initial Catalog=master;Integrated Security=True;";

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

        /// <summary>
        /// Creates a database through SQL server authentication using the given username, password and the datasource/instance.
        /// </summary>
        public static ThrowawayDatabase Create(string username, string password, string datasource, string databaseNamePrefix = null)
        {
            var connectionString = $"Password={password};Persist Security Info=True;User ID={username};Initial Catalog=master;Data Source={datasource}";
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


        /// <summary>
        /// Creates a throwaway database using the connection string provided. No need to set the Initial Catalog as it will get replaced by the name of the database that will be created.
        /// </summary>
        public static ThrowawayDatabase Create(string connectionString, string databaseNamePrefix = null)
        {
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

            var database = new ThrowawayDatabase(connectionString ?? "", databaseNamePrefix);

            if (!database.CreateDatabaseIfDoesNotExist())
            {
                throw new Exception("Could not create the throwaway database");
            }

            return database;
        }

        public void Dispose() => DropDatabaseIfCreated();
    }
}
