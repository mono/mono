//
// XmlDsigBase64TransformTest.cs - NUnit Test Cases for XmlDsigBase64Transform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class XmlDsigBase64TransformTest {

		protected XmlDsigBase64Transform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new XmlDsigBase64Transform ();
			Type t = typeof (XmlDsigBase64Transform);
		}

		[Test]
		public void Properties () 
		{
			Assertion.AssertEquals ("Algorithm", "http://www.w3.org/2000/09/xmldsig#base64", transform.Algorithm);

			Type[] input = transform.InputTypes;
			Assertion.Assert ("Input #", (input.Length == 3));
			// check presence of every supported input types
			bool istream = false;
			bool ixmldoc = false;
			bool ixmlnl = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.IO.Stream")
					istream = true;
				if (t.ToString () == "System.Xml.XmlDocument")
					ixmldoc = true;
				if (t.ToString () == "System.Xml.XmlNodeList")
					ixmlnl = true;
			}
			Assertion.Assert ("Input Stream", istream);
			Assertion.Assert ("Input XmlDocument", ixmldoc);
			Assertion.Assert ("Input XmlNodeList", ixmlnl);

			Type[] output = transform.OutputTypes;
			Assertion.Assert ("Output #", (output.Length == 1));
			// check presence of every supported output types
			bool ostream = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.IO.Stream")
					ostream = true;
			}
			Assertion.Assert ("Output Stream", ostream);
		}

		private string Stream2String (Stream s) 
		{
			StringBuilder sb = new StringBuilder ();
			int b = s.ReadByte ();
			while (b != -1) {
				sb.Append (b.ToString("X2"));
				b = s.ReadByte ();
			}
			return sb.ToString ();
		}

		private byte[] Stream2Array (Stream s) 
		{
			string st = Stream2String (s);
			byte[] array = new byte [st.Length / 2];
			for (int i=0; i < array.Length; i++) {
				string hex = st.Substring (i*2, 2);
				array [i] = Convert.ToByte(hex, 16);
			}
			return array;
		}

		static private string base64 = "XmlDsigBase64Transform";
		static private byte[] base64array = { 0x58, 0x6D, 0x6C, 0x44, 0x73, 0x69, 0x67, 0x42, 0x61, 0x73, 0x65, 0x36, 0x34, 0x54, 0x72, 0x61, 0x6E, 0x73, 0x66, 0x6F, 0x72, 0x6D };

		private XmlDocument GetDoc () 
		{
			string xml = "<Test>" + Convert.ToBase64String (base64array) + "</Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			return doc;
		}

		[Test]
		public void LoadInputAsXmlDocument () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc);
			Stream s = (Stream) transform.GetOutput ();
			byte[] output = Stream2Array (s);
			Assertion.AssertEquals("XmlDocument", base64, Encoding.UTF8.GetString (output));
		}

		[Test]
		public void LoadInputAsXmlNodeListFromXPath () 
		{
			XmlDocument doc = GetDoc ();
			XmlNodeList xpath = doc.SelectNodes ("//.");
			Assertion.AssertEquals("XPathNodeList.Count", 3, xpath.Count);
			transform.LoadInput (xpath);
			Stream s = (Stream) transform.GetOutput ();
			byte[] output = Stream2Array (s);
			Assertion.AssertEquals("XPathNodeList", base64, Encoding.UTF8.GetString (output));
		}

		[Test]
		[Ignore ("LAMESPEC or BUG but this returns nothing with MS implementation ???")]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			Stream s = (Stream) transform.GetOutput ();
			byte[] output = Stream2Array (s);
			Assertion.AssertEquals("XmlChildNodes", null, Encoding.UTF8.GetString (output));
		}

		[Test]
		public void LoadInputAsStream () 
		{
			string base64 = "XmlDsigBase64Transform";
			byte[] base64array = Encoding.UTF8.GetBytes (base64);

			MemoryStream ms = new MemoryStream ();
			byte[] x = Encoding.UTF8.GetBytes (Convert.ToBase64String (base64array));
			ms.Write (x, 0, x.Length);
			ms.Position = 0;
			transform.LoadInput (ms);
			Stream s = (Stream) transform.GetOutput ();
			byte[] output = Stream2Array (s);
			Assertion.AssertEquals("MemoryStream", base64, Encoding.UTF8.GetString (output));
		}

		[Test]
		public void LoadInputWithUnsupportedType () 
		{
			byte[] bad = { 0xBA, 0xD };
			// LAMESPEC: input MUST be one of InputType - but no exception is thrown (not documented)
			transform.LoadInput (bad);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnsupportedOutput () 
		{
			XmlDocument doc = new XmlDocument();
			object o = transform.GetOutput (doc.GetType ());
		}
	}
}