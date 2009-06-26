//
// MonoTests.System.Web.Services.Description.TypesTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class TypesTest
	{
		Types types;

		[SetUp]
		public void InitializeTypes()
		{
			types = new Types();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.AreEqual (String.Empty, types.Documentation);
			Assert.IsNotNull (types.Schemas);
			Assert.AreEqual (0, types.Schemas.Count);
			Assert.IsNotNull (types.Extensions);
			Assert.AreEqual (0, types.Extensions.Count);
		}
	}
}
