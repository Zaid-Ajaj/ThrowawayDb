using FluentAssertions;
using ThrowawayDb;
using Xunit;

namespace Tests
{
	public sealed class Create : TestsBase
	{
		private const string UserName = "sa",
			Password = "password";

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
			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions());

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

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions
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
			const string collation = "Latin1_General_CI_AS";

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions
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

		[Fact(DisplayName = "Create a new database with a username, password and options (Prefix, Collation)")]
		public void CreateDatabaseWithUserNamePasswordAndPrefixCollationOptions()
		{
			const string prefix = nameof(prefix),
				collation = "Latin1_General_CI_AS";

			using var fixture = ThrowawayDatabase.Create(UserName, Password, LocalInstanceName, new ThrowawayDatabaseOptions
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
	}
}
