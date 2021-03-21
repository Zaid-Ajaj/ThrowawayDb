using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public static class MySqlConnectionExtensions
	{
		public static void ExecuteNonQuery(this MySqlConnection @this, string cmdText)
		{
			using var command = new MySqlCommand(cmdText, @this);
			command.ExecuteNonQuery();
		}
		
		public static MySqlDataReader ExecuteReader(this MySqlConnection @this, string cmdText)
		{
			using var command = new MySqlCommand(cmdText, @this);
			return command.ExecuteReader();
		}
	}
}
