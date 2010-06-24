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
using System.Data;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;



namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class FormViewTest {	

		public class DataSourceObject
		{
			public static List<string> GetList (string sortExpression, int startRowIndex, int maximumRows) {
				return GetList ();
			}

			public static List<string> GetList (int startRowIndex, int maximumRows) {
				return GetList ();
			}

			public static List<string> GetList (string sortExpression) {
				return GetList ();
			}

			public static List<string> GetList () {
				List<string> list = new List<string> ();
				list.Add ("Norway");
				list.Add ("Sweden");
				list.Add ("France");
				list.Add ("Italy");
				list.Add ("Israel");
				list.Add ("Russia");
				return list;
			}

			public static int GetCount () {
				return GetList ().Count;
			}
		}

		public class DS : ObjectDataSource
		{
			public static List<string> GetList ()
			{
				List<string> list = new List<string> ();
				list.Add ("Norway");
				list.Add ("Sweden");
				list.Add ("France");
				list.Add ("Italy");
				list.Add ("Israel");
				list.Add ("Russia");
				return list;
			}

			public void DoRaiseDataSourceChangedEvent (EventArgs e)
			{
				RaiseDataSourceChangedEvent (e);
			}
		}
		
		public class Poker : FormView {
			public bool isInitializePager=false;
			public bool ensureDataBound=false;
			public bool controlHierarchy=false;
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

			public HtmlTextWriterTag PokerTagKey
			{
				get { return base.TagKey; }
			}
			
			public  int DoCreateChildControls (IEnumerable source,bool dataBind)
			{
				return CreateChildControls (source, dataBind);
				
			}			

			public Style DoCreateControlStyle ()
			{				
				return base.CreateControlStyle (); 
			}

			public DataSourceSelectArguments DoCreateDataSourceSelectArguments ()
			{
				return CreateDataSourceSelectArguments ();
			}

			public DataSourceView DoGetData ()
			{
				return GetData ();
			}

			public FormViewRow DoCreateRow (int itemIndex,DataControlRowType rowType,DataControlRowState rowState)
			{
				return CreateRow( itemIndex, rowType,rowState); 
			}

			public Table DoCreateTable ()
			{
				return CreateTable (); 
			}

			protected override void EnsureDataBound ()
			{
				base.EnsureDataBound ();
				ensureDataBound = true;
			}

			public void DoExtractRowValues (IOrderedDictionary filedValues, bool includeKeys)
			{
				base.ExtractRowValues (filedValues, includeKeys);
				
			}

			public bool IsRequiresDataBinding ()
			{
				return base.RequiresDataBinding;
			}

			protected override void InitializePager (FormViewRow row, PagedDataSource pageData)
			{
				base.InitializePager (row, pageData);
				isInitializePager = true;
			}

			public void DoInitializeRow (FormViewRow row)
			{
				InitializeRow (row); 
			}
			public void DoLoadControlState (object savedState)
			{
				LoadControlState (savedState);  
			}

			public void DoLoadViewState (object savedState)
			{
				LoadViewState (savedState);  
			}			

			public bool DoOnBubbleEvent (object source, EventArgs e)
			{
				return OnBubbleEvent (source, e); 
			}

			public void DoOnInit (EventArgs e)
			{
				OnInit (e); 
			}

			public void DoOnItemCommand (FormViewCommandEventArgs e)
			{
				OnItemCommand (e); 
			}

			public void DoOnItemCreated (EventArgs e)
			{
				OnItemCreated (e); 
			}

			public void DoOnItemDeleted (FormViewDeletedEventArgs e)
			{
				OnItemDeleted (e); 
			}

			public void DoOnItemDeleting (FormViewDeleteEventArgs e)
			{
				OnItemDeleting (e); 
			}

			public void DoOnItemInserted (FormViewInsertedEventArgs e)
			{
				OnItemInserted (e); 
			}

			public void DoOnItemInserting (FormViewInsertEventArgs e)
			{
				OnItemInserting (e);
			}

			public void DoOnItemUpdated (FormViewUpdatedEventArgs e)
			{
				OnItemUpdated (e); 
			}

			public void DoOnItemUpdating (FormViewUpdateEventArgs e)
			{
				OnItemUpdating (e); 
			}

			public void DoOnModeChanged (EventArgs e )
			{
				OnModeChanged (e); 
			}

			public void DoOnModeChanging (FormViewModeEventArgs  e)
			{
				OnModeChanging (e); 
			}

			public void DoOnPageIndexChanged (EventArgs e)
			{
				OnPageIndexChanged (e); 
			}

			public void DoOnPageIndexChanging (FormViewPageEventArgs e)
			{
				OnPageIndexChanging (e); 
			}

			public void DoPerformDataBinding (IEnumerable data)
			{
				PerformDataBinding (data);
			}

			protected override void PrepareControlHierarchy ()
			{

				base.PrepareControlHierarchy ();
				controlHierarchy = true;
			}

			public void DoRaisePostBackEvent (string eventArgument)
			{
				RaisePostBackEvent (eventArgument); 
			}
			
			public string Render ()
			{

				StringWriter sw = new StringWriter ();
				HtmlTextWriter tw = new HtmlTextWriter (sw);
				Render (tw);
				return sw.ToString ();

			}



			public object DoSaveControlState ()
			{
				return SaveControlState (); 
			}


			
			public void DoConfirmInitState ()
			{
				base.ConfirmInitState ();
			}
	
			public void DoOnPreRender (EventArgs e)
			{
				base.OnPreRender (e);
			}

			public void DoOnDataBinding (EventArgs e)
			{
				base.OnDataBinding (e); 
			}
			public void DoOnDataBound (EventArgs e)
			{
				base.OnDataBound (e); 
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

			public bool GetRequiresDataBinding () {
				return RequiresDataBinding;
			}
			public bool GetInitialized () {
				return Initialized;
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
		

		ArrayList myds = new ArrayList ();	
		[TestFixtureSetUp]
		public void setup ()
		{
			TestMyData.InitData();  
			myds.Add ("Item1");
			myds.Add ("Item2");
			myds.Add ("Item3");
			myds.Add ("Item4");
			myds.Add ("Item5");
			myds.Add ("Item6");

			WebTest.CopyResource (GetType (), "FormView.aspx", "FormView.aspx");
			WebTest.CopyResource (GetType (), "FormViewTest1.aspx", "FormViewTest1.aspx");
			WebTest.CopyResource (GetType (), "FormViewTest1_2.aspx", "FormViewTest1_2.aspx");
			WebTest.CopyResource (GetType (), "FormViewTest1_3.aspx", "FormViewTest1_3.aspx");
			WebTest.CopyResource (GetType (), "FormViewTest1_4.aspx", "FormViewTest1_4.aspx");
			WebTest.CopyResource (GetType (), "FormViewInsertEditDelete.aspx", "FormViewInsertEditDelete.aspx");
			WebTest.CopyResource (GetType (), "FormViewPagerVisibility.aspx", "FormViewPagerVisibility.aspx");
		}

		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();
			Assert.IsFalse (p.AllowPaging, "A1");
			Assert.AreEqual ("", p.BackImageUrl, "A2");
			Assert.IsNull (p.BottomPagerRow, "A3");
			Assert.AreEqual ("", p.Caption, "A4");
			Assert.AreEqual (TableCaptionAlign.NotSet, p.CaptionAlign, "A5");
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
		public void FormView_AssignToDefaultProperties ()
		{
			Poker p = new Poker ();
			MyTemplate customTemplate = new MyTemplate ();
			TableItemStyle tableStyle = new TableItemStyle ();			
			p.AllowPaging = true;
			Assert.AreEqual (true, p.AllowPaging, "A40");
			p.BackImageUrl = "image.jpg";
			Assert.AreEqual ("image.jpg", p.BackImageUrl, "A41");
			// ToDo: p.BottomPagerRow
			p.Caption = "Employee Details";
			Assert.AreEqual ("Employee Details", p.Caption, "A42");
			p.CaptionAlign = TableCaptionAlign.Bottom;
			Assert.AreEqual (TableCaptionAlign.Bottom, p.CaptionAlign, "A43");
			p.CaptionAlign = TableCaptionAlign.Left;
			Assert.AreEqual (TableCaptionAlign.Left, p.CaptionAlign, "A44");
			p.CaptionAlign = TableCaptionAlign.NotSet;
			Assert.AreEqual (TableCaptionAlign.NotSet, p.CaptionAlign, "A45");
			p.CaptionAlign = TableCaptionAlign.Right;
			Assert.AreEqual (TableCaptionAlign.Right, p.CaptionAlign, "A46");
			p.CaptionAlign = TableCaptionAlign.Top;
			Assert.AreEqual (TableCaptionAlign.Top, p.CaptionAlign, "A47");
			p.CellPadding = 10;
			Assert.AreEqual (10, p.CellPadding, "A48");
			p.CellSpacing = 20;
			Assert.AreEqual (20, p.CellSpacing, "A49");			
			Assert.AreEqual (FormViewMode.ReadOnly, p.CurrentMode, "A52");			
			p.DefaultMode = FormViewMode.Edit;
			Assert.AreEqual (FormViewMode.Edit, p.DefaultMode, "A53");
			p.DefaultMode = FormViewMode.Insert;
			Assert.AreEqual (FormViewMode.Insert, p.DefaultMode, "A54");
			p.DefaultMode = FormViewMode.ReadOnly;
			Assert.AreEqual (FormViewMode.ReadOnly, p.DefaultMode, "A55");
			p.EditRowStyle.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, p.EditRowStyle.BackColor, "A56");			
			p.EmptyDataRowStyle.ForeColor = Color.Purple;
			Assert.AreEqual (Color.Purple, p.EmptyDataRowStyle.ForeColor, "A57");
			p.EmptyDataTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.EmptyDataTemplate, "A58");
			p.EmptyDataText = "No data";
			Assert.AreEqual ("No data", p.EmptyDataText, "A59");
			p.EditItemTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.EditItemTemplate, "A60");
			p.FooterTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.FooterTemplate, "A61");
			p.FooterText = "Test Footer";
			Assert.AreEqual ("Test Footer", p.FooterText, "A62");
			p.FooterStyle.BorderStyle = BorderStyle.Double;
			Assert.AreEqual (BorderStyle.Double, p.FooterStyle.BorderStyle, "A63");
			p.GridLines = GridLines.Both;
			Assert.AreEqual (GridLines.Both, p.GridLines, "A64");
			p.GridLines = GridLines.Horizontal;
			Assert.AreEqual (GridLines.Horizontal, p.GridLines, "A65");
			p.GridLines = GridLines.None;
			Assert.AreEqual (GridLines.None, p.GridLines, "A66");
			p.GridLines = GridLines.Vertical;
			Assert.AreEqual (GridLines.Vertical, p.GridLines, "A67");
			p.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
			Assert.AreEqual (HorizontalAlign.Left, p.HeaderStyle.HorizontalAlign, "A68");
			p.HeaderTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.HeaderTemplate, "A69");
			p.HeaderText = "Test Header";
			Assert.AreEqual ("Test Header", p.HeaderText, "A70");
			p.HorizontalAlign = HorizontalAlign.Center;
			Assert.AreEqual (HorizontalAlign.Center, p.HorizontalAlign, "A71");
			p.HorizontalAlign = HorizontalAlign.Justify;
			Assert.AreEqual (HorizontalAlign.Justify, p.HorizontalAlign, "A72");
			p.HorizontalAlign = HorizontalAlign.Left;
			Assert.AreEqual (HorizontalAlign.Left, p.HorizontalAlign, "A73");
			p.HorizontalAlign = HorizontalAlign.NotSet;
			Assert.AreEqual (HorizontalAlign.NotSet, p.HorizontalAlign, "A74");
			p.HorizontalAlign = HorizontalAlign.Right;
			Assert.AreEqual (HorizontalAlign.Right, p.HorizontalAlign, "A75");
			p.InsertItemTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.InsertItemTemplate, "A76");
			p.InsertRowStyle.BorderStyle = BorderStyle.Outset;
			Assert.AreEqual (BorderStyle.Outset, p.InsertRowStyle.BorderStyle, "A77");
			p.ItemTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.ItemTemplate, "A78");
			p.PagerSettings.FirstPageText = "PagerSettings Test";
			Assert.AreEqual ("PagerSettings Test", p.PagerSettings.FirstPageText, "A79");
			p.PagerStyle.BorderStyle = BorderStyle.Groove;
			Assert.AreEqual (BorderStyle.Groove, p.PagerStyle.BorderStyle, "A80");
			p.PagerTemplate = customTemplate;
			Assert.AreEqual (customTemplate, p.PagerTemplate, "A81");
			p.RowStyle.ForeColor = Color.Plum;
			Assert.AreEqual (Color.Plum, p.RowStyle.ForeColor, "A82");
		}

		[Test]
		public void FormView_PageIndex ()
		{
			Poker p = new Poker ();
			Assert.AreEqual (0, p.PageIndex, "#00");
			Assert.AreEqual (false, p.GetInitialized (), "#01");
			Assert.AreEqual (false, p.GetRequiresDataBinding(), "#02");
			p.PageIndex = 2;
			Assert.AreEqual (2, p.PageIndex, "#03");
			Assert.AreEqual (false, p.GetRequiresDataBinding (), "#04");
			p.PageIndex = -1;
			Assert.AreEqual (2, p.PageIndex, "#05");
			Assert.AreEqual (false, p.GetRequiresDataBinding (), "#06");
		}

		[Test]
		[Category ("NunitWeb")]
		public void FormView_PageIndex2 ()
		{
			PageDelegates delegates = new PageDelegates ();
			delegates.Load = FormView_PageIndex2_load;
			delegates.LoadComplete = FormView_PageIndex2_loadComplete;
			PageInvoker invoker = new PageInvoker (delegates);
			WebTest test = new WebTest (invoker);
			test.Run ();
		}
		
		public static void FormView_PageIndex2_load (Page p)
		{
			Poker fv = new Poker ();
			p.Form.Controls.Add (fv);
			Assert.AreEqual (0, fv.PageIndex, "#00");
			Assert.AreEqual (false, fv.GetInitialized (), "#01");
			Assert.AreEqual (false, fv.GetRequiresDataBinding (), "#02");
			fv.PageIndex = 2;
			Assert.AreEqual (2, fv.PageIndex, "#03");
			Assert.AreEqual (false, fv.GetRequiresDataBinding (), "#04");
			fv.PageIndex = -1;
			Assert.AreEqual (2, fv.PageIndex, "#05");
			Assert.AreEqual (false, fv.GetRequiresDataBinding (), "#06");
		}
		
		public static void FormView_PageIndex2_loadComplete (Page p)
		{
			Poker fv = new Poker ();
			p.Form.Controls.Add (fv);
			Assert.AreEqual (0, fv.PageIndex, "#100");
			Assert.AreEqual (true, fv.GetInitialized (), "#101");
			Assert.AreEqual (true, fv.GetRequiresDataBinding (), "#102");
			fv.PageIndex = 2;
			Assert.AreEqual (2, fv.PageIndex, "#103");
			Assert.AreEqual (true, fv.GetRequiresDataBinding (), "#104");
			fv.PageIndex = -1;
			Assert.AreEqual (2, fv.PageIndex, "#105");
			Assert.AreEqual (true, fv.GetRequiresDataBinding (), "#106");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FormView_PageIndex_Ex ()
		{
			Poker p = new Poker ();
			p.PageIndex = -2;
		}

		[Test]
		public void FormView_ItemsProperties ()
		{
			Poker p = new Poker ();
			p.Page = new Page ();
			p.AllowPaging = true;
			p.DataSource = myds;
			p.DataBind ();
			//Assert.AreEqual (typeof (FormViewPagerRow), (p.BottomPagerRow).GetType (), "BottomPagerRow1");
			Assert.AreEqual (0, p.BottomPagerRow.ItemIndex, "BottomPagerRow2");
			Assert.AreEqual (DataControlRowType.Pager, p.BottomPagerRow.RowType, "BottomPagerRow2");
			Assert.AreEqual ("Item1", p.DataItem, "DataItem");
			Assert.AreEqual (6, p.DataItemCount, "DataItemCount");
			Assert.AreEqual (0, p.DataItemIndex, "DataItemIndex");
			Assert.AreEqual (0, p.DataItemIndex, "DataItemIndex");
			string[] str = new string[] { "1", "2", "3", "4", "5", "6" };
			Assert.AreEqual (typeof (DataKey), p.DataKey.GetType (), "DataKey");
			p.DataKeyNames = str;
			Assert.AreEqual (str, p.DataKeyNames, "DataKeyNames");
			p.ChangeMode (FormViewMode.Edit);
			Assert.AreEqual (FormViewMode.Edit, p.CurrentMode, "CurrentModeEdit");
			p.ChangeMode (FormViewMode.Insert);
			Assert.AreEqual (FormViewMode.Insert, p.CurrentMode, "CurrentModeInsert");

		}

		[Test]
		public void FormView_DefaultProtectedProperties ()
		{
			Poker fv = new Poker ();
			Assert.AreEqual (HtmlTextWriterTag.Table, fv.PokerTagKey, "TagKey");
		}

		// Protected methods

		[Test]
		public void FormView_CreateChildControls ()
		{
			Poker fv = new Poker ();
			fv.DataSource = myds;
			fv.Page = new Page ();
			Assert.AreEqual (6, fv.DoCreateChildControls (myds, true), "CreateChildControlFromDS");
			myds.Add ("item7");
			Assert.AreEqual (7, fv.DoCreateChildControls (myds, false), "CreateChildControlFromViewState");
			myds.Remove ("item7");

		}

		class MyEnumSource : IEnumerable
		{
			int _count;

			public MyEnumSource (int count) {
				_count = count;
			}

			#region IEnumerable Members

			public IEnumerator GetEnumerator () {

				for (int i = 0; i < _count; i++)
					yield return i;
			}

			#endregion
		}

		[Test]
		public void FormView_CreateChildControls2 ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.DataSource = new MyEnumSource (20);
			fv.DataBind ();
			
			Assert.AreEqual (20, fv.PageCount, "CreateChildControls#0");

			Assert.AreEqual (0, fv.DoCreateChildControls (new MyEnumSource (0), true), "CreateChildControls#1");
			Assert.AreEqual (20, fv.DoCreateChildControls (new MyEnumSource (20), true), "CreateChildControls#2");

			Assert.AreEqual (0, fv.DoCreateChildControls (new object [0], false), "CreateChildControls#3");
			Assert.AreEqual (5, fv.DoCreateChildControls (new object [5], false), "CreateChildControls#4");
		}

		[Test]
		public void FormView_CreateDataSourceSelectArguments ()
		{
			//Checks the default DataSourceSelectArgument object returned.
			Poker fv = new Poker ();
			DataSourceSelectArguments selectArgs = fv.DoCreateDataSourceSelectArguments ();
			Assert.AreEqual (0, selectArgs.MaximumRows, "CreateDataSourceSelectArguments1");
			Assert.AreEqual (false, selectArgs.RetrieveTotalRowCount, "CreateDataSourceSelectArguments2");						

		}

		[Test]
		public void FormView_CreateControlStyle ()
		{
			Poker fv = new Poker ();
			Style s = fv.DoCreateControlStyle ();
			Assert.AreEqual (typeof (TableStyle), s.GetType (), "CreateControlStyle1");
			Assert.AreEqual (GridLines.None, ((TableStyle) s).GridLines, "CreateControlStyle2");
			Assert.AreEqual ("", ((TableStyle) s).BackImageUrl, "CreateControlStyle3");
			Assert.AreEqual (0, ((TableStyle) s).CellSpacing, "CreateControlStyle4");
			Assert.AreEqual (-1, ((TableStyle) s).CellPadding, "CreateControlStyle5");

		}

		[Test]
		public void FormView_InitializeRow ()
		{
			//not implemented
		}

		[Test]
		public void FormView_InitializePager ()
		{
			Poker fv = new Poker ();
			Page page = new Page ();
			page.Controls.Add (fv);
			fv.AllowPaging = true;
			fv.DataSource = myds;
			Assert.AreEqual (false, fv.isInitializePager, "BeforeInitializePager");
			Assert.AreEqual (0, fv.PageCount, "BeforeInitializePagerPageCount");
			fv.DataBind ();
			Assert.AreEqual (true, fv.isInitializePager, "AfterInitializePager");
			Assert.AreEqual (6, fv.PageCount, "AfterInitializePagerPageCount");
		}
		
		[Test]
		public void FormView_CreateRow ()
		{
			Poker fv = new Poker ();
			fv.AllowPaging =true;
			fv.DataSource = myds;
			fv.Page = new Page ();
			fv.DataBind ();
			FormViewRow row = fv.DoCreateRow (2,DataControlRowType.DataRow ,DataControlRowState.Normal );
			Assert.AreEqual (2, row.ItemIndex, "CreatedRowItemIndex1");
			Assert.AreEqual (DataControlRowState.Normal , row.RowState, "CreatedRowState1");
			Assert.AreEqual (DataControlRowType.DataRow , row.RowType, "CreatedRowType1");			 
			row = fv.DoCreateRow (4, DataControlRowType.Footer, DataControlRowState.Edit);
			Assert.AreEqual (4, row.ItemIndex, "CreatedRowItemIndex2");
			Assert.AreEqual (DataControlRowState.Edit , row.RowState, "CreatedRowState2");
			Assert.AreEqual (DataControlRowType.Footer , row.RowType, "CreatedRowType2");
			//FormViewPagerRow pagerRow = (FormViewPagerRow)fv.DoCreateRow (3, DataControlRowType.Pager , DataControlRowState.Insert);
			//Assert.AreEqual (3, pagerRow.ItemIndex, "CreatedPageRowItemIndex");
			//Assert.AreEqual (DataControlRowState.Insert, pagerRow.RowState, "CreatedPageRowState");
			//Assert.AreEqual (DataControlRowType.Pager, pagerRow.RowType, "CreatedPageRowType");			 
			
		}

		[Test]
		public void FormView_CreateTable ()
		{
			Poker fv = new Poker ();
			Table tb = fv.DoCreateTable ();
			fv.Page = new Page ();
			Assert.AreEqual ("", tb.BackImageUrl , "CreateTable1");
			Assert.AreEqual (0, tb.Rows.Count, "CreateTable2");
			fv.DataSource = myds;
			fv.DataBind ();			
			fv.ID = "TestFormView";
			tb = fv.DoCreateTable ();
			Assert.AreEqual (-1, tb.CellPadding , "CreateTable3");			

		}

		[Test]
		public void FormView_EnsureDataBound ()
		{
			Poker fv = new Poker ();			
			fv.DataSource = myds;			
			fv.DoOnPreRender (EventArgs.Empty);
			Assert.AreEqual (true, fv.ensureDataBound, "EnsureDataBound");
			
		}

		[Test]
		public void FormView_PerformDataBinding ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			Assert.AreEqual (0,fv.DataItemCount, "BeforePerformDataBinding"); 
			fv.DoPerformDataBinding (myds);
			Assert.AreEqual (6, fv.DataItemCount, "AfterPerformDataBinding"); 
		}

		[Test]
		public void FormView_ExtractRowValues ()
		{
			Poker fv=new Poker ();
			fv.ItemTemplate = new MyTemplate ();
			fv.DataKeyNames = new string[] { "ID", "FName", "LName" };
			//IOrderedDictionary dict = (IOrderedDictionary) new OrderedDictionary (0x19);
			//fv.DoExtractRowValues (dict, true);			
			//DataTable ds = CreateDataTable ();
			//fv.DataSource = ds;
			//fv.DataBind ();
			//OrderedDictionary fieldsValues = new OrderedDictionary ();
			//fv.DoExtractRowValues (fieldsValues, true);
			//Assert.AreEqual (3, fieldsValues.Count, "ExtractRowValues1");
			//Assert.AreEqual (3, fieldsValues.Keys.Count, "ExtractRowValues2");
			//Assert.AreEqual (3, fieldsValues.Values.Count, "ExtractRowValues3");
			//Assert.AreEqual (true, fieldsValues.Contains ("ID"), "ExtractRowValues4");
			//IDictionaryEnumerator enumerator = fieldsValues.GetEnumerator ();
			//enumerator.MoveNext ();
			//Assert.AreEqual ("ID", enumerator.Key, "FieldValue1");
			//Assert.AreEqual ("1001", enumerator.Value, "FieldValue2");
			//enumerator.MoveNext ();
			//Assert.AreEqual ("FName", enumerator.Key, "FieldValue3");
			//Assert.AreEqual ("Mahesh", enumerator.Value, "FieldValue4");
			//enumerator.MoveNext ();
			//Assert.AreEqual ("LName", enumerator.Key, "FieldValue5");
			//Assert.AreEqual ("Chand", enumerator.Value, "FieldValue6");		
  
		}

		[Test]
		public void FormView_PrepareControlHierarcy ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.controlHierarchy = false;
			fv.Render ();
			Assert.AreEqual (0, fv.Controls.Count, "ControlHierarchy1");
			Assert.AreEqual (true, fv.controlHierarchy, "ControlHierarchy2");
			fv.AllowPaging = true;
			fv.DataSource = myds;
			fv.DataBind ();
			fv.controlHierarchy = false;
			fv.Render ();
			Assert.AreEqual (1, fv.Controls.Count, "ControlHierarchy3");
			Assert.AreEqual (true, fv.controlHierarchy, "ControlHierarchy4");


		}

		//Public Methods

		[Test]
		public void FormView_ChangeMode ()
		{
			Poker fv = new Poker ();
			Assert.AreEqual (FormViewMode.ReadOnly, fv.CurrentMode, "ChangeModeDefault");
			fv.ChangeMode (FormViewMode.Insert);
			Assert.AreEqual (FormViewMode.Insert, fv.CurrentMode, "ChangeModeInsert");
			fv.ChangeMode (FormViewMode.Edit);
			Assert.AreEqual (FormViewMode.Edit, fv.CurrentMode, "ChangeModeEdit");
			fv.ChangeMode (FormViewMode.ReadOnly);
			Assert.AreEqual (FormViewMode.ReadOnly, fv.CurrentMode, "ChangeModeReadOnly");
		}

		[Test]
		public void FormView_PageCount () {
			Page p = new Page ();

			Poker fv = new Poker ();
			p.Controls.Add (fv);

			ObjectDataSource data = new ObjectDataSource ();
			data.TypeName = typeof (FormViewDataObject).AssemblyQualifiedName;
			data.SelectMethod = "Select";
			p.Controls.Add (data);

			fv.DataSource = data;

			Assert.AreEqual (0, fv.PageCount, "PageCount before binding");

			fv.DataBind ();
			
			Assert.AreEqual (3, fv.PageCount, "PageCount after binding");
		}

		[Test]
		public void FormView_DataKey ()
		{
			Page p = new Page ();

			Poker fv = new Poker ();
			p.Controls.Add (fv);

			ObjectDataSource data = new ObjectDataSource ();
			data.TypeName = typeof (FormViewDataObject).AssemblyQualifiedName;
			data.SelectMethod = "Select";
			p.Controls.Add (data);

			fv.DataSource = data;
			fv.DataKeyNames = new string [] { "ID", "FName" };

			DataKey key1 = fv.DataKey;

			Assert.AreEqual (null, key1.Value, "DataKey.Value before binding");
			Assert.AreEqual (0, key1.Values.Count, "DataKey.Values count before binding");

			fv.DataBind ();

			DataKey key2 = fv.DataKey;
			DataKey key3 = fv.DataKey;

			Assert.IsFalse (Object.ReferenceEquals (key1, key2), "DataKey returns the same instans");
			Assert.IsTrue (Object.ReferenceEquals (key2, key3), "DataKey returns the same instans");
			
			Assert.AreEqual (1001, key1.Value, "DataKey.Value after binding");
			Assert.AreEqual (2, key1.Values.Count, "DataKey.Values count after binding");
			Assert.AreEqual (1001, key1.Values [0], "DataKey.Values[0] after binding");
			Assert.AreEqual ("Mahesh", key1.Values [1], "DataKey.Values[1] after binding");

			Poker copy = new Poker ();
			object state = fv.DoSaveControlState ();
			copy.DoLoadControlState (state);

			DataKey key4 = copy.DataKey;

			Assert.AreEqual (1001, key4.Value, "DataKey.Value from ViewState");
			Assert.AreEqual (2, key4.Values.Count, "DataKey.Values count from ViewState");
			Assert.AreEqual (1001, key4.Values [0], "DataKey.Values[0] from ViewState");
			Assert.AreEqual ("Mahesh", key4.Values [1], "DataKey.Values[1] from ViewState");
		}

		[Test]
		public void FormView_DataBind ()
		{
			Poker fv = new Poker ();
			fv.AllowPaging = true;
			fv.DataSource = myds;
			fv.Page = new Page ();
			Assert.AreEqual (0, fv.PageCount, "BeforeDataBind1");
			Assert.AreEqual (null, fv.DataItem, "BeforeDataBind2");
			fv.DataBind ();
			Assert.AreEqual (6, fv.PageCount, "AfterDataBind1");
			Assert.AreEqual (6, fv.DataItemCount, "AfterDataBind2");
			Assert.AreEqual ("Item1", fv.DataItem, "AfterDataBind3");
		}

		private bool isDeleted = false;

		[Test]
		public void FormView_DeleteItem ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.DataSource = myds;
			fv.DataBind ();
			Assert.AreEqual (false, isDeleted, "BeforeDeleteItem");
			fv.ItemDeleting += new FormViewDeleteEventHandler (fv_DeleteingHandler);
			fv.DeleteItem ();
			Assert.AreEqual (true, isDeleted, "BeforeDeleteItem");

		}

		public void fv_DeleteingHandler (Object sender, FormViewDeleteEventArgs e)
		{
			isDeleted = true;
		}

		private bool insertItem = false;

		[Test]
		public void FormView_InsertItem ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.ChangeMode (FormViewMode.Insert);
			fv.ItemInserting += new FormViewInsertEventHandler (insert_item);
			Assert.AreEqual (false, insertItem, "BeforeInsertItem");
			fv.InsertItem (false);
			Assert.AreEqual (true, insertItem, "AfterInsertItem");

		}

		public void insert_item (object sender, FormViewInsertEventArgs e)
		{
			insertItem = true;
		}

		private bool updateItem = false;
		[Test]
		public void FormView_UpdateItem ()
		{
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.DataSource = myds;
			fv.DataBind ();
			fv.ChangeMode (FormViewMode.Edit);
			fv.ItemUpdating += new FormViewUpdateEventHandler (update_item);
			Assert.AreEqual (false, updateItem, "BeforeUpdateItem");
			fv.UpdateItem (false);
			Assert.AreEqual (true, updateItem, "AfterUpdateItem");

		}

		public void update_item (object sender, FormViewUpdateEventArgs e)
		{
			updateItem = true;
		}

		[Test]
		public void FormView_IsBindableType ()
		{
			bool isBindable = false;
			Poker fv = new Poker ();
			isBindable = fv.IsBindableType (typeof (Decimal));
			Assert.AreEqual (true, isBindable, "IsBindableTypeDecimal");
			isBindable = fv.IsBindableType (typeof (Int32));
			Assert.AreEqual (true, isBindable, "IsBindableTypeInt32");
			isBindable = fv.IsBindableType (typeof (String));
			Assert.AreEqual (true, isBindable, "IsBindableTypeString");
			isBindable = fv.IsBindableType (typeof (Boolean));
			Assert.AreEqual (true, isBindable, "IsBindableTypeBoolean");
			isBindable = fv.IsBindableType (typeof (DateTime));
			Assert.AreEqual (true, isBindable, "IsBindableTypeDateTime");
			isBindable = fv.IsBindableType (typeof (Byte));
			Assert.AreEqual (true, isBindable, "IsBindableTypeByte");
			isBindable = fv.IsBindableType (typeof (Guid));
			Assert.AreEqual (true, isBindable, "IsBindableTypeGuid");
			isBindable = fv.IsBindableType (typeof (MyTemplate));
			Assert.AreEqual (false, isBindable, "IsBindableTypeMyTemplate");
		}

		[Test]
		public void FormView_ControlState ()		{

			Poker fv = new Poker ();
			Poker copy = new Poker ();
			string[] keys = new String[2];
			keys[0] = "key1";
			keys[1] = "key2";
			fv.DataKeyNames = keys;
			fv.BackImageUrl = "photo.jpg";			
			fv.DefaultMode  = FormViewMode.Insert  ;
			fv.ChangeMode (FormViewMode.Edit);
			object state = fv.DoSaveControlState ();
			copy.DoLoadControlState (state);
			Assert.AreEqual (2, copy.DataKeyNames.Length, "DataKeyNames.Length");
			Assert.AreEqual ("key1", copy.DataKeyNames[0], "ControlStateDataKeyValue");
			Assert.AreEqual ("key2", copy.DataKeyNames[1], "ControlStateDataKeyValue2");			
			Assert.AreEqual (FormViewMode.Insert, copy.DefaultMode, "ControlStateDefaultMode");
			Assert.AreEqual (FormViewMode.Edit, copy.CurrentMode, "ControlStateCurrentMode");

		}

		//ViewState
		[Test]
		public void FormView_ViewState ()
		{
			Poker fv = new Poker ();
			Poker copy = new Poker ();
			fv.AllowPaging = true;
			fv.HeaderText = "Testing";
			fv.CssClass = "style.css";
			object state = fv.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual (true, copy.AllowPaging, "ViewStateAllowPaging");
			Assert.AreEqual ("Testing", copy.HeaderText, "ViewStateHeaderText");
			Assert.AreEqual ("style.css", copy.CssClass, "ViewStateCssClass");
		}

		//Events 
		private bool init;
		private bool itemCommand;
		private bool itemCreated;
		private bool itemDeleted;
		private bool itemDeleting;
		private bool itemInserted;
		private bool itemInserting;
		private bool itemUpdated;
		private bool itemUpdating;
		private bool modeChanged;
		private bool modeChanging;
		private bool pageIndexChanged;
		private bool pageIndexChanging;
		private int newPageIndex;

		public void ResetEvents ()
		{
			init = false;
			itemCommand = false;
			itemCreated = false;
			itemDeleted = false;
			itemDeleting = false;
			itemInserted = false;
			itemInserting = false;
			itemUpdated = false;
			itemUpdating = false;
			modeChanged = false;
			modeChanging = false;
			pageIndexChanged = false;
			pageIndexChanging = false;
		}

		[Test]
		public void FormView_BubbleEvent ()
		{
			FormViewCommandEventArgs com;
			Poker fv = new Poker ();
			Page page = new Page ();
			Button bt = new Button ();
			fv.AllowPaging = true;
			fv.DataSource = myds;
			page.Controls.Add (fv);
			fv.DataBind ();
			ResetEvents ();
			fv.ItemCommand += new FormViewCommandEventHandler (fv_ItemCommand);
			fv.ItemDeleted += new FormViewDeletedEventHandler (fv_ItemDeleted);
			//Delete
			fv.ItemDeleting += new FormViewDeleteEventHandler (fv_ItemDeleting);
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Delete", null));
			Assert.AreEqual (false, itemDeleting, "BeforeDeleteCommandBubbleEvent");
			Assert.AreEqual (false, itemCommand, "BeforeDeleteBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Delete");
			Assert.AreEqual (true, itemDeleting, "AfterDeleteBubbleEvent");
			Assert.AreEqual (true, itemCommand, "AfterDeleteCommandBubbleEvent");


			//Insert
			itemCommand = false;
			fv.ItemInserting += new FormViewInsertEventHandler (fv_ItemInserting);
			fv.ChangeMode (FormViewMode.Insert);
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Insert", null));
			Assert.AreEqual (false, itemCommand, "BeforeInsertCommandBubbleEvent");
			Assert.AreEqual (false, itemInserting, "BeforeInsertBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Insert");
			Assert.AreEqual (true, itemCommand, "AfterInsertCommandBubbleEvent");
			Assert.AreEqual (true, itemInserting, "AfterInsertBubbleEvent");


			//Update
			itemCommand = false;
			fv.ItemUpdating += new FormViewUpdateEventHandler (fv_ItemUpdating);
			fv.ChangeMode (FormViewMode.Edit);
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Update", null));
			Assert.AreEqual (false, itemUpdating, "BeforeUpdateEvent");
			Assert.AreEqual (false, itemCommand, "BeforeUpdateCommandEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Update");
			Assert.AreEqual (true, itemCommand, "AfterUpdateCommandBubbleEvent");
			Assert.AreEqual (true, itemUpdating, "AfterUpdateBubbleEvent");


			//Cancel 
			itemCommand = false;
			fv.ModeChanging += new FormViewModeEventHandler (fv_ModeChanging);
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Cancel", null));
			Assert.AreEqual (false, itemCommand, "BeforeCancelCommandBubbleEvent");
			Assert.AreEqual (false, modeChanging, "BeforeCancelBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Cancel");
			Assert.AreEqual (true, itemCommand, "AfterCancelCommandBubbleEvent");
			Assert.AreEqual (true, modeChanging, "AfterCancelBubbleEvent");

			//Edit
			itemCommand = false;
			modeChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Edit", null));
			Assert.AreEqual (false, itemCommand, "BeforeEditCommandBubbleEvent");
			Assert.AreEqual (false, modeChanging, "BeforeEditBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Edit");
			Assert.AreEqual (true, itemCommand, "AfterEditCommandBubbleEvent");
			Assert.AreEqual (true, modeChanging, "AfterEditBubbleEvent");

			//New
			itemCommand = false;
			modeChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("New", null));
			Assert.AreEqual (false, itemCommand, "BeforeNewCommandBubbleEvent");
			Assert.AreEqual (false, modeChanging, "BeforeNewBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - New");
			Assert.AreEqual (true, itemCommand, "AfterNewCommandBubbleEvent");
			Assert.AreEqual (true, modeChanging, "AfterNewBubbleEvent");

			//Page Index default
			itemCommand = false;
			fv.PageIndexChanging += new FormViewPageEventHandler (fv_PageIndexChanging);
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Page", null));
			Assert.AreEqual (false, itemCommand, "BeforePageCommandBubbleEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforePageBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Page Index default");
			Assert.AreEqual (true, itemCommand, "AfterPageCommandBubbleEvent");
			Assert.AreEqual (true, pageIndexChanging, "AfterPageBubbleEvent");
			Assert.AreEqual (-1, newPageIndex, "PageIndex");

			//Next Page
			itemCommand = false;
			pageIndexChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Page", "Next"));
			Assert.AreEqual (false, itemCommand, "BeforeNextPageCommandBubbleEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforeNextPageBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Next Page");
			Assert.AreEqual (true, itemCommand, "AfterNextPageCommandBubbleEvent");
			Assert.AreEqual (true, pageIndexChanging, "AfterNextPageBubbleEvent");
			Assert.AreEqual (1, newPageIndex, "NextPageIndex");

			//Prev Page
			itemCommand = false;
			pageIndexChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Page", "Prev"));
			Assert.AreEqual (false, itemCommand, "BeforePrevPageCommandBubbleEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforePrevPageBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Prev Page");
			Assert.AreEqual (true, itemCommand, "AfterPrevPageCommandBubbleEvent");
			Assert.AreEqual (true, pageIndexChanging, "AfterPrevPageBubbleEvent");
			Assert.AreEqual (-1, newPageIndex, "PrevPageIndex");

			//First Page
			itemCommand = false;
			pageIndexChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Page", "First"));
			Assert.AreEqual (false, itemCommand, "BeforeFirstPageCommandBubbleEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforeFirstPageBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - First Page");
			Assert.AreEqual (true, itemCommand, "AfterFirstPageCommandBubbleEvent");
			Assert.AreEqual (true, pageIndexChanging, "AfterFirstPageBubbleEvent");
			Assert.AreEqual (0, newPageIndex, "FirstPageIndex");

			//Last Page
			itemCommand = false;
			pageIndexChanging = false;
			com = new FormViewCommandEventArgs (bt, new CommandEventArgs ("Page", "Last"));
			Assert.AreEqual (false, itemCommand, "BeforeLastPageCommandBubbleEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforeLastPageBubbleEvent");
			Assert.IsTrue (fv.DoOnBubbleEvent (bt, com), "OnBubbleEvent - Last Page");
			Assert.AreEqual (true, itemCommand, "AfterLastPageCommandBubbleEvent");
			Assert.AreEqual (true, pageIndexChanging, "AfterLastPageBubbleEvent");
			Assert.AreEqual (5, newPageIndex, "FirstPageIndex");

		}

		[Test]
		[Category("NunitWeb")]
		public void FormView_DataSourceChangedEvent ()
		{
			WebTest t = new WebTest();
			PageDelegates pd = new PageDelegates ();
			pd.Load = FormView_Init;
			pd.PreRenderComplete = FormView_Load;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
			if (t.UserData == null)
				Assert.Fail ("DataSourceChangedEvent#1");
			Assert.AreEqual ("Data_rebounded", t.UserData.ToString (), "DataSourceChangedEvent#2");
		}

		#region FormView_DataSourceChangedEvent
		public static void FormView_Init(Page p)
		{
			Poker fv = new Poker ();
			DS data = new DS ();
			p.Controls.Add (fv);
			p.Controls.Add (data);
			data.TypeName = typeof (DS).AssemblyQualifiedName;
			data.SelectMethod = "GetList";
			data.ID = "Data";
			fv.DataBinding += new EventHandler (data_DataBinding);
			fv.DataSourceID = "Data";
		}

		public static void FormView_Load (Page p)
		{
			if (p.IsPostBack) {
				DS data = (DS) p.FindControl ("Data") ;
				if (data == null)
					Assert.Fail ("Data soource control not created#1");
				data.DoRaiseDataSourceChangedEvent (new EventArgs ());
			}
		}

		public static void data_DataBinding (object sender, EventArgs e)
		{
			if (((WebControl) sender).Page.IsPostBack) 
				WebTest.CurrentTest.UserData = "Data_rebounded";
		}
		#endregion

		[Test]
		public void FormView_Events ()
		{
			ResetEvents ();
			Poker fv = new Poker ();
			fv.Page = new Page ();
			fv.Init += new EventHandler (fv_Init);
			fv.ItemCommand += new FormViewCommandEventHandler (fv_ItemCommand);
			fv.ItemCreated += new EventHandler (fv_ItemCreated);
			fv.ItemDeleted += new FormViewDeletedEventHandler (fv_ItemDeleted);
			fv.ItemDeleting += new FormViewDeleteEventHandler (fv_ItemDeleting);
			fv.ItemInserted += new FormViewInsertedEventHandler (fv_ItemInserted);
			fv.ItemInserting += new FormViewInsertEventHandler (fv_ItemInserting);
			fv.ItemUpdated += new FormViewUpdatedEventHandler (fv_ItemUpdated);
			fv.ItemUpdating += new FormViewUpdateEventHandler (fv_ItemUpdating);
			fv.ModeChanged += new EventHandler (fv_ModeChanged);
			fv.ModeChanging += new FormViewModeEventHandler (fv_ModeChanging);
			fv.PageIndexChanged += new EventHandler (fv_PageIndexChanged);
			fv.PageIndexChanging += new FormViewPageEventHandler (fv_PageIndexChanging);

			Assert.AreEqual (false, init, "BeforeInit");
			fv.DoOnInit (new EventArgs ());
			Assert.AreEqual (true, init, "AfterInit");
			Assert.AreEqual (false, itemCommand, "BeforeItemCommandEvent");
			Button bt = new Button ();
			fv.DoOnItemCommand (new FormViewCommandEventArgs (bt, new CommandEventArgs ("", null)));
			Assert.AreEqual (true, itemCommand, "AfterItemCommandEvent");
			Assert.AreEqual (false, itemCreated, "BeforeItemCreatedEvent");
			fv.DoOnItemCreated (new EventArgs ());
			Assert.AreEqual (true, itemCreated, "AfterItemCreatedEvent");
			Assert.AreEqual (false, itemDeleted, "BeforeItemDeletedEvent");
			fv.DoOnItemDeleted (new FormViewDeletedEventArgs (3, new Exception ()));
			Assert.AreEqual (true, itemDeleted, "AfterItemDeletedEvent");
			Assert.AreEqual (false, itemDeleting, "BeforeItemDeletingEvent");
			fv.DoOnItemDeleting (new FormViewDeleteEventArgs (1));
			Assert.AreEqual (true, itemDeleting, "AfterItemDeletingEvent");
			Assert.AreEqual (false, itemInserted, "BeforeItemInsertedEvent");
			fv.DoOnItemInserted (new FormViewInsertedEventArgs (2, new Exception ()));
			Assert.AreEqual (true, itemInserted, "AfterItemInsetedEvent");
			Assert.AreEqual (false, itemInserting, "BeforeItemInsertingEvent");
			fv.DoOnItemInserting (new FormViewInsertEventArgs (bt));
			Assert.AreEqual (true, itemInserting, "AfterItemInsetingEvent");
			Assert.AreEqual (false, itemUpdated, "BeforeItemUpdatedEvent");
			fv.DoOnItemUpdated (new FormViewUpdatedEventArgs (1, new Exception ()));
			Assert.AreEqual (true, itemUpdated, "AfterItemUpdatedEvent");
			Assert.AreEqual (false, itemUpdating, "BeforeItemUpdatingEvent");
			fv.DoOnItemUpdating (new FormViewUpdateEventArgs (bt));
			Assert.AreEqual (true, itemUpdating, "AfterItemUpdatingEvent");
			Assert.AreEqual (false, modeChanged, "BeforeModeChangedEvent");
			fv.DoOnModeChanged (new EventArgs ());
			Assert.AreEqual (true, modeChanged, "AfterModeChangedEvent");
			Assert.AreEqual (false, modeChanging, "BeforeModeChangingEvent");
			fv.DoOnModeChanging (new FormViewModeEventArgs (FormViewMode.Edit, true));
			Assert.AreEqual (true, modeChanging, "AfterModeChangingEvent");
			Assert.AreEqual (false, pageIndexChanged, "BeforePageIndexChangedEvent");
			fv.DoOnPageIndexChanged (new EventArgs ());
			Assert.AreEqual (true, pageIndexChanged, "AfterPageIndexChangedEvent");
			Assert.AreEqual (false, pageIndexChanging, "BeforePageIndexChangingEvent");
			fv.DoOnPageIndexChanging (new FormViewPageEventArgs (1));
			Assert.AreEqual (true, pageIndexChanging, "AfterPageIndexChangingEvent");
		}
		private void fv_Init (object sender, EventArgs e)
		{
			init = true;
		}

		private void fv_ItemCommand (object sender, FormViewCommandEventArgs e)
		{
			itemCommand = true;
		}

		private void fv_ItemCreated (object sender, EventArgs e)
		{
			itemCreated = true;
		}

		private void fv_ItemDeleted (object sender, FormViewDeletedEventArgs e)
		{
			itemDeleted = true;
		}

		private void fv_ItemDeleting (object sender, FormViewDeleteEventArgs e)
		{
			itemDeleting = true;
		}

		private void fv_ItemInserted (object sender, FormViewInsertedEventArgs e)
		{
			itemInserted = true;
		}

		private void fv_ItemInserting (object sender, FormViewInsertEventArgs e)
		{
			itemInserting = true;
		}

		private void fv_ItemUpdated (object sender, FormViewUpdatedEventArgs e)
		{
			itemUpdated = true;
		}

		private void fv_ItemUpdating (object sender, FormViewUpdateEventArgs e)
		{
			itemUpdating = true;
		}

		private void fv_ModeChanged (object sender, EventArgs e)
		{
			modeChanged = true;
		}

		private void fv_ModeChanging (object sender, FormViewModeEventArgs e)
		{
			modeChanging = true;
		}

		private void fv_PageIndexChanged (object sender, EventArgs e)
		{
			pageIndexChanged = true;
		}
		private void fv_PageIndexChanging (object sender, FormViewPageEventArgs e)
		{
			pageIndexChanging = true;
			newPageIndex = e.NewPageIndex;
			e.NewPageIndex = -1;
		}

		//Exceptions		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellPaddingException ()
		{
		       Poker p = new Poker ();
			p.CellPadding = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellSpacingException ()
		{
			Poker p = new Poker ();
			p.CellSpacing = -5;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PageIndexException ()
		{
			Poker p = new Poker ();
			p.PageIndex = -5;
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void InsertItemException ()
		{
			Poker p = new Poker ();
			p.InsertItem (true); 
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void UpdateItemException ()
		{
			Poker p = new Poker ();
			p.UpdateItem (true);
		}

		
		[Test]
		[Category ("NotDotNet")] //TODO
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
			WebTest.Unload ();
		}

		[Test]
		[Category("NunitWeb")]
		public void FormViewCssClass ()
		{
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




		[Test]
		[Category ("NunitWeb")]
//#if TARGET_JVM //BUG #6518
//                [Category ("NotWorking")]
//#endif
		public void FormView_RenderSimpleTemplate()
		{
			string renderedPageHtml = new WebTest ("FormViewTest1.aspx").Run ();
			string newHtmlValue = HtmlDiff.GetControlFromPageHtml (renderedPageHtml);
			string origHtmlValue = "<table cellspacing=\"2\" cellpadding=\"3\" rules=\"all\" border=\"1\" id=\"FormView1\" style=\"background-color:#DEBA84;border-color:#DEBA84;border-width:1px;border-style:None;\">\r\n\t<tr style=\"color:#8C4510;background-color:#FFF7E7;\">\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView1_Label1\">1</span>\n            </td>\r\n\t</tr><tr align=\"center\" style=\"color:#8C4510;\">\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView1','Page$2')\" style=\"color:#8C4510;\">2</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$3')\" style=\"color:#8C4510;\">3</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$4')\" style=\"color:#8C4510;\">4</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$5')\" style=\"color:#8C4510;\">5</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$6')\" style=\"color:#8C4510;\">6</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";         
	
			HtmlDiff.AssertAreEqual (origHtmlValue, newHtmlValue, "RenderSimpleTemplate");                  
		}

		[Test]
		[Category ("NunitWeb")]
//#if TARGET_JVM //BUG #6518
//                [Category ("NotWorking")]
//#endif
		public void FormView_RenderFooterAndPager()
		{
			string renderedPageHtml = new WebTest ("FormViewTest1_2.aspx").Run ();
			string newHtmlValue = HtmlDiff.GetControlFromPageHtml (renderedPageHtml);
			string origHtmlValue = "<table cellspacing=\"0\" cellpadding=\"4\" border=\"0\" id=\"FormView2\" style=\"color:#333333;border-collapse:collapse;\">\r\n\t<tr style=\"color:#333333;background-color:#F7F6F3;\">\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView2_Label2\">1</span>\n            </td>\r\n\t</tr><tr style=\"color:White;background-color:#5D7B9D;font-weight:bold;\">\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView2_Label3\">Footer Template Test</span>\n            </td>\r\n\t</tr><tr align=\"center\" style=\"color:White;background-color:#284775;\">\r\n\t\t<td colspan=\"2\">\n                <input type=\"submit\" name=\"FormView2$ctl01$Button1\" value=\"Prev Item\" id=\"FormView2_ctl01_Button1\" />\n                <input type=\"submit\" name=\"FormView2$ctl01$Button2\" value=\"Next Item\" id=\"FormView2_ctl01_Button2\" />\n                <input type=\"submit\" name=\"FormView2$ctl01$Button3\" value=\"First Item\" id=\"FormView2_ctl01_Button3\" />\n                <input type=\"submit\" name=\"FormView2$ctl01$Button4\" value=\"Last Item\" id=\"FormView2_ctl01_Button4\" />\n            </td>\r\n\t</tr>\r\n</table>";    
			HtmlDiff.AssertAreEqual (origHtmlValue, newHtmlValue, "FormView_RenderFooterAndPager");
		}

		[Test]
		[Category ("NunitWeb")]
//#if TARGET_JVM //BUG #6518
//                [Category ("NotWorking")]
//#endif
		public void FormView_RenderWithHeader()
		{
			string renderedPageHtml = new WebTest ("FormViewTest1_4.aspx").Run ();
			string newHtmlValue = HtmlDiff.GetControlFromPageHtml (renderedPageHtml);
			string origHtmlValue = "<table cellspacing=\"10\" cellpadding=\"3\" align=\"Right\" rules=\"all\" border=\"1\" id=\"FormView4\" style=\"background-color:White;border-color:#CCCCCC;border-width:1px;border-style:None;\">\r\n\t<tr align=\"left\" style=\"color:White;background-color:#006699;font-weight:bold;\">\r\n\t\t<td colspan=\"2\">Using Header Text property</td>\r\n\t</tr><tr align=\"center\" style=\"color:#000066;background-color:Maroon;\">\r\n\t\t<td colspan=\"2\">Using Footer Text property</td>\r\n\t</tr><tr align=\"left\" style=\"color:#000066;background-color:LightGrey;\">\r\n\t\t<td colspan=\"2\">\n                <a id=\"FormView4_ctl01_LinkButton1\" href=\"javascript:__doPostBack('FormView4$ctl01$LinkButton1','')\">Next</a>\n                <a id=\"FormView4_ctl01_LinkButton2\" href=\"javascript:__doPostBack('FormView4$ctl01$LinkButton2','')\">Prev</a>\n                <span id=\"FormView4_ctl01_Label7\">Page Index: 0</span>\n            </td>\r\n\t</tr>\r\n</table>"; 			
			HtmlDiff.AssertAreEqual (origHtmlValue, newHtmlValue, "RenderingDefaultPaging");
		}


		[Test]
		[Category ("NunitWeb")]
//#if TARGET_JVM //BUG #6518
//                [Category ("NotWorking")]
//#endif
		public void FormView_Render ()
		{
			string RenderedPageHtml = new WebTest ("FormViewTest1_3.aspx").Run ();
			string newHtmlValue = HtmlDiff.GetControlFromPageHtml (RenderedPageHtml);
			string origHtmlValue = "<table cellspacing=\"0\" cellpadding=\"2\" border=\"0\" id=\"FormView3\" style=\"color:Black;background-color:LightGoldenrodYellow;border-color:Tan;border-width:1px;border-style:solid;border-collapse:collapse;\">\r\n\t<tr align=\"center\" valign=\"top\" style=\"color:#C00000;background-color:Tan;font-weight:bold;\">\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView3_Label5\">Header Template Test</span>\n            </td>\r\n\t</tr><tr>\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView3_Label4\">1</span>\n            </td>\r\n\t</tr><tr align=\"right\" style=\"color:#FFC0FF;background-color:Tan;\">\r\n\t\t<td colspan=\"2\">\n                <span id=\"FormView3_Label6\">FormView Footer</span>\n            </td>\r\n\t</tr><tr align=\"center\" style=\"color:DarkSlateBlue;background-color:PaleGoldenrod;\">\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView3','Page$2')\" style=\"color:DarkSlateBlue;\">2</a></td><td><a href=\"javascript:__doPostBack('FormView3','Page$3')\" style=\"color:DarkSlateBlue;\">3</a></td><td><a href=\"javascript:__doPostBack('FormView3','Page$4')\" style=\"color:DarkSlateBlue;\">4</a></td><td><a href=\"javascript:__doPostBack('FormView3','Page$5')\" style=\"color:DarkSlateBlue;\">5</a></td><td><a href=\"javascript:__doPostBack('FormView3','Page$6')\" style=\"color:DarkSlateBlue;\">6</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";        
			HtmlDiff.AssertAreEqual (origHtmlValue, newHtmlValue, "RenderingDefaultPaging");
		}
		

		[Test]
		[Category ("NunitWeb")]
		public void FormView_EditPostback ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			string pageHTML = t.Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (pageHTML);
			string origHtml = "<table cellspacing=\"0\" border=\"0\" id=\"FormView1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td colspan=\"2\">\n                    <span id=\"FormView1_ID\">1001</span>&nbsp;\n                    <span id=\"FormView1_LName\">Chand</span>\n                    <span id=\"FormView1_FName\">Mahesh</span>&nbsp;\n                    <a id=\"FormView1_EditButton\" href=\"javascript:__doPostBack('FormView1$EditButton','')\">Edit</a>\n                    <a id=\"FormView1_NewButton\" href=\"javascript:__doPostBack('FormView1$NewButton','')\">New</a>\n                    <a id=\"FormView1_DeleteButton\" href=\"javascript:__doPostBack('FormView1$DeleteButton','')\">Delete</a>\n                </td>\r\n\t</tr><tr>\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView1','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$3')\">3</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "BeforeEditPostback");

			//Edit button postback (change to edit mode - buttons "Update" and "Cancel" should appear.
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$EditButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();
			newHtml = HtmlDiff.GetControlFromPageHtml (pageHTML);
			origHtml = "<table cellspacing=\"0\" border=\"0\" id=\"FormView1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td colspan=\"2\">\n                    Enter First Name:<input name=\"FormView1$FNameEdit\" type=\"text\" value=\"Mahesh\" id=\"FormView1_FNameEdit\" /><br />\n                    Enter Last Name:<input name=\"FormView1$LNameEdit\" type=\"text\" value=\"Chand\" id=\"FormView1_LNameEdit\" /><br />\n                    <a id=\"FormView1_UpdateButton\" href=\"javascript:__doPostBack('FormView1$UpdateButton','')\">Update</a>\n                    <a id=\"FormView1_CancelUpdateButton\" href=\"javascript:__doPostBack('FormView1$CancelUpdateButton','')\">Cancel</a>\n                </td>\r\n\t</tr><tr>\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView1','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$3')\">3</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "AfterEditPostback");

			//Update record postback                
			
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("FormView1$FNameEdit");
			fr.Controls.Add ("FormView1$LNameEdit");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$UpdateButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";			
			fr.Controls["FormView1$FNameEdit"].Value = "Merav";
			fr.Controls["FormView1$LNameEdit"].Value = "Test";			
			t.Request = fr;
			pageHTML = t.Run ();
			newHtml = HtmlDiff.GetControlFromPageHtml (pageHTML);
			origHtml = "<table cellspacing=\"0\" border=\"0\" id=\"FormView1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td colspan=\"2\">\n                    <span id=\"FormView1_ID\">1001</span>&nbsp;\n                    <span id=\"FormView1_LName\">Test</span>\n                    <span id=\"FormView1_FName\">Merav</span>&nbsp;\n                    <a id=\"FormView1_EditButton\" href=\"javascript:__doPostBack('FormView1$EditButton','')\">Edit</a>\n                    <a id=\"FormView1_NewButton\" href=\"javascript:__doPostBack('FormView1$NewButton','')\">New</a>\n                    <a id=\"FormView1_DeleteButton\" href=\"javascript:__doPostBack('FormView1$DeleteButton','')\">Delete</a>\n                </td>\r\n\t</tr><tr>\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView1','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$3')\">3</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "AfterUpdatePostback"); 
  
			//Postback to return to Edit mode
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$EditButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";			
			t.Request = fr;
			pageHTML = t.Run ();
			newHtml = pageHTML.Substring (pageHTML.IndexOf ("start") + 5, pageHTML.IndexOf ("end") - pageHTML.IndexOf ("start") - 5);
			Assert.AreEqual (true, pageHTML.Contains ("Merav"), "EditModePostback1");
			Assert.AreEqual (true, pageHTML.Contains ("CancelUpdateButton"), "EditModePostback2"); 

			// Cancel edited record postback
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("FormView1$FNameEdit");
			fr.Controls.Add ("FormView1$LNameEdit");
			fr.Controls["FormView1$FNameEdit"].Value = "EditFirstName";
			fr.Controls["FormView1$LNameEdit"].Value = "EditLastName";
			fr.Controls["__EVENTTARGET"].Value = "FormView1$CancelUpdateButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();
			newHtml = HtmlDiff.GetControlFromPageHtml (pageHTML);
			origHtml = "<table cellspacing=\"0\" border=\"0\" id=\"FormView1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td colspan=\"2\">\n                    <span id=\"FormView1_ID\">1001</span>&nbsp;\n                    <span id=\"FormView1_LName\">Test</span>\n                    <span id=\"FormView1_FName\">Merav</span>&nbsp;\n                    <a id=\"FormView1_EditButton\" href=\"javascript:__doPostBack('FormView1$EditButton','')\">Edit</a>\n                    <a id=\"FormView1_NewButton\" href=\"javascript:__doPostBack('FormView1$NewButton','')\">New</a>\n                    <a id=\"FormView1_DeleteButton\" href=\"javascript:__doPostBack('FormView1$DeleteButton','')\">Delete</a>\n                </td>\r\n\t</tr><tr>\r\n\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t<tr>\r\n\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('FormView1','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('FormView1','Page$3')\">3</a></td>\r\n\t\t\t</tr>\r\n\t\t</table></td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "CancelEditedRecordPostback");   
		}

		[Test (Description="Bug #578863")]
		public void FormView_PagerSettings_Visibility ()
		{
			string origHtml = "<table cellspacing=\"2\" cellpadding=\"3\" rules=\"all\" border=\"1\" id=\"FormView1\" style=\"background-color:#DEBA84;border-color:#DEBA84;border-width:1px;border-style:None;\">\r\n\t<tr style=\"color:#8C4510;background-color:#FFF7E7;\">\r\n\t\t<td colspan=\"2\">\n          <span id=\"FormView1_Label1\">1</span>\n\t</td>\r\n\t</tr>\r\n</table>";
			
			WebTest t = new WebTest ("FormViewPagerVisibility.aspx");
			string pageHtml = t.Run ();
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (pageHtml);

			HtmlDiff.AssertAreEqual (origHtml, renderedHtml, "#A1");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void FormView_FireEvent_1 ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			t.Invoker = PageInvoker.CreateOnInit (EditPostbackFireEvent_Init);
			string html = t.Run ();
			//Edit button postback (change to edit mode - buttons "Update" and "Cancel" should appear.

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$EditButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			html = t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemCommand", eventlist[0], "#1");
			Assert.AreEqual ("ModeChanging", eventlist[1], "#2");
			Assert.AreEqual ("ModeChanged", eventlist[2], "#3");
			t.UserData = null;
			
			//Update record postback                

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("FormView1$FNameEdit");
			fr.Controls.Add ("FormView1$LNameEdit");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$UpdateButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			fr.Controls["FormView1$FNameEdit"].Value = "Merav";
			fr.Controls["FormView1$LNameEdit"].Value = "Test";
			t.Request = fr;
			html = t.Run ();

			eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemCommand", eventlist[0], "#1");
			Assert.AreEqual ("ItemUpdating", eventlist[1], "#2");
			Assert.AreEqual ("ItemUpdated", eventlist[2], "#3");
			Assert.AreEqual ("ModeChanging", eventlist[3], "#4");
			Assert.AreEqual ("ModeChanged", eventlist[4], "#5");
		}

		#region FireEvents_1
		public static void EditPostbackFireEvent_Init (Page p)
		{
			
			FormView d = p.FindControl ("FormView1") as FormView;
			if (d != null) {
				d.ModeChanged +=new EventHandler(d_ModeChanged);
				d.ModeChanging+=new FormViewModeEventHandler(d_ModeChanging);
				d.ItemCommand += new FormViewCommandEventHandler (d_ItemCommand);
				d.ItemUpdating += new FormViewUpdateEventHandler (d_ItemUpdating);
				d.ItemUpdated += new FormViewUpdatedEventHandler (d_ItemUpdated);
			}
		}

		static void d_ItemUpdated (object sender, FormViewUpdatedEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemUpdated");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemUpdated");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void d_ItemUpdating (object sender, FormViewUpdateEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemUpdating");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemUpdating");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void d_ItemCommand (object sender, FormViewCommandEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemCommand");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemCommand");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void  d_ModeChanging(object sender, FormViewModeEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) 
			{
				ArrayList list = new ArrayList ();
				list.Add ("ModeChanging");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ModeChanging");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void  d_ModeChanged(object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) 
					{
				ArrayList list = new ArrayList ();
				list.Add ("ModeChanged");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ModeChanged");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void FormView_FireEvent_2 ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			t.Invoker = PageInvoker.CreateOnInit (FireEvent_2_Init);
			t.Run ();

			// Checking for itemcreated event fired.
			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemCreated", eventlist[0], "#1");
		}

		#region FireEvent_2
		public static void FireEvent_2_Init (Page p)
		{
			FormView d = p.FindControl ("FormView1") as FormView;
			if (d != null) {
				d.ItemCreated += new EventHandler (d_ItemCreated);
			}
		}

		static void d_ItemCreated (object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemCreated");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemCreated");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void FormView_FireEvent_3 ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			t.Invoker = PageInvoker.CreateOnInit (FireEvent_3_Init);
			t.Run ();
			
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$NewButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
			
			//Insert new record

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("FormView1$IDInsert");
			fr.Controls.Add ("FormView1$FNameInsert");
			fr.Controls.Add ("FormView1$LNameInsert");
			fr.Controls["FormView1$IDInsert"].Value = "33";
			fr.Controls["FormView1$FNameInsert"].Value = "InsertFirstName";
			fr.Controls["FormView1$LNameInsert"].Value = "InsertLastName";
			fr.Controls["__EVENTTARGET"].Value = "FormView1$InsertButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemInserting", eventlist[0], "#1");
			Assert.AreEqual ("ItemInserted", eventlist[1], "#2");
		}

		#region FireEvent_3
		public static void FireEvent_3_Init (Page p)
		{
			FormView d = p.FindControl ("FormView1") as FormView;
			if (d != null) {
				d.ItemInserted += new FormViewInsertedEventHandler (d_ItemInserted);
				d.ItemInserting += new FormViewInsertEventHandler (d_ItemInserting);
			}
		}

		static void d_ItemInserting (object sender, FormViewInsertEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemInserting");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemInserting");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void d_ItemInserted (object sender, FormViewInsertedEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemInserted");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemInserted");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void FormView_FireEvent_4 ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			t.Invoker = PageInvoker.CreateOnInit (FireEvent_4_Init);
			t.Run ();

			//Delete Item
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$DeleteButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("ItemDeleting", eventlist[0], "#1");
			Assert.AreEqual ("ItemDeleted", eventlist[1], "#2");

		}

		#region FireEvent_4
		public static void FireEvent_4_Init (Page p)
		{
			FormView d = p.FindControl ("FormView1") as FormView;
			if (d != null) {
				d.ItemDeleting += new FormViewDeleteEventHandler (d_ItemDeleting);
				d.ItemDeleted += new FormViewDeletedEventHandler (d_ItemDeleted);
			}
		}

		static void d_ItemDeleted (object sender, FormViewDeletedEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemDeleted");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemDeleted");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void d_ItemDeleting (object sender, FormViewDeleteEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("ItemDeleting");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("ItemDeleting");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void FormView_FireEvent_5 ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			t.Invoker = PageInvoker.CreateOnInit (FireEvent_5_Init);
			t.Run ();

			//Delete Item
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1";
			fr.Controls["__EVENTARGUMENT"].Value = "Page$2";
			t.Request = fr;
			t.Run ();

			ArrayList eventlist = t.UserData as ArrayList;
			if (eventlist == null)
				Assert.Fail ("User data does not been created fail");

			Assert.AreEqual ("PageIndexChanging", eventlist[0], "#1");
			Assert.AreEqual ("PageIndexChanged", eventlist[1], "#2");
		}

		#region FireEvent_5
		public static void FireEvent_5_Init (Page p)
		{
			FormView d = p.FindControl ("FormView1") as FormView;
			if (d != null) {
				d.PageIndexChanged+=new EventHandler(d_PageIndexChanged);
				d.PageIndexChanging+=new FormViewPageEventHandler(d_PageIndexChanging);
			}
		}

		static void d_PageIndexChanging (object sender, FormViewPageEventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("PageIndexChanging");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("PageIndexChanging");
				WebTest.CurrentTest.UserData = list;
			}
		}

		static void d_PageIndexChanged (object sender, EventArgs e)
		{
			if (WebTest.CurrentTest.UserData == null) {
				ArrayList list = new ArrayList ();
				list.Add ("PageIndexChanged");
				WebTest.CurrentTest.UserData = list;
			}
			else {
				ArrayList list = WebTest.CurrentTest.UserData as ArrayList;
				if (list == null)
					throw new NullReferenceException ();
				list.Add ("PageIndexChanged");
				WebTest.CurrentTest.UserData = list;
			}
		}
		#endregion

		[Test]
		[Category ("NunitWeb")] 
		public void FormView_InsertPostback ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			string pageHTML = t.Run ();
			Assert.AreEqual (true, pageHTML.Contains ("1001"), "BeforeInsert1");
			Assert.AreEqual (true, pageHTML.Contains ("Mahesh"), "BeforeInsert2");
			Assert.AreEqual (true, pageHTML.Contains ("Chand"), "BeforeInsert3");
			Assert.AreEqual (false, pageHTML.Contains ("Page$4"), "BeforeInsert4");
			FormRequest fr = new FormRequest (t.Response, "form1"); 
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");			
			fr.Controls["__EVENTTARGET"].Value = "FormView1$NewButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";						
			t.Request = fr;
			pageHTML = t.Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (pageHTML);
			string origHtml = "<table cellspacing=\"0\" border=\"0\" id=\"FormView1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td colspan=\"2\">\n                    Insert ID:\n                    <input name=\"FormView1$IDInsert\" type=\"text\" id=\"FormView1_IDInsert\" /><br />\n                    Insert First Name:\n                    <input name=\"FormView1$FNameInsert\" type=\"text\" id=\"FormView1_FNameInsert\" />\n                    <br />\n                    Insert Last Name:&nbsp;\n                    <input name=\"FormView1$LNameInsert\" type=\"text\" id=\"FormView1_LNameInsert\" />\n                    <a id=\"FormView1_InsertButton\" href=\"javascript:__doPostBack('FormView1$InsertButton','')\">Insert</a>\n                    <a id=\"FormView1_CancelInsertButton\" href=\"javascript:__doPostBack('FormView1$CancelInsertButton','')\">Cancel</a>\n                </td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "InsertPostback");

			//Insert new record

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls.Add ("FormView1$IDInsert");
			fr.Controls.Add ("FormView1$FNameInsert");
			fr.Controls.Add ("FormView1$LNameInsert");
			fr.Controls["FormView1$IDInsert"].Value = "33";
			fr.Controls["FormView1$FNameInsert"].Value = "InsertFirstName";
			fr.Controls["FormView1$LNameInsert"].Value ="InsertLastName";
			fr.Controls["__EVENTTARGET"].Value = "FormView1$InsertButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();			
			Assert.AreEqual (true, pageHTML.Contains ("1001"), "AfterInsert1");
			Assert.AreEqual (true, pageHTML.Contains ("Mahesh"), "AfterInsert2");
			Assert.AreEqual (true, pageHTML.Contains ("Chand"), "AfterInsert3");
			Assert.AreEqual (true, pageHTML.Contains ("Page$4"), "AfterInsert4");

			//Checking that the inserted record appears on page 4.

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");			
			fr.Controls["__EVENTTARGET"].Value = "FormView1";
			fr.Controls["__EVENTARGUMENT"].Value = "Page$4";
			t.Request = fr;
			pageHTML = t.Run ();
			Assert.AreEqual (true, pageHTML.Contains ("33"), "AfterInsert1");
			Assert.AreEqual (true, pageHTML.Contains ("InsertLastName"), "AfterInsert2");
			Assert.AreEqual (true, pageHTML.Contains ("InsertFirstName"), "AfterInsert3");
			
		}		

		[Test]
		[Category ("NunitWeb")]
		public void FormView_DeleteAndEmptyTemplatePostback ()
		{
			WebTest t = new WebTest ("FormViewInsertEditDelete.aspx");
			string pageHTML = t.Run ();
			
			Assert.AreEqual (true, pageHTML.Contains ("1001"), "BeforeDelete1");
			Assert.AreEqual (true, pageHTML.Contains ("Mahesh"), "BeforeDelete2");
			Assert.AreEqual (true, pageHTML.Contains ("Chand"), "BeforeDelete3");
			Assert.AreEqual (true, pageHTML.Contains ("Page$3"), "BeforeDelete4");	
			//Delete First Item
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$DeleteButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();			
			Assert.AreEqual (true, pageHTML.Contains ("1002"), "AfterFirstDelete1");
			Assert.AreEqual (true, pageHTML.Contains ("Talmadge"), "AfterFirstDelete2");
			Assert.AreEqual (true, pageHTML.Contains ("Melanie"), "AfterFirstDelete3");
			Assert.AreEqual (true, pageHTML.Contains ("Page$2"), "AfterFirstDelete4");
			Assert.AreEqual (false, pageHTML.Contains ("Page$3"), "AfterFirstDelete5");

			//Delete second item

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$DeleteButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();
			Assert.AreEqual (true, pageHTML.Contains ("1003"), "AfterSecondDelete1");
			Assert.AreEqual (true, pageHTML.Contains ("Bansal"), "AfterSecondDelete2");
			Assert.AreEqual (true, pageHTML.Contains ("Vinay"), "AfterSecondDelete3");
			Assert.AreEqual (false, pageHTML.Contains ("Page$2"), "AfterSecondDelete4");	

			//Delete last item and checking that the EmptyDataTemplate appears.

			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "FormView1$DeleteButton";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			pageHTML = t.Run ();			
			Assert.AreEqual (true, pageHTML.Contains ("FormView1_Label1"), "EmptyTemplateTest1"); 
			Assert.AreEqual (true, pageHTML.Contains ("The Database is empty"), "EmptyTemplateTest2");
		}

		[Test]
		public void FormView_CurrentMode () {
			FormView view = new FormView ();
			view.DefaultMode = FormViewMode.Insert;
			Assert.AreEqual (FormViewMode.Insert, view.CurrentMode, "FormView_CurrentMode#1");
			view.ChangeMode (FormViewMode.Edit);
			Assert.AreEqual (FormViewMode.Edit, view.CurrentMode, "FormView_CurrentMode#2");
		}

		[Test]
		public void FormView_CreateDataSourceSelectArguments2 () {
			DataSourceView view;
			Page p = new Page ();

			Poker dv = new Poker ();
			p.Controls.Add (dv);

			ObjectDataSource data = new ObjectDataSource ();
			data.TypeName = typeof (DataSourceObject).AssemblyQualifiedName;
			data.SelectMethod = "GetList";
			data.SortParameterName = "sortExpression";
			DataSourceSelectArguments arg;
			p.Controls.Add (data);

			dv.DataSource = data;
			dv.DataBind ();

			arg = dv.DoCreateDataSourceSelectArguments ();
			Assert.IsTrue (arg.Equals (DataSourceSelectArguments.Empty), "Default");

			dv.AllowPaging = true;
			dv.PageIndex = 2;
			arg = dv.DoCreateDataSourceSelectArguments ();
			view = dv.DoGetData ();
			Assert.IsFalse (view.CanPage);
			Assert.IsTrue (view.CanRetrieveTotalRowCount);
			Assert.IsTrue (arg.Equals (DataSourceSelectArguments.Empty), "AllowPaging = true, CanPage = false, CanRetrieveTotalRowCount = true");

			// make DataSourceView.CanPage = true
			data.EnablePaging = true;

			arg = dv.DoCreateDataSourceSelectArguments ();
			view = dv.DoGetData ();
			Assert.IsTrue (view.CanPage);
			Assert.IsFalse (view.CanRetrieveTotalRowCount);
			Assert.IsTrue (arg.Equals (new DataSourceSelectArguments (2, -1)), "AllowPaging = true, CanPage = true, CanRetrieveTotalRowCount = false");

			dv.AllowPaging = false;
			arg = dv.DoCreateDataSourceSelectArguments ();
			Assert.IsTrue (arg.Equals (DataSourceSelectArguments.Empty), "AllowPaging = false, CanPage = true, CanRetrieveTotalRowCount = false");

			// make DataSourceView.CanRetrieveTotalRowCount = true
			data.SelectCountMethod = "GetCount";

			arg = dv.DoCreateDataSourceSelectArguments ();
			Assert.IsTrue (arg.Equals (DataSourceSelectArguments.Empty), "AllowPaging = false, CanPage = true, CanRetrieveTotalRowCount = true");

			dv.AllowPaging = true;
			arg = dv.DoCreateDataSourceSelectArguments ();
			DataSourceSelectArguments arg1 = new DataSourceSelectArguments (2, 1);
			arg1.RetrieveTotalRowCount = true;
			view = dv.DoGetData ();
			Assert.IsTrue (view.CanPage);
			Assert.IsTrue (view.CanRetrieveTotalRowCount);
			Assert.IsTrue (arg.Equals (arg1), "AllowPaging = true, CanPage = true, CanRetrieveTotalRowCount = true");
		}

		[Test]
		public void FormView_GetPostBackOptions () {
			FormView fv = new FormView ();
			fv.Page = new Page ();
			IButtonControl btn = new Button ();
			btn.CausesValidation = false;
			Assert.IsFalse (btn.CausesValidation);
			Assert.AreEqual (String.Empty, btn.CommandName);
			Assert.AreEqual (String.Empty, btn.CommandArgument);
			Assert.AreEqual (String.Empty, btn.PostBackUrl);
			Assert.AreEqual (String.Empty, btn.ValidationGroup);
			PostBackOptions options = ((IPostBackContainer) fv).GetPostBackOptions (btn);
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsTrue (options.ClientSubmit);
			Assert.IsTrue (options.RequiresJavaScriptProtocol);
			Assert.AreEqual ("$", options.Argument);
			Assert.AreEqual (null, options.ActionUrl);
			Assert.AreEqual (null, options.ValidationGroup);
			Assert.IsTrue (object.ReferenceEquals (options.TargetControl, fv));

			btn.ValidationGroup = "VG";
			btn.CommandName = "CMD";
			btn.CommandArgument = "ARG";
			btn.PostBackUrl = "Page.aspx";
			Assert.IsFalse (btn.CausesValidation);
			Assert.AreEqual ("CMD", btn.CommandName);
			Assert.AreEqual ("ARG", btn.CommandArgument);
			Assert.AreEqual ("Page.aspx", btn.PostBackUrl);
			Assert.AreEqual ("VG", btn.ValidationGroup);
			options = ((IPostBackContainer) fv).GetPostBackOptions (btn);
			Assert.IsFalse (options.PerformValidation);
			Assert.IsFalse (options.AutoPostBack);
			Assert.IsFalse (options.TrackFocus);
			Assert.IsTrue (options.ClientSubmit);
			Assert.IsTrue (options.RequiresJavaScriptProtocol);
			Assert.AreEqual ("CMD$ARG", options.Argument);
			Assert.AreEqual (null, options.ActionUrl);
			Assert.AreEqual (null, options.ValidationGroup);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void FormView_GetPostBackOptions_CausesValidation () {
			FormView fv = new FormView ();
			fv.Page = new Page ();
			IButtonControl btn = new Button ();
			Assert.IsTrue (btn.CausesValidation);
			Assert.AreEqual (String.Empty, btn.CommandName);
			Assert.AreEqual (String.Empty, btn.CommandArgument);
			Assert.AreEqual (String.Empty, btn.PostBackUrl);
			Assert.AreEqual (String.Empty, btn.ValidationGroup);
			PostBackOptions options = ((IPostBackContainer) fv).GetPostBackOptions (btn);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FormView_GetPostBackOptions_Null_Argument () {
			FormView fv = new FormView ();
			fv.Page = new Page ();
			PostBackOptions options = ((IPostBackContainer) fv).GetPostBackOptions (null);
		}

		[Test]
		[Category ("NunitWeb")]
		public void FormView_RequiresDataBinding () {
			PageDelegates delegates = new PageDelegates ();
			delegates.LoadComplete = FormView_RequiresDataBinding_LoadComplete;
			PageInvoker invoker = new PageInvoker (delegates);
			WebTest t = new WebTest (invoker);
			t.Run ();
		}

		public static void FormView_RequiresDataBinding_LoadComplete (Page p) {
			Poker view = new Poker ();
			p.Form.Controls.Add (view);

			view.DataSource = new string [] { "A", "B", "C" };
			view.DataBind ();

			Assert.AreEqual (false, view.GetRequiresDataBinding ());

			view.PagerTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "PagerTemplate was set");

			view.EmptyDataTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "EmptyDataTemplate was set");

			view.HeaderTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "HeaderTemplate was set");

			view.FooterTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "FooterTemplate was set");

			view.EditItemTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "EditItemTemplate was set");

			view.InsertItemTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "InsertItemTemplate was set");

			view.ItemTemplate = new CompiledTemplateBuilder (BuildTemplateMethod);
			Assert.AreEqual (false, view.GetRequiresDataBinding (), "ItemTemplate was set");
		}

		public static void BuildTemplateMethod (Control c) { }
	}

	public class TestMyData
	{
		static IList<int> str;
		//str.(new int[] { 1, 2, 3, 4, 5, 6 });

		static TestMyData ()
		{
			InitData ();
		}

		public static void InitData()
		{
			str = new List<int> ();
			for (int i=1;i<7;i++)
				str.Add (i);
		}
		public static IList<int> GetMyList()
		{
			return str;
		}

		public static int UpdateList(int index, int value)
		{
			str[index] = value;
			return str[index];
		}

		public static int InsertList(int value)
		{
			str.Add(value);
			return value;
		}

		public static void DeleteList(int value)
		{
			str.Remove(value);
		}

	}
	
	public class MyTemplate : ITemplate
	{

		Label l = new Label ();
#region ITemplate Members

		public void InstantiateIn (Control container)
		{
			container.Controls.Add (l);

		}

		public void SetDataItem (object value)
		{
			l.Text = value.ToString ();
		}

#endregion
	}

public class FormViewDataObject
	{

		public static DataTable ds = CreateDataTable();

		public static DataTable Select()
		{

			return ds;

		}



		public static DataTable Delete(string ID, string FName, string LName)
		{

			DataRow dr = ds.Rows.Find(ID);

			if (dr != null)
			{

				ds.Rows.Remove(dr);

			}

			return ds;

		}



		public static DataTable Insert(string ID, string FName, string LName)
		{

			DataRow dr = ds.NewRow();
			dr["ID"] = ID;
			dr["FName"] = FName;
			dr["LName"] = LName;
			ds.Rows.Add(dr);
			return ds;
		}



		public static DataTable Update(string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find(ID);
			if (dr == null)
			{
				Label lbl = new Label();
				lbl.Text = "ID doesn't exist ";
				return ds;
			}
			dr["FName"] = FName;
			dr["LName"] = LName;
			return ds;

		}



		public static DataTable CreateDataTable()
		{

			DataTable aTable = new DataTable("A");
			DataColumn dtCol;
			DataRow dtRow;

			// Create ID column and add to the DataTable.

			dtCol = new DataColumn();
			dtCol.DataType = Type.GetType("System.Int32");
			dtCol.ColumnName = "ID";
			dtCol.AutoIncrement = true;
			dtCol.Caption = "ID";
			dtCol.ReadOnly = true;
			dtCol.Unique = true;
			aTable.Columns.Add(dtCol);



			// Create Name column and add to the table

			dtCol = new DataColumn();
			dtCol.DataType = Type.GetType("System.String");
			dtCol.ColumnName = "FName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "First Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add(dtCol);



			// Create Last Name column and add to the table.

			dtCol = new DataColumn();
			dtCol.DataType = Type.GetType("System.String");
			dtCol.ColumnName = "LName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "Last Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add(dtCol);



			// Create three rows to the table

			dtRow = aTable.NewRow();
			dtRow["ID"] = 1001;
			dtRow["FName"] = "Mahesh";
			dtRow["LName"] = "Chand";
			aTable.Rows.Add(dtRow);

			dtRow = aTable.NewRow();
			dtRow["ID"] = 1002;
			dtRow["FName"] = "Melanie";
			dtRow["LName"] = "Talmadge";
			aTable.Rows.Add(dtRow);

			dtRow = aTable.NewRow();
			dtRow["ID"] = 1003;
			dtRow["FName"] = "Vinay";
			dtRow["LName"] = "Bansal";
			aTable.Rows.Add(dtRow);

			aTable.PrimaryKey = new DataColumn[] { aTable.Columns["ID"] };
			return aTable;

		}
	}
}


#endif
