using System;

namespace ThrowawayDb
{
    public class SnapshotScope : IDisposable
    {
        public ThrowawayDatabase Db { get; private set; }

        public SnapshotScope(ThrowawayDatabase db)
        {
            Db = db;
            Db.CreateSnapshot();
        }

        public void Dispose()
        {
            Db?.RestoreSnapshot();
            Db = null;
        }
    }
}
