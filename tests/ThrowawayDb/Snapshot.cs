using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using ThrowawayDb;
using Xunit;

namespace Tests
{
	public sealed class Snapshot : TestsBase
	{
		[Fact(DisplayName = "Create a snapshot and restore it")]
		public void CreateAndRestoreSnapshot()
		{
			const string tblTest = nameof(tblTest);

			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName);
			var connection = fixture.OpenConnection();

			using (var cmd = new SqlCommand($"CREATE TABLE {tblTest} (test int)", connection))
				cmd.ExecuteNonQuery();

			var items = GetItems(fixture).ToArray();
			items.Should().BeEmpty();

			// Create a snapshot and populate the table
			fixture.CreateSnapshot();

			using (var cmd = new SqlCommand($"INSERT {tblTest} VALUES (@i)", connection))
			{
				cmd.Parameters.Add("i", SqlDbType.Int);

				foreach (var i in Enumerable.Range(1, 3))
				{
					cmd.Parameters[0].Value = i;
					cmd.ExecuteNonQuery();
				}
			}

			items = GetItems(fixture).ToArray();
			items.Should().BeEquivalentTo(new[] { 1, 2, 3 });

			// Restore the snapshot
			fixture.RestoreSnapshot();

			items = GetItems(fixture).ToArray();
			items.Should().BeEmpty();

			static IEnumerable<int> GetItems(ThrowawayDatabase fixture)
			{
				using var connection = fixture.OpenConnection();

				using var cmd = new SqlCommand($"SELECT * FROM {tblTest}", connection);
				using var reader = cmd.ExecuteReader();

				while (reader.Read() && reader.HasRows)
					yield return reader.GetInt32(0);
			}
		}

		[Fact(DisplayName = "Create a snapshot only once")]
		public void CreateSnapshotOnlyOnce()
		{
			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName);

			foreach (var _ in Enumerable.Range(1, 3))
				fixture.CreateSnapshot();

			fixture.RestoreSnapshot();
		}

		[Fact(DisplayName = "Not restore a snapshot if it has not been created")]
		public void NotRestoreSnapshotIfNotCreated()
		{
			using var fixture = ThrowawayDatabase.FromLocalInstance(LocalInstanceName);
			fixture.RestoreSnapshot();
		}
	}
}
