using FluentAssertions;
using MySql.Data.MySqlClient;
using Xunit;

namespace ThrowawayDb.MySql.Tests
{
	public sealed class Create : ThrowawayDatabaseTestsBase
	{
		[Fact(DisplayName = "Create a new database with a username and password")]
		public void CreateDatabaseWithUserNameAndPassword()
		{
			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName);

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and prefix")]
		public void CreateDatabaseWithUserNamePasswordAndPrefix()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, prefix);

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and default options")]
		public void CreateDatabaseWithUserNamePasswordAndOptions()
		{
			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName,  new ThrowawayDatabaseOptions());

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and options (Prefix)")]
		public void CreateDatabaseWithUserNamePasswordAndPrefixOptions()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName,  new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and options (Collation)")]
		public void CreateDatabaseWithUserNamePasswordAndCollationOptions()
		{
			const string collation = "utf8mb4_ja_0900_as_cs_ks";

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName,  new ThrowawayDatabaseOptions
			{
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			GetCollation(fixture)
				.Should()
				.Be(collation);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and options (Collation, Charset)")]
		public void CreateDatabaseWithUserNamePasswordAndCollationCharsetOptions()
		{
			const string collation = "ujis_japanese_ci";

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions
			{
				Collation = collation,
			});

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			GetCollation(fixture)
				.Should()
				.Be(collation);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a username, password and options (Prefix, Collation)")]
		public void CreateDatabaseWithUserNamePasswordAndPrefixCollationOptions()
		{
			const string prefix = nameof(prefix),
				collation = "utf8mb4_ja_0900_as_cs_ks";

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName,  new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix,
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			GetCollation(fixture)
				.Should()
				.Be(collation);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Theory(DisplayName = "Create a new database with a user name and password, but without a collation if it is null or a white space")]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void CreateDatabaseWithUserNamePasswordCollationNullOrWhiteSpace(string collation)
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix,
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string")]
		public void CreateDatabaseWithConnectionString()
		{
			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString);

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string and prefix")]
		public void CreateDatabaseWithConnectionStringPrefix()
		{
			const string prefix = nameof(prefix);

			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, prefix);

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string and default options")]
		public void CreateDatabaseWithConnectionStringAndOptions()
		{
			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, new ThrowawayDatabaseOptions());

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string and options (Prefix)")]
		public void CreateDatabaseWithConnectionStringAndPrefixOptions()
		{
			const string prefix = nameof(prefix);

			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string and options (Collation)")]
		public void CreateDatabaseWithConnectionStringAndCollationOptions()
		{
			const string collation = "utf8mb4_ja_0900_as_cs_ks";

			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, new ThrowawayDatabaseOptions
			{
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			GetCollation(fixture)
				.Should()
				.Be(collation);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with a connection string and options (Prefix, Collation)")]
		public void CreateDatabaseWithConnectionStringAndPrefixCollationOptions()
		{
			const string prefix = nameof(prefix),
				collation = "utf8mb4_ja_0900_as_cs_ks";

			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix,
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			GetCollation(fixture)
				.Should()
				.Be(collation);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Theory(DisplayName = "Create a new database with a connection string, but without a collation if it is null or a white space")]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public void CreateDatabaseWithConnectionStringWithoutCollationIfNullOrWhiteSpace(string collation)
		{
			const string prefix = nameof(prefix);

			var connectionString = new MySqlConnectionStringBuilder
			{
				PersistSecurityInfo = true,
				Server = LocalInstanceName,
				UserID = UserName,
				Password = Password
			}.ConnectionString;

			using var fixture = ThrowawayDatabase.Create(connectionString, new ThrowawayDatabaseOptions
			{
				DatabaseNamePrefix = prefix,
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}
	}
}
