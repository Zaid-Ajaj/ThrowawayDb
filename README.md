# ThrowawayDb

Easily create a disposable database that integration tests dead simple for Sql server using throwaway databases.

### Available Packages

| Package              | Supports   | Version                                                                                                                               |
| -------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| ThrowawayDb          | SQL Server | [![Nuget](https://img.shields.io/nuget/v/ThrowawayDb.svg?colorB=green)](https://www.nuget.org/packages/ThrowawayDb)                   |
| ThrowawayDb.Postgres | Postgres   | [![Nuget](https://img.shields.io/nuget/v/ThrowawayDb.Postgres.svg?colorB=green)](https://www.nuget.org/packages/ThrowawayDb.Postgres) |
| ThrowawayDb.MySQL    | MySQL      | [![Nuget](https://img.shields.io/nuget/v/ThrowawayDb.MySql.svg?colorB=green)](https://www.nuget.org/packages/ThrowawayDb.MySql)       |

## Using SQL Server

Install `ThrowawayDb` from Nuget
```
dotnet add package ThrowawayDb
```
Use from your code
```csharp
public static void Main(string[] args)
{
    using (var database = ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS"))
    {
        Console.WriteLine($"Created database {database.Name}");

        // - Apply database migrations here if necessary
        // - Seed the database with data
        // - Execute your code against this database
        using var connection = new SqlConnection(database.ConnectionString);
        connection.Open();
        using var cmd = new SqlCommand("SELECT 1", connection);
        var result = Convert.ToInt32(cmd.ExecuteScalar());
        Console.WriteLine(result);
    }
}
```
In the snippet above, a dummy database with a random name is created using the current local instance of SQL Express on my machine. Moreover, the object `database` is an `IDisposable` and will drop the database at the end of the `using` clause when it's `Dispose()` is executed. It is that simple!

You can create the throwaway database in multiple ways:
```cs
// from Sql Express server locally with Integrated security
ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS");

// Uses the default instance locally where Data Source = .
ThrowawayDatabase.FromDefaultLocalInstance();

// Using SQL Authentication with user credentials and an arbibrary host
ThrowawayDatabase.Create(username: "Zaid", password: "strongPassword", host: "192.168.1.100");

// Using a connection string where Initial Catalog is master
ThrowawayDatabase.Create(connectionString: "Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;");

// Specify a custom database prefix, otherwise it just uses ThrowawayDb
ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS", "dbprefix_");
```

#### SQL server snapshot
You can create a snapshot of the database and restore it at a later point:
```cs
using var database = ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS");

// execute necessary actions before initialising a snapshot
database.CreateSnapshot();

// execute some other actions which modify the database
database.RestoreSnapshot();
```
A snapshot can also be created as a snapshot scope. This way you ensure that the RestoreSnapshot method is always called.
```cs
using var database = ThrowawayDatabase.FromLocalInstance("localhost\\SQLEXPRESS");

using(var scope = database.CreateSnapshotScope())
{
    // execute actions that modifies the database, and that should only last inside this scope.
}
```

## Using PostgreSQL server
Install `ThrowawayDb.Postgres` from Nuget
```
dotnet add package ThrowawayDb.Postgres
```
use from your code:
```cs
static void Main(string[] args)
{
    using (var database = ThrowawayDatabase.Create(username: "postgres", password: "postgres", host: "localhost"))
    {
        using var connection = new NpgsqlConnection(database.ConnectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand("SELECT 1", connection);
        var result = Convert.ToInt32(cmd.ExecuteScalar());
        Console.WriteLine(result);
    }
}
```

## Using MySQL server
You can create the throwaway database in multiple ways:
```cs
// Using SQL Authentication with user credentials and an arbibrary host
ThrowawayDatabase.Create(userName: "Zaid", password: "strongPassword", server: "localhost");

// Using a connection string where Initial Catalog is master
ThrowawayDatabase.Create(connectionString: "server=localhost;user id=Zaid;password=strongPassword;");

// Specify a custom database prefix, otherwise it just uses ThrowawayDb
ThrowawayDatabase.Create(userName: "Zaid", password: "strongPassword", server: "localhost", "dbprefix_");

// Specify all options
ThrowawayDatabase.Create("server=localhost;user id=Zaid;password=strongPassword;", new ThrowawayDatabaseOptions
{
    DatabaseNamePrefix = "dbprefix_",
    Collation = "ujis_japanese_ci"
});
```
#### MySQL snapshot
You can create a snapshot of the database and restore it at a later point:
```cs
using var database = ThrowawayDatabase.Create("Zaid", "strongPassword", "localhost");

// execute necessary actions before initialising a snapshot
database.CreateSnapshot();

// execute some other actions which modify the database
database.RestoreSnapshot();
```

### Running build targets for development
```
./build.sh --target {TargetName}
```
or
```
build --target {TargetName}
```
Where `{TargetName}` is the name of a target in build/buid.cs