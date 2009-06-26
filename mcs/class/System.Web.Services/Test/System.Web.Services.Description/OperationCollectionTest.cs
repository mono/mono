//
// MonoTests.System.Web.Services.Description.OperationCollectionTest.cs
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
	public class OperationCollectionTest
	{
		OperationCollection operations;

		[SetUp]
		public void InitializeOperations()
		{
			// workaround for internal constructor
			PortType portType = new PortType();
			operations = portType.Operations;
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.AreEqual (0, operations.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestAddNullObject()
		{
			operations.Add(null);
		}

		[Test]
		public void TestAddValidOperation()
		{
			operations.Add(new Operation());	
			Assert.AreEqual (1, operations.Count);
		}
	}
}
