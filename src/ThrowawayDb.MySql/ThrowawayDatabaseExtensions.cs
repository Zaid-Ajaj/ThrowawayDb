using MySql.Data.MySqlClient;

namespace ThrowawayDb.MySql
{
	public static class ThrowawayDatabaseExtensions
	{
		public static MySqlConnection OpenConnection(this ThrowawayDatabase @this)
		{
			var connection = new MySqlConnection(@this.ConnectionString);
			connection.Open();

			return connection;
		}
	}
}
