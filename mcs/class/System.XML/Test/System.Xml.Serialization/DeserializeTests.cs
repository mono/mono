//
// System.Xml.DeserializationTests
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//
//
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class Sample
	{
		public string Text;
		public string [] ArrayText;
	}

	[TestFixture]
	public class DeserializationTests
	{
		object result;

		private void Deserialize (Type t, string xml)
		{
			StringReader sr = new StringReader (xml);
			XmlReader xr = new XmlTextReader (sr);
			Deserialize (t, xr);
		}

		private void Deserialize (Type t, XmlReader xr)
		{
			XmlSerializer ser = new XmlSerializer (t);
			result = ser.Deserialize (xr);
		}

		[Test]
		public void SimpleDeserialize ()
		{
			Deserialize (typeof (Sample), "<Sample><Text>Test.</Text></Sample>");
			Assertion.AssertEquals (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assertion.AssertEquals ("Test.", sample.Text);
		}

		[Test]
		public void DeserializeInt ()
		{
			Deserialize (typeof (int), "<int>10</int>");
			Assertion.AssertEquals (typeof (int), result.GetType ());
			Assertion.AssertEquals (10, result);
		}

		[Test]
		public void DeserializeSimpleArray ()
		{
			Deserialize (typeof (Sample), "<Sample><ArrayText><string>Test1</string><string>Test2</string></ArrayText></Sample>");
			Assertion.AssertEquals (typeof (Sample), result.GetType ());
			Sample sample = result as Sample;
			Assertion.AssertEquals ("Test1", sample.ArrayText [0]);
			Assertion.AssertEquals ("Test2", sample.ArrayText [1]);
		}

	}
}
