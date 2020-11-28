using System;
using System.Data.SqlClient;
using ThrowawayDb;

namespace tests
{
	internal class Program
    {
		private const string ConnectionString = "localhost\\SQLEXPRESS";

		private static void Main(string[] args)
        {
            var arguments = string.Join(", ", args);
            Console.WriteLine($"Arguments [{arguments}]");

            var testCases = new Action<ThrowawayDatabase>[]
            {
	            TestDatabaseCreation
            };

            using var fixture = ThrowawayDatabase.FromLocalInstance(ConnectionString);

            for (var i = 0; i < testCases.Length; i++)
            {
	            Console.WriteLine("Running test #{0}: {1}", i + 1, testCases[i].Method.Name);
	            testCases[i](fixture);
            }
        }

	    private static void TestDatabaseCreation(ThrowawayDatabase fixture)
	    {
		    Console.WriteLine($"Created database {fixture.Name}");
		    
		    // Apply migrations / seed data if necessary
		    // execute code against this newly generated database
		    using var connection = new SqlConnection(fixture.ConnectionString);
		    connection.Open();
		    
		    using var cmd = new SqlCommand("SELECT 1", connection);
		    var result = Convert.ToInt32(cmd.ExecuteScalar());
		    Console.WriteLine(result); // prints 1
	    }
    }
}
