using FluentAssertions;
using Npgsql;
using System.Threading.Tasks;
using Xunit;

namespace ThrowawayDb.Postgres.Tests
{
    public sealed class PureDatabaseTests
    {
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
            using var database = ThrowawayDatabase.Create("postgres", "postgres", "localhost");
            using var connection = new NpgsqlConnection(database.ConnectionString);
            
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
