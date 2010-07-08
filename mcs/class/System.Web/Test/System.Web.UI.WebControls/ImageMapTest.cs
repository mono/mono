//
// Tests for System.Web.UI.WebControls.ImageMap.cs
//
// Author:
//  Hagit Yidov (hagity@mainsoft.com
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
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{
	class PokerImageMap : ImageMap
	{
		// View state Stuff
		public PokerImageMap ()
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

		public void DoOnClick (ImageMapEventArgs e)
		{
			base.OnClick (e);
		}

		public void DoOnBubbleEven (Object source, ImageMapEventArgs e)
		{
			base.OnBubbleEvent (source, e);
		}

		// Render Method
		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			Render (tw);
			return sw.ToString ();
		}
	}

	[TestFixture]
	public class ImageMapTest
	{
		[TestFixtureSetUp]
		public void SetUp ()
		{
			WebTest.CopyResource (GetType (), "NoEventValidation.aspx", "NoEventValidation.aspx");
		}



		[Test]
		public void ImageMap_DefaultProperties ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			Assert.AreEqual (0, imageMap.StateBag.Count, "ViewState.Count");
			Assert.AreEqual (true, imageMap.Enabled, "Enabled");
			Assert.AreEqual (HotSpotMode.NotSet, imageMap.HotSpotMode, "HotSpotMode");
			Assert.AreEqual (0, imageMap.HotSpots.Count, "HotSpots.Count");
			Assert.AreEqual (string.Empty, imageMap.Target, "Target");
		}

		[Test]
		public void ImageMap_AssignToDefaultProperties ()
		{
			PokerImageMap imageMap = new PokerImageMap ();

			Assert.AreEqual (0, imageMap.StateBag.Count, "ViewState.Count");

			imageMap.Enabled = true;
			Assert.AreEqual (true, imageMap.Enabled, "Enabled");
			Assert.AreEqual (0, imageMap.StateBag.Count, "ViewState.Count-1");

			imageMap.HotSpotMode = HotSpotMode.Navigate;
			Assert.AreEqual (HotSpotMode.Navigate, imageMap.HotSpotMode, "HotSpotMode");
			Assert.AreEqual (1, imageMap.StateBag.Count, "ViewState.Count-2");

			imageMap.HotSpots.Add (new CircleHotSpot ());
			Assert.AreEqual (1, imageMap.HotSpots.Count, "HotSpots.Count");
			Assert.AreEqual (1, imageMap.StateBag.Count, "ViewState.Count-3");

			imageMap.Target = "Target";
			Assert.AreEqual ("Target", imageMap.Target, "Target");
			Assert.AreEqual (2, imageMap.StateBag.Count, "ViewState.Count-4");
		}

		[Test]
		public void ImageMap_Defaults_Render ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
#if NET_4_0
			string originalHtml = "<img src=\"\" />";
#else
			string originalHtml = "<img src=\"\" style=\"border-width:0px;\" />";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderDefault");
		}

		[Test]
		public void ImageMap_AssignedValues_RenderNavigate ()
		{
			// HotSpotMode = Navigate using NavigateURL
			//-----------------------------------------
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = true;
			imageMap.HotSpotMode = HotSpotMode.Navigate;
			imageMap.Target = "Target";
			CircleHotSpot circle = new CircleHotSpot ();
			circle.NavigateUrl = "NavigateURL";
			imageMap.HotSpots.Add (circle);
#if NET_4_0
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" href=\"NavigateURL\" target=\"Target\" title=\"\" alt=\"\" />\r\n</map>";
#else
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" style=\"border-width:0px;\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" href=\"NavigateURL\" target=\"Target\" title=\"\" alt=\"\" />\r\n</map>";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderNavigateTextAssigned");
		}

		[Test]
		public void ImageMap_AssignedValues_RenderNavigateCircle ()
		{
			// Circle.HotSpotMode = Navigate
			//------------------------------
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = true;
			CircleHotSpot circle = new CircleHotSpot ();
			circle.AccessKey = "A";
			circle.AlternateText = "Circle";
			circle.HotSpotMode = HotSpotMode.Navigate;
			circle.NavigateUrl = "NavigateURL";
			circle.TabIndex = 1;
			circle.Radius = 10;
			circle.X = 30;
			circle.Y = 40;
			imageMap.HotSpots.Add (circle);
#if NET_4_0
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"30,40,10\" href=\"NavigateURL\" title=\"Circle\" alt=\"Circle\" accesskey=\"A\" tabindex=\"1\" />\r\n</map>";
#else
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" style=\"border-width:0px;\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"30,40,10\" href=\"NavigateURL\" title=\"Circle\" alt=\"Circle\" accesskey=\"A\" tabindex=\"1\" />\r\n</map>";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderNavigateCircleTextAssigned");
		}

		[Test]
		public void ImageMap_AssignedValues_RenderNavigateShapes ()
		{
			// Rectangle/Polygon.HotSpotMode = Navigate 
			//-----------------------------------------
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = true;
			imageMap.HotSpotMode = HotSpotMode.NotSet;
			RectangleHotSpot rect = new RectangleHotSpot ();
			rect.AccessKey = "R";
			rect.AlternateText = "Rectangle";
			rect.HotSpotMode = HotSpotMode.Navigate;
			rect.NavigateUrl = "NavigateUrlRect";
			rect.TabIndex = 1;
			rect.Bottom = 10;
			rect.Top = 20;
			rect.Left = 30;
			rect.Right = 40;
			imageMap.HotSpots.Add (rect);
			imageMap.HotSpotMode = HotSpotMode.Navigate;
			PolygonHotSpot poly = new PolygonHotSpot ();
			poly.AccessKey = "P";
			poly.AlternateText = "Polygon";
			poly.NavigateUrl = "NavigateUrlPoly";
			poly.TabIndex = 2;
			poly.Coordinates = "10,20,30,40,50,60,100,200";
			imageMap.HotSpots.Add (poly);
#if NET_4_0
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"rect\" coords=\"30,20,40,10\" href=\"NavigateUrlRect\" title=\"Rectangle\" alt=\"Rectangle\" accesskey=\"R\" tabindex=\"1\" /><area shape=\"poly\" coords=\"10,20,30,40,50,60,100,200\" href=\"NavigateUrlPoly\" title=\"Polygon\" alt=\"Polygon\" accesskey=\"P\" tabindex=\"2\" />\r\n</map>";
#else
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" style=\"border-width:0px;\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"rect\" coords=\"30,20,40,10\" href=\"NavigateUrlRect\" title=\"Rectangle\" alt=\"Rectangle\" accesskey=\"R\" tabindex=\"1\" /><area shape=\"poly\" coords=\"10,20,30,40,50,60,100,200\" href=\"NavigateUrlPoly\" title=\"Polygon\" alt=\"Polygon\" accesskey=\"P\" tabindex=\"2\" />\r\n</map>";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderNavigateShapesTextAssigned");
		}

		[Test]
		public void ImageMap_AssignedValues_RenderInactive ()
		{
			// HotSpotMode = Inactive
			//-----------------------
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = true;
			imageMap.HotSpotMode = HotSpotMode.Inactive;
			imageMap.Target = "Target";
			imageMap.HotSpots.Add (new CircleHotSpot ());
#if NET_4_0
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" nohref=\"true\" title=\"\" alt=\"\" />\r\n</map>";
#else
			string originalHtml = "<img src=\"\" usemap=\"#ImageMap\" style=\"border-width:0px;\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" nohref=\"true\" title=\"\" alt=\"\" />\r\n</map>";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderInaciveTextAssigned");
		}

		[Test]
		public void ImageMap_AssignedValues_RenderDisabled ()
		{
			// Enabled = false
			//----------------
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = false;
			imageMap.HotSpotMode = HotSpotMode.Navigate;
			imageMap.Target = "Target";
			CircleHotSpot circle = new CircleHotSpot ();
			circle.NavigateUrl = "NavigateURL";
			imageMap.HotSpots.Add (circle);
#if NET_4_0
			string originalHtml = "<img class=\"aspNetDisabled\" src=\"\" usemap=\"#ImageMap\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" target=\"Target\" title=\"\" alt=\"\" />\r\n</map>";
#else
			string originalHtml = "<img disabled=\"disabled\" src=\"\" usemap=\"#ImageMap\" style=\"border-width:0px;\" /><map name=\"ImageMap\" id=\"ImageMap\">\r\n\t<area shape=\"circle\" coords=\"0,0,0\" href=\"NavigateURL\" target=\"Target\" title=\"\" alt=\"\" />\r\n</map>";
#endif
			string renderedHtml = imageMap.Render ();
			HtmlDiff.AssertAreEqual (originalHtml, renderedHtml, "RenderDisabledTextAssigne");
		}

		[Test]
		public void ImageMap_ViewState ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			imageMap.Enabled = true;
			Assert.AreEqual (true, imageMap.Enabled, "Enabled-beforecopy");
			imageMap.HotSpotMode = HotSpotMode.Navigate;
			Assert.AreEqual (HotSpotMode.Navigate, imageMap.HotSpotMode, "HotSpotMode-beforecopy");
			imageMap.HotSpots.Add (new CircleHotSpot ());
			Assert.AreEqual (1, imageMap.HotSpots.Count, "HotSpots.Count-beforecopy");
			imageMap.Target = "Target";
			Assert.AreEqual ("Target", imageMap.Target, "Target-beforecopy");
			object state = imageMap.SaveState ();
			PokerImageMap copy = new PokerImageMap ();
			copy.LoadState (state);
			Assert.AreEqual (true, copy.Enabled, "Enabled-aftercopy");
			Assert.AreEqual (HotSpotMode.Navigate, copy.HotSpotMode, "HotSpotMode-aftercopy");
			//Assert.AreEqual(1, copy.HotSpots.Count, "HotSpots.Count-aftercopy");
			Assert.AreEqual ("Target", copy.Target, "Target-aftercopy");
		}

		// Events Stuff
		private bool clicked = false;
		private string pbValue;

		private void ImageMapClickHandler (object sender, ImageMapEventArgs e)
		{
			clicked = true;
			pbValue = e.PostBackValue;
		}

		private void ResetEvents ()
		{
			clicked = false;
			pbValue = "Init";
		}

		[Test]
		public void ImageMap_Event ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			ResetEvents ();
			imageMap.HotSpotMode = HotSpotMode.PostBack;
			imageMap.Click += new ImageMapEventHandler (ImageMapClickHandler);
			Assert.AreEqual (false, clicked, "BeforeClick");
			imageMap.DoOnClick (new ImageMapEventArgs ("HotSpotName"));
			Assert.AreEqual (true, clicked, "AfterClick");
		}

		[Test]
		public void ImageMap_EventCircle ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			ResetEvents ();
			imageMap.HotSpotMode = HotSpotMode.NotSet;
			CircleHotSpot circle = new CircleHotSpot ();
			circle.HotSpotMode = HotSpotMode.PostBack;
			circle.PostBackValue = "myCircle";
			imageMap.HotSpots.Add (circle);
			imageMap.Click += new ImageMapEventHandler (ImageMapClickHandler);
			Assert.AreEqual ("Init", pbValue, "BeforeClick");
			imageMap.DoOnClick (new ImageMapEventArgs (circle.PostBackValue));
			Assert.AreEqual ("myCircle", pbValue, "AfterClick");
		}

		[Test]
		public void ImageMap_EventRectangle ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			ResetEvents ();
			imageMap.HotSpotMode = HotSpotMode.PostBack;
			RectangleHotSpot rect = new RectangleHotSpot ();
			rect.PostBackValue = "myRect";
			imageMap.HotSpots.Add (rect);
			imageMap.Click += new ImageMapEventHandler (ImageMapClickHandler);
			Assert.AreEqual ("Init", pbValue, "BeforeClick");
			imageMap.DoOnClick (new ImageMapEventArgs (rect.PostBackValue));
			Assert.AreEqual ("myRect", pbValue, "AfterClick");
		}

		[Test]
		public void ImageMap_EventPolygon ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			ResetEvents ();
			imageMap.HotSpotMode = HotSpotMode.NotSet;
			PolygonHotSpot poly = new PolygonHotSpot ();
			poly.HotSpotMode = HotSpotMode.PostBack;
			poly.PostBackValue = "myPoly";
			imageMap.HotSpots.Add (poly);
			imageMap.Click += new ImageMapEventHandler (ImageMapClickHandler);
			Assert.AreEqual ("Init", pbValue, "BeforeClick");
			imageMap.DoOnClick (new ImageMapEventArgs (poly.PostBackValue));
			Assert.AreEqual ("myPoly", pbValue, "AfterClick");
		}

		public void ImageMap_BubbleEvent ()
		{
			PokerImageMap imageMap = new PokerImageMap ();
			ResetEvents ();
			ImageMapEventArgs args = new ImageMapEventArgs ("HotSpotName");
			imageMap.Click += new ImageMapEventHandler (ImageMapClickHandler);
			Assert.AreEqual (false, clicked, "BeforeClick");
			imageMap.DoOnBubbleEven (imageMap, args);
			Assert.AreEqual (true, clicked, "AfterClick");
		}

		private static void ImageMapClickHandler2 (object sender, ImageMapEventArgs e)
		{
			WebTest.CurrentTest.UserData = e.PostBackValue;
		}

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBackRectangle ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (myPageLoad));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "imgmap";
			fr.Controls["__EVENTARGUMENT"].Value = "0";
			t.Request = fr;
			t.Run ();
			Assert.AreEqual ("Rectangle", t.UserData, "AfterPostBack");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBackFireEvent ()
		{
			WebTest t = new WebTest ("NoEventValidation.aspx");
			t.Invoker = PageInvoker.CreateOnLoad (PostBackFireEvent_Init);
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "imgmap";
			fr.Controls["__EVENTARGUMENT"].Value = "0";
			t.Request = fr;
			t.Run ();
			if (t.UserData == null)
				Assert.Fail ("Event not fired fail");
			Assert.AreEqual ("ImageMapClickHandler", t.UserData.ToString (), "PostBackFireEvent");
		}

		#region PostBackFireEvent
		public static void PostBackFireEvent_Init (Page p)
		{
			ImageMap imgmap = new ImageMap ();
			imgmap.ID = "imgmap";
			imgmap.HotSpotMode = HotSpotMode.NotSet;
			imgmap.Click += new ImageMapEventHandler (ImageMapClickHandler3);
			RectangleHotSpot rect = new RectangleHotSpot ();
			rect.HotSpotMode = HotSpotMode.PostBack;
			rect.PostBackValue = "Rectangle";
			imgmap.HotSpots.Add (rect);
			p.Form.Controls.Add (imgmap);
		}

		public static void ImageMapClickHandler3 (object sender, ImageMapEventArgs e)
		{
			WebTest.CurrentTest.UserData = "ImageMapClickHandler";
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBackCircle ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (myPageLoad));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "imgmap";
			fr.Controls["__EVENTARGUMENT"].Value = "2";
			t.Request = fr;
			t.Run ();
			Assert.AreEqual ("Circle", t.UserData, "AfterPostBack");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBackPolygon ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (myPageLoad));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "imgmap";
			fr.Controls["__EVENTARGUMENT"].Value = "1";
			t.Request = fr;
			t.Run ();
			Assert.AreEqual ("Polygon", t.UserData, "AfterPostBack");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBack_RenderBefore ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (myPageLoad));
			#region orig
#if NET_4_0
			string strTarget = "<img id=\"imgmap\" src=\"\" usemap=\"#ImageMapimgmap\" /><map name=\"ImageMapimgmap\" id=\"ImageMapimgmap\">\r\n\t<area shape=\"rect\" coords=\"0,0,0,0\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;0&#39;)\" title=\"\" alt=\"\" /><area shape=\"poly\" coords=\"\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;1&#39;)\" title=\"\" alt=\"\" /><area shape=\"circle\" coords=\"0,0,0\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;2&#39;)\" title=\"\" alt=\"\" />\r\n</map>";
#else
			string strTarget = "<img id=\"imgmap\" src=\"\" usemap=\"#ImageMapimgmap\" style=\"border-width:0px;\" /><map name=\"ImageMapimgmap\" id=\"ImageMapimgmap\">\r\n\t<area shape=\"rect\" coords=\"0,0,0,0\" href=\"javascript:__doPostBack('imgmap','0')\" title=\"\" alt=\"\" /><area shape=\"poly\" coords=\"\" href=\"javascript:__doPostBack('imgmap','1')\" title=\"\" alt=\"\" /><area shape=\"circle\" coords=\"0,0,0\" href=\"javascript:__doPostBack('imgmap','2')\" title=\"\" alt=\"\" />\r\n</map>";
#endif
			#endregion
			string RenderedPageHtml = t.Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (strTarget, RenderedControlHtml, "BeforePostBack");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ImageMap_PostBack_RenderAfter ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (myPageLoad));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "imgmap";
			fr.Controls["__EVENTARGUMENT"].Value = "0";
			t.Request = fr;
			#region orig
#if NET_4_0
			string strTarget = "<img id=\"imgmap\" src=\"\" usemap=\"#ImageMapimgmap\" /><map name=\"ImageMapimgmap\" id=\"ImageMapimgmap\">\r\n\t<area shape=\"rect\" coords=\"0,0,0,0\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;0&#39;)\" title=\"\" alt=\"\" /><area shape=\"poly\" coords=\"\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;1&#39;)\" title=\"\" alt=\"\" /><area shape=\"circle\" coords=\"0,0,0\" href=\"javascript:__doPostBack(&#39;imgmap&#39;,&#39;2&#39;)\" title=\"\" alt=\"\" />\r\n</map>";
#else
			string strTarget = "<img id=\"imgmap\" src=\"\" usemap=\"#ImageMapimgmap\" style=\"border-width:0px;\" /><map name=\"ImageMapimgmap\" id=\"ImageMapimgmap\">\r\n\t<area shape=\"rect\" coords=\"0,0,0,0\" href=\"javascript:__doPostBack('imgmap','0')\" title=\"\" alt=\"\" /><area shape=\"poly\" coords=\"\" href=\"javascript:__doPostBack('imgmap','1')\" title=\"\" alt=\"\" /><area shape=\"circle\" coords=\"0,0,0\" href=\"javascript:__doPostBack('imgmap','2')\" title=\"\" alt=\"\" />\r\n</map>";
#endif
			#endregion
			string RenderedPageHtml = t.Run ();
			string RenderedControlHtml = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			HtmlDiff.AssertAreEqual (strTarget, RenderedControlHtml, "AfterPostBack");
		}

		public static void myPageLoad (Page page)
		{
			WebTest.CurrentTest.UserData = "Init";
			ImageMap imgmap = new ImageMap ();
			imgmap.ID = "imgmap";
			imgmap.HotSpotMode = HotSpotMode.NotSet;
			imgmap.Click += new ImageMapEventHandler (ImageMapClickHandler2);
			RectangleHotSpot rect = new RectangleHotSpot ();
			rect.HotSpotMode = HotSpotMode.PostBack;
			rect.PostBackValue = "Rectangle";
			imgmap.HotSpots.Add (rect);
			PolygonHotSpot poly = new PolygonHotSpot ();
			poly.HotSpotMode = HotSpotMode.PostBack;
			poly.PostBackValue = "Polygon";
			imgmap.HotSpots.Add (poly);
			imgmap.HotSpotMode = HotSpotMode.PostBack;
			CircleHotSpot circle = new CircleHotSpot ();
			circle.PostBackValue = "Circle";
			imgmap.HotSpots.Add (circle);
			// Two marks for getting controls from form
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			page.Form.Controls.Add (lcb);
			page.Form.Controls.Add (imgmap);
			page.Form.Controls.Add (lce);
		}

		[SetUp]
		public void SetUpTest ()
		{
			Thread.Sleep (100);
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
	}
}


#endif
