//
// XmlDsigXsltTransformTest.cs - NUnit Test Cases for XmlDsigXsltTransform
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

public class XmlDsigXsltTransformTest : TestCase {

	public XmlDsigXsltTransformTest () : base ("System.Security.Cryptography.Xml.XmlDsigXsltTransform testsuite") {}
	public XmlDsigXsltTransformTest (string name) : base (name) {}

	protected XmlDsigXsltTransform transform;

	protected override void SetUp () 
	{
		transform = new XmlDsigXsltTransform ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (XmlDsigXsltTransformTest)); 
		}
	}

	public void TestProperties () 
	{
		AssertEquals ("Algorithm", "http://www.w3.org/TR/1999/REC-xslt-19991116", transform.Algorithm);

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

	private string Stream2Array (Stream s) 
	{
		StringBuilder sb = new StringBuilder ();
		int b = s.ReadByte ();
		while (b != -1) {
			sb.Append (b.ToString("X2"));
			b = s.ReadByte ();
		}
		return sb.ToString ();
	}

	public void Test () 
	{
		string test = "<Test>XmlDsigXsltTransform</Test>";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (test);

		transform.LoadInnerXml (doc.ChildNodes);
		Stream s = (Stream) transform.GetOutput ();
		string output = Stream2Array (s);

		// load as XmlDocument
		transform.LoadInput (doc);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);

		// load as XmlNodeList
		transform.LoadInput (doc.ChildNodes);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);

		// load as Stream
		MemoryStream ms = new MemoryStream ();
		doc.Save (ms);
		ms.Position = 0;
		transform.LoadInput (ms);
		s = (Stream) transform.GetOutput ();
		output = Stream2Array (s);
	}

	protected void AssertEquals (string msg, XmlNodeList expected, XmlNodeList actual) 
	{
		for (int i=0; i < expected.Count; i++) {
			if (expected[i].OuterXml != actual[i].OuterXml) {
				Fail (msg + " [" + i + "] expected " + expected[i].OuterXml + " bug got " + actual[i].OuterXml);
			}
		}
	}

	public void TestLoadInnerXml () 
	{
		string value = "<Transform Algorithm=\"http://www.w3.org/TR/1999/REC-xslt-19991116\" />";
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (value);

		transform.LoadInnerXml (doc.ChildNodes);
		// note: GetInnerXml is protected so we can't AssertEquals :-(
		// unless we use reflection (making ti a lot more complicated)
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