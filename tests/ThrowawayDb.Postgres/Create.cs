using System;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace ThrowawayDb.Postgres.Tests
{
	public sealed class Create : ThrowawayDatabaseTestsBase
	{
		[Fact(DisplayName = "Create a new database")]
		public void CreateNewDatabase()
		{
			using var database = ThrowawayDatabase.Create("postgres", "postgres", "localhost");
			using var connection = new NpgsqlConnection(database.ConnectionString);
			connection.Open();

			using var cmd = new NpgsqlCommand("SELECT 1", connection);
			var result = Convert.ToInt32(cmd.ExecuteScalar());

			result
				.Should()
				.Be(1);
		}
	}
}
