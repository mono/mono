//
// Tests for System.Web.UI.WebControls.Adapters.MenuAdapter
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
	public class MenuAdapterTest
	{
		MyMenu c;
		MyMenuAdapter a;
		StringWriter sw;
		HtmlTextWriter w;
		Page p;
		EventArgs e;

		[SetUp]
		public void SetUp ()
		{
			p = new Page ();
			c = new MyMenu ();
			c.RenderingMode = MenuRenderingMode.Table;
			a = new MyMenuAdapter (c);
			p.Controls.Add(c);
			sw = new StringWriter ();
			w = new HtmlTextWriter (sw);
			e = new EventArgs();
		}
		
		[Test]
		public void OnInit ()
		{
			a.OnInit (e);
			Assert.IsTrue (p.RequiresControlState (c), "OnInit #1");
			Assert.AreEqual (e, c.on_init_arg, "OnInit #2");
		}

		[Test]
		public void OnPreRender ()
		{
			a.OnPreRender (e);
			Assert.AreEqual (e, c.on_pre_render_arg, "OnPreRender #1");
		}

		[Test]
		public void RaisePostBackEvent ()
		{
			((IPostBackEventHandler)a).RaisePostBackEvent ("eventArg");
			Assert.AreEqual ("eventArg", c.raise_post_back_event_arg, "RaisePostBackEvent #1");
		}
		
		[Test]
		public void RenderBeginTag ()
		{
			a.RenderBeginTag (w);
			Assert.AreEqual ("RenderBeginTag\n", sw.ToString ().Replace ("\r", ""), "RenderBeginTag #1");
		}

		[Test]
		public void RenderContentsTag ()
		{
			a.RenderContents (w);
			Assert.AreEqual ("RenderContents\n", sw.ToString ().Replace ("\r", ""), "RenderContents #1");
		}

		[Test]
		public void RenderEndTag ()
		{
			a.RenderEndTag (w);
			Assert.AreEqual ("RenderEndTag\n", sw.ToString ().Replace ("\r", ""), "RenderEndTag #1");
		}

		[Test]
		public void RenderItem ()
		{
			MenuItem item = new MenuItem("menu item text");

			// This has to stay to work around event validation errors. If it's removed,
			// then RenderItem will eventually attempt to register for event validation,
			// which can only be done during the Render phase.
			item.NavigateUrl = "http://example.com/";
			a.RenderItem (w, item, 0);
			Assert.IsTrue (sw.ToString ().IndexOf("menu item text") != -1, "RenderItem #1");
		}

		[Test]
		public void Control ()
		{
			Assert.AreEqual (c, a.Control, "Control #1");
		}
		


#region Support classes
		
		class MyMenu : Menu
		{
		
			internal EventArgs on_init_arg;
			protected internal override void OnInit (EventArgs e)
			{
				on_init_arg = e;
				base.OnInit (e);
			}

			internal EventArgs on_pre_render_arg;
			protected internal override void OnPreRender (EventArgs e)
			{
				on_pre_render_arg = e;
				base.OnPreRender (e);
			}

			internal string raise_post_back_event_arg;
			protected internal override void RaisePostBackEvent (string eventArgument)
			{
				raise_post_back_event_arg = eventArgument;
			}

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

		class MyMenuAdapter : SystemWebTestShim.MenuAdapter
		{
			internal MyMenuAdapter (Menu c) : base (c)
			{
			}

			internal new void OnInit (EventArgs e)
			{
				base.OnInit (e);
			}

			internal new void OnPreRender (EventArgs e)
			{
				base.OnPreRender (e);
			}

			new internal void RenderBeginTag (HtmlTextWriter w)
			{
				base.RenderBeginTag (w);
			}

			new internal void RenderContents (HtmlTextWriter w)
			{
				base.RenderContents (w);
			}

			new internal void RenderEndTag (HtmlTextWriter w)
			{
				base.RenderEndTag (w);
			}

			new internal Menu Control {
				get { return base.Control; }
			}

			internal new void RenderItem (HtmlTextWriter w, MenuItem mi, int i)
			{
				base.RenderItem (w, mi, i);
			}
		}
		
#endregion
	}
}
