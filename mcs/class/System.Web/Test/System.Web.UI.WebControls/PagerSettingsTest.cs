//
// Tests for System.Web.UI.WebControls.PagerSettingsTest.cs
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

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;


namespace MonoTests.System.Web.UI.WebControls
{
	

	[TestFixture]
	public class PagerSettingsTest
	{
		private bool _eventchecker;

		[Test]
		public void PagerSettings_DefaultProperty ()
		{
			PagerSettings pager = new PagerSettings ();
			Assert.AreEqual ("", pager.FirstPageImageUrl, "FirstPageImageUrl");
			Assert.AreEqual ("&lt;&lt;", pager.FirstPageText, "FirstPageText");
			Assert.AreEqual ("", pager.LastPageImageUrl, "LastPageImageUrl");
			Assert.AreEqual ("&gt;&gt;", pager.LastPageText, "LastPageText");
			Assert.AreEqual (PagerButtons.Numeric, pager.Mode, "Mode");
			Assert.AreEqual ("", pager.NextPageImageUrl, "NextPageImageUrl");
			Assert.AreEqual ("&gt;", pager.NextPageText, "NextPageText");
			Assert.AreEqual (10, pager.PageButtonCount, "PageButtonCount");
			Assert.AreEqual (PagerPosition.Bottom, pager.Position, "Position");
			Assert.AreEqual ("", pager.PreviousPageImageUrl, "PreviousPageImageUrl");
			Assert.AreEqual ("&lt;", pager.PreviousPageText, "PreviousPageText");
			Assert.AreEqual (true, pager.Visible, "Visible");
		}

		[Test]
		public void PagerSettings_AssignProperty ()
		{
			PagerSettings pager = new PagerSettings ();
			pager.FirstPageImageUrl = "test";
			Assert.AreEqual ("test", pager.FirstPageImageUrl, "FirstPageImageUrl");
			pager.FirstPageText = "test";
			Assert.AreEqual ("test", pager.FirstPageText, "FirstPageText");
			pager.LastPageImageUrl = "test";
			Assert.AreEqual ("test", pager.LastPageImageUrl, "LastPageImageUrl");
			pager.LastPageText = "test";
			Assert.AreEqual ("test", pager.LastPageText, "LastPageText");
			pager.Mode = PagerButtons.NextPrevious;
			Assert.AreEqual (PagerButtons.NextPrevious, pager.Mode, "Mode");
			pager.NextPageImageUrl = "test";
			Assert.AreEqual ("test", pager.NextPageImageUrl, "NextPageImageUrl");
			pager.NextPageText = "test";
			Assert.AreEqual ("test", pager.NextPageText, "NextPageText");
			pager.PageButtonCount = 20;
			Assert.AreEqual (20, pager.PageButtonCount, "PageButtonCount");
			pager.Position = PagerPosition.Top;
			Assert.AreEqual (PagerPosition.Top, pager.Position, "Position");
			pager.PreviousPageImageUrl = "test";
			Assert.AreEqual ("test", pager.PreviousPageImageUrl, "PreviousPageImageUrl");
			pager.PreviousPageText = "test";
			Assert.AreEqual ("test", pager.PreviousPageText, "PreviousPageText");
			pager.Visible = false;
			Assert.AreEqual (false, pager.Visible, "Visible");
		}

		[Test]
		public void PagerSettings_ToString ()
		{
			PagerSettings pager = new PagerSettings ();
			string result = pager.ToString ();
			Assert.AreEqual ("", result, "ToString");
		}

		[Test]
		public void PagerSettings_PropertyChanged ()
		{
			PagerSettings pager = new PagerSettings ();
			pager.PropertyChanged += new EventHandler (pager_PropertyChanged);
			pager.FirstPageImageUrl = "test";
			eventassert ("FirstPageImageUrl");
			pager.FirstPageText = "test";
			eventassert ("FirstPageText");
			pager.LastPageImageUrl = "test";
			eventassert ("LastPageImageUrl");
			pager.LastPageText = "test";
			eventassert ("LastPageText");
			pager.Mode = PagerButtons.NextPrevious;
			eventassert ("Mode");
			pager.NextPageImageUrl = "test";
			eventassert ("NextPageImageUrl");
			pager.NextPageText = "test";
			eventassert ("NextPageText");
			pager.PageButtonCount = 20;
			eventassert ("PageButtonCount");
			pager.PreviousPageImageUrl = "test";
			eventassert ("PreviousPageImageUrl");
			pager.PreviousPageText = "test";
			eventassert ("PreviousPageText");
		}

		private void pager_PropertyChanged (object o, EventArgs e)
		{
			_eventchecker = true;
		}

		private void eventassert (string message)
		{
			Assert.IsTrue (_eventchecker, message);
			_eventchecker = false;
		}
	}
}
#endif