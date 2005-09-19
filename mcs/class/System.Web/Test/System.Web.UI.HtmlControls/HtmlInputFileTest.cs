//
// HtmlInputFileTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputFile
//
// Author:
//	Chris Toshok	(toshok@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
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

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlInputFilePoker : HtmlInputFile {

		public HtmlInputFilePoker ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public void DoRenderAttributes (HtmlTextWriter writer)
		{
			RenderAttributes (writer);
		}
	}

	[TestFixture]
	public class HtmlInputFileTest {

		[Test]
		public void Defaults ()
		{
			HtmlInputFilePoker p = new HtmlInputFilePoker ();

			/* MS throws a null exception on both
			 * get_PostedFile and get_Value in this test,
			 * which makes me think (in the PostedFile
			 * case at least) they're directly accessing
			 * Page.Request.Files (which our test doesn't
			 * support) */

			Assert.AreEqual ("", p.Accept, "A1");
			Assert.AreEqual (-1, p.MaxLength, "A2");
			//Assert.IsNull (p.PostedFile, "A3");
			Assert.AreEqual (-1, p.Size, "A4");
			//Assert.AreEqual ("", p.Value, "A5");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ValueSetter ()
		{
			HtmlInputFilePoker p = new HtmlInputFilePoker ();
			p.Value = "/etc/passwd";
		}

		[Test]
		public void Attribute_Count ()
		{
			HtmlInputFilePoker p = new HtmlInputFilePoker ();

			p.Accept = "*.*";
			p.MaxLength = 50;
			p.Size = 20;

			Assert.AreEqual (4, p.Attributes.Count, "A1");
		}

#if false
		[Test]
		public void ViewState ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
#if NET_2_0
			p.CausesValidation = false;
			p.ValidationGroup = "VG";
#endif
			object s = p.SaveState();
			HtmlInputButtonPoker copy = new HtmlInputButtonPoker ();
			copy.LoadState (s);

#if NET_2_0
			Assert.IsFalse (copy.CausesValidation, "A1");
			Assert.AreEqual ("VG", p.ValidationGroup, "A2");
#endif
		}
#endif

		[Test]
		public void RenderAttributes ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputFilePoker p = new HtmlInputFilePoker ();

			p.DoRenderAttributes (tw);
			Assert.AreEqual (" name type=\"file\" /", sw.ToString (), "A1");
		}
	}	
}

