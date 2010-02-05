using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class AddressHeaderTest
	{
		[Test]
		public void WriteAddressHeaderTest ()
		{
			AddressHeader h = AddressHeader.CreateAddressHeader (1);

			StringWriter sw = new StringWriter ();
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			XmlWriter w = XmlWriter.Create (sw, s);
			h.WriteAddressHeader (w);

			w.Close ();

			Assert.AreEqual (
				@"<int xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">1</int>",
				sw.ToString ());
		}

		[Test]
		public void CreateAddressHeader ()
		{
			AddressHeader h = AddressHeader.CreateAddressHeader ("foo", "urn:foo", null);
		}

		[Test]
		[Category ("NotDotNet")]
		// It should work, but MS implementation expects body to be
		// IComparable.
		public void EqualsTest ()
		{
			AddressHeader h = AddressHeader.CreateAddressHeader (
				"foo", "urn:foo", null);
			AddressHeader h2 = AddressHeader.CreateAddressHeader (
				"foo", "urn:foo", null);
			Assert.IsFalse (h.Equals (null), "#1"); // never throw nullref
			Assert.IsTrue (h.Equals (h2), "#2");
		}

		[Test]
		public void GetAddressHeaderReader ()
		{
			var h = AddressHeader.CreateAddressHeader ("foo", String.Empty, null);
			var r = h.GetAddressHeaderReader ();
			r.MoveToContent ();
			Assert.AreEqual ("foo", r.LocalName, "#1");
			Assert.AreEqual ("true", r.GetAttribute ("nil", XmlSchema.InstanceNamespace), "#2");
		}

		[Test]
		public void WriteAddressHeader ()
		{
			var h = AddressHeader.CreateAddressHeader ("foo", "urn:foo", null);
			var sw = new StringWriter ();
			var xw = XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
			h.WriteAddressHeader (xw);
			xw.Close ();
			Assert.AreEqual ("<?xml version=\"1.0\" encoding=\"utf-16\"?><foo i:nil=\"true\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:foo\" />", sw.ToString (), "#1");
		}
	}
}
