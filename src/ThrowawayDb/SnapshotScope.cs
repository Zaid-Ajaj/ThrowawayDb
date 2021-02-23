using System;

namespace ThrowawayDb
{
    public class SnapshotScope : IDisposable
    {
        private ThrowawayDatabase _db;

        public SnapshotScope(ThrowawayDatabase db)
        {
            _db = db;
            _db.CreateSnapshot();
        }

        public void Dispose()
        {
            _db?.RestoreSnapshot();
            _db = null;
        }
    }
}
