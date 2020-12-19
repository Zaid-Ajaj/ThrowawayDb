using System;
using System.Data.SqlClient;
using ThrowawayDb;

namespace Tests
{
	public abstract class TestsBase
	{
		protected const string LocalInstanceName = "localhost\\SQLEXPRESS",
			GlobalConnectionString = "Data Source=" + LocalInstanceName + ";Initial Catalog=master;Integrated Security=True;";

		protected static bool DatabaseExists(string name)
		{
			using var connection = new SqlConnection(GlobalConnectionString);
			connection.Open();

			using var cmd = new SqlCommand($"SELECT CASE WHEN DB_ID(@{nameof(name)}) IS NULL THEN 0 ELSE 1 END", connection);
			cmd.Parameters.AddWithValue(nameof(name), name);

			var result = cmd.ExecuteScalar();
			return Convert.ToInt32(result) == 1;
		}

		protected static bool CheckCommandExecution(ThrowawayDatabase fixture)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new SqlCommand("SELECT 1", connection);
			return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
		}

		public static string GetCollation(ThrowawayDatabase fixture)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new SqlCommand("SELECT collation_name FROM sys.databases WHERE name = @name", connection);
			cmd.Parameters.AddWithValue("name", fixture.Name);

			return Convert.ToString(cmd.ExecuteScalar());
		}
	}
}
