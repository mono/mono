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
	public class MessageHeaderTest
	{
		[Test]
		public void TestDefaultValues ()
		{
			MessageHeader h = MessageHeader.CreateHeader ("foo", "bar", 1);
			Assert.AreEqual (String.Empty, h.Actor);
			Assert.IsFalse (h.MustUnderstand);
			Assert.IsFalse (h.Relay);
		}

		[Test]
		public void TestIsMessageVersionSupported ()
		{
			MessageHeader h;

			// by default, both versions are supported.
			h = MessageHeader.CreateHeader ("foo", "bar", 1);
			Assert.IsTrue (h.IsMessageVersionSupported (MessageVersion.Soap11WSAddressing10), "#1");
			Assert.IsTrue (h.IsMessageVersionSupported (MessageVersion.Soap12WSAddressing10), "#2");

			// SOAP 1.1 is not supported if Actor == Soap12.NextDestinationActorValue
			h = MessageHeader.CreateHeader ("foo", "bar", 1, false, MessageVersion.Soap12WSAddressing10.Envelope.NextDestinationActorValue);
			Assert.IsFalse (h.IsMessageVersionSupported (MessageVersion.Soap11WSAddressing10), "#3");
			Assert.IsTrue (h.IsMessageVersionSupported (MessageVersion.Soap12WSAddressing10), "#4");

			// SOAP 1.1 is not supported if Actor == Soap12's UltimateDestinationActor
			h = MessageHeader.CreateHeader ("foo", "bar", 1, false, MessageVersion.Soap12WSAddressing10.Envelope.GetUltimateDestinationActorValues () [1]);
			Assert.IsFalse (h.IsMessageVersionSupported (MessageVersion.Soap11WSAddressing10), "#5");
			Assert.IsTrue (h.IsMessageVersionSupported (MessageVersion.Soap12WSAddressing10), "#6");

			// SOAP 1.2 is not supported if Actor == Soap11.NextDestinationActorValue
			h = MessageHeader.CreateHeader ("foo", "bar", 1, false, MessageVersion.Soap11WSAddressing10.Envelope.NextDestinationActorValue);
			Assert.IsTrue (h.IsMessageVersionSupported (MessageVersion.Soap11WSAddressing10), "#7");
			Assert.IsFalse (h.IsMessageVersionSupported (MessageVersion.Soap12WSAddressing10), "#8");
		}

		[Test]
		public void TestToString ()
		{
			MessageHeader h = MessageHeader.CreateHeader ("foo", "bar", 1);
			StringBuilder sb = new StringBuilder ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			XmlWriter w = XmlWriter.Create (sb, settings);

			h.WriteHeader (w, MessageVersion.Soap12WSAddressing10);
			w.Close ();
			
			// ToString is the same as WriteHeader (minus the XML declaration)
			Assert.AreEqual (sb.ToString (), h.ToString ());
		}

		[Test]
		public void TestWriteStartHeader ()
		{
			int value = 1;

			MessageHeader h = MessageHeader.CreateHeader ("foo", "bar", value);                        

			StringBuilder sb = new StringBuilder ();                                            
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (
				XmlWriter.Create (sb));                     
			h.WriteStartHeader (w, MessageVersion.Soap12WSAddressing10);
			w.Close ();
			string actual = sb.ToString ();
			
			sb = new StringBuilder ();
			XmlWriter w2 = XmlWriter.Create (sb);
			w2.WriteStartElement (h.Name, h.Namespace);
			w2.Close ();
			string expected = sb.ToString ();

			// WriteStartHeader == WriteStartElement (msg.Name, msg.Namespace)
			Assert.AreEqual (expected, actual);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestWriteHeaderContent ()
		{
			int value = 1;

			MessageHeader h = MessageHeader.CreateHeader ("foo", "bar", value);
			XmlObjectSerializer f = new DataContractSerializer (typeof (int));

			StringBuilder sb = new StringBuilder ();
			XmlWriterSettings settings = new XmlWriterSettings ();
			settings.ConformanceLevel = ConformanceLevel.Auto;                        
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (
				XmlWriter.Create (sb, settings));

			w.WriteStartElement ("dummy-root");
			h.WriteHeaderContents (w, MessageVersion.Soap12WSAddressing10);
			w.WriteEndElement ();
			w.Flush ();
			string actual = sb.ToString ();

			f.WriteObject (w, value);
			string expected = sb.ToString ();

			// the output of WriteHeaderContent is the same as XmlSerializer.Serialize
			Assert.AreEqual (expected, actual);
		}

		[Test]
		public void TestWriteHeaderAttributes ()
		{
			MessageHeader h = MessageHeader.CreateHeader ("foo", "bar", 1, 
				true, "some actor", true);
			StringBuilder sb = new StringBuilder ();                
			XmlDictionaryWriter w = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sb));
		       
			h.WriteStartHeader (w, MessageVersion.Soap12WSAddressing10);
			w.WriteEndElement ();
			w.Close ();
		}

		[Test]
		public void TestGenericConstructor ()
		{
			MessageHeader<int> h = new MessageHeader<int> ();

			Assert.AreEqual (null, h.Actor);
			Assert.AreEqual (default (int), h.Content);
			Assert.IsFalse (h.MustUnderstand);
			Assert.IsFalse (h.Relay);
		}

		[Test]
		public void CreateEndpointAddressTypeHeader ()
		{
			MessageHeader.CreateHeader (
				"ReplyTo",
				"http://www.w3.org/2005/08/addressing",
				new EndpointAddress ("http://localhost:8080"));
			MessageHeader.CreateHeader (
				"ReplyTo",
				"http://www.w3.org/2005/08/addressing",
				new EndpointAddress ("http://localhost:8080"),
				new DataContractSerializer (typeof (EndpointAddress)));
		}
	}
}
