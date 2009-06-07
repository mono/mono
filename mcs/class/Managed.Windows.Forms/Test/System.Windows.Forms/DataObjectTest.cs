// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc.
//

#if NET_2_0

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class DataObjectTest : TestHelper
	{
		[Test]
		public void TestConvertible ()
		{
			DataObject o = new DataObject ();
			o.SetData (DataFormats.Text, false, "abc");

			Assert.AreEqual (new string [] { DataFormats.Text }, o.GetFormats (), "#01");
			Assert.AreEqual (new string [] { DataFormats.Text }, o.GetFormats (true), "#02");
			Assert.AreEqual (new string [] { DataFormats.Text }, o.GetFormats (false), "#03");

			o = new DataObject ();
			o.SetData (DataFormats.Text, true, "abc");

			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#B1");
			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#B2");
			Assert.AreEqual (new string [] { DataFormats.Text }, o.GetFormats (false), "#B3");


			o = new DataObject ();
			o.SetData (DataFormats.UnicodeText, true, "abc");

			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#C1");
			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#C2");
			Assert.AreEqual (new string [] { DataFormats.UnicodeText }, o.GetFormats (false), "#C3");

			o = new DataObject ();
			o.SetData (DataFormats.UnicodeText, false, "abc");

			Assert.AreEqual (new string [] { DataFormats.UnicodeText }, o.GetFormats (), "#D1");
			Assert.AreEqual (new string [] { DataFormats.UnicodeText}, o.GetFormats (), "#D2");
			Assert.AreEqual (new string [] { DataFormats.UnicodeText }, o.GetFormats (false), "#D3");

			o = new DataObject ();
			o.SetData (DataFormats.StringFormat, true, "abc");

			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#C1");
			Assert.AreEqual (new string [] { DataFormats.StringFormat, DataFormats.UnicodeText, DataFormats.Text }, o.GetFormats (), "#C2");
			Assert.AreEqual (new string [] { DataFormats.StringFormat }, o.GetFormats (false), "#C3");

			o = new DataObject ();
			o.SetData (DataFormats.StringFormat, false, "abc");

			Assert.AreEqual (new string [] { DataFormats.StringFormat }, o.GetFormats (), "#D1");
			Assert.AreEqual (new string [] { DataFormats.StringFormat }, o.GetFormats (), "#D2");
			Assert.AreEqual (new string [] { DataFormats.StringFormat }, o.GetFormats (false), "#D3");
		}
	
		[Test]
		public void TestAudio ()
		{
			DataObject d = new DataObject ();
			byte[] b = new byte[] { 1, 2, 3 };

			d.SetAudio (b);

			Assert.AreEqual (true, d.ContainsAudio (), "A1");
			Assert.AreEqual (false, d.ContainsFileDropList (), "A2");
			Assert.AreEqual (false, d.ContainsImage (), "A3");
			Assert.AreEqual (false, d.ContainsText (), "A4");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.CommaSeparatedValue), "A5");

			Assert.AreEqual (b.Length, d.GetAudioStream ().Length, "A6");
		}

		[Test]
		public void TestFileDrop ()
		{
			DataObject d = new DataObject ();
			StringCollection sc = new StringCollection ();
			
			sc.AddRange (new string[] {"A", "B", "C"});

			d.SetFileDropList (sc);

			Assert.AreEqual (false, d.ContainsAudio (), "A1");
			Assert.AreEqual (true, d.ContainsFileDropList (), "A2");
			Assert.AreEqual (false, d.ContainsImage (), "A3");
			Assert.AreEqual (false, d.ContainsText (), "A4");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.CommaSeparatedValue), "A5");

			Assert.AreEqual (sc.Count, d.GetFileDropList ().Count, "A6");
		}

		[Test]
		public void TestImage ()
		{
			DataObject d = new DataObject ();
			Image i = new Bitmap (16, 16);
			
			d.SetImage (i);

			Assert.AreEqual (false, d.ContainsAudio (), "A1");
			Assert.AreEqual (false, d.ContainsFileDropList (), "A2");
			Assert.AreEqual (true, d.ContainsImage (), "A3");
			Assert.AreEqual (false, d.ContainsText (), "A4");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.CommaSeparatedValue), "A5");

			Assert.AreSame (i, d.GetImage (), "A6");
		}

		[Test]
		public void TestText ()
		{
			DataObject d = new DataObject ();

			d.SetText ("yo");

			Assert.AreEqual (false, d.ContainsAudio (), "A1");
			Assert.AreEqual (false, d.ContainsFileDropList (), "A2");
			Assert.AreEqual (false, d.ContainsImage (), "A3");
			Assert.AreEqual (true, d.ContainsText (), "A4");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.CommaSeparatedValue), "A5");

			Assert.AreEqual ("yo", d.GetText (), "A6");
			Assert.AreEqual ("yo", d.GetData (DataFormats.StringFormat), "A6-1");
			
			d.SetText ("<html></html>", TextDataFormat.Html);
			Assert.AreEqual (true, d.ContainsText (), "A7");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.CommaSeparatedValue), "A8");
			Assert.AreEqual (true, d.ContainsText (TextDataFormat.Html), "A9");
			Assert.AreEqual (false, d.ContainsText (TextDataFormat.Rtf), "A10");
			Assert.AreEqual (true, d.ContainsText (TextDataFormat.Text), "A11");
			Assert.AreEqual (true, d.ContainsText (TextDataFormat.UnicodeText), "A12");

			// directly put a string
			d.SetData ("yo");

			Assert.AreEqual (true, d.ContainsText (TextDataFormat.Text), "A13");
			Assert.AreEqual (true, d.ContainsText (TextDataFormat.UnicodeText), "A14");

			Assert.AreEqual ("yo", d.GetData (DataFormats.StringFormat), "A15");
			Assert.AreEqual ("yo", d.GetData (DataFormats.Text), "A16");
			Assert.AreEqual ("yo", d.GetData (DataFormats.UnicodeText), "A17");
		}

	}
}
#endif
