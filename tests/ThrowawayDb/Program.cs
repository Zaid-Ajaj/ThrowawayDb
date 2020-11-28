using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FluentAssertions;
using ThrowawayDb;

namespace tests
{
	internal class Program
	{
		private const string LocalInstanceName = "localhost\\SQLEXPRESS",
			GlobalConnectionString = "Data Source=" + LocalInstanceName + ";Initial Catalog=master;Integrated Security=True;";

		private static void Main(string[] args)
		{
			var arguments = string.Join(", ", args);
			Console.WriteLine($"Arguments [{arguments}]");

			var testCases = new Action[]
			{
				CreateDatabase,
				CreateAndRestoreSnapshot,
				CreateSnapshotOnlyOnce,
				NotRestoreSnapshotIfNotCreated
			};

			for (var i = 0; i < testCases.Length; i++)
			{
				Console.WriteLine("Running test #{0}: {1}", i + 1, testCases[i].Method.Name);
				testCases[i]();

				Console.WriteLine();
			}
		}

		private static ThrowawayDatabase CreateFixture() =>
			ThrowawayDatabase.FromLocalInstance(LocalInstanceName);

		private static string FormatItems<T>(IEnumerable<T> items) =>
			new StringBuilder()
				.Append("[")
				.AppendJoin(',', items)
				.Append("]")
				.ToString();

		private static void CreateDatabase()
		{
			string databaseName;

			using (var fixture = CreateFixture())
			{
				databaseName = fixture.Name;
				Console.WriteLine($"Created database {fixture.Name}");

				DatabaseExists(databaseName)
					.Should()
					.BeTrue();

				// Apply migrations / seed data if necessary
				// execute code against this newly generated database
				using var connection = fixture.OpenConnection();

				using var cmd = new SqlCommand("SELECT 1", connection);
				var result = Convert.ToInt32(cmd.ExecuteScalar());
				Console.WriteLine(result); // prints 1

				result
					.Should()
					.Be(1);
			}

			DatabaseExists(databaseName)
				.Should()
				.BeFalse();

			static bool DatabaseExists(string name)
			{
				using var connection = new SqlConnection(GlobalConnectionString);
				connection.Open();

				using var cmd = new SqlCommand($"SELECT CASE WHEN DB_ID(@{nameof(name)}) IS NULL THEN 0 ELSE 1 END", connection);
				cmd.Parameters.AddWithValue(nameof(name), name);

				var result = cmd.ExecuteScalar();
				return Convert.ToInt32(result) == 1;
			}
		}

		private static void CreateAndRestoreSnapshot()
		{
			const string tblTest = nameof(tblTest);

			using var fixture = CreateFixture();
			var connection = fixture.OpenConnection();

			using (var cmd = new SqlCommand($"CREATE TABLE {tblTest} (test int)", connection))
				cmd.ExecuteNonQuery();

			var items = GetItems(fixture).ToArray();
			Console.WriteLine("Items before snapshot: {0}", FormatItems(items));
			items.Should().BeEmpty();

			// Create a snapshot and populate the table
			fixture.CreateSnapshot();

			using (var cmd = new SqlCommand($"INSERT {tblTest} VALUES (@i)", connection))
			{
				cmd.Parameters.Add("i", SqlDbType.Int);

				foreach (var i in Enumerable.Range(1, 3))
				{
					cmd.Parameters[0].Value = i;
					cmd.ExecuteNonQuery();
				}
			}

			items = GetItems(fixture).ToArray();
			Console.WriteLine("Items before restore: {0}", FormatItems(items));
			items.Should().BeEquivalentTo(new[] { 1, 2, 3 });

			// Restore the snapshot
			fixture.RestoreSnapshot();

			items = GetItems(fixture).ToArray();
			Console.WriteLine("Items after restore: {0}", FormatItems(items));
			items.Should().BeEmpty();

			static IEnumerable<int> GetItems(ThrowawayDatabase fixture)
			{
				using var connection = fixture.OpenConnection();

				using var cmd = new SqlCommand($"SELECT * FROM {tblTest}", connection);
				using var reader = cmd.ExecuteReader();

				while (reader.Read() && reader.HasRows)
					yield return reader.GetInt32(0);
			}
		}

		private static void CreateSnapshotOnlyOnce()
		{
			using var fixture = CreateFixture();

			foreach (var i in Enumerable.Range(1, 3))
			{
				Console.WriteLine("Creating a snapshot: {0}", i);
				fixture.CreateSnapshot();
			}

			Console.WriteLine("Restoring the snapshot");
			fixture.RestoreSnapshot();
		}

		private static void NotRestoreSnapshotIfNotCreated()
		{
			using var fixture = CreateFixture();

			Console.WriteLine("Restoring a snapshot which does not exist");
			fixture.RestoreSnapshot();
		}
	}
}
