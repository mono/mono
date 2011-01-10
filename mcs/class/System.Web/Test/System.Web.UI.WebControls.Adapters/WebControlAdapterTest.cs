//
// Tests for System.Web.UI.WebControls.Adapters.WebControlAdapter
//
// Author:
//	Dean Brettle (dean@brettle.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;
using System.Web.Configuration;
using MonoTests.SystemWeb.Framework;


namespace MonoTests.System.Web.UI.WebControls.Adapters
{
	[TestFixture]
	public class WebControlAdapterTest
	{
		MyWebControl c;
		MyWebControlAdapter a;
		StringWriter sw;
		HtmlTextWriter w;

		[SetUp]
		public void SetUp ()
		{
			c = new MyWebControl ();
			a = new MyWebControlAdapter (c);
			sw = new StringWriter ();
			w = new HtmlTextWriter (sw);			
		}
		
		[Test]
		public void RenderBeginTag ()
		{
			a.RenderBeginTag (w);
			Assert.AreEqual ("RenderBeginTag\n", sw.ToString (), "RenderBeginTag #1");
		}

		[Test]
		public void RenderContentsTag ()
		{
			a.RenderContents (w);
			Assert.AreEqual ("RenderContents\n", sw.ToString (), "RenderContents #1");
		}

		[Test]
		public void RenderEndTag ()
		{
			a.RenderEndTag (w);
			Assert.AreEqual ("RenderEndTag\n", sw.ToString (), "RenderEndTag #1");
		}

		[Test]
		public void Render ()
		{
			a.Render (w);
			Assert.AreEqual ("RenderBeginTag\nRenderContents\nRenderEndTag\n", sw.ToString (), "Render #1");
		}
		
		[Test]
		public void Control ()
		{
			Assert.AreEqual (c, a.Control, "Control #1");
		}
		
		[Test]
		public void IsEnabled ()
		{
			MyWebControl parent = new MyWebControl ();
			parent.Controls.Add (c);
			Assert.IsTrue (a.IsEnabled, "IsEnabled #1");
			parent.Enabled = false;
			Assert.IsFalse (a.IsEnabled, "IsEnabled #2");
			parent.Enabled = true;
			c.Enabled = false;
			Assert.IsFalse (a.IsEnabled, "IsEnabled #3");
		}
		
#region Support classes
		
		class MyWebControl : WebControl
		{			
			public override void RenderBeginTag (HtmlTextWriter w)
			{
				w.WriteLine("RenderBeginTag");
			}

			protected internal override void RenderContents (HtmlTextWriter w)
			{
				w.WriteLine("RenderContents");
			}

			public override void RenderEndTag (HtmlTextWriter w)
			{
				w.WriteLine("RenderEndTag");
			}

		}

		class MyWebControlAdapter : SystemWebTestShim.WebControlAdapter
		{
			internal MyWebControlAdapter (WebControl wc) : base (wc)
			{
			}
			
			new internal void RenderBeginTag (HtmlTextWriter w)
			{
				base.RenderBeginTag(w);
			}

			new internal void RenderContents (HtmlTextWriter w)
			{
				base.RenderContents(w);
			}

			new internal void RenderEndTag (HtmlTextWriter w)
			{
				base.RenderEndTag(w);
			}

			new internal void Render (HtmlTextWriter w)
			{
				base.Render(w);
			}
			
			new internal WebControl Control {
				get { return base.Control; }
			}
			
			new internal bool IsEnabled {
				get { return base.IsEnabled; }
			}
		}
#endregion
	}
}
#endif
