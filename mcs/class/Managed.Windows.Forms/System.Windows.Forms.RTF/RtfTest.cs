//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//      Jackson Harper (jackson@ximian.com)
//

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.RTF;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class RtfTest
	{
		// class that converts chunks of RTF into HTML
		internal class RtfToHtml {

			private RTF parser;
			private StringBuilder text_buffer;

			public RtfToHtml (RTF parser)
			{
				this.parser = parser;

				parser.ClassCallback [TokenClass.Text] = new ClassDelegate (HandleText);
				parser.ClassCallback [TokenClass.Control] = new ClassDelegate (HandleControl);

				text_buffer = new StringBuilder ();
			}

			public void Run ()
			{
				parser.Read ();
			}

			public string GetText ()
			{
				return text_buffer.ToString ();
			}

			private void HandleText (RTF rtf)
			{
				text_buffer.Append (rtf.EncodedText);
			}

			private void HandleControl (RTF rtf)
			{
				switch (rtf.Major) {
				case Major.CharAttr:
					switch (rtf.Minor) {
					case Minor.Bold:
						text_buffer.Append (rtf.Param == RTF.NoParam ? "<b>" : "</b>");
						break;
					case Minor.Italic:
						text_buffer.Append (rtf.Param == RTF.NoParam ? "<i>" : "</i>");
						break;
					case Minor.StrikeThru:
						text_buffer.Append (rtf.Param == RTF.NoParam ? "<s>" : "</s>");
						break;
					}
					break;
			        case Major.SpecialChar:
					switch (rtf.Minor) {
					case Minor.Par:
						text_buffer.Append ("<p>");
						break;
					}
					break;
				}
			}
		}

		[Test]
		public void TestEmptyDoc ()
		{
			RTF parser = new RTF (TextStream ("{\\rtf1}"));
			RtfToHtml r = new RtfToHtml (parser);

			r.Run ();

			Assert.AreEqual (String.Empty, r.GetText (), "emptydoc-1");
		}

		[Test]
		public void TestSimpleDoc1 ()
		{
			Assert.AreEqual ("text", ParsedText ("{\\rtf1 text}"), "simpledoc1-1");
			Assert.AreEqual ("text", ParsedText ("{\\rtf1\ntext}"), "simpledoc1-2");
			Assert.AreEqual ("text", ParsedText ("\\rtf1\ntext\n}"), "simpledoc1-3");
			Assert.AreEqual ("text", ParsedText ("\\rtf1\n text}"), "simpledoc1-4");
			Assert.AreEqual ("text ", ParsedText ("\\rtf1\n text \n}"), "simpledoc1-5");
			Assert.AreEqual ("text ", ParsedText ("\\rtf1\r\n text \r\n}"), "simpledoc1-6");
			Assert.AreEqual ("text ", ParsedText ("\\rtf1\n\n\n text \n\n\n}"), "simpledoc1-7");
		}

		[Test]
		public void TestSimpleParagraphs ()
		{
			Assert.AreEqual ("<p>", ParsedText ("\\rtf1\\par}"), "simplepar-1");
			Assert.AreEqual ("<p><p>", ParsedText ("\\rtf1\\par\\par}"), "simplepar-2");
			Assert.AreEqual (String.Empty, ParsedText ("\\rtf1 \\partext}"), "simplepar-3");
			Assert.AreEqual ("<p>text", ParsedText ("\\rtf1 \\par text}"), "simplepar-4");
			Assert.AreEqual ("<p>text<p>", ParsedText ("\\rtf1 \\par text\\par}"), "simplepar-5");
		}

		[Test]
		public void TestSimpleBold ()
		{
			Assert.AreEqual ("<b>text", ParsedText ("{\\rtf1 {\\b text}}"), "simplebold-1");
			Assert.AreEqual ("<b>text</b>", ParsedText ("{\\rtf1 \\b text\\b0}"), "simplebold-2");
			Assert.AreEqual ("<b>text</b>", ParsedText ("{\\rtf1 \\b text\\b0}"), "simplebold-3");
			Assert.AreEqual ("<b>text </b>", ParsedText ("{\\rtf1 \\b text \\b0}"), "simplebold-4");
			Assert.AreEqual ("<b><b>text", ParsedText ("{\\rtf1 \\b\\b text}"), "simplebold-5");
			Assert.AreEqual ("<b><b>text</b></b>", ParsedText ("{\\rtf1 \\b\\b text\\b0\\b0}"), "simplebold-6");
		}

		[Test]
		public void TestSimpleItalic ()
		{
			Assert.AreEqual ("<i>text", ParsedText ("{\\rtf1 {\\i text}}"), "simpleitalic-1");
			Assert.AreEqual ("<i>text</i>", ParsedText ("{\\rtf1 \\i text\\i0}"), "simpleitalic-2");
			Assert.AreEqual ("<i>text</i>", ParsedText ("{\\rtf1 \\i text\\i0}"), "simpleitalic-3");
			Assert.AreEqual ("<i>text </i>", ParsedText ("{\\rtf1 \\i text \\i0}"), "simpleitalic-4");
			Assert.AreEqual ("<i><i>text", ParsedText ("{\\rtf1 \\i\\i text}"), "simpleitalic-5");
			Assert.AreEqual ("<i><i>text</i></i>", ParsedText ("{\\rtf1 \\i\\i text\\i0\\i0}"), "simpleitalic-6");
		}

		[Test]
		public void TestSimpleStrikeThru ()
		{
			Assert.AreEqual ("<s>text", ParsedText ("{\\rtf1 {\\strike text}}"), "simplestrike-1");
			Assert.AreEqual ("<s>text</s>", ParsedText ("{\\rtf1 \\strike text\\strike0}"), "simplestrike-2");
			Assert.AreEqual ("<s>text</s>", ParsedText ("{\\rtf1 \\strike text\\strike0}"), "simplestrike-3");
			Assert.AreEqual ("<s>text </s>", ParsedText ("{\\rtf1 \\strike text \\strike0}"), "simplestrike-4");
			Assert.AreEqual ("<s><s>text", ParsedText ("{\\rtf1 \\strike\\strike text}"), "simplestrike-5");
			Assert.AreEqual ("<s><s>text</s></s>", ParsedText ("{\\rtf1 \\strike\\strike text\\strike0\\strike0}"), "simplestrike-6");
		}

		private string ParsedText (string text)
		{
			RTF parser = new RTF (TextStream (text));
			RtfToHtml r = new RtfToHtml (parser);

			r.Run ();

			return r.GetText ();
		}

		private MemoryStream TextStream (string text)
		{
			MemoryStream res = new MemoryStream ();
			StreamWriter writer = new StreamWriter (res);

			writer.Write (text);
			writer.Flush ();

			res.Seek (0, SeekOrigin.Begin);
			return res;
		}
	}
}


