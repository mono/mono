//
// Tests for System.Web.UI.Adapters.ControlAdapter
//
// Author:
//	Dean Brettle (dean@brettle.com)
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Web.UI.Adapters;
using System.Web.Configuration;
using MonoTests.SystemWeb.Framework;


namespace MonoTests.System.Web.UI.Adapters
{
	[TestFixture]
	public class ControlAdapterTest
	{
		[Test (Description="Bug #517172")]
		public void CreateChildControls ()
		{
			MyControl c = new MyControl ();
			c.AdapterCallCreateChildControls ();
			Assert.IsTrue (c.create_child_controls_called, "CreateChildControls #1");
		}
		
		[Test]
		public void BeginRender ()
		{
			MyControlAdapter a = new MyControlAdapter ();
			MyHtmlTextWriter w = new MyHtmlTextWriter ();
			a.CallBeginRender (w);
			Assert.IsTrue (w.begin_render_called, "BeginRender #1");
		}
		
		[Test]
		public void EndRender ()
		{
			MyControlAdapter a = new MyControlAdapter ();
			MyHtmlTextWriter w = new MyHtmlTextWriter ();
			a.CallEndRender (w);
			Assert.IsTrue (w.end_render_called, "EndRender #1");
		}

		[Test]
		public void Render ()
		{
			MyControl c = new MyControl();
			MyHtmlTextWriter w = new MyHtmlTextWriter ();
			c.AdapterCallRender (w);
			Assert.IsTrue (c.render_called, "Render #1");
		}
		
		[Test]
		public void RenderChildren ()
		{
			MyControl c = new MyControl ();
			MyHtmlTextWriter w = new MyHtmlTextWriter ();
			c.AdapterCallRenderChildren (w);
			Assert.IsTrue (c.render_children_called, "RenderChildren #1");
		}

		[Test]
		public void OnInit ()
		{
			MyControl c = new MyControl ();
			EventArgs e = new EventArgs ();
			c.AdapterCallOnInit (e);
			Assert.AreEqual (e, c.on_init_arg, "OnInit #1");
		}

		[Test]
		public void OnLoad ()
		{
			MyControl c = new MyControl ();
			EventArgs e = new EventArgs ();
			c.AdapterCallOnLoad (e);
			Assert.AreEqual (e, c.on_load_arg, "OnLoad #1");
		}
		
		[Test]
		public void OnPreRender ()
		{
			MyControl c = new MyControl ();
			EventArgs e = new EventArgs ();
			c.AdapterCallOnPreRender (e);
			Assert.AreEqual (e, c.on_pre_render_arg, "OnPreRender #1");
		}

		[Test]
		public void OnUnload ()
		{
			MyControl c = new MyControl ();
			EventArgs e = new EventArgs ();
			c.AdapterCallOnUnload (e);
			Assert.AreEqual (e, c.on_unload_arg, "OnUnload #1");
		}
		
		[Test]
		public void Page ()
		{
			MyControl c = new MyControl ();
			c.Page = new Page ();
			c.AdapterGetPage ();
			Assert.AreEqual (c.Page, c.AdapterGetPage (), "Page #1");
		}
		
		[Test]
		public void PageAdapter ()
		{
			MyControl c = new MyControl ();
			PageAdapter pa = new MyPageAdapter ();
			c.Page = new MyPage (pa);
			c.AdapterGetPageAdapter ();
			Assert.AreEqual (c.Page.PageAdapter, c.AdapterGetPageAdapter (), "PageAdapter #1");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void Browser () 
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit (Browser_OnInit));
			t.Run ();
		}
				
		public static void Browser_OnInit (Page p) 
		{
			MyControl c = new MyControl ();
			p.Controls.Add (c);

			Assert.AreEqual (p.Request.Browser, c.AdapterGetBrowser (), "Browser #1");
		}
		
		
		[Test]
		[Category ("NunitWeb")]
		public void ConfigCapabilitiesNotCalled () 
		{
			WebTest t = new WebTest (new HandlerInvoker (ConfigCapabilitiesNotCalled_Reset));
			t.Run ();
			t = new WebTest (PageInvoker.CreateOnInit (ConfigCapabilitiesNotCalled_OnInit));
			t.Run ();
		}
		
		public static void ConfigCapabilitiesNotCalled_Reset ()
		{
#if !TARGET_DOTNET
			SystemWebTestShim.HttpCapabilitiesBase.GetConfigCapabilities_called = false;
#endif
		}
			
		private static EventHandler end_request_handler;
		
		public static void ConfigCapabilitiesNotCalled_OnInit (Page p) 
		{
			end_request_handler = new EventHandler (ConfigCapabilitiesNotCalled_EndRequest);
			HttpContext.Current.ApplicationInstance.EndRequest += end_request_handler;
		}
		
		public static void ConfigCapabilitiesNotCalled_EndRequest (object sender, EventArgs args)
		{
			// Ensure that this handler doesn't stick around for other tests.
			HttpContext.Current.ApplicationInstance.EndRequest -= end_request_handler;
		
			// GetConfigCapabilities() should not have been called because there are no
			// files in App_Browsers/.
#if !TARGET_DOTNET
			Assert.IsFalse (SystemWebTestShim.HttpCapabilitiesBase.GetConfigCapabilities_called,
				"ConfigCapabilitiesNotCalled #1");
#endif
		}
		
#region Support classes
		
		class MyPageAdapter : PageAdapter
		{
			internal MyPageAdapter () : base ()
			{
			}
		}
		
		class MyPage : Page
		{
			internal MyPage (PageAdapter pa) : base ()
			{
				page_adapter = pa;
			}
			
			private PageAdapter page_adapter;
			
			protected override ControlAdapter ResolveAdapter ()
			{
				return page_adapter;
			}
		}

		class MyControl : Control
		{
			internal bool create_child_controls_called;
			protected internal override void CreateChildControls ()
			{
				create_child_controls_called = true;
			}
			
			internal bool render_called = false;
			protected internal override void Render (HtmlTextWriter w)
			{
				render_called = true;
			}
			
			internal bool render_children_called = false;
			protected internal override void RenderChildren (HtmlTextWriter w)
			{
				render_children_called = true;
			}

			internal EventArgs on_init_arg = null;
			protected internal override void OnInit (EventArgs e)
			{
				on_init_arg = e;
			}

			internal EventArgs on_load_arg = null;
			protected internal override void OnLoad (EventArgs e)
			{
				on_load_arg = e;
			}
			
			internal EventArgs on_pre_render_arg = null;
			protected internal override void OnPreRender (EventArgs e)
			{
				on_pre_render_arg = e;
			}

			internal EventArgs on_unload_arg = null;
			protected internal override void OnUnload (EventArgs e)
			{
				on_unload_arg = e;
			}

			internal MyControlAdapter adapter = new MyControlAdapter ();
			protected override ControlAdapter ResolveAdapter ()
			{
				return adapter;
			}
			
			internal void AdapterCallRender (HtmlTextWriter w)
			{
				((MyControlAdapter)Adapter).CallRender (w);
			}

			internal void AdapterCallRenderChildren (HtmlTextWriter w)
			{
				((MyControlAdapter)Adapter).CallRenderChildren (w);
			}

			internal void AdapterCallCreateChildControls ()
			{
				((MyControlAdapter)Adapter).CallCreateChildControls ();
			}
			
			internal void AdapterCallOnInit (EventArgs e)
			{
				((MyControlAdapter)Adapter).CallOnInit (e);
			}

			internal void AdapterCallOnLoad (EventArgs e)
			{
				((MyControlAdapter)Adapter).CallOnLoad (e);
			}

			internal void AdapterCallOnPreRender (EventArgs e)
			{
				((MyControlAdapter)Adapter).CallOnPreRender (e);
			}

			internal void AdapterCallOnUnload (EventArgs e)
			{
				((MyControlAdapter)Adapter).CallOnUnload (e);
			}

			internal Page AdapterGetPage ()
			{
				return ((MyControlAdapter)Adapter).GetPage ();
			}

			internal PageAdapter AdapterGetPageAdapter ()
			{
				return ((MyControlAdapter)Adapter).GetPageAdapter ();
			}

			internal HttpBrowserCapabilities AdapterGetBrowser ()
			{
				return ((MyControlAdapter)Adapter).GetBrowser ();
			}
		}

		class MyControlAdapter : ControlAdapter
		{
			internal MyControlAdapter () : base ()
			{
			}

			internal void CallCreateChildControls ()
			{
				CreateChildControls ();
			}
			
			internal void CallBeginRender (HtmlTextWriter w)
			{
				BeginRender (w);
			}

			internal void CallEndRender (HtmlTextWriter w)
			{
				EndRender (w);
			}

			internal void CallRender (HtmlTextWriter w)
			{
				Render (w);
			}

			internal void CallRenderChildren (HtmlTextWriter w)
			{
				RenderChildren (w);
			}

			internal void CallOnInit (EventArgs e)
			{
				OnInit (e);
			}

			internal void CallOnLoad (EventArgs e)
			{
				OnLoad (e);
			}

			internal void CallOnPreRender (EventArgs e)
			{
				OnPreRender (e);
			}

			internal void CallOnUnload (EventArgs e)
			{
				OnUnload (e);
			}

			internal Page GetPage ()
			{
				return Page;
			}

			internal PageAdapter GetPageAdapter ()
			{
				return PageAdapter;
			}
			
			internal HttpBrowserCapabilities GetBrowser ()
			{
				return Browser;
			}
		}
		
		class MyHtmlTextWriter : HtmlTextWriter
		{
			internal MyHtmlTextWriter () : base (new StringWriter ())
			{
			}
			
			internal bool begin_render_called = false;
			public override void BeginRender ()
			{
				begin_render_called = true;
			}

			internal bool end_render_called = false;
			public override void EndRender ()
			{
				end_render_called = true;
			}
		}
#endregion
	}
}
#endif
