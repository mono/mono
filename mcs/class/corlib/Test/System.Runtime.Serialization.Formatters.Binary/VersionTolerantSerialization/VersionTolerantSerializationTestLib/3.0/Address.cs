using System;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v3-Street";
		private string City = "v3-City";
		private string CountryCode = "v3-CountryCode";

		public override string ToString () {
			return String.Format ("v3 obj {0} {1} {2}", Street, City, CountryCode);
		}
	}
}
