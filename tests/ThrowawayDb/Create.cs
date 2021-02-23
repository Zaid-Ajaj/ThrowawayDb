using Microsoft.Data.SqlClient;
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
            const string collation = "Japanese_CI_AS";

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
                collation = "Japanese_CI_AS";

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
            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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

            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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
            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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

            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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
            const string collation = "Japanese_CI_AS";

            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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
                collation = "Japanese_CI_AS";

            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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

            var connectionString = new SqlConnectionStringBuilder
            {
                PersistSecurityInfo = true,
                DataSource = LocalInstanceName,
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
