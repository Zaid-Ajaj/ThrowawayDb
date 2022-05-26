using FluentAssertions;
using Npgsql;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ThrowawayDb.Postgres.Tests
{
    public class DatabaseSetupFixture : IDisposable
    {
        public ThrowawayDatabase Database { get; private set; }

        public DatabaseSetupFixture()
        {
            Database = ThrowawayDatabase.Create("postgres", "postgres", "localhost");
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }

    public sealed class DatabaseWithSchemaTests : IClassFixture<DatabaseSetupFixture>
    {
        public readonly DatabaseSetupFixture fixture;

        public DatabaseWithSchemaTests(DatabaseSetupFixture fixture)
        {
            this.fixture = fixture;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        public async Task CreateAndInsert(int id)
        {
            using var schema = await ThrowawaySchema.TryCreateAsync(fixture.Database);
            using var connection = new NpgsqlConnection(schema.ConnectionString);
            
            await connection.OpenAsync();

            using var command = new NpgsqlBatch(connection)
            {
                BatchCommands =
                {
                    new ("CREATE TABLE test(id INT);"),
                    new ("INSERT INTO test VALUES ($1);") { Parameters = { new () {  Value = id } } },
                    new ("SELECT id FROM test;")
                }
            };

            var result = await command.ExecuteScalarAsync() as int?;
            
            result.Should().Be(id);
        }
    }
}
