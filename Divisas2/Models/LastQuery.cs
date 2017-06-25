namespace Divisas2.Models
{
	using SQLite.Net.Attributes;

	public class LastQuery
    {
		[PrimaryKey]
		public int LastQueryId { get; set; }

		public string CodeRateSource { get; set; }

		public string CodeRateTarget { get; set; }

        public override int GetHashCode()
        {
            return LastQueryId;  
        }
	}
}
