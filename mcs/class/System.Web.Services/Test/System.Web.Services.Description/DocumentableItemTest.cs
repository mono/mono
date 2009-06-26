//
// MonoTests.System.Web.Services.Description.DocumentableItemTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class DocumentableItemTest
	{
		DocumentableItem item;

		[SetUp]
		public void InitializeItem()
		{
			// workaround for base class
			item = new Types();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.AreEqual (String.Empty, item.Documentation);
		}
		
		[Test]
		public void TestNullDocumentationString()
		{
			item.Documentation = null;
			Assert.AreEqual (String.Empty, item.Documentation);
		}
	}
}
