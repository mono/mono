//
// MonoTests.System.Web.Services.Description.OperationMessageCollectionTest.cs
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
	public class OperationMessageCollectionTest
	{
		OperationMessageCollection operations;

		[SetUp]
		public void InitializeOperation()
		{
			// workaround for internal constructor
			Operation op = new Operation();
			operations = op.Messages;
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assertion.AssertEquals(OperationFlow.None, operations.Flow);
			Assertion.AssertNull(operations.Input);
			Assertion.AssertNull(operations.Output);
			Assertion.AssertEquals(0, operations.Count);
		}

		[Test]
		public void TestAddInput()
		{
			operations.Add(new OperationInput());
			
			Assertion.AssertEquals(OperationFlow.OneWay, operations.Flow);
			Assertion.AssertNotNull(operations.Input);
			Assertion.AssertNull(operations.Output);
			Assertion.AssertEquals(1, operations.Count);
		}
		
		[Test]
		public void TestAddOutput()
		{
			operations.Add(new OperationOutput());
			
			Assertion.AssertEquals(OperationFlow.Notification, operations.Flow);
			Assertion.AssertNull(operations.Input);
			Assertion.AssertNotNull(operations.Output);
			Assertion.AssertEquals(1, operations.Count);
		}

		[Test]
		public void TestAddInputAndOutput()
		{
			operations.Add(new OperationInput());
			operations.Add(new OperationOutput());
			
			Assertion.AssertEquals(OperationFlow.RequestResponse, operations.Flow);
			Assertion.AssertNotNull(operations.Input);
			Assertion.AssertNotNull(operations.Output);
			Assertion.AssertEquals(2, operations.Count);
		}

		[Test]
		public void TestAddOutputAndInput()
		{
			operations.Add(new OperationOutput());
			operations.Add(new OperationInput());
			
			Assertion.AssertEquals(OperationFlow.SolicitResponse, operations.Flow);
			Assertion.AssertNotNull(operations.Input);
			Assertion.AssertNotNull(operations.Output);
			Assertion.AssertEquals(2, operations.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestAddNull()
		{
			operations.Add(null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestAddFault()
		{
			operations.Add(new OperationFault());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddInputAndInput()
		{
			operations.Add(new OperationInput());
			operations.Add(new OperationInput());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddOutputAndOutput()
		{
			operations.Add(new OperationOutput());
			operations.Add(new OperationOutput());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestAddThreeOperationMessages()
		{
			operations.Add(new OperationOutput());
			operations.Add(new OperationOutput());
			operations.Add(new OperationOutput());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestAddInputAndFault()
		{
			operations.Add(new OperationInput());
			operations.Add(new OperationFault());
		}
	}
}
