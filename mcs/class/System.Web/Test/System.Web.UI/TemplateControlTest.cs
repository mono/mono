//
// Tests for System.Web.UI.WebControls.TemplateControlTest.cs
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
using MonoTests.SystemWeb.Framework;
using NUnit.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Threading;


namespace MonoTests.System.Web.UI.WebControls
{
	
	class PokerTemplateControl:TemplateControl
	{
		public PokerTemplateControl ()
		{
			TrackViewState ();
		}

		public bool DoSupportAutoEvents
		{
			get { return base.SupportAutoEvents; }
		}

		protected override void Construct ()
		{
			TemplateControlTest.eventchecker = true;
			base.Construct ();
		}

		
		public void DoOnAbortTransaction (EventArgs e)
		{
			base.OnAbortTransaction (e);
		}

		public void DoOnCommitTransaction (EventArgs e)
		{
			base.OnCommitTransaction (e);
		}

		public void DoOnError (EventArgs e)
		{
			base.OnError (e);
		}

		public object DoEval (string str)
		{
			return base.Eval (str);
		}
	}


	[TestFixture]
	public class TemplateControlTest
	{
		public static bool eventchecker;
		public string message = "My message text";

		[TestFixtureSetUp]
		public void GridViewInit ()
		{
			WebTest.CopyResource (GetType (), "TemplateUserControl.ascx", "TemplateUserControl.ascx");
		}

		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}

		[Test]
		public void TemplateControl_DefaultProperty ()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			Assert.AreEqual (true, t.EnableTheming, "EnableTheming");
			Assert.AreEqual (true, t.DoSupportAutoEvents, "SupportAutoEvents");
		}


		[Test]
		[Category ("NunitWeb")]
		public void TemplateControl_LoadControl ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadControlTest));
			string html = t.Run ();
			if (html.IndexOf ("TemplateUserControl") < 0)
				Assert.Fail ("LoadControl failed");
		}

		public static void LoadControlTest (Page p)
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			p.Form.Controls.Add (t.LoadControl ("TemplateUserControl.ascx"));
		}

		[Test]
		[Category ("NunitWeb")]
		public void TemplateControl_LoadTemplate ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (LoadTemplateTest));
			string html = t.Run ();
			if (html.IndexOf ("TemplateUserControl") < 0)
				Assert.Fail ("LoadTemplate failed");
		}

		public static void LoadTemplateTest (Page p)
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			ITemplate tmp = t.LoadTemplate ("TemplateUserControl.ascx");
			tmp.InstantiateIn (p.Form);
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NotDotNet")]  // Must be removed after adding AppRelativeVirtualPath property
		[Category ("NunitWeb")]
		public void TemplateControl_ParseControl ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ParseControlTest));
			string html = t.Run ();
			if (html.IndexOf ("<span id=\"lb\">test</span>") < 0)
				Assert.Fail ("ParseControl failed");
		}

		public static void ParseControlTest (Page p)
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			//Does not have definition , must be uncommented
			//t.AppRelativeVirtualPath = "~\\";
			Control c = t.ParseControl ("<asp:label id='lb' runat='server' text='test' />");
			p.Controls.Add(c);
		}

		[Test]
		public void TemplateControl_ReadStringResource ()
		{
			// p.s. MSDN
			// The ReadStringResource method is not intended for use from within your code
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NunitWeb")]
		public void TemplateControl_TestDeviceFilter ()
		{
			//Have no definition to TestDeviceFilter
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (DoTestDeviceFilter));
			string html = t.Run ();
			
		}

		public static void DoTestDeviceFilter (Page p)
		{
			//Have no definition to TestDeviceFilter
			// bool res = p.TestDeviceFilter("test");
			// Assert.AreEqual (false, res, "TestDeviceFilter#1");
			//Have no definition to TestDeviceFilter
			// res = p.TestDeviceFilter ("IE");
			// Assert.AreEqual (true, res, "TestDeviceFilter#2");
		}

		[Test]
		public void TemplateControl_Construct ()
		{
			eventchecker = false;	
			PokerTemplateControl t = new PokerTemplateControl ();
			Assert.AreEqual (true, eventchecker, "Construct Failed");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TemplateControl_Eval ()
		{
			// In this test aspx page used as template control
			WebTest.CopyResource (GetType (), "EvalTest.aspx", "EvalTest.aspx");
			WebTest t = new WebTest ("EvalTest.aspx");
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = _templatePreRender;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
			string html = t.Run ();
			if (html.IndexOf ("My databind test") < 0)
				Assert.Fail ("Eval not done fail");
		}

		public static void _templatePreRender (Page p)
		{
			Repeater rep = p.FindControl ("Repeater1") as Repeater;
			if (rep == null)
				Assert.Fail ("Aspx page not creation failed");
			Assert.AreEqual (1, rep.Items.Count, "Data items bounding failed");
		}

		[Test]
		public void TemplateControl_XPath_XPathSelect ()
		{
			//These two method are tested on XmlDataSourceTest.cs
		}

		[Test]
		public void TemplateControl_CreateResourceBasedLiteralControl ()
		{
			// The CreateResourceBasedLiteralControl method is not intended 
			// for use from within your code. 
		}

		[Test]
		public void TemplateControl_SetStringResourcePointer ()
		{
			// The SetStringResourcePointer method is not intended 
			// for use from within your code. 
		}

		[Test]
		public void TemplateControl_TemplateControl ()
		{
			Assert.IsNull (new Control ().TemplateControl);
			PokerTemplateControl t = new PokerTemplateControl ();
			Assert.AreEqual (t, t.TemplateControl);
		}
		
		[Test]
		public void TemplateControl_WriteUTF8ResourceString ()
		{
			//This method supports the .NET Framework infrastructure and is not intended to be used directly from your code. 
			//Writes a resource string to an HtmlTextWriter control. 
			//The WriteUTF8ResourceString method is used by generated classes and is not intended for use from within your code. 
		}

		// Events 
		bool abortTransaction;
		bool commitTransaction;
		bool error;

		[Test]
		public void TemplateControl_AbortTransaction ()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			t.AbortTransaction += new EventHandler (t_AbortTransaction);
			Assert.AreEqual (false, abortTransaction, "Before transaction aborted");
			t.DoOnAbortTransaction (new EventArgs ());
			Assert.AreEqual (true, abortTransaction, "After transaction aborted");
		}

		void t_AbortTransaction (object sender, EventArgs e)
		{
			abortTransaction = true;
		}

		[Test]
		public void TemplateControl_CommitTransaction ()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			t.CommitTransaction += new EventHandler (t_CommitTransaction);
			Assert.AreEqual (false, commitTransaction, "Before transaction Commited");
			t.DoOnCommitTransaction (new EventArgs ());
			Assert.AreEqual (true, commitTransaction, "After transaction Commited");
		}

		void t_CommitTransaction (object sender, EventArgs e)
		{
			commitTransaction = true;
		}

		[Test]
		public void TemplateControl_Error ()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			t.Error += new EventHandler (t_Error);
			Assert.AreEqual (false, error, "Before error");
			t.DoOnError (new EventArgs ());
			Assert.AreEqual (true, error, "After error");
		}

		void t_Error (object sender, EventArgs e)
		{
			error = true;
		}


		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TemplateControl_EvalException ()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			t.Page = new Page ();
			t.DoEval (null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TemplateControl_LoadControlException()
		{
			PokerTemplateControl t = new PokerTemplateControl ();
			t.LoadControl ((string)null);
		}


		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}
