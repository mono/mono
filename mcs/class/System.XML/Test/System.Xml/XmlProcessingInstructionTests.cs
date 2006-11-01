//
// System.Xml.XmlTextWriterTests
//
// Authors:
//   Kral Ferch <kral_ferch@hotmail.com>
//   Martin Willemoes Hansen <mwh@sysrq.dk>
//
// (C) 2002 Kral Ferch
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlProcessingInstructionTests : Assertion
	{
		XmlDocument document;
		XmlProcessingInstruction pi;

		[SetUp]
		public void GetReady ()
		{
			document = new XmlDocument ();
		}

		[Test]
		public void InnerAndOuterXml ()
		{
			pi = document.CreateProcessingInstruction ("foo", "bar");
			AssertEquals (String.Empty, pi.InnerXml);
			AssertEquals ("<?foo bar?>", pi.OuterXml);
		}
	}
}
