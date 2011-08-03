//
// Tests for System.Web.UI.WebControls.ObjectDataSourceView
//
// Author:
//	Chris Toshok (toshok@novell.com)
//      Konstantin Triger (kostat@mainsoft.com)
//	Yoni Klain (yonik@mainsoft.com)
//

//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
// WARNING NOTES  : ALL NUNITWEB TESTS DOING UNLOAD BETWEEN TESTS FOR RELOAD
// OBJECT DATA SOURCE DEFAULT DATA 


#if NET_2_0

using NUnit.Framework;
using System;
using Sys = System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Threading;

namespace MonoTests.System.Web.UI.WebControls
{
	class ObjectViewPoker : ObjectDataSourceView
	{
		public ObjectViewPoker (ObjectDataSource ds, string name, HttpContext context)
			: base (ds, name, context)
		{
			TrackViewState ();
		}

		public bool GetIsTrackingViewState ()
		{
			return base.IsTrackingViewState;
		}

		public object SaveToViewState ()
		{
			return SaveViewState ();
		}

		public void LoadFromViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		public void DoOnDeleting (ObjectDataSourceMethodEventArgs e)
		{
			base.OnDeleting (e);
		}

		public void DoOnInserting (ObjectDataSourceMethodEventArgs e)
		{
			base.OnInserting (e);
		}

		public void DoOnFiltering (ObjectDataSourceFilteringEventArgs e)
		{
			base.OnFiltering (e);
		}

		public void DoOnObjectCreating (ObjectDataSourceEventArgs e)
		{
			base.OnObjectCreating (e);
		}

		public void DoOnObjectCreated (ObjectDataSourceEventArgs e)
		{
			base.OnObjectCreated (e);
		}

		public void DoOnObjectDisposing (ObjectDataSourceDisposingEventArgs e)
		{
			base.OnObjectDisposing (e);
		}

		public void DoOnSelecting (ObjectDataSourceSelectingEventArgs e)
		{
			base.OnSelecting (e);
		}

		public void DoOnUpdating (ObjectDataSourceMethodEventArgs e)
		{
			base.OnUpdating (e);
		}

		public void DoOnUpdated (ObjectDataSourceStatusEventArgs e)
		{
			base.OnUpdated (e);
		}

		public void DoOnDeleted (ObjectDataSourceStatusEventArgs e)
		{
			base.OnDeleted (e);
		}

		public void DoOnInserted (ObjectDataSourceStatusEventArgs e)
		{
			base.OnInserted (e);
		}

		public void DoOnSelected (ObjectDataSourceStatusEventArgs e)
		{
			base.OnSelected (e);
		}

		public int DoExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			return base.ExecuteDelete (keys, oldValues);
		}

		public void DoOnDataSourceViewChanged ()
		{
			base.OnDataSourceViewChanged (new EventArgs ());
		}

	}

	[TestFixture]
	public class ObjectDataSourceViewTest
	{

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		[SetUp()]
		public void Setup () 
		{
			eventsCalled = null;
		}

		[Test]
		public void Defaults ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			Assert.IsFalse (sql.CanDelete, "CanDelete");
			Assert.IsFalse (sql.CanInsert, "CanInsert");
			Assert.IsFalse (sql.CanPage, "CanPage");
			Assert.IsTrue (sql.CanSort, "CanSort");
			Assert.IsFalse (sql.CanUpdate, "CanUpdate");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "ConflictDetection");
			Assert.IsFalse (sql.ConvertNullToDBNull, "ConvertNullToDBNull");
			Assert.AreEqual ("", sql.DataObjectTypeName, "DataObjectTypeName");
			Assert.AreEqual ("", sql.DeleteMethod, "DeleteMethod");
			Assert.IsNotNull (sql.DeleteParameters, "DeleteParameters");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "DeleteParameters.Count");
			Assert.IsFalse (sql.EnablePaging, "EnablePaging");
			Assert.AreEqual ("", sql.InsertMethod, "InsertMethod");
			Assert.IsNotNull (sql.InsertParameters, "InsertParameters");
			Assert.AreEqual (0, sql.InsertParameters.Count, "InsertParameters.Count");
			Assert.AreEqual ("", sql.FilterExpression, "FilterExpression");
			Assert.IsNotNull (sql.FilterParameters, "FilterParameters");
			Assert.AreEqual (0, sql.FilterParameters.Count, "FilterParameters.Count");
			Assert.AreEqual ("maximumRows", sql.MaximumRowsParameterName, "MaximumRowsParameterName");
			Assert.AreEqual ("", sql.SelectCountMethod, "SelectCountMethod");
			Assert.AreEqual ("", sql.SelectMethod, "SelectMethod");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "OldValuesParameterFormatString");
			Assert.IsNotNull (sql.SelectParameters, "SelectParameters");
			Assert.AreEqual (0, sql.SelectParameters.Count, "SelectParameters.Count");
			Assert.AreEqual ("", sql.SortParameterName, "SortParameterName");
			Assert.IsNotNull (sql.UpdateParameters, "UpdateParameters");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "UpdateParameters.Count");
			Assert.AreEqual ("startRowIndex", sql.StartRowIndexParameterName, "StartRowIndexParameterName");
			Assert.AreEqual ("", sql.TypeName, "TypeName");
			Assert.AreEqual ("", sql.UpdateMethod, "UpdateMethod");
			Assert.AreEqual (true, sql.GetIsTrackingViewState (), "IsTrackingViewState");
			Assert.IsTrue (sql.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount");
		}

		[Test]
		public void DefaultsAssignProperties ()
		{

			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "ConflictDetection");

			sql.ConvertNullToDBNull = true;
			Assert.IsTrue (sql.ConvertNullToDBNull, "ConvertNullToDBNull");

			sql.DataObjectTypeName = "test";
			Assert.AreEqual ("test", sql.DataObjectTypeName, "DataObjectTypeName");

			sql.DeleteMethod = "test";
			Assert.AreEqual ("test", sql.DeleteMethod, "DeleteMethod");

			sql.EnablePaging = true;
			Assert.IsTrue (sql.EnablePaging, "EnablePaging");

			sql.InsertMethod = "test";
			Assert.AreEqual ("test", sql.InsertMethod, "InsertMethod");

			sql.FilterExpression = "test";
			Assert.AreEqual ("test", sql.FilterExpression, "FilterExpression");

			sql.MaximumRowsParameterName = "test";
			Assert.AreEqual ("test", sql.MaximumRowsParameterName, "MaximumRowsParameterName");

			sql.SelectCountMethod = "test";
			Assert.AreEqual ("test", sql.SelectCountMethod, "SelectCountMethod");

			sql.SelectMethod = "test";
			Assert.AreEqual ("test", sql.SelectMethod, "SelectMethod");

			sql.OldValuesParameterFormatString = "test";
			Assert.AreEqual ("test", sql.OldValuesParameterFormatString, "OldValuesParameterFormatString");

			sql.StartRowIndexParameterName = "test";
			Assert.AreEqual ("test", sql.StartRowIndexParameterName, "StartRowIndexParameterName");

			sql.TypeName = "test";
			Assert.AreEqual ("test", sql.TypeName, "TypeName");

			sql.UpdateMethod = "test";
			Assert.AreEqual ("test", sql.UpdateMethod, "UpdateMethod");

			Assert.AreEqual ("DefaultView", sql.Name, "Name");

		}

		[Test]
		public void ViewStateSupport () 
		{
			ObjectDataSourceView view;
			MyDataSource ds = new MyDataSource ();

			ds.ID = "ObjectDataSource2";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";
			ds.SelectCountMethod = "SelectCount";

			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			((IStateManager) view).TrackViewState ();

			Parameter p1 = new Parameter ("test", TypeCode.String);

			Assert.IsTrue (((IStateManager) view).IsTrackingViewState, "IsTrackingViewState");
			Assert.IsTrue (((IStateManager) view.FilterParameters).IsTrackingViewState, "FilterParameters.IsTrackingViewState");
			Assert.IsTrue (((IStateManager) view.SelectParameters).IsTrackingViewState, "SelecteParameters.IsTrackingViewState");
			Assert.IsFalse (((IStateManager) view.DeleteParameters).IsTrackingViewState, "DeleteParameters.IsTrackingViewState");
			Assert.IsFalse (((IStateManager) view.InsertParameters).IsTrackingViewState, "InsertParameters.IsTrackingViewState");
			Assert.IsFalse (((IStateManager) view.UpdateParameters).IsTrackingViewState, "UpdateParameters.IsTrackingViewState");

			object state = ((IStateManager) view).SaveViewState ();
			Assert.IsNull (state, "view ViewState not null");

			view.DeleteParameters.Add (p1);
			view.InsertParameters.Add (p1);
			//view.UpdateParameters.Add (p1);

			state = ((IStateManager) view).SaveViewState ();
			Assert.IsNull (state, "view ViewState not null");

			view.FilterParameters.Add (p1);
			//view.SelectParameters.Add (p1);

			state = ((IStateManager) view).SaveViewState ();
			Assert.IsNotNull (state, "view ViewState not null");

			state = ((IStateManager) view.FilterParameters).SaveViewState ();
			Assert.IsNotNull (state, "FilterParamenters ViewState not null");
			state = ((IStateManager) view.SelectParameters).SaveViewState ();
			Assert.IsNull (state, "SelectParameters ViewState not null");

			state = ((IStateManager) view.DeleteParameters).SaveViewState ();
			Assert.IsNotNull (state, "DeleteParameters ViewState not null");
			state = ((IStateManager) view.InsertParameters).SaveViewState ();
			Assert.IsNotNull (state, "InsertParameters ViewState not null");
			state = ((IStateManager) view.UpdateParameters).SaveViewState ();
			Assert.IsNull (state, "UpdateParameters ViewState not null");
		}

		[Test]
		public void ViewState ()
		{
			// Note :
			// IStateManager implementation allows public access to control state
			// Nothing added to viewstate

			ObjectDataSourceView view;
			MyDataSource ds = new MyDataSource ();

			ds.ID = "ObjectDataSource2";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";
			ds.SelectCountMethod = "SelectCount";

			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			((IStateManager) view).TrackViewState ();

			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.ConvertNullToDBNull = true;
			view.DataObjectTypeName = "test";
			view.DeleteMethod = "test";
			view.EnablePaging = true;
			view.InsertMethod = "test";
			view.FilterExpression = "test";
			view.MaximumRowsParameterName = "test";
			view.SelectCountMethod = "test";
			view.SelectMethod = "test";
			view.OldValuesParameterFormatString = "test";
			view.StartRowIndexParameterName = "test";
			view.TypeName = "test";
			view.UpdateMethod = "test";

			object state = ((IStateManager) view).SaveViewState ();
			Assert.IsNull (state, "ViewState#1");

			ObjectDataSourceView copy = new ObjectDataSourceView (ds, "DefaultView", null);
			((IStateManager) copy).LoadViewState (state);

		}

		[Test]
		public void CanDelete ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.DeleteMethod = "DeleteMethod";
			Assert.IsTrue (sql.CanDelete, "A1");

			sql.DeleteMethod = "";
			Assert.IsFalse (sql.CanDelete, "A2");

			sql.DeleteMethod = null;
			Assert.IsFalse (sql.CanDelete, "A3");
		}

		[Test]
		public void CanInsert ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.InsertMethod = "InsertMethod";
			Assert.IsTrue (sql.CanInsert, "A1");

			sql.InsertMethod = "";
			Assert.IsFalse (sql.CanInsert, "A2");

			sql.InsertMethod = null;
			Assert.IsFalse (sql.CanInsert, "A3");
		}

		[Test]
		public void CanUpdate ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			sql.UpdateMethod = "UpdateMethod";
			Assert.IsTrue (sql.CanUpdate, "A1");

			sql.UpdateMethod = "";
			Assert.IsFalse (sql.CanUpdate, "A2");

			sql.UpdateMethod = null;
			Assert.IsFalse (sql.CanUpdate, "A3");
		}

		[Test]
		public void CanRetrieveTotalRowCount () 
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectDataSourceView view = new ObjectDataSourceView (ds, "DefaultView", null);

			Assert.IsFalse (view.CanPage, "CanPage#1");
			Assert.IsTrue (view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount#1");

			view.EnablePaging = true;
			Assert.IsTrue (view.CanPage, "CanPage#2");
			Assert.IsFalse (view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount#2");

			view.SelectCountMethod = "SelectCountMethod";
			Assert.IsTrue (view.CanPage, "CanPage#3");
			Assert.IsTrue (view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount#3");

			view.EnablePaging = false;
			Assert.IsFalse (view.CanPage, "CanPage#4");
			Assert.IsTrue (view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount#4");
		}

		[Test]
		public void OldValuesParameterFormatString ()
		{
			ObjectDataSource ds = new ObjectDataSource ();

			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A1");

			ds.OldValuesParameterFormatString = "hi {0}";

			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A2");

			ds.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A3");

			ds.OldValuesParameterFormatString = "{0}";
			sql.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A4");
		}

		[Test]
		[Category("NunitWeb")]
		public void DeleteMethod ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (delete));
			string html = t.Run ();
			Assert.AreEqual (-1, html.IndexOf("Yonik"), "ObjectDataSourceViewDelete");
		}

		public static void delete (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			ObjectDataSourceView view;
			GridView grid = new GridView ();
			MyDataSource ds = new MyDataSource ();
			ds.ID = "ObjectDataSource2";

			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";
			ds.DeleteMethod = "Delete";
			ds.InsertMethod = "Insert";
			ds.UpdateMethod = "Update";
			Parameter p1 = new Parameter ("ID", TypeCode.String);
			Parameter p2 = new Parameter ("FName", TypeCode.String);
			Parameter p3 = new Parameter ("LName", TypeCode.String);
			ds.DeleteParameters.Add (p1);
			ds.DeleteParameters.Add (p2);
			ds.DeleteParameters.Add (p3);
			grid.ID = "Grid";
			grid.DataKeyNames = new string[] { "ID", "FName", "LName" };
			grid.DataSourceID = "ObjectDataSource2";
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (ds);
			p.Form.Controls.Add (grid);
			p.Form.Controls.Add (lce);
			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			view.Deleting += new ObjectDataSourceMethodEventHandler (Event);

			DataSourceObject.InitDS ();

			Hashtable table = new Hashtable ();
			table.Add ("ID", "1001");
			table.Add ("FName", "Yonik");
			table.Add ("LName", "Laim");
			view.Delete (table, null);
			Assert.AreEqual (true, view.CanDelete, "CanDelete");
			Assert.AreEqual ("Delete", view.DeleteMethod, "DeleteMethod");
			Assert.AreEqual (3, view.DeleteParameters.Count, "DeleteParameters.Count");
			Assert.AreEqual ("ID", view.DeleteParameters[0].Name, "DeleteParametersName#1");
			Assert.AreEqual ("FName", view.DeleteParameters[1].Name, "DeleteParametersName#2");
			Assert.AreEqual ("LName", view.DeleteParameters[2].Name, "DeleteParametersName#3");
			ObjectDataSourceViewTest.Eventassert ("Delete event has not fired");
		}


		[Test]
		[Category ("NunitWeb")]
		public void SelectMethod ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (select));
			string html = t.Run ();
#if NET_4_0
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Mahesh</td><td>Chand</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Mahesh</td><td>Chand</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "ObjectDataSourceViewSelect");
		}

		public static void select (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			ObjectDataSourceView view;
			GridView grid = new GridView ();
			MyDataSource ds = new MyDataSource ();
			ds.ID = "ObjectDataSource2";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";

			grid.ID = "Grid";
			grid.DataKeyNames = new string[] { "ID" };
			grid.DataSourceID = "ObjectDataSource2";
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (ds);
			p.Form.Controls.Add (grid);
			p.Form.Controls.Add (lce);
			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			view.Selecting += new ObjectDataSourceSelectingEventHandler (view_Selecting);

			DataSourceObject.InitDS ();

			DataView view1 = (DataView) view.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, view1.Count, "SelectedRowsCount");
			Assert.AreEqual (1001, view1[0].Row["ID"], "SelectedRowsValue#1");
			Assert.AreEqual ("Mahesh", view1[0].Row["FName"], "SelectedRowsValue#2");
			Assert.AreEqual ("Chand", view1[0].Row["LName"], "SelectedRowsValue#3");
			ObjectDataSourceViewTest.Eventassert ("Select event has not fired");
		}

		[Test]
		[Category ("NunitWeb")]
		public void SelectCountMethod ()
		{
			// This method will render grid view with paging 
			// Note : ObjectDataSource will return page counter 5 hard coded
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (selectcount));
			string html = t.Run ();
#if NET_4_0
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Mahesh</td><td>Chand</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td colspan=\"3\"><table>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack(&#39;Grid&#39;,&#39;Page$2&#39;)\">2</a></td><td><a href=\"javascript:__doPostBack(&#39;Grid&#39;,&#39;Page$3&#39;)\">3</a></td><td><a href=\"javascript:__doPostBack(&#39;Grid&#39;,&#39;Page$4&#39;)\">4</a></td><td><a href=\"javascript:__doPostBack(&#39;Grid&#39;,&#39;Page$5&#39;)\">5</a></td>\r\n\t\t\t\t</tr>\r\n\t\t\t</table></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#else
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Mahesh</td><td>Chand</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td colspan=\"3\"><table border=\"0\">\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('Grid','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('Grid','Page$3')\">3</a></td><td><a href=\"javascript:__doPostBack('Grid','Page$4')\">4</a></td><td><a href=\"javascript:__doPostBack('Grid','Page$5')\">5</a></td>\r\n\t\t\t\t</tr>\r\n\t\t\t</table></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
#endif
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "ObjectDataSourceViewSelectCount");
		}

		public static void selectcount (Page p)
		{
			// This method will render grid view with paging 
			// Note : ObjectDataSource will return page counter 5 hard coded

			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			ObjectDataSourceView view;
			GridView grid = new GridView ();
			MyDataSource ds = new MyDataSource ();
			ds.ID = "ObjectDataSource2";
			ds.EnablePaging = true;
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";
			ds.SelectCountMethod = "SelectCount";


			grid.ID = "Grid";
			grid.DataKeyNames = new string[] { "ID" };
			grid.DataSourceID = "ObjectDataSource2";
			grid.AllowPaging = true;
			grid.PageSize = 1;

			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (ds);
			p.Form.Controls.Add (grid);
			p.Form.Controls.Add (lce);
			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			Assert.IsTrue (view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount");
		}


		[Test]
		[Category ("NunitWeb")]
		public void InsertMethod ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (insert));
			string html = t.Run ();
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Mahesh</td><td>Chand</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1000</td><td>Yonik</td><td>Laim</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "ObjectDataSourceViewInsert");
		}

		public static void insert (Page p)
		{
			DataSourceObject.InitDS ();

			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			ObjectDataSourceView view;
			GridView grid = new GridView ();
			MyDataSource ds = new MyDataSource ();
			ds.ID = "ObjectDataSource1";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			ds.SelectMethod = "Select";
			ds.InsertMethod = "Insert";


			Parameter p1 = new Parameter ("ID", TypeCode.String);
			Parameter p2 = new Parameter ("FName", TypeCode.String);
			Parameter p3 = new Parameter ("LName", TypeCode.String);

			ds.InsertParameters.Add (p1);
			ds.InsertParameters.Add (p2);
			ds.InsertParameters.Add (p3);

			grid.ID = "Grid";
			grid.DataSourceID = "ObjectDataSource1";
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (ds);
			p.Form.Controls.Add (grid);
			p.Form.Controls.Add (lce);
			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
			view.Inserting += new ObjectDataSourceMethodEventHandler (Event);

			Hashtable table = new Hashtable ();
			table.Add ("ID", "1000");
			table.Add ("FName", "Yonik");
			table.Add ("LName", "Laim");
			view.Insert (table);
			Assert.AreEqual (true, view.CanInsert, "CanInsert");
			Assert.AreEqual ("Insert", view.InsertMethod, "InsertMethod");
			Assert.AreEqual (3, view.InsertParameters.Count, "InsertParameters.Count");
			Assert.AreEqual ("ID", view.InsertParameters[0].Name, "InsertParametersName#1");
			Assert.AreEqual ("FName", view.InsertParameters[1].Name, "InsertParametersName#2");
			Assert.AreEqual ("LName", view.InsertParameters[2].Name, "InsertParametersName#3");
			ObjectDataSourceViewTest.Eventassert ("Insert event has not fired");
		}

		[Test]
		[Category ("NunitWeb")]
		public void UpdateMethod ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (update));
			string html = t.Run ();
			string origin = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" id=\"Grid\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">ID</th><th scope=\"col\">FName</th><th scope=\"col\">LName</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>1001</td><td>Yonik</td><td>Laim</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			string renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origin, renderedHtml, "ObjectDataSourceViewUpdate");
		}

		public static void update (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			ObjectDataSourceView view;
			GridView grid = new GridView ();
			MyDataSource ds = new MyDataSource ();
			ds.ID = "ObjectDataSource1";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.DataSourceObject";
			
			ds.SelectMethod = "Select";
			ds.DeleteMethod = "Delete";
			ds.InsertMethod = "Insert";
			ds.UpdateMethod = "Update";

			Parameter p1 = new Parameter ("ID", TypeCode.String);
			Parameter p2 = new Parameter ("FName", TypeCode.String);
			Parameter p3 = new Parameter ("LName", TypeCode.String);

			ds.UpdateParameters.Add (p1);
			ds.UpdateParameters.Add (p2);
			ds.UpdateParameters.Add (p3);

			grid.ID = "Grid";
			grid.DataSourceID = "ObjectDataSource1";
			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (ds);
			p.Form.Controls.Add (grid);
			p.Form.Controls.Add (lce);
			view = (ObjectDataSourceView) ds.DoGetView ("defaultView");
			view.Updating += new ObjectDataSourceMethodEventHandler (Event);

			DataSourceObject.InitDS ();

			Hashtable table = new Hashtable ();
			table.Add ("ID", "1001");
			table.Add ("FName", "Yonik");
			table.Add ("LName", "Laim");
			view.Update (null, table, null);
			Assert.AreEqual (true, view.CanUpdate, "CanUpdate");
			Assert.AreEqual ("Update", view.UpdateMethod, "UpdateMethod");
			Assert.AreEqual (3, view.UpdateParameters.Count, "UpdateParameters.Count");
			Assert.AreEqual ("ID", view.UpdateParameters[0].Name, "UpdateParametersName#1");
			Assert.AreEqual ("FName", view.UpdateParameters[1].Name, "UpdateParametersName#2");
			Assert.AreEqual ("LName", view.UpdateParameters[2].Name, "UpdateParametersName#3");
			ObjectDataSourceViewTest.Eventassert ("Update event has not fired");
		}


		//Events
		[Test]
		public void UpdateEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.Updating += new ObjectDataSourceMethodEventHandler (Event);
			view.DoOnUpdating (new ObjectDataSourceMethodEventArgs (null));
			Eventassert ("UpdateEvent");
			view.Updated += new ObjectDataSourceStatusEventHandler (view_Status);
			view.DoOnUpdated (new ObjectDataSourceStatusEventArgs (null, null));
			Eventassert ("UpdateEvent");
		}

		[Test]
		public void SelectEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.Selecting += new ObjectDataSourceSelectingEventHandler (view_Selecting);
			view.DoOnSelecting (new ObjectDataSourceSelectingEventArgs (null, new DataSourceSelectArguments (), false));
			Eventassert ("SelectingEvent");
			view.Selected += new ObjectDataSourceStatusEventHandler (view_Status);
			view.DoOnSelected (new ObjectDataSourceStatusEventArgs (null, null));
			Eventassert ("SelectedEvent");
		}

		[Test]
		public void InsertEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.Inserting += new ObjectDataSourceMethodEventHandler (Event);
			view.DoOnInserting (new ObjectDataSourceMethodEventArgs (null));
			Eventassert ("InsertingEvent");
			view.Inserted += new ObjectDataSourceStatusEventHandler (view_Status);
			view.DoOnInserted (new ObjectDataSourceStatusEventArgs (null, null));
			Eventassert ("InsertedEvent");
		}

		[Test]
		public void DeleteEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.Deleting += new ObjectDataSourceMethodEventHandler (Event);
			view.DoOnDeleting (new ObjectDataSourceMethodEventArgs (null));
			Eventassert ("DeletingEvent");
			view.Deleted += new ObjectDataSourceStatusEventHandler (view_Status);
			view.DoOnDeleted (new ObjectDataSourceStatusEventArgs (null, null));
			Eventassert ("DeletedEvent");
		}

		[Test]
		public void FilterEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.Filtering += new ObjectDataSourceFilteringEventHandler (view_Filtering);
			view.DoOnFiltering (new ObjectDataSourceFilteringEventArgs (null));
			Eventassert ("FilterEvent");
		}

		[Test]
		public void ObjectCreatingEvent ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.ObjectCreating += new ObjectDataSourceObjectEventHandler (view_ObjectCreate);
			view.DoOnObjectCreating (new ObjectDataSourceEventArgs (null));
			Eventassert ("ObjectCreatingEvent");
			view.ObjectCreated += new ObjectDataSourceObjectEventHandler (view_ObjectCreate);
			view.DoOnObjectCreated (new ObjectDataSourceEventArgs (null));
			Eventassert ("ObjectCreatedEvent");
			view.ObjectDisposing += new ObjectDataSourceDisposingEventHandler (view_ObjectDisposing);
		}

		[Test]
		public void ObjectDisposing ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.ObjectDisposing += new ObjectDataSourceDisposingEventHandler (view_ObjectDisposing);
			view.DoOnObjectDisposing (new ObjectDataSourceDisposingEventArgs (null));
			Eventassert ("ObjectDisposing");
		}

		IEnumerable returnedData;
		void SelectCallback (IEnumerable data)
		{
			returnedData = data;
		}
		
		[Test] // bug #471767
		public void SelectReturnsObjectArray ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ds.TypeName=typeof(DataSourceObject).AssemblyQualifiedName;
			ds.SelectMethod="SelectObject";

			DataSourceView dsv = ((IDataSource)ds).GetView (String.Empty);
			dsv.Select (DataSourceSelectArguments.Empty, new DataSourceViewSelectCallback (SelectCallback));
			Assert.IsTrue (returnedData != null, "#A1");
			Assert.AreEqual (typeof (object[]), returnedData.GetType (), "#A2");

			object[] data = returnedData as object[];
			Assert.AreEqual (1, data.Length, "#A3");
			Assert.AreEqual (typeof (MyCustomDataObject), data [0].GetType (), "#A4");
		}
		
		enum InitViewType
		{
			MatchParamsToValues,
			MatchParamsToOldValues,
			DontMatchParams,
		}

		public class DummyDataSourceObject
		{
			public static IEnumerable Select (string filter) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Select(filter = {0})", filter));
				return new string [] { "one", "two", "three" };
			}

			public static int Update (string ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Update(ID = {0})", ID));
				return 1;
			}

			public static int Update (string ID, string oldvalue_ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Update(ID = {0}, oldvalue_ID = {1})", ID, oldvalue_ID));
				return 1;
			}

			public static int UpdateOther (string ID, string OtherValue, string oldvalue_ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("UpdateOther(ID = {0}, OtherValue = {1}, oldvalue_ID = {2})", ID, OtherValue, oldvalue_ID));
				return 1;
			}

			public static int Insert (string ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Insert(ID = {0})", ID));
				return 1;
			}

			public static int Insert (string ID, string oldvalue_ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Insert(ID = {0}, oldvalue_ID = {1})", ID, oldvalue_ID));
				return 1;
			}

			public static int InsertOther (string ID, string OtherValue) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("InsertOther(ID = {0}, OtherValue = {1})", ID, OtherValue));
				return 1;
			}

			public static int Delete (string ID, string oldvalue_ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Delete(ID = {0}, oldvalue_ID = {1})", ID, oldvalue_ID));
				return 1;
			}

			public static int Delete (string oldvalue_ID) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("Delete(oldvalue_ID = {0})", oldvalue_ID));
				return 1;
			}

			public static int DeleteOther (string oldvalue_ID, string OtherValue) 
			{
				if (eventsCalled == null) {
					eventsCalled = new ArrayList ();
				}
				eventsCalled.Add (String.Format ("DeleteOther(oldvalue_ID = {0}, OtherValue = {1})", oldvalue_ID, OtherValue));
				return 1;
			}
		}

		public class AlwaysChangingParameter : Parameter
		{
			int evaluateCount;

			public AlwaysChangingParameter (string name, TypeCode type, string defaultValue)
				: base (name, type, defaultValue) 
			{
				evaluateCount = 0;
			}

#if NET_4_0
			internal
#endif
			protected override object Evaluate (HttpContext context, Control control) 
			{
				evaluateCount++;
				return String.Format ("{0}{1}", DefaultValue, evaluateCount);
			}
		}

		#region help_results
		class eventAssert
		{
			private static int _testcounter;
			private static bool _eventChecker;
			private eventAssert ()
			{
				_testcounter = 0;
			}

			public static bool eventChecker
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					_eventChecker = value;
				}
			}

			static private void testAdded ()
			{
				_testcounter++;
				_eventChecker = false;
			}

			public static void IsTrue (string msg)
			{
				Assert.IsTrue (_eventChecker, msg + "#" + _testcounter);
				testAdded ();
			
			}

			public static void IsFalse (string msg)
			{
				Assert.IsFalse (_eventChecker, msg + "#" + _testcounter);
				testAdded ();
			}
		}
		#endregion

		[Test]
		public void ObjectDataSourceView_DataSourceViewChanged ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);
			sql.DataSourceViewChanged += new EventHandler (sql_DataSourceViewChanged);
			
			sql.DoOnDataSourceViewChanged ();
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.ConvertNullToDBNull = true;
			eventAssert.IsFalse ("DataSourceViewChanged");

			sql.DataObjectTypeName = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.DeleteMethod = "test";
			eventAssert.IsFalse ("DataSourceViewChanged");

			sql.EnablePaging = true;
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.InsertMethod = "test";
			eventAssert.IsFalse ("DataSourceViewChanged");

			sql.FilterExpression = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.MaximumRowsParameterName = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.SelectCountMethod = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.SelectMethod = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.OldValuesParameterFormatString = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.StartRowIndexParameterName = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.TypeName = "test";
			eventAssert.IsTrue ("DataSourceViewChanged");

			sql.UpdateMethod = "test";
			eventAssert.IsFalse ("DataSourceViewChanged");
		}

		void sql_DataSourceViewChanged (object sender, EventArgs e)
		{
			eventAssert.eventChecker = true;
		}

		[Test]
		public void SelectCountMethod_DataSourceViewChanged ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.DataSourceViewChanged += new EventHandler (view_DataSourceViewChanged);

			Assert.AreEqual ("", view.SelectCountMethod);
			view.SelectCountMethod = null;
			Assert.AreEqual (1, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0]);
			Assert.AreEqual ("", view.SelectCountMethod);

			view.SelectCountMethod = null;
			Assert.AreEqual (2, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [1]);
			Assert.AreEqual ("", view.SelectCountMethod);

			view.SelectCountMethod = "";
			Assert.AreEqual (2, eventsCalled.Count);
		}

		[Test]
		public void SelectMethod_DataSourceViewChanged2 ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.DataSourceViewChanged += new EventHandler (view_DataSourceViewChanged);

			Assert.AreEqual ("", view.SelectMethod);
			view.SelectMethod = null;
			Assert.AreEqual (1, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0]);
			Assert.AreEqual ("", view.SelectMethod);

			view.SelectMethod = null;
			Assert.AreEqual (2, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [1]);
			Assert.AreEqual ("", view.SelectMethod);

			view.SelectMethod = "";
			Assert.AreEqual (2, eventsCalled.Count);
		}

		[Test]
		public void SelectMethod_DataSourceViewChanged1 ()
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			view.DataSourceViewChanged+=new EventHandler(view_DataSourceViewChanged);

			view.SelectMethod = "select_1";
			Assert.AreEqual (1, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0]);

			view.SelectMethod = "select_2";
			Assert.AreEqual (2, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [1]);

			view.SelectMethod = "select_2";
			Assert.AreEqual (2, eventsCalled.Count);

			view.SelectCountMethod = "selectCount_1";
			Assert.AreEqual (3, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2]);

			view.SelectCountMethod = "selectCount_2";
			Assert.AreEqual (4, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [3]);

			view.SelectCountMethod = "selectCount_2";
			Assert.AreEqual (4, eventsCalled.Count);
		}

		private static void InitializeView (ObjectViewPoker view, InitViewType initType, out Hashtable keys, out Hashtable old_value, out Hashtable new_value) 
		{
			view.TypeName = typeof (DummyDataSourceObject).AssemblyQualifiedName;
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.SelectMethod = "Select";
			if (initType == InitViewType.DontMatchParams) {
				view.UpdateMethod = "UpdateOther";
				view.InsertMethod = "InsertOther";
				view.DeleteMethod = "DeleteOther";
			}
			else {
				view.UpdateMethod = "Update";
				view.InsertMethod = "Insert";
				view.DeleteMethod = "Delete";
			}

			Parameter selectParameter = null;
			Parameter insertParameter = null;
			Parameter updateParameter = null;
			Parameter deleteParameter = null;

			selectParameter = new AlwaysChangingParameter ("filter", TypeCode.String, "p_ValueSelect");
			view.SelectParameters.Add (selectParameter);

			switch (initType) {
			case InitViewType.MatchParamsToOldValues:
				insertParameter = new AlwaysChangingParameter ("oldvalue_ID", TypeCode.String, "p_OldValueInsert");
				view.InsertParameters.Add (insertParameter);
				updateParameter = new AlwaysChangingParameter ("oldvalue_ID", TypeCode.String, "p_OldValueUpdate");
				view.UpdateParameters.Add (updateParameter);
				deleteParameter = new AlwaysChangingParameter ("oldvalue_ID", TypeCode.String, "p_OldValueDelete");
				view.DeleteParameters.Add (deleteParameter);
				break;

			case InitViewType.MatchParamsToValues:
				insertParameter = new AlwaysChangingParameter ("ID", TypeCode.String, "p_ValueInsert");
				view.InsertParameters.Add (insertParameter);
				updateParameter = new AlwaysChangingParameter ("ID", TypeCode.String, "p_ValueUpdate");
				view.UpdateParameters.Add (updateParameter);
				deleteParameter = new AlwaysChangingParameter ("ID", TypeCode.String, "p_ValueDelete");
				view.DeleteParameters.Add (deleteParameter);
				break;

			case InitViewType.DontMatchParams:
				insertParameter = new AlwaysChangingParameter ("OtherValue", TypeCode.String, "p_OtherValueInsert");
				view.InsertParameters.Add (insertParameter);
				updateParameter = new AlwaysChangingParameter ("OtherValue", TypeCode.String, "p_OtherValueUpdate");
				view.UpdateParameters.Add (updateParameter);
				deleteParameter = new AlwaysChangingParameter ("OtherValue", TypeCode.String, "p_OtherValueDelete");
				view.DeleteParameters.Add (deleteParameter);
				break;
			}

			view.SelectParameters.ParametersChanged += new EventHandler (SelectParameters_ParametersChanged);
			view.InsertParameters.ParametersChanged += new EventHandler (InsertParameters_ParametersChanged);
			view.UpdateParameters.ParametersChanged += new EventHandler (UpdateParameters_ParametersChanged);
			view.DeleteParameters.ParametersChanged += new EventHandler (DeleteParameters_ParametersChanged);

			keys = new Hashtable ();
			keys.Add ("ID", "k_1001");

			old_value = new Hashtable ();
			old_value.Add ("ID", "ov_1001");

			new_value = new Hashtable ();
			new_value.Add ("ID", "n_1001");

			view.DataSourceViewChanged += new EventHandler (view_DataSourceViewChanged);
		}

		private static IList eventsCalled;

		static void view_DataSourceViewChanged (object sender, EventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("view_DataSourceViewChanged");
		}

		static void SelectParameters_ParametersChanged (object sender, EventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("SelectParameters_ParametersChanged");
		}

		static void InsertParameters_ParametersChanged (object sender, EventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("InsertParameters_ParametersChanged");
		}

		static void UpdateParameters_ParametersChanged (object sender, EventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("UpdateParameters_ParametersChanged");
		}

		static void DeleteParameters_ParametersChanged (object sender, EventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("DeleteParameters_ParametersChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_Select () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToValues, out keys, out old_values, out new_values);

			view.Select (DataSourceSelectArguments.Empty);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0], "view_DataSourceViewChanged");
			Assert.AreEqual ("SelectParameters_ParametersChanged", eventsCalled [1], "SelectParameters_ParametersChanged");
			Assert.AreEqual ("Select(filter = p_ValueSelect1)", eventsCalled [2], "DataSource Method params");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchInsert () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToValues, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("Insert(ID = n_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldInsert () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToOldValues, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("Insert(ID = n_1001, oldvalue_ID = p_OldValueInsert1)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchInsert () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.DontMatchParams, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("InsertOther(ID = n_1001, OtherValue = p_OtherValueInsert1)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchUpdate () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToValues, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("Update(ID = n_1001, oldvalue_ID = k_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldUpdate () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToOldValues, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("Update(ID = n_1001, oldvalue_ID = k_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchUpdate () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.DontMatchParams, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("UpdateOther(ID = n_1001, OtherValue = p_OtherValueUpdate1, oldvalue_ID = k_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchDelete () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToValues, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("Delete(ID = p_ValueDelete1, oldvalue_ID = k_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldDelete () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.MatchParamsToOldValues, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("Delete(oldvalue_ID = k_1001)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchDelete () 
		{
			ObjectViewPoker view = new ObjectViewPoker (new ObjectDataSource (), "", null);
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			InitializeView (view, InitViewType.DontMatchParams, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("DeleteOther(oldvalue_ID = k_1001, OtherValue = p_OtherValueDelete1)", eventsCalled [1], "DataSource Method params");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [2], "view_DataSourceViewChanged");
		}

		/// <summary>
		/// Helper methods
		/// </summary>

		private static bool event_checker;

		private static void Eventassert (string message)
		{
			Assert.IsTrue (ObjectDataSourceViewTest.event_checker, message);
			ObjectDataSourceViewTest.event_checker = false;
		}

		static void Event (object sender, ObjectDataSourceMethodEventArgs e)
		{
			ObjectDataSourceViewTest.event_checker = true;
		}

		static void view_Selecting (object sender, ObjectDataSourceSelectingEventArgs e)
		{
			event_checker = true;
		}

		void view_Filtering (object sender, ObjectDataSourceFilteringEventArgs e)
		{
			event_checker = true;
		}

		void view_ObjectCreate (object sender, ObjectDataSourceEventArgs e)
		{
			event_checker = true;
		}

		void view_Status (object sender, ObjectDataSourceStatusEventArgs e)
		{
			event_checker = true;
		}

		void view_ObjectDisposing (object sender, ObjectDataSourceDisposingEventArgs e)
		{
			event_checker = true;
		}

		private class MyDataSource : ObjectDataSource
		{
			public DataSourceView DoGetView (string viewName)
			{
				return base.GetView (viewName);
			}

			public void DoTrackViewState ()
			{
				base.TrackViewState ();
			}
		}
	}

	public class MyCustomDataObject
	{
	}
	
	public class DataSourceObject
	{
		private static int maximumRows;
		public static DataTable ds = CreateDataTable ();
		public static void InitDS ()
		{
			ds = CreateDataTable ();
		}
		public static DataTable Select ()
		{
			return ds;
		}

		public static MyCustomDataObject SelectObject ()
		{
			return new MyCustomDataObject ();
		}
		
		[Sys.ComponentModel.DataObjectMethod(Sys.ComponentModel.DataObjectMethodType.Select, false)]
		public static DataTable Select (int maximumRows, short startRowIndex) {
			Assert.Fail ("Should not be called since not default Select DataObjectMethod");
			return null;
		}

		[Sys.ComponentModel.DataObjectMethod(Sys.ComponentModel.DataObjectMethodType.Select, true)]
		public static DataTable Select (int maximumRows, int startRowIndex)
		{
			DataSourceObject.maximumRows = maximumRows;
			if (ds.Rows.Count > maximumRows) {
				DataTable temp = ds.Clone ();
				int i = 0;
				while (i < maximumRows && startRowIndex <= ds.Rows.Count) {
					object[] o = ds.Rows[startRowIndex].ItemArray;
					temp.Rows.Add (o);
					i++;
					startRowIndex++;
				}
				return temp;
			}
			return ds;
		}

		public static DataTable Delete (string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find (ID);
			if (dr != null) {
				ds.Rows.Remove (dr);
			}
			return ds;
		}

		public static DataTable Insert (string ID, string FName, string LName)
		{
			DataRow dr = ds.NewRow ();
			dr["ID"] = ID;
			dr["FName"] = FName;
			dr["LName"] = LName;
			ds.Rows.Add (dr);
			return ds;
		}

		public static DataTable Update (string ID, string FName, string LName)
		{
			foreach (DataRow row in ds.Rows) {
				if (row["ID"].ToString () == ID) {
					row["FName"] = FName;
					row["LName"] = LName;
				}
			}
			return ds;
		}


		public static int SelectCount ()
		{

			//Note: This is return 5 only for test goal
			return 5;
		}

		public static DataTable CreateDataTable ()
		{
			DataTable aTable = new DataTable ("A");
			DataColumn dtCol;
			DataRow dtRow;

			// Create ID column and add to the DataTable.
			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.Int32");
			dtCol.ColumnName = "ID";
			dtCol.AutoIncrement = true;
			dtCol.Caption = "ID";
			dtCol.ReadOnly = true;
			dtCol.Unique = true;
			aTable.Columns.Add (dtCol);

			// Create Name column and add to the table
			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.String");
			dtCol.ColumnName = "FName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "First Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add (dtCol);

			// Create Last Name column and add to the table.
			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.String");
			dtCol.ColumnName = "LName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "Last Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add (dtCol);

			// Create three rows to the table
			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1001;
			dtRow["FName"] = "Mahesh";
			dtRow["LName"] = "Chand";
			aTable.Rows.Add (dtRow);

			aTable.PrimaryKey = new DataColumn[] { aTable.Columns["ID"] };
			return aTable;
		}
	}
}

#endif
