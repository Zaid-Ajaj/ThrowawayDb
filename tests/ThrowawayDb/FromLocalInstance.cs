using System;
using System.Data.SqlClient;
using FluentAssertions;
using ThrowawayDb;
using Xunit;

namespace Tests
{
	public sealed class FromLocalInstance : TestsBase
	{
		[Fact(DisplayName = "Create a new database with a default prefix")]
		public void CreateDatabase()
		{
			string databaseName;

			using (var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName))
			{
				databaseName = fixture.Name;

				DatabaseExists(databaseName)
					.Should()
					.BeTrue();

				CheckCommandExecution(fixture)
					.Should()
					.BeTrue();
			}

			DatabaseExists(databaseName)
				.Should()
				.BeFalse();

			static bool DatabaseExists(string name)
			{
				using var connection = new SqlConnection("Data Source=" + LocalInstanceName + ";Initial Catalog=master;Integrated Security=True;");
				connection.Open();

				using var cmd = new SqlCommand($"SELECT CASE WHEN DB_ID(@{nameof(name)}) IS NULL THEN 0 ELSE 1 END", connection);
				cmd.Parameters.AddWithValue(nameof(name), name);

				var result = cmd.ExecuteScalar();
				return Convert.ToInt32(result) == 1;
			}
		}

		[Fact(DisplayName = "Create a new database with a custom prefix")]
		public void CreateDatabaseWithPrefix()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, prefix);

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with default options")]
		public void CreateDatabaseWithDefaultOptions()
		{
			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions());

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with options (Prefix)")]
		public void CreateDatabaseWithPrefixOptions()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with options (Collation)")]
		public void CreateDatabaseWithCollationOptions()
		{
			const string collation = "Japanese_CI_AS";

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
			{
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			GetCollation(fixture)
				.Should()
				.Be(collation);
		}

		[Fact(DisplayName = "Create a new database with options (Prefix, Collation)")]
		public void CreateDatabaseWithPrefixCollationOptions()
		{
			const string prefix = nameof(prefix),
				collation = "Japanese_CI_AS";

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix,
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			GetCollation(fixture)
				.Should()
				.Be(collation);
		}
	}
}
