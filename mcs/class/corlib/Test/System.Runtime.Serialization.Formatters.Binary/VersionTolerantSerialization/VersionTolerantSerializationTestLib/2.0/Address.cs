using System;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street = "v2-Street";
		private string City = "v2-City";

		public override string ToString () {
			return String.Format ("v2 obj {0} {1}", Street, City);
		}
	}
}
