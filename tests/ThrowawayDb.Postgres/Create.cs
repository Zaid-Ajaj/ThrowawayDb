using System;
using System.Threading.Tasks;
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

    public sealed class CreateSchema : ThrowawayDatabaseTestsBase
    {
        [Fact(DisplayName = "Create a new schema")]
        public async Task CreateNewSchemaAsync()
        {
            using var database = ThrowawayDatabase.Create("postgres", "postgres", "localhost");
            using var schema = await ThrowawaySchema.TryCreateAsync(database);
            using var connection = new NpgsqlConnection(schema.ConnectionString);

            connection.Open();

            using var cmd = new NpgsqlBatch(connection)
            {
                BatchCommands =
                {
                    new("CREATE TABLE test(id INT)"),
                    new("INSERT INTO test(id) values(1);"),
                    new("SELECT id FROM test;")
                }
            };

            try
            {
                var reader = cmd.ExecuteReader();
                if (await reader.ReadAsync())
                {
                    var result = Convert.ToInt32(reader.GetInt32(0));
                    result
                        .Should()
                        .Be(1);

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
