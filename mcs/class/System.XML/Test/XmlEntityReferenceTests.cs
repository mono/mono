//
// System.Xml.XmlEntityReference.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlEntityReferenceTests : TestCase
	{
		public XmlEntityReferenceTests () : base ("MonoTests.System.Xml.XmlEntityReferenceTests testsuite") {}
		public XmlEntityReferenceTests (string name) : base (name) {}

		protected override void SetUp ()
		{
		}

		public void TestWriteTo ()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml("<root/>");
			XmlEntityReference er = doc.CreateEntityReference("foo");
			doc.DocumentElement.AppendChild(er);
			AssertEquals ("Name", "foo", er.Name);
			AssertEquals ("WriteTo", "<root>&foo;</root>", doc.DocumentElement.OuterXml);
		}
	}
}
