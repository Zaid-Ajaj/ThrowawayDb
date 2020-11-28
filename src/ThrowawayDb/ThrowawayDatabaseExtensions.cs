using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace ThrowawayDb
{
	public static class ThrowawayDatabaseExtensions
	{
		/// <summary>
		/// Opens an SQL connection with an instance of the created <see cref="ThrowawayDatabase"/>.
		/// </summary>
		/// <param name="this">An instance of <see cref="ThrowawayDatabase"/></param>
		/// <returns>A new instance of the SQL connection</returns>
		public static SqlConnection OpenConnection(this ThrowawayDatabase @this)
		{
			var connection = new SqlConnection(@this.ConnectionString);
			connection.Open();

			return connection;
		}

		/// <summary>
		/// Opens an SQL connection with an instance of the created <see cref="ThrowawayDatabase"/>.
		/// </summary>
		/// <param name="this">An instance of <see cref="ThrowawayDatabase"/></param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> for opening the SQL connection task</param>
		/// <returns>Task representing an asynchronous operation</returns>
		public static async Task<SqlConnection> OpenConnectionAsync(this ThrowawayDatabase @this, CancellationToken cancellationToken = default)
		{
			var connection = new SqlConnection(@this.ConnectionString);

			await connection.OpenAsync(cancellationToken)
				.ConfigureAwait(false);

			return connection;
		}
	}
}