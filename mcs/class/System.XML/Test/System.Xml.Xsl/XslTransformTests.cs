//
// System.Xml.Serialization.XmlSerializer.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using System.Xml.Xsl;
using NUnit.Framework;

namespace MonoTests.System.Xml.Xsl
{
	[TestFixture]
	public class XslTransformTests
	{
		XmlDocument doc;
		XslTransform xslt;
		XmlDocument result;

		[SetUp]
		public void GetReady()
		{
			doc = new XmlDocument ();
			xslt = new XslTransform ();
			result = new XmlDocument ();
		}

		[Test]
		public void TestBasicTransform ()
		{
			doc.LoadXml ("<root/>");
			xslt.Load ("XmlFiles/xsl/empty.xsl");
			xslt.Transform ("XmlFiles/xsl/empty.xsl", "XmlFiles/xsl/result.xml");
			result.Load ("XmlFiles/xsl/result.xml");
			Assertion.AssertEquals ("count", 1, result.ChildNodes.Count);
		}
	}
}
