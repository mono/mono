using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageFaultTest
	{
		[Test]
		public void CreateFault ()
		{
			var msg = Message.CreateMessage (XmlReader.Create (new StreamReader (TestResourceHelper.GetFullPathOfResource ("Test/Resources/soap-fault.xml"))), 0x10000, MessageVersion.Default);
			MessageFault.CreateFault (msg, 0x10000);
		}

		[Test]
		public void CreateFaultWithNumberCode ()
		{
			var msgVersion = MessageVersion.CreateVersion (EnvelopeVersion.Soap11, AddressingVersion.None);

			var msg = Message.CreateMessage (XmlReader.Create (new StreamReader (TestResourceHelper.GetFullPathOfResource ("Test/Resources/soap-fault-number.xml"))), 0x10000, msgVersion);
			MessageFault.CreateFault (msg, 0x10000);
		}

		[Test]
		public void CreateFaultMessageVersionNone ()
		{
			var msg = Message.CreateMessage (MessageVersion.None, new FaultCode ("DestinationUnreachable"), "typical error", null);
			var fault = MessageFault.CreateFault (msg, 0x10000);
			Assert.AreEqual ("DestinationUnreachable", fault.Code.Name, "#1");
			Assert.AreEqual ("typical error", fault.Reason.ToString (), "#2");
		}

		[Test]
		[ExpectedException (typeof (CommunicationException))]
		public void CreateFaultIncomplete ()
		{
			var msg = Message.CreateMessage (XmlReader.Create (new StreamReader (TestResourceHelper.GetFullPathOfResource ("Test/Resources/soap-fault-incomplete.xml"))), 0x10000, MessageVersion.Default);
			MessageFault.CreateFault (msg, 0x10000);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateFaultIncomplete2 ()
		{
			MessageFault.CreateFault (new FaultCode ("s:Sender"), (FaultReason) null);
		}

		[Test]
//		[ExpectedException (typeof (CommunicationException))]
		public void CreateFaultIncomplete3 ()
		{
			MessageFault.CreateFault (new FaultCode ("s:Sender"), new FaultReason ("anyways"));
		}

		[Test]
		[ExpectedException (typeof (CommunicationException))]
		public void CreateFaultIncomplete4 ()
		{
			var msg = Message.CreateMessage (XmlReader.Create (new StreamReader (TestResourceHelper.GetFullPathOfResource ("Test/Resources/soap-fault-incomplete4.xml"))), 0x10000, MessageVersion.Default);
			MessageFault.CreateFault (msg, 0x10000);
		}

		[Test]
		public void CreateFaultFromMessage ()
		{
			var xml = @"
<s:Envelope xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>
  <s:Header>
    <a:Action s:mustUnderstand='1'>http://www.w3.org/2005/08/addressing/fault</a:Action>
  </s:Header>
  <s:Body>
    <s:Fault>
      <faultcode>a:ActionNotSupported</faultcode>
      <faultstring xml:lang='en-US'>some error</faultstring>
      <faultactor>Random</faultactor>
    </s:Fault>
  </s:Body>
</s:Envelope>";
			var msg = Message.CreateMessage (XmlReader.Create (new StringReader (xml)), 0x1000, MessageVersion.Soap11WSAddressing10);
			MessageFault.CreateFault (msg, 1000);
			msg = Message.CreateMessage (XmlReader.Create (new StringReader (xml)), 0x1000, MessageVersion.Soap11WSAddressing10);
			var mb = msg.CreateBufferedCopy (1000);
			MessageFault.CreateFault (mb.CreateMessage (), 1000);
		}
	}
}
