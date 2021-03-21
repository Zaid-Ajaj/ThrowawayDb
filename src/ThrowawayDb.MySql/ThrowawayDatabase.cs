using System;
using System.IO;
using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public class ThrowawayDatabase : IDisposable
	{
		private const string DefaultDatabaseNamePrefix = "ThrowawayDb";
		private readonly string _databaseName;
		private string _snapshotPath = string.Empty;

		private ThrowawayDatabase(string connectionString, string databaseName)
		{
			_databaseName = databaseName;
			ConnectionString = connectionString;
		}

		internal string ConnectionString { get; }

		public static ThrowawayDatabase Create(string server, string userName, string password)
		{
			var connectionString = new MySqlConnectionStringBuilder
			{
				Server = server,
				UserID = userName,
				Password = password
			}.ConnectionString;

			return Create(connectionString);
		}

		public static ThrowawayDatabase Create(string connectionString)
		{
			if (!TryPingDatabase(connectionString))
				throw new Exception("Could not connect to the database");

			if (!TryCreateDatabase(connectionString, out connectionString, out var databaseName))
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

			var snapshotName = $"{_databaseName}_ss.sql";
			var snapshotPath = CreateSnapshotPath(snapshotName);

			using var connection = this.OpenConnection();
			connection.CreateBackup(snapshotPath);

			_snapshotPath = snapshotName;
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
				connection.ExecuteNonQuery($"DROP DATABASE {_databaseName}");
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

		private static string CreateSnapshotPath(string snapshotName) =>
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Snapshot", $"{snapshotName}.sql");

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

		private static bool TryCreateDatabase(string connectionString, out string databaseConnectionString, out string databaseName)
		{
			databaseName = $"{DefaultDatabaseNamePrefix}{Guid.NewGuid().ToString("n").Substring(0, 10)}";
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

				connection.ExecuteNonQuery($"CREATE DATABASE {databaseName};");
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
