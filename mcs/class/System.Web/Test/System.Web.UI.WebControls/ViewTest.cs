//
// Tests for System.Web.UI.WebControls.View.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{

	class PokerView : View
	{
		public PokerView ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object o)
		{
			LoadViewState (o);
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			Render (tw);
			return sw.ToString ();
		}

		public void DoOnActivate (EventArgs e)
		{
			base.OnActivate (e);
		}

		public void DoOnDeactivate (EventArgs e)
		{
			base.OnDeactivate (e);
		}


	}

	[TestFixture]
	public class ViewTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
		}

		[Test]
		public void View_DefaultProperties ()
		{
			PokerView b = new PokerView ();
			Assert.AreEqual (0, b.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (true, b.EnableTheming, "ViewEnableTheming");		
		}
				
		[Test]
		public void View_NotWorkingDefaultProperties ()
		{
			PokerView b = new PokerView ();
			Assert.AreEqual (false, b.Visible, "ViewVisible");
		}

		[Test]
		public void View_AssignToDefaultProperties ()
		{
			PokerView b = new PokerView ();			
			b.EnableTheming = false;
			Assert.AreEqual (false, b.EnableTheming, "ThemingValidation");
		}


		[Test]
		public void View_Defaults_Render ()
		{
			PokerView b = new PokerView ();
			string html = b.Render ();
			Assert.AreEqual (b.Render (), string.Empty, "RenderViewState");
		}

		[Test]
		public void View_RenderStateWithChilds ()
		{
			PokerView pv = new PokerView ();
			Button btn = new Button ();
			btn.ID = "btn";
			btn.Text = "MyTestButton";
			pv.Controls.Add (btn);
			string my = pv.Render ();
			Assert.AreEqual (@"<input type=""submit"" name=""btn"" value=""MyTestButton"" id=""btn"" />", my, "RenderViewStateWithChilds");
		}



		// Events Stuff
		private bool activated = false;
		private bool deactivated = false;

		private void ViewActivate (object sender, EventArgs e)
		{
			activated = true;
		}

		private void ViewDeActivate (object sender, EventArgs e)
		{
			deactivated = true;
		}

		private void ResetEvents ()
		{
			activated = false;
			deactivated = false;
		}


		[Test]
		public void View_Events ()
		{
			PokerView pv = new PokerView ();
			ResetEvents ();
			pv.Activate += new EventHandler (ViewActivate);
			Assert.AreEqual (false, activated, "BeforeActivate");
			pv.DoOnActivate (new EventArgs ());
			Assert.AreEqual (true, activated, "AfterActivate");
			ResetEvents ();
			pv.Deactivate += new EventHandler (ViewDeActivate);
			Assert.AreEqual (false, deactivated, "BeforeDeactivate");
			pv.DoOnDeactivate (new EventArgs ());
			Assert.AreEqual (true, deactivated, "AfterDeactivate");
		}		

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void View_Visible_Assign ()
		{
			PokerView b = new PokerView ();
			b.Visible = true;
		}

		[Test]
		[Category ("NunitWeb")]
		public void View_Events_Base_PostBack ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnInit (new PageDelegate (EventsTest));
			string html = t.Run ();

			if (html.IndexOf ("View_1_is_active") < 0)
				Assert.Fail ("View_Events#1 Failed");

			if (html.IndexOf ("Activate") < 0)
				Assert.Fail ("View_Events#3 Failed");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("bt");
			fr.Controls["bt"].Value = "Button";
			t.Request = fr;
			html = t.Run ();

			if (html.IndexOf ("Deactivate") < 0) {
				Assert.Fail ("View_Events#4 Failed");
			}

			if (html.IndexOf ("View_2_is_active") < 0)
				Assert.Fail ("View_Events#5 Failed");
		}

		#region base_events
		public static void EventsTest (Page p)
		{
			MultiView MultiView1 = new MultiView ();
			MultiView1.ID = "MultiView1";
			View view_1 = new View ();
			view_1.ID = "view_1";
			View view_2 = new View ();
			view_2.ID = "view_2";
			Button bt = new Button ();
			bt.ID = "bt";
			bt.CommandName = "NextView";

			view_1.Controls.Add (bt);
			view_1.Controls.Add (new LiteralControl ("View_1_is_active"));
			view_2.Controls.Add (new LiteralControl ("View_2_is_active"));

			view_1.Activate += new EventHandler (view_1_Activate);
			view_1.Deactivate += new EventHandler (view_1_Deactivate);
			MultiView1.Views.Add (view_1);
			MultiView1.Views.Add (view_2);
			p.Controls.Add (MultiView1);
			MultiView1.ActiveViewIndex = 0;
		}

		public static void view_1_Deactivate (object sender, EventArgs e)
		{
			View v = sender as View;
			if (v == null)
				Assert.Fail ("View_Events#2 Failed");
			v.Page.Controls.Add (new LiteralControl ("Deactivate"));
		}
		public static void view_1_Activate (object sender, EventArgs e)
		{
			View v = sender as View;
			if (v == null)
				Assert.Fail ("View_Events#2 Failed");
			v.Page.Controls.Add (new LiteralControl ("Activate"));
		}
		#endregion

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}

#endif

