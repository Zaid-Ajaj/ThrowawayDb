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
                Console.WriteLine($"Created database {database.Name}");
                // Apply migrations / seed data if necessary
                // execute code against this newly generated database
                using (var connection = new SqlConnection(database.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand("SELECT 1", connection))
                    {
                        var result = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine(result); // prints 1
                    }
                }
            }
        }
    }
}
