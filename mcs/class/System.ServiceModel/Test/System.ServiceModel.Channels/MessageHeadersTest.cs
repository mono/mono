using System;
using System.Collections;
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
	public class MessageHeadersTest
	{
		string wsa1 = "http://www.w3.org/2005/08/addressing";

		[Test] // it is somehow allowed ...
		public void AddDuplicate ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Default);
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:foo"));
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:bar"));
		}

		[Test]
		public void AddDuplicate2 ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Default);
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:foo", true, "whoa"));
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:bar", true, "whee"));
		}

		[Test]
		public void TestConstructor ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Soap12WSAddressing10);
			Assert.AreEqual (0, headers.Count);

			headers = new MessageHeaders (MessageVersion.Default);
			Assert.AreEqual (0, headers.Count);
		}

		[Test]
		public void TestFindHeader ()
		{
			// <a:Action mU="true">test</a:Action>
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			Assert.AreEqual (1, m.Headers.Count, "#0");

			// <MyHeader xmlns="bar">1</MyHeader>
			m.Headers.Add (MessageHeader.CreateHeader ("MyHeader", "bar", 1));
			// <MyHeader xmlns="baz" role="john">1</MyHeader>
			m.Headers.Add (MessageHeader.CreateHeader ("MyHeader", "baz", 1, false, "john"));

			MessageHeaders headers = m.Headers;
			// The first header is at 0
			Assert.AreEqual (0, headers.FindHeader ("Action", wsa1), "#1");

			// The return value of FindHeader maps to its places in the headers
			Assert.AreEqual (1, headers.FindHeader ("MyHeader", "bar"), "#2");

			// If a header has actor (role) specified, then it must be provided
			Assert.AreEqual (-1, headers.FindHeader ("MyHeader", "baz"), "#3");
			Assert.AreEqual (2, headers.FindHeader ("MyHeader", "baz", "john"), "#4");
		}

		[Test]
		public void TestFindHeaderWithMultipleIdenticalHeaders ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			m.Headers.Add (MessageHeader.CreateHeader (
				"Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing", 1));

			MessageHeaders headers = m.Headers;

			headers.FindHeader ("Action", "http://schemas.xmlsoap.org/ws/2004/08/addressing");
		}

		[Test]
		public void TestGetHeader ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeaders headers = m.Headers;

			Assert.AreEqual ("test", headers.GetHeader<string> (0));
		}

		[Test, ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TestGetHeaderOutOfRange ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeaders headers = m.Headers;

			Assert.AreEqual ("test", headers.GetHeader<int> (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetHeaderNullSerializer ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Default);
			string ns = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
			headers.Add (MessageHeader.CreateHeader ("Action", ns, "urn:foo"));
			headers.GetHeader<string> (0, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetHeaderNullSerializer2 ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Default);
			string ns = "http://schemas.xmlsoap.org/ws/2004/08/addressing";
			headers.Add (MessageHeader.CreateHeader ("Action", ns, "urn:foo"));
			headers.GetHeader<string> ("Action", ns, (XmlObjectSerializer) null);
		}

		[Test, ExpectedException (typeof (MessageHeaderException))]
		public void TestGetHeaderNonexistent ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeaders headers = m.Headers;

			headers.GetHeader<string>("name", "namespace");
		}

		[Test]
		public void SetWSAddressingHeadersNullToNonSoap ()
		{
			Message m = Message.CreateMessage (MessageVersion.None, "test", 1);
			m.Headers.From = null;
			m.Headers.MessageId = null;
			m.Headers.ReplyTo = null;
			m.Headers.FaultTo = null;
			m.Headers.RelatesTo = null;
		}

		[Test]
		public void TestInsert ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);

			m.Headers.Add (MessageHeader.CreateHeader ("FirstHeader", "ns", "first"));
			Assert.AreEqual (1, m.Headers.FindHeader ("FirstHeader", "ns"));

			m.Headers.Insert (1, MessageHeader.CreateHeader ("InsertedHeader", "ns", "insert"));
			
 			Assert.AreEqual (1, m.Headers.FindHeader ("InsertedHeader", "ns"));
 			Assert.AreEqual (2, m.Headers.FindHeader ("FirstHeader", "ns"));
		}

		[Test]
		public void TestWriteStartHeaderSimple ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeader h = MessageHeader.CreateHeader ("FirstHeader", "ns", "first");
			m.Headers.Add (h);

			StringBuilder sb = new StringBuilder ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter w = XmlWriter.Create (sb, s);
			XmlDictionaryWriter dw = XmlDictionaryWriter.CreateDictionaryWriter (w);
			m.Headers.WriteStartHeader (1, dw);
			dw.Close ();
			w.Close ();
			string actual = sb.ToString ();

			sb = new StringBuilder ();
			w = XmlWriter.Create (sb, s);
			dw = XmlDictionaryWriter.CreateDictionaryWriter (w);			
			h.WriteStartHeader (dw, MessageVersion.Soap12WSAddressing10);
			dw.Close ();
			w.Close ();
			string expected = sb.ToString (); 

			
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestWriteStartHeaderWithActor ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeader h = MessageHeader.CreateHeader ("FirstHeader", "ns", "first", true, "actor");
			m.Headers.Add (h);

			StringBuilder sb = new StringBuilder ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter w = XmlWriter.Create (sb, s);
			XmlDictionaryWriter dw = XmlDictionaryWriter.CreateDictionaryWriter (w);
			m.Headers.WriteStartHeader (1, dw);
			dw.Close ();
			w.Close ();
			string actual = sb.ToString ();

			sb = new StringBuilder ();
			w = XmlWriter.Create (sb, s);
			dw = XmlDictionaryWriter.CreateDictionaryWriter (w);
			h.WriteStartHeader (dw, MessageVersion.Soap12WSAddressing10);
			dw.Close ();
			w.Close ();
			string expected = sb.ToString ();

			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestWriteHeaderContents ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);

			m.Headers.Add (MessageHeader.CreateHeader ("FirstHeader", "ns", "first"));
			Assert.AreEqual (1, m.Headers.FindHeader ("FirstHeader", "ns"));

			m.Headers.Add (MessageHeader.CreateHeader ("SecondHeader", "ns", 2));

			StringBuilder sb = new StringBuilder ();
			// Even if XmlWriter is allowed to write fragment,
			// DataContractSerializer never allows it to write
			// content in contentOnly mode.
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			//settings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter w = XmlWriter.Create (sb, settings);
			w.WriteStartElement ("root");
			w.WriteStartElement ("HEADER1");
			m.Headers.WriteHeaderContents (1, w);
			w.WriteEndElement ();
			w.WriteStartElement ("HEADER2");
			m.Headers.WriteHeaderContents (2, w);
			w.WriteEndElement ();
			w.WriteEndElement ();
			w.Close ();
			
			Assert.AreEqual ("<root><HEADER1>first</HEADER1><HEADER2>2</HEADER2></root>", sb.ToString ());
		}

		[Test]
		[Ignore ("I believe that with AddressingVersion.None it should not output any mustUnderstand addressing headers.")]
		public void WriteHeaderContentsAddressingNone ()
		{
			// This Action header is IMO extraneous.
			string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\"><s:Header><Action s:mustUnderstand=\"1\" xmlns=\"http://schemas.microsoft.com/ws/2005/05/addressing/none\">Echo</Action></s:Header><s:Body><z:anyType i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\"/></s:Body></s:Envelope>";
			Message m = Message.CreateMessage (MessageVersion.Soap12, "Echo", (object) null);
			StringWriter sw = new StringWriter ();
			using (XmlWriter w = XmlWriter.Create (sw)) {
				m.WriteMessage (w);
			}
			Assert.AreEqual (xml, sw.ToString ());
		}

		[Test]
		public void TestAction ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			m.Headers.Add (MessageHeader.CreateHeader ("FirstHeader", "ns", "first"));
			Assert.AreEqual ("test", m.Headers.Action, "#1");

			MessageHeaders headers = new MessageHeaders (MessageVersion.Default, 1);
			Assert.AreEqual (null, headers.Action, "#2");
			headers.Add (MessageHeader.CreateHeader ("Action", "http://www.w3.org/2005/08/addressing", "test"));

			MessageHeaderInfo info = headers [0];
			Assert.AreEqual ("Action", info.Name, "#2-1");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", info.Namespace, "#2-2");
			Assert.AreEqual (false, info.MustUnderstand, "#2-3");
			Assert.AreEqual (String.Empty, info.Actor, "#2-4");

			Assert.AreEqual ("test", headers.Action, "#3");
			headers.Clear ();
			Assert.AreEqual (null, headers.Action, "#4");
		}

		[Test]
		public void MessageCreateAddsAction ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			MessageHeaderInfo info = m.Headers [0];
			Assert.AreEqual ("Action", info.Name, "#1");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", info.Namespace, "#2");
			Assert.AreEqual (true, info.MustUnderstand, "#3");
			Assert.AreEqual (String.Empty, info.Actor, "#4");
		}

		[Test]
		public void GetReaderAtHeader ()
		{
			Message m = Message.CreateMessage (MessageVersion.Default, "test", 1);
			XmlDictionaryReader r = m.Headers.GetReaderAtHeader (0);

			XmlDictionaryReader r2 = m.Headers.GetReaderAtHeader (0);
			using (XmlWriter x = XmlWriter.Create (TextWriter.Null)) {
				r2.MoveToContent ();
				while (!r2.EOF)
					x.WriteNode (r2, false);
			}

			/*
			// Seems like there is some attribute order differences 
			// in XmlDictionaryReader or XmlWriter, so don't compare
			// XML in raw strings.

			StringWriter sw = new StringWriter ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlWriter w = XmlWriter.Create (sw, settings);
			Assert.AreEqual (ReadState.Interactive, r.ReadState, "#1-1");
			Assert.AreEqual (XmlNodeType.Element, r.NodeType, "#1-2");
			while (!r.EOF)
				w.WriteNode (r, false);
			w.Flush ();
			Assert.AreEqual ("<Action a:mustUnderstand=\"1\" xmlns=\"http://www.w3.org/2005/08/addressing\" xmlns:a=\"http://www.w3.org/2003/05/soap-envelope\">test</Action>", sw.ToString (), "#2");
			*/
			Assert.AreEqual (ReadState.Interactive, r.ReadState, "#1-1");
			Assert.AreEqual (XmlNodeType.Element, r.NodeType, "#1-2");
			Assert.AreEqual ("Action", r.LocalName, "#1-3");
			Assert.AreEqual ("http://www.w3.org/2005/08/addressing", r.NamespaceURI, "#1-4");
			Assert.AreEqual ("1", r.GetAttribute ("mustUnderstand", "http://www.w3.org/2003/05/soap-envelope"), "#1-5");
			Assert.AreEqual ("test", r.ReadElementContentAsString (), "#1-6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetReaderAtHeaderOutOfRange ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Default, 1);
			h.GetReaderAtHeader (0);
		}

		[Test]
		[ExpectedException (typeof (MessageHeaderException))] // multiple headers: "Action"
		public void CopyHeadersFrom ()
		{
			Message msg = Message.CreateMessage (MessageVersion.Default, "urn:myaction");
			Message msg2 = Message.CreateMessage (MessageVersion.Default, "urn:myaction2");
			msg2.Headers.CopyHeadersFrom (msg);
			Assert.AreEqual ("urn:myaction2", msg2.Headers.Action);
		}

		[Test]
		public void CopyHeadersFrom2 ()
		{
			Message msg = Message.CreateMessage (MessageVersion.Default, "urn:myaction");
			Message msg2 = Message.CreateMessage (MessageVersion.Default, "urn:myaction2");
			msg2.Headers.Action = null;
			msg2.Headers.CopyHeadersFrom (msg);
			Assert.AreEqual ("urn:myaction", msg2.Headers.Action);
		}

		[Test]
		public void AddressingNoneAndAction ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.Action = "urn:foo";
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddressingNoneAndFrom ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.From = new EndpointAddress ("http://localhost:8080");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddressingNoneAndFaultTo ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.FaultTo = new EndpointAddress ("http://localhost:8080");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddressingNoneAndMessageId ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.MessageId = new UniqueId (Guid.NewGuid ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddressingNoneAndRelatesTo ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.RelatesTo = new UniqueId (Guid.NewGuid ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddressingNoneAndReplyTo ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.ReplyTo = new EndpointAddress ("http://localhost:8080");
		}

		[Test]
		public void AddressingNoneAndTo ()
		{
			MessageHeaders h = new MessageHeaders (MessageVersion.Soap12);
			h.To = new Uri ("http://localhost:8080");
		}

		[Test]
		[ExpectedException (typeof (MessageHeaderException))]
		public void DuplicateActionFindError ()
		{
			MessageHeaders headers = new MessageHeaders (MessageVersion.Default);
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:foo"));
			headers.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:bar"));
			Assert.Fail (String.Format ("Action should not be caught", headers.Action)); // access to Action results in an error. If it does not, then simply assert fail.
		}

		[Test]
		[ExpectedException (typeof (MessageHeaderException))]
		public void CopyHeadersFrom_Merge ()
		{
			var h1 = new MessageHeaders (MessageVersion.Default);
			var h2 = new MessageHeaders (MessageVersion.Default);
			h1.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:foo"));
			h2.Add (MessageHeader.CreateHeader ("Action", wsa1, "urn:bar"));
			h1.CopyHeadersFrom (h2); // it somehow allow dups!
			Assert.Fail (String.Format ("Action should not be caught", h1.Action)); // access to Action results in an error. If it does not, then simply assert fail.
		}

	}
}
