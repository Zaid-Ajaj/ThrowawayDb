using FluentAssertions;
using ThrowawayDb;
using Xunit;

namespace Tests
{
	public sealed class FromLocalInstance : TestsBase
	{
		[Fact(DisplayName = "Create a new database with a default prefix")]
		public void CreateDatabase()
		{
			string databaseName;

			using (var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName))
			{
				databaseName = fixture.Name;

				DatabaseExists(databaseName)
					.Should()
					.BeTrue();

				CheckCommandExecution(fixture)
					.Should()
					.BeTrue();
			}

			DatabaseExists(databaseName)
				.Should()
				.BeFalse();
		}

		[Fact(DisplayName = "Create a new database with a custom prefix")]
		public void CreateDatabaseWithPrefix()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName);

			fixture.Name
				.Should()
				.StartWith(prefix);

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with default options")]
		public void CreateDatabaseWithDefaultOptions()
		{
			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions());

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			CheckCommandExecution(fixture)
				.Should()
				.BeTrue();
		}

		[Fact(DisplayName = "Create a new database with options (Prefix)")]
		public void CreateDatabaseWithPrefixOptions()
		{
			const string prefix = nameof(prefix);

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
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

		[Fact(DisplayName = "Create a new database with options (Collation)")]
		public void CreateDatabaseWithCollationOptions()
		{
			const string collation = "Japanese_90_CI_AS_KI";

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
			{
				Collation = collation
			});

			fixture.Name
				.Should()
				.StartWith("ThrowawayDb");

			GetCollation(fixture)
				.Should()
				.Be(collation);
		}

		[Fact(DisplayName = "Create a new database with options (Prefix, Collation)")]
		public void CreateDatabaseWithPrefixCollationOptions()
		{
			const string prefix = nameof(prefix),
				collation = "Japanese_90_CI_AS_KI";

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName, new ThrowawayDatabaseOptions
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
		}
	}
}
