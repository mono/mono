//
// XmlDsigBase64TransformTest.cs - NUnit Test Cases for XmlDsigBase64Transform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class XmlDsigBase64TransformTest : TestCase {

	public XmlDsigBase64TransformTest () : base ("System.Security.Cryptography.Xml.XmlDsigBase64Transform testsuite") {}
	public XmlDsigBase64TransformTest (string name) : base (name) {}

	protected XmlDsigBase64Transform transform;

	protected override void SetUp () 
	{
		transform = new XmlDsigBase64Transform ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (XmlDsigBase64TransformTest)); 
		}
	}

	public void TestProperties () 
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

	public void TestLoadInput () 
	{
		string base64 = "XmlDsigBase64Transform";
		UTF8Encoding utf8 = new UTF8Encoding ();
		byte[] base64array = utf8.GetBytes (base64);
		string xml = "<Test>" + Convert.ToBase64String (base64array) + "</Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (xml);

		// load as XmlDocument
		transform.LoadInput (doc);
		Stream s = (Stream) transform.GetOutput ();
		byte[] output = Stream2Array (s);
		AssertEquals("XmlDocument", base64, utf8.GetString (output));

		// load as XmlNodeList
		XmlNodeList xpath = doc.SelectNodes ("//.");
		transform.LoadInput (xpath);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);
		// works with MS ??? why does xpath return 3 nodes ???
		// AssertEquals("XPathNodeList", base64, utf8.GetString (output));

		// load as XmlNodeList 
		transform.LoadInput (doc.ChildNodes);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);
		// FIXME: works with Mono ??? why doesn't this works with MS ???
		AssertEquals("XmlChildNodes", base64, utf8.GetString (output));

		// load as Stream
		MemoryStream ms = new MemoryStream ();
		byte[] x = utf8.GetBytes (Convert.ToBase64String (base64array));
		ms.Write (x, 0, x.Length);
		ms.Position = 0;
		transform.LoadInput (ms);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);
		AssertEquals("MemoryStream", base64, utf8.GetString (output));
	}

	public void TestUnsupportedInput () 
	{
		byte[] bad = { 0xBA, 0xD };
		// LAMESPEC: input MUST be one of InputType - but no exception is thrown (not documented)
		transform.LoadInput (bad);
	}

	public void TestUnsupportedOutput () 
	{
		try {
			XmlDocument doc = new XmlDocument();
			object o = transform.GetOutput (doc.GetType ());
			Fail ("Expected ArgumentException but got none");
		}
		catch (ArgumentException) {
			// this is what we expected
		}
		catch (Exception e) {
			Fail ("Expected ArgumentException but got: " + e.ToString ());
		}
	}
}

}