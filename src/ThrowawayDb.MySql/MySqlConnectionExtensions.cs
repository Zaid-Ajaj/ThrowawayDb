using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public static class MySqlConnectionExtensions
	{
		public static void ExecuteNonQuery(this MySqlConnection @this, string cmdText)
		{
			using var cmd = new MySqlCommand(cmdText, @this);
			cmd.ExecuteNonQuery();
		}

		public static MySqlDataReader ExecuteReader(this MySqlConnection @this, string cmdText)
		{
			using var cmd = new MySqlCommand(cmdText, @this);
			return cmd.ExecuteReader();
		}

		internal static void CreateBackup(this MySqlConnection @this, string backupPath)
		{
			using var cmd = new MySqlCommand { Connection = @this };
			using var bkp = new MySqlBackup(cmd);

			bkp.ExportToFile(backupPath);
		}

		internal static void RestoreBackup(this MySqlConnection @this, string backupPath)
		{
			using var cmd = new MySqlCommand { Connection = @this };
			using var bkp = new MySqlBackup(cmd);
			
			bkp.ImportFromFile(backupPath);
		}
	}
}
