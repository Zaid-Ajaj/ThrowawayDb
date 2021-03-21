using System;
using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public class ThrowawayDatabase : IDisposable
	{
		private const string DefaultDatabaseNamePrefix = "ThrowawayDb";
		private readonly string _connectionString;
		private readonly string _databaseName;

		private ThrowawayDatabase(string connectionString, string databaseName)
		{
			_connectionString = connectionString;
			_databaseName = databaseName;
		}

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

		public void Dispose()
		{
			using var connection = new MySqlConnection(_connectionString);
			connection.ExecuteNonQuery($"DROP DATABASE {_databaseName}");
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
