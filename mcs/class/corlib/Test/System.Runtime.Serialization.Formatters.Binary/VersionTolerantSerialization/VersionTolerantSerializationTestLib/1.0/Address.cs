using System;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v1-street";
		private string City = "v1-city";
		private string Country = "v1-country";

		public override string ToString () {
			return String.Format ("v1 obj {0} {1} {2}", Street, City, Country);
		}
	}
}
