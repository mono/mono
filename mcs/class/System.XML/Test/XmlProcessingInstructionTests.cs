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

namespace Ximian.Mono.Tests
{
	public class XmlProcessingInstructionTests : TestCase
	{
		public XmlProcessingInstructionTests () : base ("Ximian.Mono.Tests.XmlProcessingInstructionTests testsuite") {}
		public XmlProcessingInstructionTests (string name) : base (name) {}

		XmlDocument document;
		XmlProcessingInstruction pi;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
		}

		public void TestInnerAndOuterXml ()
		{
			pi = document.CreateProcessingInstruction ("foo", "bar");
			AssertEquals (String.Empty, pi.InnerXml);
			AssertEquals ("<?foo bar?>", pi.OuterXml);
		}
	}
}
