//
// System.Xml.XmlTextWriterTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlTextTests : TestCase
	{
		public XmlTextTests () : base ("MonoTests.System.Xml.XmlTextTests testsuite") {}
		public XmlTextTests (string name) : base (name) {}

		XmlDocument document;
		XmlText text;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}

		public void TestInnerAndOuterXml ()
		{
			text = document.CreateTextNode ("&<>\"'");
			AssertEquals (String.Empty, text.InnerXml);
			AssertEquals ("&amp;&lt;&gt;\"'", text.OuterXml);
		}
	}
}
