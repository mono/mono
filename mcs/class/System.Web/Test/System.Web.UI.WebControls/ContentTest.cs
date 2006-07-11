//
// Tests for System.Web.UI.WebControls.ContentTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
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


#if NET_2_0


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{


	class PokerContent : Content
	{
		public PokerContent ()
		{
			TrackViewState ();
		}
		public void DoOnDataBinding ()
		{
			base.OnDataBinding(new EventArgs()) ;
		}
		public void DoOnInit ()
		{
			base.OnInit (new EventArgs ());
		}
		public void DoOnLoad ()
		{
			base.OnLoad (new EventArgs ());
		}
		public void DoOnPreRender ()
		{
			base.OnPreRender (new EventArgs ());
		}
		public void DoOnUnLoad ()
		{
			base.OnUnload (new EventArgs ());
		}
	}

	
	[TestFixture]
	public class ContentTest
	{
		[Test]
		public void Content_DefaultProperty ()
		{
			PokerContent pc = new PokerContent();
			Assert.AreEqual (String.Empty, pc.ContentPlaceHolderID, "ContentPlaceHolderID");
		}

		private bool OnDataBinding;
		private bool OnInit;
		private bool OnLoad;
		private bool OnPreRender;
		private bool OnUnLoad;

		private void OnUnLoadHendler (Object sender, EventArgs args)
		{
			OnUnLoad = true;
		}

		private void OnPreRenderHendler (Object sender, EventArgs args)
		{
			OnPreRender = true;
		}

		private void OnLoadHandler (Object sender, EventArgs args)
		{
			OnLoad = true;
		}

		private void OnDataBindingHandler (Object sender, EventArgs args)
		{
			OnDataBinding = true;
		}
		private void OnInitHandler (Object sender, EventArgs args)
		{
			OnInit = true;
		}

		[Test]
		public void Events_DataBinding ()
		{
			PokerContent pc = new PokerContent ();
			pc.DataBinding += new EventHandler (OnDataBindingHandler);
			// DataBiding event
			Assert.AreEqual (false, OnDataBinding, "BeforeDataBinding");
			pc.DoOnDataBinding ();
			Assert.AreEqual (true, OnDataBinding, "AfterDataBinding");
		}

		[Test]
		public void Events_Init ()
		{
			PokerContent pc = new PokerContent ();
			pc.Init += new EventHandler (OnInitHandler);
			// Init event
			Assert.AreEqual (false, OnInit, "BeforeInit");
			pc.DoOnInit ();
			Assert.AreEqual (true, OnInit, "AfterInit");
		}

		[Test]
		public void Events_PreRender ()
		{
			PokerContent pc = new PokerContent ();
			pc.PreRender += new EventHandler (OnPreRenderHendler);
			// PreRender event
			Assert.AreEqual (false, OnPreRender, "BeforePreRender");
			pc.DoOnPreRender ();
			Assert.AreEqual (true, OnPreRender, "AfterPreRender");
		}

		[Test]
		public void Events_Load ()
		{
			PokerContent pc = new PokerContent ();
			pc.Load += new EventHandler (OnLoadHandler);
			// Load event
			Assert.AreEqual (false, OnLoad, "BeforeLoad");
			pc.DoOnLoad ();
			Assert.AreEqual (true, OnLoad, "AfterLoad");
		}

		[Test]
		public void Events_Unload ()
		{
			PokerContent pc = new PokerContent ();
			pc.Unload += new EventHandler (OnUnLoadHendler);
			// Unload event
			Assert.AreEqual (false, OnUnLoad, "BeforeUnLoad");
			pc.DoOnUnLoad ();
			Assert.AreEqual (true, OnUnLoad, "AfterUnLoad");
		}
			
		[Test]
		[ExpectedException (typeof(NotSupportedException))]
		public void Content_PropertyExeption ()
		{
			PokerContent pc = new PokerContent ();
			pc.ContentPlaceHolderID = "fake";
		}

	}
}
#endif
