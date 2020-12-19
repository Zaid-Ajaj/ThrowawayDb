using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace ThrowawayDb
{
    public class ThrowawayDatabase : IDisposable
    {
        private const string DefaultDatabaseNamePrefix = "ThrowawayDb";

        private readonly string _originalConnectionString;
        private bool _databaseCreated;
        private string _snapshotName;

        /// <summary>Returns the connection string of the database that was created</summary>
        public string ConnectionString { get; }
        /// <summary>Returns the name of the database that was created</summary>
        public string Name { get; }

        private ThrowawayDatabase(string originalConnectionString, ThrowawayDatabaseOptions options)
        {
            // Default constructor is private
            _originalConnectionString = originalConnectionString;
            (ConnectionString, Name) = DeriveThrowawayConnectionString(originalConnectionString, options);
        }

        private bool IsSnapshotCreated() =>
            _databaseCreated && !string.IsNullOrEmpty(_snapshotName);

        private void DropDatabaseIfCreated()
        {
            if (!_databaseCreated)
                return;

            using (var connection = new SqlConnection(_originalConnectionString))
            {
                connection.Open();
                connection.TerminateDatabaseConnections(Name);

                if (IsSnapshotCreated())
                {
                    using (var cmd = new SqlCommand($"DROP DATABASE [{_snapshotName}]", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                using (var cmd = new SqlCommand($"DROP DATABASE {Name}", connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool CreateDatabaseIfDoesNotExist(ThrowawayDatabaseOptions options)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(ConnectionString);
                if (!builder.TryGetValue("Initial Catalog", out var database))
                    return false;

                var databaseName = database.ToString();
                var connectionStringOfMaster = ConnectionString.Replace(databaseName, "master");
                using (var connection = new SqlConnection(connectionStringOfMaster))
                {
                    connection.Open();
                    var cmdText = "SELECT NAME from sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
                    using (var cmd = new SqlCommand(cmdText, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var databaseExists = false;
                            while (reader.Read())
                            {
                                if (reader.GetString(0) == databaseName)
                                {
                                    databaseExists = true;
                                    _databaseCreated = true;
                                    break;
                                }
                            }

                            if (!databaseExists)
                            {
                                using (var otherConnection = new SqlConnection(connectionStringOfMaster))
                                {
                                    otherConnection.Open();

                                    cmdText = $"CREATE DATABASE {databaseName}";
                                    if (!string.IsNullOrWhiteSpace(options.Collation))
	                                    cmdText += $" COLLATE {options.Collation}";
                                    
                                    using (var createCmd = new SqlCommand(cmdText, otherConnection))
                                    {
                                        createCmd.ExecuteNonQuery();
                                        Debug.Print($"Successfully created database {databaseName}");
                                        _databaseCreated = true;
                                    }
                                }
                            }
                            else
                            {
                                _databaseCreated = true;
                            }
                        }
                    }
                }

                return true;

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

        private static (string connectionString, string databaseName) DeriveThrowawayConnectionString(string originalConnectionString, ThrowawayDatabaseOptions options)
        {
            var builder = new SqlConnectionStringBuilder(originalConnectionString);
            var databasePrefix = string.IsNullOrWhiteSpace(options.DatabaseNamePrefix) ? DefaultDatabaseNamePrefix : options.DatabaseNamePrefix;

            var databaseName = $"{databasePrefix}{Guid.NewGuid().ToString("n").Substring(0, 10)}";

            if (builder.TryGetValue("Initial Catalog", out _))
            {
                builder.Remove("Initial Catalog");
            }

            builder.InitialCatalog = databaseName;
            return (builder.ConnectionString, databaseName);
        }

        /// <summary>
        /// Uses the given instance as the Data Source of the connection string along with integration security assuming that the current user has direct access to his or her Sql server instance.
        /// </summary>
        public static ThrowawayDatabase FromLocalInstance(string instance, string databaseNamePrefix = null) =>
	        FromLocalInstance(instance, new ThrowawayDatabaseOptions
	        {
		        DatabaseNamePrefix = databaseNamePrefix
	        });

        /// <summary>
        /// Uses the given instance as the Data Source of the connection string along with integration security assuming that the current user has direct access to his or her Sql server instance.
        /// </summary>
        public static ThrowawayDatabase FromLocalInstance(string instance, ThrowawayDatabaseOptions options)
        {
            var connectionString = $"Data Source={instance};Initial Catalog=master;Integrated Security=True;";
	        
            if (!TryPingDatabase(connectionString))
            {
                throw new Exception("Could not connect to the database");
            }

	        var database = new ThrowawayDatabase(connectionString, options);
	        if (!database.CreateDatabaseIfDoesNotExist(options))
	        {
		        throw new Exception("Could not create the throwaway database");
	        }

	        return database;
        }

        /// <summary>
        /// Creates a database through SQL server authentication using the given username, password and the datasource/instance.
        /// </summary>
        public static ThrowawayDatabase Create(string username, string password, string dataSource, string databaseNamePrefix = null) =>
	        Create(username, password, dataSource, new ThrowawayDatabaseOptions
	        {
		        DatabaseNamePrefix = databaseNamePrefix
	        });

        /// <summary>
        /// Creates a database through SQL server authentication using the given username, password and the datasource/instance.
        /// </summary>
        public static ThrowawayDatabase Create(string username, string password, string dataSource, ThrowawayDatabaseOptions options)
        {
	        var connectionString = $"Password={password};Persist Security Info=True;User ID={username};Initial Catalog=master;Data Source={dataSource}";
	        return Create(connectionString, options);
        }

        /// <summary>
        /// Creates a throwaway database using the connection string provided. No need to set the Initial Catalog as it will get replaced by the name of the database that will be created.
        /// </summary>
        public static ThrowawayDatabase Create(string connectionString, string databaseNamePrefix = null) =>
	        Create(connectionString, new ThrowawayDatabaseOptions
	        {
		        DatabaseNamePrefix = databaseNamePrefix
	        });

        /// <summary>
        /// Creates a throwaway database using the connection string provided. No need to set the Initial Catalog as it will get replaced by the name of the database that will be created.
        /// </summary>
        public static ThrowawayDatabase Create(string connectionString, ThrowawayDatabaseOptions options)
        {
	        if (!TryPingDatabase(connectionString))
	        {
		        throw new Exception("Could not connect to the database");
	        }

	        var database = new ThrowawayDatabase(connectionString ?? string.Empty, options);
	        if (!database.CreateDatabaseIfDoesNotExist(options))
	        {
		        throw new Exception("Could not create the throwaway database");
	        }

	        return database;
        }

        /// <summary>
        /// Creates a new snapshot of the <see cref="ThrowawayDatabase"/> in case there is no snapshot created.<br/>
        /// Does nothing if a snapshot has already been created.
        /// </summary>
        public void CreateSnapshot()
        {
            if (IsSnapshotCreated())
                return;

            var snapshotName = $"{Name}_ss";

            using (var connection = new SqlConnection(_originalConnectionString))
            {
                connection.Open();

                string fileName, cmdText = "SELECT TOP 1 physical_name FROM sys.master_files WHERE name = 'master'";
                using (var cmd = new SqlCommand(cmdText, connection))
                {
                    var physicalName = (string)cmd.ExecuteScalar() ?? string.Empty;
                    fileName = Path.GetDirectoryName(physicalName);
                }

                if (string.IsNullOrEmpty(fileName))
                    return;

                fileName = Path.Combine(fileName, $"{snapshotName}.ss");

                // TODO: apply collation here?
                cmdText = $"CREATE DATABASE [{snapshotName}] ON ( NAME = [{Name}], FILENAME = [{fileName}] ) AS SNAPSHOT OF [{Name}]";
                using (var cmd = new SqlCommand(cmdText, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                _snapshotName = snapshotName;
            }
        }

        /// <summary>
        /// Restores a snapshot of the <see cref="ThrowawayDatabase"/> in case the snapshot has been previously created.<br/>
        /// Does nothing if a snapshot has not been created.
        /// </summary>
        public void RestoreSnapshot()
        {
            if (!IsSnapshotCreated())
                return;

            using (var connection = new SqlConnection(_originalConnectionString))
            {
                connection.Open();
                connection.TerminateDatabaseConnections(Name);

                var cmdText = $"RESTORE DATABASE [{Name}] FROM DATABASE_SNAPSHOT = @{nameof(_snapshotName)}";
                using (var cmd = new SqlCommand(cmdText, connection))
                {
                    cmd.Parameters.AddWithValue(nameof(_snapshotName), _snapshotName);
                    cmd.ExecuteNonQuery();
                }

                cmdText = $"ALTER DATABASE [{Name}] SET MULTI_USER";
                using (var cmd = new SqlCommand(cmdText, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Dispose() => DropDatabaseIfCreated();
    }
}
