using System;
using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql.Tests
{
	public abstract class ThrowawayDatabaseTestsBase
	{
		protected const string LocalInstanceName = "localhost", UserName = "Zaid", Password = "strongPassword";

		protected static bool CheckCommandExecution(ThrowawayDatabase fixture)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new MySqlCommand("SELECT 1", connection);
			return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
		}

		public static string GetCollation(ThrowawayDatabase fixture, string name = null)
		{
			using var connection = fixture.OpenConnection();

			using var cmd = new MySqlCommand("SELECT default_collation_name FROM information_schema.schemata WHERE schema_name = @name LIMIT 1", connection);
			cmd.Parameters.AddWithValue("name", name ?? fixture.Name);

			return Convert.ToString(cmd.ExecuteScalar());
		}
	}
}
