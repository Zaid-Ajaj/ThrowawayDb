using System;
using System.IO;
using System.Text;
using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public class ThrowawayDatabase : IDisposable
	{
		private const string DefaultDatabaseNamePrefix = "ThrowawayDb";
		private string _snapshotPath = string.Empty;

		private ThrowawayDatabase(string connectionString, string databaseName)
		{
			Name = databaseName;
			ConnectionString = connectionString;
		}

		public string ConnectionString { get; }

		public string Name { get; }

		public static ThrowawayDatabase Create(string userName, string password, string server, string prefix) =>
			Create(userName, password, server, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix
			});

		public static ThrowawayDatabase Create(string connectionString, string prefix) =>
			Create(connectionString, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix
			});

		public static ThrowawayDatabase Create(string userName, string password, string server, ThrowawayDatabaseOptions? options = null)
		{
			var connectionString = new MySqlConnectionStringBuilder
			{
				Server = server,
				UserID = userName,
				Password = password
			}.ConnectionString;

			return Create(connectionString, options);
		}

		public static ThrowawayDatabase Create(string connectionString, ThrowawayDatabaseOptions? options = null)
		{
			if (!TryPingDatabase(connectionString))
				throw new Exception("Could not connect to the database");

			options ??= new ThrowawayDatabaseOptions();
			if (!TryCreateDatabase(connectionString, options, out connectionString, out var databaseName))
				throw new Exception("Could not create the throwaway database");

			return new ThrowawayDatabase(connectionString, databaseName);
		}

		/// <summary>
		/// Creates a new snapshot of the <see cref="ThrowawayDatabase"/> in case there is no snapshot created.<br/>
		/// Does nothing if a snapshot has already been created.
		/// </summary>
		public void CreateSnapshot()
		{
			if (IsSnapshotCreated())
				return;

			var snapshotPath = CreateSnapshotPath($"{Name}_ss.sql");

			using var connection = this.OpenConnection();
			connection.CreateBackup(snapshotPath);

			_snapshotPath = snapshotPath;
		}

		/// <summary>
		/// Restores a snapshot of the <see cref="ThrowawayDatabase"/> in case the snapshot has been previously created.<br/>
		/// Does nothing if a snapshot has not been created.
		/// </summary>
		public void RestoreSnapshot()
		{
			if (!IsSnapshotCreated())
				return;

			using var connection = this.OpenConnection();
			connection.RestoreBackup(_snapshotPath);
		}

		public void Dispose()
		{
			try
			{
				using var connection = this.OpenConnection();
				connection.ExecuteNonQuery($"DROP DATABASE {Name}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Cannot drop the database: {ex}");
			}

			if (IsSnapshotCreated())
				File.Delete(_snapshotPath);

			_snapshotPath = string.Empty;
		}

		private bool IsSnapshotCreated() =>
			!string.IsNullOrEmpty(_snapshotPath);

		private static string CreateSnapshotPath(string snapshotName)
		{
			var path = Path.GetDirectoryName(typeof(ThrowawayDatabase).Assembly.Location) ?? string.Empty;
			path = Path.Combine(path, "Snapshot");
			Directory.CreateDirectory(path);
			
			return Path.Combine(path, $"{snapshotName}.sql");
		}

		private static bool TryPingDatabase(string connectionString)
		{
			try
			{
				using var connection = new MySqlConnection(connectionString);
				connection.Open();

				connection.ExecuteNonQuery("SELECT 1;");
				return true;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error while pinging the MySQL server at '{connectionString}'");
				Console.WriteLine(ex.Message);
				Console.ForegroundColor = ConsoleColor.White;
				return false;
			}
		}

		private static bool TryCreateDatabase(string connectionString, ThrowawayDatabaseOptions options, out string databaseConnectionString, out string databaseName)
		{
			var prefix = string.IsNullOrEmpty(options.DatabaseNamePrefix) ? DefaultDatabaseNamePrefix : options.DatabaseNamePrefix;
			databaseName = $"{prefix}{Guid.NewGuid().ToString("n").Substring(0, 10)}";

			var builder = new MySqlConnectionStringBuilder(connectionString)
			{
				Database = databaseName
			};

			databaseConnectionString = builder.ConnectionString;
			builder.Remove("Database");

			try
			{
				using var connection = new MySqlConnection(builder.ConnectionString);
				connection.Open();

				using (var reader = connection.ExecuteReader("SHOW DATABASES;"))
				{
					while (reader.Read())
						if (reader.GetString(0) == databaseName)
							return true;
				}

				var cmdTextBuilder = new StringBuilder()
					.AppendFormat("CREATE DATABASE {0}", databaseName);

				if (!string.IsNullOrWhiteSpace(options.Collation))
					cmdTextBuilder.AppendFormat(" COLLATE {0}", options.Collation);

				connection.ExecuteNonQuery(cmdTextBuilder.Append(';').ToString());
				Console.WriteLine("Database created successfully");
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}
	}
}
