//
// MessageTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class MessageTest
	{
		public static string GetSerializedMessage (Message msg)
		{
			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			using (XmlWriter xw = XmlWriter.Create (sw, settings)) {
				msg.WriteMessage (xw);
			}
			return sw.ToString ();
		}

		[Test]
		public void CreateNullAction ()
		{
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw)) {
				Message.CreateMessage (MessageVersion.Default, (string) null).WriteMessage (w);
			}
			Assert.IsTrue (sw.ToString ().IndexOf ("Action") < 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateNullVersion ()
		{
			StringWriter sw = new StringWriter ();
			Message.CreateMessage (null, "http://tempuri.org/MyAction");
		}

		[Test]
		public void CreateSimpleAndWrite ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">Ack</a:Action></s:Header><s:Body /></s:Envelope>";

			Message msg = Message.CreateMessage (
				MessageVersion.Default, "Ack");
			// It should be XML comparison, not string comparison
			Assert.AreEqual (xml, GetSerializedMessage (msg));
		}

		[Test]
		public void CreateSimpleNonPrimitive ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">Sample1</a:Action></s:Header><s:Body><MessageTest.Sample1 xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.datacontract.org/2004/07/MonoTests.System.ServiceModel.Channels""><Member1>member1</Member1><Member2><Member>sample2 member</Member></Member2></MessageTest.Sample1></s:Body></s:Envelope>";

			Message msg = Message.CreateMessage (
				MessageVersion.Default, "Sample1", new Sample1 ());
			// It should be XML comparison, not string comparison
			Assert.AreEqual (xml, GetSerializedMessage (msg));
		}

		[DataContract]
		class Sample1
		{
			[DataMember]
			public string Member1 = "member1";

			[DataMember]
			public Sample2 Member2 = new Sample2 ();
		}

		[DataContract]
		class Sample2
		{
			[DataMember]
			public string Member = "sample2 member";
		}

		[Test]
		public void CreateSimpleSoap11WSA1_0 ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><a:Action s:mustUnderstand=""1"">http://tempuri.org/IFoo/Echo</a:Action></s:Header><s:Body><string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">hoge</string></s:Body></s:Envelope>";

			Message msg = Message.CreateMessage (
				MessageVersion.Soap11WSAddressing10,
				"http://tempuri.org/IFoo/Echo",
				"hoge");
			// It should be XML comparison, not string comparison
			Assert.AreEqual (xml, GetSerializedMessage (msg));
		}

		[Test]
		public void CreateFaultMessageAndWrite ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">http://www.w3.org/2005/08/addressing/fault</a:Action></s:Header><s:Body><s:Fault><s:Code><s:Value xmlns:a=""urn:me"">a:me</s:Value></s:Code><s:Reason><s:Text xml:lang="""">am so lazy</s:Text></s:Reason></s:Fault></s:Body></s:Envelope>";

			FaultCode fc = new FaultCode ("me", "urn:me");
			FaultReasonText ft = new FaultReasonText ("am so lazy", CultureInfo.InvariantCulture);
			Message msg = Message.CreateMessage (
				MessageVersion.Default,
				MessageFault.CreateFault (fc, new FaultReason (ft)),
				"http://www.w3.org/2005/08/addressing/fault");
			// It should be XML comparison, not string comparison
			Assert.AreEqual (xml, GetSerializedMessage (msg));
		}

		[Test]
		public void CreateAndWriteBodyObject ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">Ack</a:Action></s:Header><s:Body><string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">TEST</string></s:Body></s:Envelope>";

			Message msg = Message.CreateMessage (
				MessageVersion.Default, "Ack", "TEST");
			// It should be XML comparison, not string comparison
			Assert.AreEqual (xml, GetSerializedMessage (msg));
		}

		// From XmlReader

		[Test]
		public void CreateFromXmlReader ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">Ack</a:Action><TestHeaderItem>test</TestHeaderItem></s:Header><s:Body></s:Body></s:Envelope>";

			XmlReader r = XmlReader.Create (new StringReader (xml));
			Message msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			Assert.AreEqual (MessageVersion.Default, msg.Version, "#2");
			Assert.AreEqual (2, msg.Headers.Count, "#4");
			Assert.AreEqual ("Ack", msg.Headers.Action, "#3");
			Assert.IsNull (msg.Headers.FaultTo, "#5");
			Assert.IsNull (msg.Headers.From, "#6");
			Assert.IsNull (msg.Headers.MessageId, "#7");
			Assert.IsTrue (msg.IsEmpty, "#8");

			MessageHeaderInfo hi = msg.Headers [0];
			Assert.AreEqual ("Action", hi.Name, "#h1-1");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", hi.Namespace, "#h1-2");
			Assert.AreEqual (String.Empty, hi.Actor, "#h1-3");
			Assert.IsTrue (hi.MustUnderstand, "#h1-4");
			Assert.IsFalse (hi.Relay, "#h1-5");
			Assert.IsFalse (hi.IsReferenceParameter, "#h1-6");

			int count = 0;

			/* FIXME: test UnderstoodHeaders later
			foreach (MessageHeaderInfo i in msg.Headers.UnderstoodHeaders) {
				count++;
				Assert.AreEqual ("", i.Actor, "#9");
				Assert.IsFalse (i.IsReferenceParameter, "#10");
				Assert.IsTrue (i.MustUnderstand, "#11");
				Assert.AreEqual ("Action", i.Name, "#12");
				Assert.AreEqual ("http://www.w3.org/2005/08/addressing", i.Namespace, "#13");
				Assert.AreEqual (false, i.Relay, "#14");
			}
			Assert.AreEqual (1, count, "#15"); // allow only once
			*/

			// MS implementation closes XmlReader regardless of
			// its initial state, which isn't good.
			//Assert.AreEqual (ReadState.Closed, r.ReadState, "#1");

			r = XmlReader.Create (new StringReader (xml));
			r.MoveToContent ();
			msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			// ditto
			// Assert.AreEqual (ReadState.Closed, r.ReadState, "#1-2");

			string xml2 = @"<Wrap><s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header><a:Action s:mustUnderstand=""1"">Ack</a:Action></s:Header><s:Body /></s:Envelope></Wrap>";

			r = XmlReader.Create (new StringReader (xml2));
			r.MoveToContent ();
			r.Read (); // s:Envelope
			msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			// ditto
			// Assert.AreEqual (ReadState.Closed, r.ReadState, "#1-3");
		}

		[Test]
		public void CreateFromXmlReader2 ()
		{
			string xml = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">hoge</string></s:Body></s:Envelope>";
			XmlReader r = XmlReader.Create (new StringReader (xml));
			Message msg = Message.CreateMessage (r, 10000, MessageVersion.Soap11WSAddressing10);
			Assert.AreEqual (0, msg.Headers.Count, "#1");
			Assert.IsNull (msg.Headers.Action, "#2");
			Assert.IsFalse (msg.IsEmpty, "#3");
			Assert.AreEqual ("hoge", msg.GetBody<string> (), "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetReaderAtBodyContentsEmptyError ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header></s:Header><s:Body></s:Body></s:Envelope>";

			XmlReader r = XmlReader.Create (new StringReader (xml));
			Message msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			msg.GetReaderAtBodyContents ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetReaderAtBodyContentsTwice ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header></s:Header><s:Body>TEST</s:Body></s:Envelope>";

			XmlReader r = XmlReader.Create (new StringReader (xml));
			Message msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			msg.GetReaderAtBodyContents ();
			msg.GetReaderAtBodyContents ();
		}

		[Test]
		public void GetReaderAtBodyContentsAfterSourceClosed ()
		{
			string xml = @"<s:Envelope xmlns:a=""http://www.w3.org/2005/08/addressing"" xmlns:s=""http://www.w3.org/2003/05/soap-envelope""><s:Header></s:Header><s:Body>TEST</s:Body></s:Envelope>";

			Message msg;
			using (XmlReader r = XmlReader.Create (new StringReader (xml))) {
				msg = Message.CreateMessage (r, 10000, MessageVersion.Default);
			}
			// The reader is already closed by nature of using statement.
			XmlDictionaryReader r2 = msg.GetReaderAtBodyContents ();
			Assert.AreEqual (ReadState.Closed, r2.ReadState);
		}

		[Test]
		public void WriteMessagePOX ()
		{
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><z:anyType i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" />";
			Message m = Message.CreateMessage (MessageVersion.None, "Blah", (object) null);
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw)) {
				m.WriteMessage (w);
			}
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void ReadHeaders1 ()
		{
			string xml = "<s:Envelope xmlns:s='http://www.w3.org/2003/05/soap-envelope'><s:Header><custom1 /><custom2>bah</custom2></s:Header><s:Body/></s:Envelope>";
			using (XmlReader r = XmlReader.Create (new StringReader (xml))) {
				Message m = Message.CreateMessage (r, 1000, MessageVersion.Default);
				Assert.AreEqual (2, m.Headers.Count, "#1");
				Assert.AreEqual (String.Empty, m.Headers.GetHeader<string> (0), "#2");
				Assert.AreEqual ("bah", m.Headers.GetHeader<string> (1), "#3");
				// 0, 1, then 0
				Assert.AreEqual (String.Empty, m.Headers.GetHeader<string> (0), "#2");
			}
		}

		[Test]
		public void ReadHeaders2 ()
		{
			string xml = "<s:Envelope xmlns:s='http://www.w3.org/2003/05/soap-envelope'><s:Header><u:Timestamp xmlns:u='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'><u:Created>blah</u:Created><u:Expires>bleh</u:Expires></u:Timestamp></s:Header><s:Body/></s:Envelope>";
			using (XmlReader r = XmlReader.Create (new StringReader (xml))) {
				Message m = Message.CreateMessage (r, 1000, MessageVersion.Default);
				Assert.AreEqual (1, m.Headers.Count, "#1");
			}
		}

		[Test]
		public void ReadHeadersMustUnderstand ()
		{
			// it includes mustUnderstand, but is ignored at
			// this stage.
			string xml = "<s:Envelope xmlns:s='http://www.w3.org/2003/05/soap-envelope'><s:Header><blah s:mustUnderstand='true'>foo</blah></s:Header><s:Body/></s:Envelope>";
			using (XmlReader r = XmlReader.Create (new StringReader (xml))) {
				Message m = Message.CreateMessage (r, 1000, MessageVersion.Default);
				Assert.AreEqual (1, m.Headers.Count, "#1");
			}
		}

		[Test]
		public void ToStringSomehowDoesNotConsumeMessage ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			Assert.AreEqual (m.ToString (), m.ToString ());
		}

		[Test]
		public void ToStringThenWriteMessageTwice ()
		{
			var mmm = Message.CreateMessage (MessageVersion.None, "urn:foo", XmlReader.Create (new StringReader ("<root>test</root>")));
			mmm.ToString ();
			mmm.ToString ();
			mmm.WriteMessage (XmlWriter.Create (TextWriter.Null));
			try {
				mmm.WriteMessage (XmlWriter.Create (TextWriter.Null));
				Assert.Fail ("not allowed; should raise InvalidOperationException");
			} catch (InvalidOperationException) {
				// dare avoid ExpectedException, to verify that the first call to WriteMessage() is valid.
			}
		}

		[Test]
		public void IsFault ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "action", 1);
			Assert.IsFalse (m.IsFault, "#1");
			m = Message.CreateMessage (MessageVersion.Default, new FaultCode ("ActionNotSupported", "urn:myfault"), "I dunno", "urn:myaction");
			Assert.IsTrue (m.IsFault, "#2");
		}

		[Test]
		public void IsFault2 ()
		{
			string xml = @"
<s:Envelope xmlns:a='http://www.w3.org/2005/08/addressing' xmlns:s='http://www.w3.org/2003/05/soap-envelope'>
  <s:Header>
    <a:Action s:mustUnderstand='1'>http://www.w3.org/2005/08/addressing/fault</a:Action>
  </s:Header>
  <s:Body>
    <s:Fault xmlns:s='http://www.w3.org/2003/05/soap-envelope'>
      <s:Code>
        <s:Value>s:Sender</s:Value>
        <s:Subcode>
          <s:Value>a:ActionNotSupported</s:Value>
        </s:Subcode>
      </s:Code>
      <s:Reason>
        <s:Text xml:lang='ja-JP'>message</s:Text>
      </s:Reason>
    </s:Fault>
  </s:Body>
</s:Envelope>";
			var msg = Message.CreateMessage (MessageVersion.Soap11, "urn:foo", XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual ("urn:foo", msg.Headers.Action, "#1");
			msg.ToString ();
			Assert.IsFalse (msg.IsFault, "#2"); // version mismatch

			msg = Message.CreateMessage (MessageVersion.Soap12, "urn:foo", XmlReader.Create (new StringReader (xml)));
			Assert.AreEqual ("urn:foo", msg.Headers.Action, "#3");
			msg.ToString ();
			Assert.IsFalse (msg.IsFault, "#4"); // version match, but it doesn't set as true. It is set true only when it is constructed with fault objects.
		}

		[Test]
		public void State ()
		{
			var msg = Message.CreateMessage (MessageVersion.Soap11, "urn:foo", (object) null);
			var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (TextWriter.Null));
			msg.WriteStartEnvelope (xw);
			Assert.AreEqual (MessageState.Created, msg.State, "#1");
			msg.WriteStartBody (xw);
			Assert.AreEqual (MessageState.Created, msg.State, "#2");
		}
	}
}
