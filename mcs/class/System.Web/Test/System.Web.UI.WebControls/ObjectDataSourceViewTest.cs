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

		public bool IsTrackingViewState ()
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

	}

	[TestFixture]
	public class ObjectDataSourceViewTest
	{

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
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
			Assert.AreEqual (true, sql.IsTrackingViewState (), "IsTrackingViewState");
		}

		[Test]
		public void DefaultsNotWorking ()
		{
			ObjectDataSource ds = new ObjectDataSource ();
			ObjectViewPoker sql = new ObjectViewPoker (ds, "DefaultView", null);
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

			Parameter p1 = new Parameter ("test", TypeCode.String);
			ds.SelectParameters.Add (p1);
			ds.FilterParameters.Add (p1);

			view = (ObjectDataSourceView) ds.DoGetView ("DefaultView");
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

			((IStateManager) view).TrackViewState ();
			object state = ((IStateManager) view).SaveViewState ();

			ObjectDataSourceView copy = new ObjectDataSourceView (ds, "DefaultView", null);
			((IStateManager) copy).LoadViewState (state);

			Assert.AreEqual (null, state, "ViewState#1");
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
		public void CanRetrieveTotalRowCount () {
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
			string origin = @"<div>
						<table cellspacing=""0"" rules=""all"" border=""1"" id=""Grid"" style=""border-collapse:collapse;"">
							<tr>
								<th scope=""col"">ID</th><th scope=""col"">FName</th><th scope=""col"">LName</th>
							</tr><tr>
								<td>1001</td><td>Mahesh</td><td>Chand</td>
							</tr>
						</table>
					</div>";
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (html), "ObjectDataSourceViewSelect");
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
			string origin = @"<div>
						<table cellspacing=""0"" rules=""all"" border=""1"" id=""Grid"" style=""border-collapse:collapse;"">
							<tr>
								<th scope=""col"">ID</th><th scope=""col"">FName</th><th scope=""col"">LName</th>
							</tr><tr>
								<td>1001</td><td>Mahesh</td><td>Chand</td>
							</tr><tr>
								<td colspan=""3""><table border=""0"">
									<tr>
										<td><span>1</span></td><td><a href=""javascript:__doPostBack('Grid','Page$2')"">2</a></td><td><a href=""javascript:__doPostBack('Grid','Page$3')"">3</a></td><td><a href=""javascript:__doPostBack('Grid','Page$4')"">4</a></td><td><a href=""javascript:__doPostBack('Grid','Page$5')"">5</a></td>
									</tr>
								</table></td>
							</tr>
						</table>
					</div>";
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (html), "ObjectDataSourceViewSelectCount");
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
			string origin = @"<div>
						<table cellspacing=""0"" rules=""all"" border=""1"" id=""Grid"" style=""border-collapse:collapse;"">
							<tr>
								<th scope=""col"">ID</th><th scope=""col"">FName</th><th scope=""col"">LName</th>
							</tr><tr>
								<td>1001</td><td>Mahesh</td><td>Chand</td>
							</tr><tr>
								<td>1000</td><td>Yonik</td><td>Laim</td>
							</tr>
						</table>
					</div>";
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (html), "ObjectDataSourceViewInsert");
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
			string origin = @"<div>
						<table cellspacing=""0"" rules=""all"" border=""1"" id=""Grid"" style=""border-collapse:collapse;"">
							<tr>
								<th scope=""col"">ID</th><th scope=""col"">FName</th><th scope=""col"">LName</th>
							</tr><tr>
								<td>1001</td><td>Yonik</td><td>Laim</td>
							</tr>
						</table>
					</div>";
			HtmlDiff.AssertAreEqual (origin, HtmlDiff.GetControlFromPageHtml (html), "ObjectDataSourceViewUpdate");
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
