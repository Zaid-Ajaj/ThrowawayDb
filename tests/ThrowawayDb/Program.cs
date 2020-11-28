using System;
using System.Data.SqlClient;
using FluentAssertions;
using ThrowawayDb;

namespace tests
{
	internal class Program
	{
		private const string LocalInstanceName = "localhost\\SQLEXPRESS";

		private static ThrowawayDatabase CreateFixture() =>
			ThrowawayDatabase.FromLocalInstance(LocalInstanceName);

		private static void Main(string[] args)
		{
			var arguments = string.Join(", ", args);
			Console.WriteLine($"Arguments [{arguments}]");

			var testCases = new Action[]
			{
				CreateDatabase
			};

			for (var i = 0; i < testCases.Length; i++)
			{
				Console.WriteLine("Running test #{0}: {1}", i + 1, testCases[i].Method.Name);
				testCases[i]();

				Console.WriteLine();
			}
		}

		private static void CreateDatabase()
		{
			string databaseName;

			using (var fixture = CreateFixture())
			{
				databaseName = fixture.Name;
				Console.WriteLine($"Created database {fixture.Name}");

				CheckDatabaseExists(databaseName)
					.Should()
					.BeTrue();

				// Apply migrations / seed data if necessary
				// execute code against this newly generated database
				using var connection = new SqlConnection(fixture.ConnectionString);
				connection.Open();

				using var cmd = new SqlCommand("SELECT 1", connection);
				var result = Convert.ToInt32(cmd.ExecuteScalar());
				Console.WriteLine(result); // prints 1

				result
					.Should()
					.Be(1);
			}

			CheckDatabaseExists(databaseName)
				.Should()
				.BeFalse();

			static bool CheckDatabaseExists(string name)
			{
				using var connection = new SqlConnection($"Data Source={LocalInstanceName};Initial Catalog=master;Integrated Security=True;");
				connection.Open();

				using var cmd = new SqlCommand($"SELECT CASE WHEN DB_ID(@{nameof(name)}) IS NULL THEN 0 ELSE 1 END", connection);
				cmd.Parameters.AddWithValue(nameof(name), name);

				var result = cmd.ExecuteScalar();
				return Convert.ToInt32(result) == 1;
			}
		}
	}
}
