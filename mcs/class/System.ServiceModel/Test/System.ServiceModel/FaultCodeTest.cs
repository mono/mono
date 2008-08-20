using System;
using System.ServiceModel;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class FaultCodeTest
	{
		FaultCode code;

		[Test]
		public void TestDefaults ()
		{
			code = new FaultCode ("foo");
			Assert.AreEqual ("foo", code.Name);
			Assert.AreEqual (String.Empty, code.Namespace);
			Assert.AreEqual (null, code.SubCode);			
		}

		[Test]
		public void TestReceiverFaultCode ()
		{
			code = FaultCode.CreateReceiverFaultCode ("foo", "bar");
			Assert.IsTrue (code.IsReceiverFault);	
			Assert.AreEqual ("Receiver", code.Name);
			Assert.AreEqual (String.Empty, code.Namespace);
			Assert.AreEqual ("foo", code.SubCode.Name);
			Assert.AreEqual ("bar", code.SubCode.Namespace);								
			
			code = new FaultCode ("Receiver");			
			Assert.IsTrue (code.IsReceiverFault);

			code = new FaultCode ("something else");			
			Assert.IsFalse (code.IsReceiverFault);
		}

		[Test]
		public void TestSenderFaultCode ()
		{
			code = FaultCode.CreateSenderFaultCode ("foo", "bar");
			Assert.IsTrue (code.IsSenderFault);	
			Assert.AreEqual ("Sender", code.Name);
			Assert.AreEqual (String.Empty, code.Namespace);
			Assert.AreEqual ("foo", code.SubCode.Name);
			Assert.AreEqual ("bar", code.SubCode.Namespace);								
			
			code = new FaultCode ("Sender");			
			Assert.IsTrue (code.IsSenderFault);

			code = new FaultCode ("something else");			
			Assert.IsFalse (code.IsReceiverFault);
		}

		[Test]
		public void TestIsPredefinedCode ()
		{
			code = new FaultCode ("foo");
			Assert.IsTrue (code.IsPredefinedFault);

			code = new FaultCode ("foo", String.Empty);
			Assert.IsTrue (code.IsPredefinedFault);

			code = new FaultCode ("foo", "bar");
			Assert.IsFalse (code.IsPredefinedFault);

			code = FaultCode.CreateReceiverFaultCode (new FaultCode ("foo", "bar"));
			Assert.IsTrue (code.IsPredefinedFault);
			Assert.IsFalse (code.SubCode.IsPredefinedFault);

			code = FaultCode.CreateReceiverFaultCode (new FaultCode ("foo"));
			Assert.IsTrue (code.IsPredefinedFault);
			Assert.IsTrue (code.SubCode.IsPredefinedFault);

		}

		[Test]
		public void TestNamespace ()
		{
			code = new FaultCode ("foo");
			Assert.AreEqual (String.Empty, code.Namespace);
			Assert.IsTrue (code.IsPredefinedFault);			
		}
	}
}
