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
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
		}

		public void TestCData ()
		{
			xtw.WriteCData ("foo");
			AssertEquals ("WriteCData had incorrect output.", sw.GetStringBuilder().ToString(), "<![CDATA[foo]]>");
		}

		public void TestComment ()
		{
			xtw.WriteComment ("foo");
			AssertEquals ("WriteComment had incorrect output.", "<!--foo-->", sw.GetStringBuilder().ToString());
		}

		public void TestElementEmpty ()
		{
			xtw.WriteStartElement ("foo");
			xtw.WriteEndElement ();
			AssertEquals ("Incorrect output.", "<foo />", sw.GetStringBuilder().ToString());
		}

		public void TestElementWriteElementString ()
		{
			xtw.WriteElementString ("foo", "bar");
			AssertEquals ("WriteElementString has incorrect output.", "<foo>bar</foo>", sw.GetStringBuilder().ToString());
		}

		public void TestProcessingInstructionInvalid ()
		{
			try {
				xtw.WriteProcessingInstruction("fo?>o", "bar");
				Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }

			try {
				xtw.WriteProcessingInstruction("foo", "ba?>r");
				Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }

			try {
				xtw.WriteProcessingInstruction("", "bar");
				Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }

			try {
				xtw.WriteProcessingInstruction(null, "bar");
				Fail("Should have thrown an ArgumentException.");
			} catch (ArgumentException) { }
		}
	}
}
