//
// XmlDsigBase64TransformTest.cs - NUnit Test Cases for XmlDsigBase64Transform
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	// Note: GetInnerXml is protected in XmlDsigBase64Transform making it
	// difficult to test properly. This class "open it up" :-)
	public class UnprotectedXmlDsigBase64Transform : XmlDsigBase64Transform {

		public XmlNodeList UnprotectedGetInnerXml () {
			return base.GetInnerXml ();
		}
	}

	[TestFixture]
	public class XmlDsigBase64TransformTest : Assertion {

		protected UnprotectedXmlDsigBase64Transform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new UnprotectedXmlDsigBase64Transform ();
			Type t = typeof (XmlDsigBase64Transform);
		}

		[Test]
		public void Properties () 
		{
			AssertEquals ("Algorithm", "http://www.w3.org/2000/09/xmldsig#base64", transform.Algorithm);

			Type[] input = transform.InputTypes;
			Assert ("Input #", (input.Length == 3));
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
			Assert ("Input Stream", istream);
			Assert ("Input XmlDocument", ixmldoc);
			Assert ("Input XmlNodeList", ixmlnl);

			Type[] output = transform.OutputTypes;
			Assert ("Output #", (output.Length == 1));
			// check presence of every supported output types
			bool ostream = false;
			foreach (Type t in input) {
				if (t.ToString () == "System.IO.Stream")
					ostream = true;
			}
			Assert ("Output Stream", ostream);
		}

		[Test]
		public void Types ()
		{
			Type [] input = transform.InputTypes;
			input [0] = null;
			input [1] = null;
			input [2] = null;
			// property does not return a clone
			foreach (Type t in transform.InputTypes) {
				AssertNull (t);
			}
			// it's not a static array
			XmlDsigBase64Transform t2 = new XmlDsigBase64Transform ();
			foreach (Type t in t2.InputTypes) {
				AssertNotNull (t);
			}
		}

		[Test]
		public void GetInnerXml () 
		{
			XmlNodeList xnl = transform.UnprotectedGetInnerXml ();
			AssertNull ("Default InnerXml", xnl);
		}

		private string Stream2String (Stream s) 
		{
			StreamReader sr = new StreamReader (s);
			return sr.ReadToEnd ();
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
			string output = Stream2String (s);
			AssertEquals("XmlDocument", base64, output);
		}

		[Test]
		public void LoadInputAsXmlNodeListFromXPath () 
		{
			XmlDocument doc = GetDoc ();
			XmlNodeList xpath = doc.SelectNodes ("//.");
			AssertEquals("XPathNodeList.Count", 3, xpath.Count);
			transform.LoadInput (xpath);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2String (s);
			AssertEquals ("XPathNodeList", base64, output);
		}

		[Test]
		public void LoadInputAsXmlNodeList () 
		{
			XmlDocument doc = GetDoc ();
			transform.LoadInput (doc.ChildNodes);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2String (s);
			// Note that ChildNodes does not contain the text node.
			AssertEquals ("XmlChildNodes", String.Empty, output);
		}

		[Test]
		public void LoadInputAsStream () 
		{
			MemoryStream ms = new MemoryStream ();
			byte[] x = Encoding.UTF8.GetBytes (Convert.ToBase64String (base64array));
			ms.Write (x, 0, x.Length);
			ms.Position = 0;
			transform.LoadInput (ms);
			Stream s = (Stream) transform.GetOutput ();
			string output = Stream2String (s);
			AssertEquals ("MemoryStream", base64, output);
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
