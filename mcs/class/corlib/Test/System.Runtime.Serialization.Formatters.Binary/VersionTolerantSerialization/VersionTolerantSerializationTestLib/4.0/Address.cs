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
	}
}
