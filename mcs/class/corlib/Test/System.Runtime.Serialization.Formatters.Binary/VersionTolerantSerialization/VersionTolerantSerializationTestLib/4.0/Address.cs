using System;
using System.Runtime.Serialization;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v4-Street";
		private string City = "v4-City";
		private string CountryCode = "v4-CountryCode";

		[OptionalField (VersionAdded = 4)]
		private string PostCode;

		public override string ToString () {
			return String.Format ("v4 obj {0} {1} {2}", Street, City, CountryCode);
		}

	}
}
