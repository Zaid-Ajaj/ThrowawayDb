# ThrowawayDb
Dead simple integration tests for Sql server using throwaway databases.

```csharp
public static void Main(string[] args)
{
    using (var database = ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS"))
    {
        Console.WriteLine($"Created database {database.Name}");
        
        // - Apply database migrations here if necessary
        // - Seed the database with data
        // - Execute your code against this database

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
```
In the snippet above, a dummy database with a random name is created using the current local instance of SQL Express on my machine. Moreover, the object `database` is an `IDisposable` and will drop the database at the end of the `using` clause when it's `Dispose()` is executed. It is that simple!

You can create the throwaway database in multiple ways:
```cs
// from Sql Express server locally with Integrated security
ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS") 

// Using SQL Authentication with user credentials and an arbibrary host
ThrowawayDatabase.Create(username: "Zaid", password: "strongPassword", host: "192.168.1.100")

// Using a connection string where Initial Catalog is master
ThrowawayDatabase.Create(connectionString: "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;")
```

