//
// XmlDsigC14NWithCommentsTransformTest.cs 
//	- NUnit Test Cases for XmlDsigC14NWithCommentsTransform
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class XmlDsigC14NWithCommentsTransformTest {

		protected XmlDsigC14NWithCommentsTransform transform;

		[SetUp]
		protected void SetUp () 
		{
			transform = new XmlDsigC14NWithCommentsTransform ();
		}

		[Test]
		public void Properties () 
		{
			Assertion.AssertEquals ("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", transform.Algorithm);

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