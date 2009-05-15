using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageFaultTest
	{
		[Test]
		[ExpectedException (typeof (CommunicationException))]
		public void CreateFaultIncomplete ()
		{
			var msg = Message.CreateMessage (XmlReader.Create (new StreamReader ("Test/System.ServiceModel.Channels/soap-fault-incomplete.xml")), 0x10000, MessageVersion.Default);
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
	}
}
