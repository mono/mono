//
// MonoTests.System.Web.Services.Description.OperationMessageTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;
using System.Xml;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class OperationMessageTest
	{
		OperationMessage operation;

		[SetUp]
		public void InitializeOperation()
		{
			// workaround: OperationInput, OperationOutput and OperationFault are all empty derivations of OperationMessage
			operation = new OperationInput();
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assertion.AssertEquals(String.Empty, operation.Documentation);
			Assertion.AssertNull(operation.Name);
			Assertion.AssertEquals(XmlQualifiedName.Empty, operation.Message);
			Assertion.AssertNull(operation.Operation);
		}
	}
}
