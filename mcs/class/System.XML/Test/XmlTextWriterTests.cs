//
// System.Xml.XmlTextWriterTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlTextWriterTests : TestCase
	{
		public XmlTextWriterTests () : base ("Ximian.Mono.Tests.XmlTextWriterTests testsuite") {}
		public XmlTextWriterTests (string name) : base (name) {}

		StringWriter sw;
		XmlTextWriter xtw;

		protected override void SetUp ()
		{
			sw = new StringWriter();
			xtw = new XmlTextWriter(sw);
		}

		public void TestCData ()
		{
			xtw.WriteCData("foo");
			AssertEquals("WriteCData had incorrect output.", sw.GetStringBuilder().ToString(), "<![CDATA[foo]]>");
		}

		public void TestComment ()
		{
			xtw.WriteComment("foo");
			AssertEquals("WriteComment had incorrect output.", sw.GetStringBuilder().ToString(), "<!--foo-->");
		}
	}
}
