namespace ThrowawayDb.MySql
{
	public sealed class ThrowawayDatabaseOptions
	{
		public string DatabaseNamePrefix { get; set; } = string.Empty;

		public string CharacterSet { get; set; } = string.Empty;

		public string Collation { get; set; } = string.Empty;
	}
}
