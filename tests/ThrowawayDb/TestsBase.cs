using System;
using System.Data.SqlClient;
using ThrowawayDb;

namespace Tests
{
	public abstract class TestsBase
	{
		protected const string LocalInstanceName = "localhost\\SQLEXPRESS";

		protected static bool CheckCommandExecution(ThrowawayDatabase fixture)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new SqlCommand("SELECT 1", connection);
			return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
		}

		public static string GetCollation(ThrowawayDatabase fixture, string name = null)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new SqlCommand("SELECT collation_name FROM sys.databases WHERE name = @name", connection);
			cmd.Parameters.AddWithValue("name", name ?? fixture.Name);

			return Convert.ToString(cmd.ExecuteScalar());
		}
	}
}
