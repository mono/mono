//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
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
using System.Threading;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	public class  ObjectDataSourcePoker : ObjectDataSource
	{
		public ObjectDataSourcePoker () // constructor
		{
			
		TrackViewState ();
		}

		public void DoRaiseDataSourceChangedEvent ()
		{
			base.RaiseDataSourceChangedEvent (new EventArgs ());
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

		public void DoOnBubbleEvent (Object source, EventArgs e)
		{
			base.OnBubbleEvent (source, e);
		}

		public object DoSaveControlState ()
		{
			return base.SaveControlState ();
		}

		public void DoLoadControlState (object savedState)
		{
			 base.LoadControlState (savedState);
		}

		public new DataSourceView GetView (string viewName)
		{
			return base.GetView (viewName);
		}
	}

	#region Hellp_class_view
	public class CustomObjectDataSourceView : ObjectDataSourceView
	{
		public CustomObjectDataSourceView (ObjectDataSource owner, string name, HttpContext context)
			: base (owner, name, context)
		{
		}

		public new int ExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return base.ExecuteUpdate (keys, values, oldValues);
		}

		public new int ExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			return base.ExecuteDelete (keys, oldValues);
		}

		public new IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			return base.ExecuteSelect (arguments);
		}

		public new int ExecuteInsert (IDictionary values)
		{
			return base.ExecuteInsert (values);
		}

	}
	#endregion

	[TestFixture]
	public class ObjectDataSourceTest
	{
		bool eventChecker;
		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		public static void InitObjectDataSource (ObjectDataSourcePoker ds, string action)
		{
			Parameter p1, p2, p3;
			switch (action) {		
				
			case "insert":	p1 = new Parameter ("ID", TypeCode.String, "1004");
					p2 = new Parameter ("fname", TypeCode.String, "David");
					p3 = new Parameter ("LName", TypeCode.String, "Eli");
					break;
				
			case "update": 	p1 = new Parameter ("ID", TypeCode.String, "1001");
					p2 = new Parameter ("FName", TypeCode.String, "David");
					p3 = new Parameter ("LName", TypeCode.String, "Eli");
					break;
			case "DBNull":  p1 = new Parameter ("ID");
					p2 = new Parameter ("FName");
					p3 = new Parameter ("LName");
					break;
				
			default: 	p1 = new Parameter ("ID", TypeCode.String, "1001");
					p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
					p3 = new Parameter ("LName", TypeCode.String, "chand");
					break;
				
			}
			ds.SelectMethod = "GetMyData";
			ds.DeleteMethod = "Delete";
			ds.InsertMethod = "Insert";
			ds.UpdateMethod = "Update";
			ds.SelectCountMethod = "SelectCount";
			ds.DeleteParameters.Add (p1);
			ds.DeleteParameters.Add (p2);
			ds.DeleteParameters.Add (p3);
			ds.InsertParameters.Add (p1);
			ds.InsertParameters.Add (p2);
			ds.InsertParameters.Add (p3);
			ds.UpdateParameters.Add (p1);
			ds.UpdateParameters.Add (p2);
			ds.UpdateParameters.Add (p3);
			ds.ID = "MyObject";
			ds.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
										      

		}

		//Default properties
		

		[Test]		
		public void ObjectDataSource_DefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			
			Assert.AreEqual (ConflictOptions.OverwriteChanges, ods.ConflictDetection, "ConflictDetection");			
			Assert.AreEqual ("",ods.DataObjectTypeName ,"DataObjectTypeName ");
			Assert.AreEqual ("", ods.DeleteMethod, "DeleteMethod");
			Assert.AreEqual (typeof(ParameterCollection),ods.DeleteParameters.GetType (),"DeleteParameters");			
			Assert.AreEqual (false, ods.EnablePaging, "EnablePaging ");
			Assert.AreEqual ("", ods.FilterExpression, "FilterExpression ");
			Assert.AreEqual (typeof (ParameterCollection), ods.FilterParameters.GetType (), "FilterParameters");
			Assert.AreEqual ("", ods.InsertMethod, "InsertMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.InsertParameters.GetType (), "InsertParameters ");
			Assert.AreEqual ("maximumRows", ods.MaximumRowsParameterName, "MaximumRowsParameterName");
			Assert.AreEqual ("{0}", ods.OldValuesParameterFormatString, "OldValuesParameterFormatString");
			Assert.AreEqual ("", ods.SelectCountMethod, "SelectCountMethod");
			Assert.AreEqual ("", ods.SelectMethod, "SelectMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.SelectParameters.GetType (), "SelectParameters");
			Assert.AreEqual ("", ods.SortParameterName, "SortParameterName");			
			Assert.AreEqual ("startRowIndex", ods.StartRowIndexParameterName, "StartRowIndexParameterName");
			Assert.AreEqual ("", ods.TypeName, "TypeName");
			Assert.AreEqual ("", ods.UpdateMethod, "UpdateMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.UpdateParameters.GetType (), "UpdateParameters");
			Assert.AreEqual (0, ods.CacheDuration, "CacheDuration");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, ods.CacheExpirationPolicy, "CacheExpirationPolicy");
			Assert.AreEqual ("", ods.CacheKeyDependency, "CacheKeyDependency");
			Assert.AreEqual (false, ods.ConvertNullToDBNull, "ConvertNullToDBNull ");
			Assert.AreEqual (false, ods.EnableCaching, "EnableCaching ");
			Assert.AreEqual ("", ods.SqlCacheDependency, "SqlCacheDependency");
			
		}

		//Non default properties values

		[Test]		
		public void ObjectDataSource_AssignToDefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"");				
			ods.ConflictDetection = ConflictOptions.CompareAllValues;
			Assert.AreEqual (ConflictOptions.CompareAllValues, ods.ConflictDetection, "ConflictDetection");			
			ods.DataObjectTypeName = "MyData";
			Assert.AreEqual ("MyData", ods.DataObjectTypeName, "DataObjectTypeName ");
			Assert.AreEqual ("Delete", ods.DeleteMethod, "DeleteMethod");
			Assert.AreEqual (3, ods.DeleteParameters.Count, "DeleteParameters");			
			ods.EnablePaging = true;
			Assert.AreEqual (true, ods.EnablePaging, "EnablePaging ");
			ods.FilterExpression = "ID='{0}'";
			Assert.AreEqual ("ID='{0}'", ods.FilterExpression, "FilterExpression ");
			TextBox TextBox1=new TextBox ();
			TextBox1.Text ="1001"; 
			FormParameter p=new FormParameter ("ID","TextBox1");
			p.DefaultValue = "1002";
			ods.FilterParameters.Add (p);  
			Assert.AreEqual ("ID", ods.FilterParameters[0].Name, "FilterParameters1");
			Assert.AreEqual ("1002", ods.FilterParameters[0].DefaultValue , "FilterParameters2");
			Assert.AreEqual ("TextBox1", ((FormParameter )ods.FilterParameters[0]).FormField, "FilterParameters3");
			Assert.AreEqual ("Insert", ods.InsertMethod, "InsertMethod ");
			Assert.AreEqual ("ID", ods.InsertParameters[0].Name , "InsertParameters ");
			ods.MaximumRowsParameterName = "SelectCount";
			Assert.AreEqual ("SelectCount", ods.MaximumRowsParameterName, "MaximumRowsParameterName");
			ods.OldValuesParameterFormatString = "ID";
			Assert.AreEqual ("ID", ods.OldValuesParameterFormatString, "OldValuesParameterFormatString");
			Assert.AreEqual ("SelectCount", ods.SelectCountMethod, "SelectCountMethod");
			Assert.AreEqual ("GetMyData", ods.SelectMethod, "SelectMethod ");
			Parameter dummy = new Parameter ();
			dummy.Name = "Test";
			ods.SelectParameters.Add (dummy);
			Assert.AreEqual ("Test", ods.SelectParameters[0].Name , "SelectParameters");
			ods.SortParameterName = "sortExpression";
			Assert.AreEqual ("sortExpression", ods.SortParameterName, "SortParameterName");			
			ods.StartRowIndexParameterName = "ID";
			Assert.AreEqual ("ID", ods.StartRowIndexParameterName, "StartRowIndexParameterName");
			Assert.AreEqual (typeof (MyTableObject).AssemblyQualifiedName, ods.TypeName, "TypeName");
			Assert.AreEqual ("Update", ods.UpdateMethod, "UpdateMethod ");
			Assert.AreEqual ("FName", ods.UpdateParameters[1].Name, "UpdateParameters");
			ods.CacheDuration = 1000;
			Assert.AreEqual (1000, ods.CacheDuration, "CacheDuration");
			ods.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			Assert.AreEqual (DataSourceCacheExpiry.Sliding, ods.CacheExpirationPolicy, "CacheExpirationPolicy");
			ods.CacheKeyDependency = "ID";
			Assert.AreEqual ("ID", ods.CacheKeyDependency, "CacheKeyDependency");
			ods.ConvertNullToDBNull = true;
			Assert.AreEqual (true, ods.ConvertNullToDBNull, "ConvertNullToDBNull ");
			ods.EnableCaching = true;
			Assert.AreEqual (true, ods.EnableCaching, "EnableCaching ");
			ods.SqlCacheDependency = "Northwind:Employees";
			Assert.AreEqual ("Northwind:Employees", ods.SqlCacheDependency, "SqlCacheDependency");
			
		}

		//ViewState

		[Test]
		public void ObjectDataSource_ViewState ()
		{
			ObjectDataSourcePoker  ods = new ObjectDataSourcePoker ();
			//InitObjectDataSource (ods,"");	

			ods.CacheDuration = 1;
			ods.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			ods.CacheKeyDependency = "key";
			ods.ConflictDetection = ConflictOptions.CompareAllValues;
			ods.ConvertNullToDBNull = true;
			ods.DataObjectTypeName = "DataObjectType";
			ods.DeleteMethod = "deleteMethod";
			ods.EnableCaching = true;
			ods.EnablePaging = true;
			ods.FilterExpression = "filter expression";
			ods.InsertMethod = "insertMethod";
			ods.MaximumRowsParameterName = "maxRows";
			ods.OldValuesParameterFormatString = "old_{0}";
			ods.SelectCountMethod = "selectCountMethod";
			ods.SelectMethod = "selectMethod";
			ods.SortParameterName = "sortParamName";
			ods.SqlCacheDependency = "cacheDependency";
			ods.StartRowIndexParameterName = "startRow";
			ods.TypeName = "TypeName";
			ods.UpdateMethod = "updateMethod";

			object state = ods.SaveState ();
			Assert.IsNull (state, "ViewState is null");

			ObjectDataSourcePoker copy = new ObjectDataSourcePoker ();
			copy.LoadState (state);

		}

		//Properties functionality

		public void ObjectDataSource_ConflictDetection ()
		{ 
			//Not implemented			 
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_ConvertNullToDBNull ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (new PageDelegate (ConvertNullToDBNull))).Run ();
		}

		
		public static void ConvertNullToDBNull (Page p)
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"DBNull");
			bool dbnull = false;
			ods.ConvertNullToDBNull = true;
			try {
				ods.Delete ();
			}
			catch (Exception ex) {
				Assert.AreEqual (true,
					ex.Message.Contains ("type 'System.DBNull' cannot be converted to type 'System.String'") || // dotnet
					ex.Message.Contains ("parameters"), "ConvertNullToDBNull"); // mono
				dbnull = true;
			}
			Assert.AreEqual (true, dbnull, "ConvertNullToDBNull2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterExpression ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterExpression))).Run ();
			string newHtml= HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = "<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td>ID</td><td>FName</td><td>LName</td>\r\n\t</tr><tr>\r\n\t\t<td>1002</td><td>Melanie</td><td>Talmadge</td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "FilterExpression");
		}


		public static void FilterExpression (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			DataGrid dg = new DataGrid ();			
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.FilterExpression = "ID='1002'";
			p.Controls.Add (lcb); 
			p.Controls.Add (dg);
			p.Controls.Add (ods);
			p.Controls.Add (lce); 
			dg.DataSource = ods;
			dg.DataBind ();
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterParameter ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterParameter))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = "<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n\t<tr>\r\n\t\t<td>ID</td><td>FName</td><td>LName</td>\r\n\t</tr><tr>\r\n\t\t<td>1003</td><td>Vinay</td><td>Bansal</td>\r\n\t</tr>\r\n</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "FilterExpression");
		}

		public static void FilterParameter (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			DataGrid dg = new DataGrid ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.FilterExpression = "{0}";
			Parameter p1 = new Parameter ("ID", TypeCode.String, "ID=1003");
			ods.FilterParameters.Add (p1); 
			p.Controls.Add (lcb);
			p.Controls.Add (dg);
			p.Controls.Add (ods);
			p.Controls.Add (lce);
			dg.DataSource = ods;
			dg.DataBind ();

		}


		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_EnablePaging ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EnablePaging))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<th scope=\"col\">Name</th><th scope=\"col\">Number</th>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Number0</td><td>0</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Number1</td><td>1</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Number2</td><td>2</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Number3</td><td>3</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>Number4</td><td>4</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td colspan=\"2\"><table border=\"0\">\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td><span>1</span></td><td><a href=\"javascript:__doPostBack('ctl01','Page$2')\">2</a></td><td><a href=\"javascript:__doPostBack('ctl01','Page$3')\">3</a></td><td><a href=\"javascript:__doPostBack('ctl01','Page$4')\">4</a></td>\r\n\t\t\t\t</tr>\r\n\t\t\t</table></td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "EnablePaging");
		}

		public static void EnablePaging (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			GridView  gv = new GridView ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.ID = "ObjectDataSource1";
			ods.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			ods.SelectMethod = "SelectForPaging";
			ods.EnablePaging = true;
			ods.SelectCountMethod = "SelectCount";
			ods.MaximumRowsParameterName = "maxRows";
			ods.StartRowIndexParameterName = "startIndex";
			gv.AllowPaging = true;
			gv.PageSize = 5;
			p.Controls.Add (lcb);
			p.Controls.Add (gv);
			p.Controls.Add (ods);
			p.Controls.Add (lce);
			gv.DataSourceID = "ObjectDataSource1";
			gv.DataBind ();	

		}

		//public methods

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Delete ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (DeleteMethod))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = "<div>\r\n\t<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">\r\n\t\t<tr>\r\n\t\t\t<td>ID</td><td>1002</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>FName</td><td>Melanie</td>\r\n\t\t</tr><tr>\r\n\t\t\t<td>LName</td><td>Talmadge</td>\r\n\t\t</tr>\r\n\t</table>\r\n</div>";

			HtmlDiff.AssertAreEqual (origHtml, newHtml, "DeleteRender");
		}

		public static void DeleteMethod (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			DetailsView dv = new DetailsView ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.Deleted += new ObjectDataSourceStatusEventHandler (odc_Deleted);
			ods.Deleting += new ObjectDataSourceMethodEventHandler (odc_Deleting);	
			InitObjectDataSource (ods,"");
			dv.Page = p;
			ods.Page = p;
			dv.DataKeyNames = new string[] { "ID" };
			dv.DataSource = ods;
			p.Controls.Add (lcb); 
			p.Controls.Add (ods);
			p.Controls.Add (dv);
			p.Controls.Add (lce); 
			dv.DataBind ();
			Assert.AreEqual (3, dv.DataItemCount, "BeforeDelete1");
			Assert.AreEqual (1001, dv.SelectedValue, "BeforeDelete2");
			Assert.AreEqual (false, deleting, "BeforeDeletingEvent");
			Assert.AreEqual (false, deleted, "BeforeDeletedEvent");
			ods.Delete ();			
			dv.DataBind ();
			Assert.AreEqual (true, deleting, "AfterDeletingEvent");
			Assert.AreEqual (true, deleted, "AfterDeletedEvent");
			Assert.AreEqual (2, dv.DataItemCount, "BeforeDelete1");
			Assert.AreEqual (1002, dv.SelectedValue, "BeforeDelete2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Select ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (SelectMethod))).Run ();
		}

		
		public static void SelectMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"");			
			ods.Selected += new ObjectDataSourceStatusEventHandler (odc_Selected);
			ods.Selecting += new ObjectDataSourceSelectingEventHandler (odc_Selecting);
			p.Controls.Add (ods);
			Assert.AreEqual (false, selecting, "BeforeSelectingEvent");
			Assert.AreEqual (false, selected, "BeforeSelectedEvent");			
			IEnumerable table = (IEnumerable) ods.Select ();
			Assert.AreEqual (3,((DataView) table).Count, "ItemsCount");
			Assert.AreEqual ("Mahesh", ((DataView) table)[0].Row.ItemArray[1], "FirstItemData");
			Assert.AreEqual (1002, ((DataView) table)[1].Row.ItemArray[0], "SecondItemData");
			Assert.AreEqual ("Bansal", ((DataView) table)[2].Row.ItemArray[2], "ThirdItemData");			
			Assert.AreEqual (true, selecting, "AfterSelectingEvent");
			Assert.AreEqual (true, selected, "AfterSelectedEvent");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Select_Cached ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (SelectMethodCached))).Run ();
		}


		public static void SelectMethodCached (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.EnableCaching = true;
			InitObjectDataSource (ods, "");
			p.Controls.Add (ods);
			ods.Selecting += new ObjectDataSourceSelectingEventHandler (odc_Selecting);

			selecting = false;
			IEnumerable table = (IEnumerable) ods.Select ();
			Assert.AreEqual (true, selecting, "AfterSelectingEvent");

			selecting = false;
			IEnumerable table2 = (IEnumerable) ods.Select ();
			Assert.AreEqual (false, selecting, "AfterSelectingEvent");
		}
		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Insert ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (InsertMethod))).Run ();
		}

		public static void InsertMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"insert");
			ods.Inserted += new ObjectDataSourceStatusEventHandler (odc_Inserted);
			ods.Inserting += new ObjectDataSourceMethodEventHandler (odc_Inserting);
			p.Controls.Add (ods);			
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "BeforeInsert");
			Assert.AreEqual (false, inserted , "BeforeInsertedEvent");
			Assert.AreEqual (false, inserting , "BeforeInsertingEvent");
			ods.Insert ();		
			Assert.AreEqual (4, ((DataView) ods.Select ()).Count , "AfterInsert1");
			Assert.AreEqual (1004,((DataView) ods.Select ())[3].Row.ItemArray[0], "AfterInsert2");
			Assert.AreEqual ("David", ((DataView) ods.Select ())[3].Row.ItemArray[1], "AfterInsert3");
			Assert.AreEqual (true, inserted, "AfterInsertedEvent");
			Assert.AreEqual (true, inserting, "AfterInsertingEvent");
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Update ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (UpdateMethod))).Run ();
		}

		public static void UpdateMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "update");
			ods.Updated += new ObjectDataSourceStatusEventHandler (odc_Updated);
			ods.Updating += new ObjectDataSourceMethodEventHandler (odc_Updating);
			p.Controls.Add (ods);
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "BeforeUpdate1");
			Assert.AreEqual (1001, ((DataView) ods.Select ())[0].Row.ItemArray[0], "BeforeUpdate2");
			Assert.AreEqual ("Mahesh", ((DataView) ods.Select ())[0].Row.ItemArray[1], "BeforeUpdate3");
			Assert.AreEqual ("Chand", ((DataView) ods.Select ())[0].Row.ItemArray[2], "BeforeUpdate4");
			Assert.AreEqual (false, updated, "BeforeUpdateEvent");
			Assert.AreEqual (false, updating, "BeforeUpdatingEvent");
			ods.Update ();
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "AfterUpdate1");
			Assert.AreEqual (1001, ((DataView) ods.Select ())[0].Row.ItemArray[0], "AfterUpdate2");
			Assert.AreEqual ("David", ((DataView) ods.Select ())[0].Row.ItemArray[1], "AfterUpdate3");
			Assert.AreEqual ("Eli", ((DataView) ods.Select ())[0].Row.ItemArray[2], "AfterUpdate4");
			Assert.AreEqual (true, updated, "AfterUpdateEvent");
			Assert.AreEqual (true, updating, "AfterUpdatingEvent");
		}

		

		//Events

		private static bool deleted = false;
		private static bool deleting = false;
		private static bool filtering = false;
		private static bool inserted = false;
		private static bool inserting = false;
		private static bool objectCreated = false;
		private static bool objectCreating = false;
		private static bool objectDisposing = false;
		private static bool selected = false;
		private static bool selecting = false;
		private static bool updated = false;
		private static bool updating = false;

		// Tests for events Select,Update,Delete and Insert include in Select,Update,Delete and Insert methods tests.

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Events ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EventsTest))).Run ();
		}

				
		public static void EventsTest (Page p)
		{				
			
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			DetailsView dv = new DetailsView  ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.ObjectCreated += new ObjectDataSourceObjectEventHandler (odc_ObjectCreated);
			ods.ObjectCreating += new ObjectDataSourceObjectEventHandler (odc_ObjectCreating);
			InitObjectDataSource (ods,"");
			ods.FilterExpression = "ID='1001'";			
			dv.Page = p;
			ods.Page = p;
			dv.DataKeyNames = new string[] { "ID" };
			dv.DataSource = ods;
			p.Controls.Add (ods);
			p.Controls.Add (dv);
			dv.DataBind ();							
			ods.Filtering += new ObjectDataSourceFilteringEventHandler (odc_Filtering);
			Assert.AreEqual (false, filtering, "BeforeFilteringEvent");
			ods.Select ();
			Assert.AreEqual (true, filtering, "AfterFilteringEvent");
			ods.ObjectDisposing += new ObjectDataSourceDisposingEventHandler (odc_ObjectDisposing);
			//ToDo: Dispose, ObjectCreated and ObjectCreating should be tested.
			
		}
		
		static void odc_Updating (object sender, ObjectDataSourceMethodEventArgs e)
		{
			updating = true;
		}

		static void odc_Updated (object sender, ObjectDataSourceStatusEventArgs e)
		{
			updated = true;
		}

		static void odc_Selecting (object sender, ObjectDataSourceSelectingEventArgs e)
		{
			selecting = true;
		}

		static void odc_Selected (object sender, ObjectDataSourceStatusEventArgs e)
		{
			selected = true;
		}

		static void odc_ObjectDisposing (object sender, ObjectDataSourceDisposingEventArgs e)
		{
			objectDisposing = true;
		}

		static void odc_ObjectCreating (object sender, ObjectDataSourceEventArgs e)
		{
			objectCreating = true;
		}

		static void odc_ObjectCreated (object sender, ObjectDataSourceEventArgs e)
		{
			objectCreated = true;
		}

		static void odc_Inserting (object sender, ObjectDataSourceMethodEventArgs e)
		{
			inserting = true;
		}

		static void odc_Inserted (object sender, ObjectDataSourceStatusEventArgs e)
		{
			inserted = true;
		}

		static void odc_Filtering (object sender, ObjectDataSourceFilteringEventArgs e)
		{
			filtering = true;
		}

		static void odc_Deleting (object sender, ObjectDataSourceMethodEventArgs e)
		{
			deleting = true;
		}

		static void odc_Deleted (object sender, ObjectDataSourceStatusEventArgs e)
		{
			deleted = true;			
		}

		[Test]
		public void ObjectDataSource_SelectExecute ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.SelectMethod = "GetMyData";
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectParameters.Add (new Parameter ("Fname", TypeCode.String, "TestSelect"));
			ArrayList ls =(ArrayList) view.ExecuteSelect (new DataSourceSelectArguments (""));
			Assert.AreEqual ("TestSelect", ls[0], "SelectExecute");
		}

		[Test]
		public void ObjectDataSource_SelectExecuteCaseSensitive ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.SelectMethod = "GetMyData";
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectParameters.Add (new Parameter ("fname", TypeCode.String, "TestSelect"));
			ArrayList ls = (ArrayList) view.ExecuteSelect (new DataSourceSelectArguments (""));
			Assert.AreEqual ("TestSelect", ls[0], "SelectExecuteCaseSensitive");
		}

		[Test]
		public void ObjectDataSource_DeleteExecute ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "Delete";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "p_Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "p_chand");
			view.DeleteParameters.Add (p1);
			view.DeleteParameters.Add (p2);
			view.DeleteParameters.Add (p3);
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			
			Hashtable keys = new Hashtable();
			keys.Add("ID","k_test_id");
			view.ExecuteDelete (keys, null);
			Assert.AreEqual (true, MyTableObject.DeleteWithParamsAndKeys, "DeleteExecute");
			Assert.AreEqual ("1001, p_Mahesh, p_chand, k_test_id", MyTableObject.UpdatePassedValues, "DeleteExecute Values");
		}

		[Test]
		public void ObjectDataSource_DeleteExecuteParameterCaseSensitive_1 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "Delete";
			Parameter p1, p2, p3;
			p1 = new Parameter ("id", TypeCode.String, "1001");
			p2 = new Parameter ("fname", TypeCode.String, "Mahesh");
			p3 = new Parameter ("lname", TypeCode.String, "chand");
			view.DeleteParameters.Add (p1);
			view.DeleteParameters.Add (p2);
			view.DeleteParameters.Add (p3);
			view.OldValuesParameterFormatString = "oldvalue_{0}";

			Hashtable value = new Hashtable ();
			value.Add ("ID", "test_id");
			view.ExecuteDelete (value, null);
			Assert.AreEqual (true, MyTableObject.DeleteWithParamsAndKeys, "DeleteExecuteParameterCaseSensitive");
		}

		[Test]
		public void ObjectDataSource_DeleteExecuteMethodCaseSensitive ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "delete";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.DeleteParameters.Add (p1);
			view.DeleteParameters.Add (p2);
			view.DeleteParameters.Add (p3);
			view.OldValuesParameterFormatString = "oldvalue_{0}";

			Hashtable value = new Hashtable ();
			value.Add ("ID", "test_id");
			view.ExecuteDelete (value, null);
			Assert.AreEqual (true, MyTableObject.DeleteWithParamsAndKeys, "DeleteExecuteMethodCaseSensitive");
		}

		[Test]
		public void ObjectDataSource_DeleteExecuteCompareAllValues () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "DeleteOldValues";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteDelete (keys, old_value);
			Assert.AreEqual (true, MyTableObject.DeleteWithOldValuesCompareAllValues, "DeleteExecuteCompareAllValues");
			Assert.AreEqual ("ov_1001, ov_Mahesh, ov_chand", MyTableObject.UpdatePassedValues, "DeleteExecuteCompareAllValues Values");
		}

		[Test]
		public void ObjectDataSource_DeleteExecuteDataType () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.DataObjectTypeName = typeof (NewData).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "Delete";
			view.ConflictDetection = ConflictOptions.OverwriteChanges;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteDelete (keys, old_value);
			Assert.AreEqual (true, MyTableObject.DeleteWithDataObjectTypeName, "DeleteExecuteDataType");
			Assert.AreEqual ("k_1001, , ", MyTableObject.UpdatePassedValues, "DeleteExecuteDataType Values");
		}

		[Test]
		public void ObjectDataSource_DeleteExecuteDataTypeCompareAllValues () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.DataObjectTypeName = typeof (NewData).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "Delete";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteDelete (keys, old_value);
			Assert.AreEqual (true, MyTableObject.DeleteWithDataObjectTypeName, "DeleteExecuteDataTypeCompareAllValues");
			Assert.AreEqual ("ov_1001, ov_Mahesh, ov_chand", MyTableObject.UpdatePassedValues, "DeleteExecuteDataTypeCompareAllValues Values");
		}

		[Test]
		public void ObjectDataSource_InsertExecute_1 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.InsertMethod = "Insert";

			//This hashtable ovveride 
			Hashtable value = new Hashtable ();
			value.Add ("ID", "test_id");
			view.ExecuteInsert (value);
			Assert.AreEqual (true, MyTableObject.InsertWithParameters, "InsertExecute#1");
		}

		[Test]
		public void ObjectDataSource_InsertExecute_2 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.InsertMethod = "Insert";

			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.InsertParameters.Add (p1);
			view.InsertParameters.Add (p2);
			view.InsertParameters.Add (p3);

			//This hashtable ovveride 
			Hashtable value = new Hashtable ();
			value.Add ("T", "test_id");

			//Merge parameters
			view.ExecuteInsert (value);
			Assert.AreEqual (true, MyTableObject.InsertWithMergedParameters, "InsertExecute#2");
		}

		[Test]
		public void ObjectDataSource_InsertParametersCaseSensitive ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.InsertMethod = "Insert";

			Parameter p1, p2, p3;
			p1 = new Parameter ("id", TypeCode.String, "1001");
			p2 = new Parameter ("fname", TypeCode.String, "Mahesh");
			p3 = new Parameter ("lname", TypeCode.String, "chand");
			view.InsertParameters.Add (p1);
			view.InsertParameters.Add (p2);
			view.InsertParameters.Add (p3);

			//This hashtable ovveride 
			Hashtable value = new Hashtable ();
			value.Add ("t", "test_id");

			//Merge parameters
			view.ExecuteInsert (value);
			Assert.AreEqual (true, MyTableObject.InsertWithMergedParameters, "InsertParametersCaseSensitive");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_1()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "Update";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);
					
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			Hashtable value = new Hashtable ();
			value.Add ("P", "1000");
			view.ExecuteUpdate (value, null, null);
			Assert.AreEqual (true, MyTableObject.UpdateWithOldValueCollection, "UpdateExecute#1");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_2 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "TryUpdate";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);

			view.OldValuesParameterFormatString = "oldvalue_{0}";
			Hashtable value = new Hashtable ();
			value.Add ("P", "1001");
			
			view.ExecuteUpdate (null, value, null);
			Assert.AreEqual (true, MyTableObject.UpdateWithMergedCollection, "UpdateExecute#2");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_CompareAllValues ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "TryUpdate_1";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);

			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			
			
			Hashtable value = new Hashtable ();
			value.Add ("ID", "1001");

			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.ExecuteUpdate (null,null, value);
			Assert.AreEqual (true, MyTableObject.UpdateWithCompareAllValues, "CompareAllValues");
		}

		private static void InitializeView (CustomObjectDataSourceView view, out Hashtable keys, out Hashtable old_value, out Hashtable new_value) 
		{
			Parameter p1, p2, p3, p4;
			p1 = new Parameter ("oldvalue_ID", TypeCode.String, "p_1001");
			p2 = new Parameter ("FName", TypeCode.String, "p_Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "p_chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);

			p4 = new Parameter ("oldvalue_ID", TypeCode.String, "p_1001");
			view.DeleteParameters.Add (p4);

			view.OldValuesParameterFormatString = "oldvalue_{0}";

			keys = new Hashtable ();
			keys.Add ("ID", "k_1001");

			old_value = new Hashtable ();
			old_value.Add ("ID", "ov_1001");
			old_value.Add ("FName", "ov_Mahesh");
			old_value.Add ("LName", "ov_chand");

			new_value = new Hashtable ();
			new_value.Add ("ID", "n_1001");
			new_value.Add ("FName", "n_Mahesh");
			new_value.Add ("LName", "n_chand");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_CompareAllValues2 () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "UpdateCompareAllValues";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteUpdate (keys, new_value, old_value);
			Assert.AreEqual (true, MyTableObject.UpdateCompareAllValuesCalled, "CompareAllValues2");
			Assert.AreEqual ("n_1001, n_Mahesh, n_chand, k_1001, ov_Mahesh, ov_chand", MyTableObject.UpdatePassedValues, "CompareAllValues2 Values");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_OverwriteChanges () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "UpdateOverwriteChanges";
			view.ConflictDetection = ConflictOptions.OverwriteChanges;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteUpdate (keys, new_value, old_value);
			Assert.AreEqual (true, MyTableObject.UpdateOverwriteChangesCalled, "OverwriteChanges");
			Assert.AreEqual ("n_1001, n_Mahesh, n_chand, k_1001", MyTableObject.UpdatePassedValues, "OverwriteChanges Values");
		}

		[Test]
		public void ObjectDataSource_UpdateExecute_DataObjectTypeName ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.DataObjectTypeName = typeof (NewData).AssemblyQualifiedName;
			
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "Update";
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.ExecuteUpdate (null, null, null);
			Assert.AreEqual (true, MyTableObject.UpdateWithDataObjectTypeName, "UpdateExecute_DataObjectTypeName");
			Assert.AreEqual (", , ", MyTableObject.UpdatePassedValues, "UpdateExecute_DataObjectTypeName Values");
		}
		[Test]
		public void ObjectDataSource_UpdateExecute_DataObjectTypeName2 () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.DataObjectTypeName = typeof (NewData).AssemblyQualifiedName;

			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "Update";
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.ConflictDetection = ConflictOptions.OverwriteChanges;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteUpdate (keys, new_value, old_value);
			Assert.AreEqual (true, MyTableObject.UpdateWithDataObjectTypeName, "UpdateExecute_DataObjectTypeName2");
			Assert.AreEqual ("n_1001, n_Mahesh, n_chand", MyTableObject.UpdatePassedValues, "UpdateExecute_DataObjectTypeName Values");
		}


		[Test]
		public void ObjectDataSource_UpdateExecute_DataObjectTypeNameCompareAllValues () 
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.DataObjectTypeName = typeof (NewData).AssemblyQualifiedName;

			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "Update";
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			Hashtable keys;
			Hashtable old_value;
			Hashtable new_value;
			InitializeView (view, out keys, out old_value, out new_value);

			view.ExecuteUpdate (keys, new_value, old_value);
			Assert.AreEqual (true, MyTableObject.UpdateWithDataObjectTypeNameAllValues, "UpdateExecute_DataObjectTypeNameCompareAllValues");
			Assert.AreEqual ("n_1001, n_Mahesh, n_chand, k_1001, ov_Mahesh, ov_chand", MyTableObject.UpdatePassedValues, "UpdateExecute_DataObjectTypeName Values");
		}

		[Test]
		public void ObjectDataSource_DataSourceChanged ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			((IDataSource) ods).DataSourceChanged += new EventHandler (ObjectDataSourceTest_DataSourceChanged);
			
			// Check if event raised
			ods.DoRaiseDataSourceChangedEvent ();
			Assert.IsTrue (eventChecker, "DataSourceChanged#1");

			eventChecker = false;
			ods.ConflictDetection = ConflictOptions.CompareAllValues;
			Assert.IsFalse (eventChecker, "DataSourceChanged#2");

			eventChecker = false;
			ods.DataObjectTypeName = "MyData";
			Assert.IsFalse (eventChecker, "DataSourceChanged#3");

			eventChecker = false;
			ods.EnablePaging = true;
			Assert.IsFalse (eventChecker, "DataSourceChanged#4");

			eventChecker = false;
			ods.FilterExpression = "ID='{0}'";
			Assert.IsFalse (eventChecker, "DataSourceChanged#5");


			eventChecker = false;
			TextBox TextBox1 = new TextBox ();
			TextBox1.Text = "1001";
			FormParameter p = new FormParameter ("ID", "TextBox1");
			p.DefaultValue = "1002";
			ods.FilterParameters.Add (p);
			Assert.IsFalse (eventChecker, "DataSourceChanged#6");

			eventChecker = false;
			ods.MaximumRowsParameterName = "SelectCount";
			Assert.IsFalse (eventChecker, "DataSourceChanged#7");

			eventChecker = false;
			ods.OldValuesParameterFormatString = "ID";
			Assert.IsFalse (eventChecker, "DataSourceChanged#8");

			eventChecker = false;
			Parameter dummy = new Parameter ();
			dummy.Name = "Test";
			ods.SelectParameters.Add (dummy);
			Assert.IsFalse (eventChecker, "DataSourceChanged#9");

			eventChecker = false;
			ods.SortParameterName = "sortExpression";
			Assert.IsFalse (eventChecker, "DataSourceChanged#10");

			eventChecker = false;
			ods.StartRowIndexParameterName = "ID";
			Assert.IsFalse (eventChecker, "DataSourceChanged#11");

			eventChecker = false; 
			ods.CacheDuration = 1000;
			Assert.IsFalse (eventChecker, "DataSourceChanged#12");

			eventChecker = false;
			ods.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			Assert.IsFalse (eventChecker, "DataSourceChanged#13");

			eventChecker = false;
			ods.CacheKeyDependency = "ID";
			Assert.IsFalse (eventChecker, "DataSourceChanged#14");

			eventChecker = false;
			ods.ConvertNullToDBNull = true;
			Assert.IsFalse (eventChecker, "DataSourceChanged#15");

			eventChecker = false;
			ods.EnableCaching = true;
			Assert.IsFalse (eventChecker, "DataSourceChanged#16");

			eventChecker = false;
			ods.SqlCacheDependency = "Northwind:Employees";
			Assert.IsFalse (eventChecker, "DataSourceChanged#17");
		}

		void ObjectDataSourceTest_DataSourceChanged (object sender, EventArgs e)
		{
			eventChecker = true;
		}

		//Excpetions
		[Test]  // Note: on ConflictOptions.CompareAllValues old values cannot be null;
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_UpdateExecute_CompareAllValues_Exception ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "TryUpdate_1";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);

			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.ExecuteUpdate (null, null, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_UpdateExecute_3 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.UpdateMethod = "Update";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.UpdateParameters.Add (p1);
			view.UpdateParameters.Add (p2);
			view.UpdateParameters.Add (p3);

			view.OldValuesParameterFormatString = "oldvalue_{0}";
			Hashtable value = new Hashtable ();
			value.Add ("ID", "1000");
			view.ExecuteUpdate (value, null, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_InsertParameterException ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.InsertMethod = "Insert";

			Parameter p1, p2, p3;
			p1 = new Parameter ("id", TypeCode.String, "1001");
			p2 = new Parameter ("fname", TypeCode.String, "Mahesh");
			p3 = new Parameter ("lname", TypeCode.String, "chand");
			view.InsertParameters.Add (p1);
			view.InsertParameters.Add (p2);
			view.InsertParameters.Add (p3);

			//This hashtable ovveride 
			Hashtable value = new Hashtable ();
			value.Add ("z", "test_id");

			//Merge parameters
			view.ExecuteInsert (value);
			Assert.AreEqual (true, MyTableObject.InsertWithMergedParameters, "InsertExecute");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_DeleteExecuteMethodParameterException()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "delete";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			view.DeleteParameters.Add (p1);
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			Hashtable value = new Hashtable ();
			value.Add ("ID", "test_id");
			view.ExecuteDelete (value, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_DeleteExecuteOldValueException ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectMethod = "GetMyData";
			view.DeleteMethod = "Delete";
			Parameter p1, p2, p3;
			p1 = new Parameter ("ID", TypeCode.String, "1001");
			p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
			p3 = new Parameter ("LName", TypeCode.String, "chand");
			view.DeleteParameters.Add (p1);
			view.DeleteParameters.Add (p2);
			view.DeleteParameters.Add (p3);
			view.OldValuesParameterFormatString = "oldvalue_{0}";

			Hashtable value = new Hashtable ();
			value.Add ("ID", "test_id");
			value.Add ("FName", "test_FName");
			view.ExecuteDelete (value, null);
			Assert.AreEqual (true, MyTableObject.DeleteWithParamsAndKeys, "DeleteExecute");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_SelectExecuteException_1 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.SelectMethod = "GetMyData";
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectParameters.Add (new Parameter ("Name", TypeCode.String, "TestSelect"));
			IEnumerable res = view.ExecuteSelect (new DataSourceSelectArguments (""));
		}

		[ExpectedException (typeof (InvalidOperationException))]
		public void ObjectDataSource_SelectExecuteException_2 ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			CustomObjectDataSourceView view = new CustomObjectDataSourceView (ods, "CustomView", null);
			view.SelectMethod = "Fake";
			view.TypeName = typeof (MyTableObject).AssemblyQualifiedName;
			view.SelectParameters.Add (new Parameter ("Fname", TypeCode.String, "TestSelect"));
			IEnumerable res = view.ExecuteSelect (new DataSourceSelectArguments (""));
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NunitWeb")]
		public void ObjectDataSource_EnableCachingException ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EnableCachingException))).Run ();
		}

		
		public static void EnableCachingException (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.SelectMethod = "SelectException";
			ods.EnableCaching = true;			
			p.Controls.Add (ods);			
			IEnumerable table = (IEnumerable) ods.Select ();
			
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterExpressionException ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterExpressionException))).Run ();
		}


		public static void FilterExpressionException (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.SelectMethod = "SelectException";
			ods.FilterExpression  = "ID='1001'";
			p.Controls.Add (ods);
			IEnumerable table = (IEnumerable) ods.Select ();
		}
	}

	# region Object_Data_Source_DAL
	public class MyTableObject 
	{
		public static DataTable ds = CreateDataTable ();
		public MyTableObject ()
		{
			ResetEventCheckers ();
		}
		
		public static DataTable GetMyData ()
		{
			return ds;
		}

		public static ArrayList GetMyData (string Fname)
		{
			ArrayList ar = new ArrayList ();
			ar.Add (Fname);
			return ar; 
		}

		public static DbDataReader SelectException ()
		{
			return new DataTableReader (new DataTable ());
		}

		public static int SelectCount ()
		{
			return 20;
		}

		
		public static DataTable Delete (string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find (ID);
			ds.Rows.Remove (dr);
			return ds;

		}

		public static DataTable Delete (string ID, string FName, string LName, string oldvalue_ID)
		{
			DeleteWithParamsAndKeys = true;
			UpdatePassedValues = String.Join (", ", new string [] { ID, FName, LName, oldvalue_ID });
			DataRow dr = ds.Rows.Find (ID);
			try {
				ds.Rows.Remove (dr); }
			catch{}
			return ds;
		}

		public static DataTable DeleteOldValues (string oldvalue_ID, string oldvalue_FName, string oldvalue_LName) 
		{
			DeleteWithOldValuesCompareAllValues = true;
			UpdatePassedValues = String.Join (", ", new string [] { oldvalue_ID, oldvalue_FName, oldvalue_LName });
			return ds;
		}

		public static DataTable Delete (NewData anyName) 
		{
			UpdatePassedValues = String.Join (", ", new string [] { anyName.ID, anyName.FName, anyName.LName });
			DeleteWithDataObjectTypeName = true;
			return ds;
		}

		public static bool DeleteWithParamsAndKeys;
		public static bool DeleteWithOldValuesCompareAllValues;
		public static bool DeleteWithDataObjectTypeName;
		public static bool UpdateWithOldValueCollection;
		public static bool UpdateWithMergedCollection;
		public static bool InsertWithParameters;
		public static bool InsertWithMergedParameters;
		public static bool UpdateWithCompareAllValues;
		public static bool UpdateWithDataObjectTypeName;
		public static bool UpdateWithDataObjectTypeNameAllValues;
		public static bool UpdateCompareAllValuesCalled;
		public static bool UpdateOverwriteChangesCalled;
		public static string UpdatePassedValues;
	
		private void ResetEventCheckers()
		{
			DeleteWithParamsAndKeys = false;
			DeleteWithOldValuesCompareAllValues = false;
			DeleteWithDataObjectTypeName = false;
			InsertWithParameters = false;
			InsertWithMergedParameters = false;
			UpdateWithOldValueCollection = false;
			UpdateWithMergedCollection = false;
			UpdateWithCompareAllValues = false;
			UpdateWithDataObjectTypeName = false;
			UpdateWithDataObjectTypeNameAllValues = false;
			UpdateCompareAllValuesCalled = false;
			UpdateOverwriteChangesCalled = false;
			UpdatePassedValues = "";
		}

		public static DataTable Update (string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find (ID);
			if (dr == null) {
				Label lbl = new Label ();
				lbl.Text = "ID doesn't exist. update only FName and LName";
				return ds;
			}
			dr["FName"] = FName;
			dr["LName"] = LName;
			return ds;

		}

		public static DataTable TryUpdate_1 (string ID, string FName, string LName, string oldvalue_ID)
		{
			UpdateWithCompareAllValues = true;
			return ds;
		}

		
		public static DataTable TryUpdate (string ID, string FName, string LName, string P)
		{
			UpdateWithMergedCollection = true;
			return ds;
		}

		public static DataTable Update (string ID, string FName, string LName, string oldvalue_P )
		{
			UpdateWithOldValueCollection = true;
			return ds;
		}

		public static DataTable Update (NewData data)
		{
			UpdatePassedValues = String.Join (", ", new string [] { data.ID, data.FName, data.LName});
			UpdateWithDataObjectTypeName = true;
			return ds;
		}

		public static DataTable Update (NewData data, NewData oldvalue_data) 
		{
			UpdatePassedValues = String.Join (", ", new string [] { data.ID, data.FName, data.LName, oldvalue_data.ID, oldvalue_data.FName, oldvalue_data.LName });
			UpdateWithDataObjectTypeNameAllValues = true;
			return ds;
		}

		public static DataTable UpdateCompareAllValues (string ID, string FName, string LName,
													   string oldvalue_ID, string oldvalue_FName, string oldvalue_LName) 
		{
			UpdatePassedValues = String.Join (", ", new string [] {ID, FName, LName, oldvalue_ID, oldvalue_FName, oldvalue_LName });
			UpdateCompareAllValuesCalled = true;
			return ds;
		}

		public static DataTable UpdateOverwriteChanges (string ID, string FName, string LName, string oldvalue_ID) 
		{
			UpdatePassedValues = String.Join (", ", new string [] { ID, FName, LName, oldvalue_ID });
			UpdateOverwriteChangesCalled = true;
			return ds;
		}


		public static DataTable Insert (string ID)
		{
			InsertWithParameters = true;
			return ds;
		}

		public static DataTable Insert (string ID, string FName, string LName,string T)
		{
			InsertWithMergedParameters = true;
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

			// Add the column to the DataColumnCollection.

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


			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1002;
			dtRow["FName"] = "Melanie";
			dtRow["LName"] = "Talmadge";
			aTable.Rows.Add (dtRow);

			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1003;
			dtRow["FName"] = "Vinay";
			dtRow["LName"] = "Bansal";
			aTable.Rows.Add (dtRow);

			aTable.PrimaryKey = new DataColumn[] { aTable.Columns["ID"] };
			return aTable;

		}

		public static DataTable SelectForPaging (int startIndex, int maxRows)
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Name", typeof (string));
			table.Columns.Add ("Number", typeof (int));
			int current;
			for (int i = 0; i < maxRows; i++) {
				current = i + startIndex;
				table.Rows.Add (new object[] { "Number" + current.ToString (), current });
			}
			return table;
		}



	}
	#endregion

	#region DataObjectTypeName
	public class NewData
	{
		private string IDValue;
		private string FNameValue;
		private string LNameValue;

		public string LName
		{
			get { return LNameValue; }
			set { LNameValue = value; }
		}
		
		public string FName
		{
			get { return FNameValue; }
			set { FNameValue = value; }
		}

		public string ID
		{
			get { return IDValue; }
			set { IDValue = value; }
		}

	}
	#endregion

}
#endif
