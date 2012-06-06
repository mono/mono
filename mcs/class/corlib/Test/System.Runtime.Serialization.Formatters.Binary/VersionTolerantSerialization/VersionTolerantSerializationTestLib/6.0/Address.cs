using System;
using System.Runtime.Serialization;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v6-Street";
		private string City = "v6-City";
		private string CountryCode = "v6-CountryCode";

		[OptionalField (VersionAdded = 4)]
		private string PostCode;

		[OptionalField (VersionAdded = 5)]
		private string AreaCode = "6";

		[OptionalField (VersionAdded = 6)]
		private int Id = 6;

		public override string ToString () {
			return String.Format ("v6 obj {0} {1} {2}", Street, City, CountryCode);
		}

	}
}
