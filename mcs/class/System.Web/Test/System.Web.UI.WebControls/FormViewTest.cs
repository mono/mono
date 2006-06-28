//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Chris Toshok (toshok@ximian.com)
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
//

#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FormViewTest {	
		class Poker : FormView {

			bool _onPageIndexChangingCalled = false;
			bool _onPageIndexChangedCalled = false;

			public Poker () {
				TrackViewState ();
			}

			public object SaveState () {
				return SaveViewState ();
			}

			public void LoadState (object state) {
				LoadViewState (state);
			}
			
			public void DoConfirmInitState ()
			{
				base.ConfirmInitState ();
			}
	
			public void DoOnPreRender (EventArgs e)
			{
				base.OnPreRender (e);
			}
			
			public bool DoOnBubbleEvent (object source, EventArgs e) {
				return base.OnBubbleEvent (source, e);
			}
			
			public bool OnPageIndexChangingCalled {
				set { _onPageIndexChangingCalled = value; }
				get { return _onPageIndexChangingCalled; }
			}
	
			public bool OnPageIndexChangedCalled {
				set { _onPageIndexChangedCalled = value; }
				get { return _onPageIndexChangedCalled; }
			}
	
			protected override void OnPageIndexChanging (FormViewPageEventArgs e) {
				OnPageIndexChangingCalled = true;
				base.OnPageIndexChanging (e);
			}
	
			protected override void OnPageIndexChanged (EventArgs e) {
				OnPageIndexChangedCalled = true;
				base.OnPageIndexChanged (e);
			}
		}
		
		class Template : ITemplate
		{
			bool _instantiated;
			
			public bool Instantiated {
			       get { return _instantiated; }
			}
			
			#region ITemplate Members
			
			public void InstantiateIn (Control container) {
			       _instantiated = true;
			}
			
			#endregion
		}
		
		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();

			Assert.IsFalse (p.AllowPaging, "A1");
			Assert.AreEqual ("", p.BackImageUrl, "A2");
			Assert.IsNull (p.BottomPagerRow, "A3");
			Assert.AreEqual ("", p.Caption, "A4");
			Assert.AreEqual (TableCaptionAlign.NotSet, p.CaptionAlign ,"A5");
			Assert.AreEqual (-1, p.CellPadding, "A6");
			Assert.AreEqual (0, p.CellSpacing, "A7");
			Assert.AreEqual (FormViewMode.ReadOnly, p.CurrentMode, "A8");
			Assert.AreEqual (FormViewMode.ReadOnly, p.DefaultMode, "A9");
			Assert.IsNotNull (p.DataKeyNames, "A10");
			Assert.AreEqual (0, p.DataKeyNames.Length, "A10.1");
			Assert.IsNotNull (p.DataKey, "A11");
			Assert.AreEqual (0, p.DataKey.Values.Count, "A11.1");
			Assert.IsNull (p.EditItemTemplate, "A12");
			Assert.IsNotNull (p.EditRowStyle, "A13");

			Assert.IsNotNull (p.EmptyDataRowStyle, "A14");

			Assert.IsNull (p.EmptyDataTemplate, "A15");
			Assert.AreEqual ("", p.EmptyDataText, "A16");
			Assert.IsNull (p.FooterRow, "A17");
			Assert.IsNull (p.FooterTemplate, "A18");
			Assert.AreEqual ("", p.FooterText, "A19");
			Assert.IsNotNull (p.FooterStyle, "A20");

			Assert.AreEqual (GridLines.None, p.GridLines, "A21");
			Assert.IsNull (p.HeaderRow, "A22");
			Assert.IsNotNull (p.HeaderStyle, "A23");

			Assert.IsNull (p.HeaderTemplate, "A24");
			Assert.AreEqual ("", p.HeaderText, "A25");
			Assert.AreEqual (HorizontalAlign.NotSet, p.HorizontalAlign, "A26");
			Assert.IsNull (p.InsertItemTemplate, "A27");
			Assert.IsNotNull (p.InsertRowStyle, "A28");

			Assert.IsNull (p.ItemTemplate, "A29");
			Assert.AreEqual (0, p.PageCount, "A30");
			Assert.AreEqual (0, p.PageIndex, "A31");
			Assert.IsNull (p.PagerTemplate, "A32");
			Assert.IsNull (p.Row, "A33");
			Assert.IsNotNull (p.RowStyle, "A34");

			Assert.IsNull (p.SelectedValue, "A35");
			Assert.IsNull (p.TopPagerRow, "A36");
			Assert.IsNull (p.DataItem, "A37");
			Assert.AreEqual (0, p.DataItemCount, "A38");
			Assert.AreEqual (0, p.DataItemIndex, "A39");
		}
		
		[Test]
		public void PageIndex ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ds.ID = "ObjectDataSource1";
			ds.TypeName = "System.Guid";
			ds.SelectMethod = "ToByteArray";
			Page p = new Page ();
			Poker f = new Poker ();
			f.Page = p;
			ds.Page = p;
			p.Controls.Add (f);
			p.Controls.Add (ds);
			f.DataSourceID = "ObjectDataSource1";
			f.DoConfirmInitState ();
			f.DoOnPreRender (EventArgs.Empty);
			object cur = f.DataItem;
			f.PageIndex = 1;
			Assert.IsTrue (cur != f.DataItem, "#01");

			CommandEventArgs cargs = new CommandEventArgs ("Page", "Prev");
			FormViewCommandEventArgs fvargs = new FormViewCommandEventArgs (f, cargs);
			f.DoOnBubbleEvent (f, fvargs);
			Assert.IsTrue (f.OnPageIndexChangingCalled, "#02");
			Assert.IsTrue (f.OnPageIndexChangedCalled, "#03");
			f.OnPageIndexChangingCalled = false;
			f.OnPageIndexChangedCalled = false;

			f.DoOnBubbleEvent (f, fvargs);
			Assert.IsTrue (f.OnPageIndexChangingCalled, "#04");
			Assert.IsFalse (f.OnPageIndexChangedCalled, "#05");
			f.OnPageIndexChangingCalled = false;
			f.OnPageIndexChangedCalled = false;

			f.PageIndex = f.PageCount - 1;
			cargs = new CommandEventArgs ("Page", "Next");
			fvargs = new FormViewCommandEventArgs (f, cargs);
			f.DoOnBubbleEvent (f, fvargs);
			Assert.IsTrue (f.OnPageIndexChangingCalled, "#06");
			Assert.IsFalse (f.OnPageIndexChangedCalled, "#07");
			f.OnPageIndexChangingCalled = false;
			f.OnPageIndexChangedCalled = false;
		}
		
		[Test]
		public void PageCount ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ds.ID = "ObjectDataSource1";
			ds.TypeName = "System.Guid";
			ds.SelectMethod = "ToByteArray";
			Page p = new Page ();
			Poker f = new Poker ();
			f.Page = p;
			ds.Page = p;
			p.Controls.Add (f);
			p.Controls.Add (ds);
			f.DataSourceID = "ObjectDataSource1";
			f.DoConfirmInitState ();
			f.DoOnPreRender (EventArgs.Empty);
			f.PageIndex = 1;
			Assert.AreEqual (16, f.PageCount, "#01");
		}
		
		[Test]
		public void InsertTemplate () {
			ObjectDataSource ds = new ObjectDataSource ();
			ds.ID = "ObjectDataSource1";
			ds.TypeName = "System.Collections.ArrayList";
			ds.SelectMethod = "ToArray";
			Page p = new Page ();
			Poker f = new Poker ();
			Template itemTemplate = new Template ();
			Template emptyTemplate = new Template ();
			Template insertTemplate = new Template ();
			f.ItemTemplate = itemTemplate;
			f.EmptyDataTemplate = emptyTemplate;
			f.InsertItemTemplate = insertTemplate;
			f.DefaultMode = FormViewMode.Insert;
			f.Page = p;
			ds.Page = p;
			p.Controls.Add (f);
			p.Controls.Add (ds);
			f.DataSourceID = "ObjectDataSource1";
			f.DoConfirmInitState ();
			f.DoOnPreRender (EventArgs.Empty);
			
			f.AllowPaging = true;
			Assert.IsFalse(itemTemplate.Instantiated, "#01");
			Assert.IsFalse(emptyTemplate.Instantiated, "#02");
			Assert.IsTrue(insertTemplate.Instantiated, "#03");
		}
		
		[TestFixtureTearDown]
		public void TearDown ()
		{
			Thread.Sleep (100);
		        WebTest.Unload ();
		}
	
		//[TestFixtureStartUp]
		//public void StartUp ()
		//{
		//	WebTest.CopyResource (GetType (), "FormView.aspx", "FormView.aspx");
		//}

		[Test]
		[Category("NunitWeb")]
		public void FormViewCssClass ()
		{
			WebTest.CopyResource (GetType (), "FormView.aspx", "FormView.aspx");
			string res = new WebTest ("FormView.aspx").Run ();
			Assert.IsTrue (Regex.IsMatch (
				res, ".*<table[^>]*class=\"[^\"]*test1[^\"]*\"[^>]*>.*",
				RegexOptions.IgnoreCase|RegexOptions.Singleline),
				"check that <table class=\"test1\"> is found. Actual: "+res);
			Assert.IsFalse (Regex.IsMatch (
				res, ".*<table[^>]*class=\"\"[^>]*>.*",
				RegexOptions.IgnoreCase|RegexOptions.Singleline),
				"check that <table class=\"\"> is not found. Actual: "+res);
		}

	}
}
#endif
