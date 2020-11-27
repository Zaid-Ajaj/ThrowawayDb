using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ThrowawayDb
{
	public static class ThrowawayDatabaseExtensions
	{
		public static SqlConnection OpenConnection(this ThrowawayDatabase @this)
		{
			var connection = new SqlConnection(@this.ConnectionString);
			connection.Open();

			return connection;
		}

		public static async Task<SqlConnection> OpenConnectionAsync(this ThrowawayDatabase @this, CancellationToken cancellationToken = default)
		{
			var connection = new SqlConnection(@this.ConnectionString);

			await connection.OpenAsync(cancellationToken)
				.ConfigureAwait(false);

			return connection;
		}
	}
}