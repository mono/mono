using System;
using System.Runtime.Serialization;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v5-Street";
		private string City = "v5-City";
		private string CountryCode = "v5-CountryCode";

		[OptionalField (VersionAdded = 4)]
		private string PostCode;

		[OptionalField (VersionAdded = 5)]
		private string AreaCode = "5";

		public override string ToString () {
			return String.Format ("v5 obj {0} {1} {2}", Street, City, CountryCode);
		}

	}
}
