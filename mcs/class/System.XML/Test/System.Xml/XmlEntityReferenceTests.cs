//
// System.Xml.XmlEntityReference.cs
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Atsushi Enomoto
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlEntityReferenceTests : Assertion
	{
		[Test]
		public void WriteTo ()
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
