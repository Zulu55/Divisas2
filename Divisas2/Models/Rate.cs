namespace Divisas2.Models
{
    using SQLite.Net.Attributes;

	public class Rate
	{
		[PrimaryKey, AutoIncrement]
		public int RateId { get; set; }

		public string Code { get; set; }

		public double TaxRate { get; set; }

		public string Name { get; set; }

		public string FullName
		{
			get { return string.Format("({0}) {1}", Code, Name); }
		}

		public string CodeRate
		{
			get { return string.Format("{0}{1}", Code, TaxRate); }
		}

		public override int GetHashCode()
		{
			return RateId;
		}
	}
}