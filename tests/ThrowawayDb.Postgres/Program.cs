using System;
using Npgsql;
using ThrowawayDb.Postgres;

namespace tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = String.Join(", ", args);
            Console.WriteLine($"Arguments [{arguments}]");

            using (var database = ThrowawayDatabase.Create("postgres", "postgres", "localhost"))
            using (var connection = new NpgsqlConnection(database.ConnectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT 1", connection))
                {
                    var result = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine(result);
                }
            }
        }
    }
}
