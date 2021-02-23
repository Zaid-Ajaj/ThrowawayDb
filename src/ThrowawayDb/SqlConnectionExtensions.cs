using Microsoft.Data.SqlClient;

namespace ThrowawayDb
{
    internal static class SqlConnectionExtensions
    {
        internal static void TerminateDatabaseConnections(this SqlConnection @this, string databaseName)
        {
            var cmdText = $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
            using (var cmd = new SqlCommand(cmdText, @this))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
