using System;
using System.Runtime.Serialization;

namespace VersionTolerantSerializationTestLib
{
	[Serializable]
	public class Address
	{
		private string Street;
		private string City;
		private string CountryCode;

		[OptionalField (VersionAdded = 4)]
		private string PostCode;

		[OptionalField (VersionAdded = 5)]
		private string AreaCode = "0";

		[OptionalField (VersionAdded = 6)]
		private int Id = 0;
	}
}
