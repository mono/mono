//
// Tests for System.Web.UI.WebControls.Adapters.HideDisabledControlAdapter
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
	public class HideDisabledControlAdapterTest
	{
		[Test]
		public void Render ()
		{
			WebControl parent = new MyWebControl();
			MyWebControl c = new MyWebControl ();
			SystemWebTestShim.HideDisabledControlAdapter a = new SystemWebTestShim.HideDisabledControlAdapter (c);
			StringWriter sw;
			HtmlTextWriter w;

			sw = new StringWriter();
			w = new HtmlTextWriter(sw);
			a.Render (w);
			Assert.AreEqual ("RenderBeginTag\nRenderContents\nRenderEndTag\n", sw.ToString().Replace ("\r", ""), "Render #1");
			
			
			sw = new StringWriter();
			w = new HtmlTextWriter(sw);
			c.Enabled = false;
			a.Render (w);			
			Assert.AreEqual ("", sw.ToString(), "Render #2");
			
			sw = new StringWriter();
			w = new HtmlTextWriter(sw);
			parent.Enabled = false;
			c.Enabled = true;
			parent.Controls.Add(c);
			a.Render (w);			
			Assert.AreEqual ("", sw.ToString(), "Render #3");
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
#endregion
	}
}
