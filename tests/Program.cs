using System;
using System.Data.SqlClient;
using ThrowawayDb;

namespace tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = String.Join(", ", args);
            Console.WriteLine($"Arguments [{arguments}]");

            using (var database = ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS"))
            {
                using (var connection = new SqlConnection(database.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT 1", connection))
                    {
                        var result = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine(result);
                    }
                }
            }
        }
    }
}
