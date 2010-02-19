//
// Tests for System.Web.UI.WebControls.SqlDataSource
//
// Author:
//	Chris Toshok (toshok@novell.com)
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
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Text;
using System.Data;

using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

namespace MonoTests.System.Web.UI.WebControls
{
	class SqlPoker : SqlDataSource
	{
		public SqlPoker ()
		{
			TrackViewState ();
		}

		public object SaveToViewState ()
		{
			return SaveViewState ();
		}

		public void LoadFromViewState (object savedState)
		{
			LoadViewState (savedState);
		}

		public void DoRaiseDataSourceChangedEvent ()
		{
			base.RaiseDataSourceChangedEvent(new EventArgs());
		}
	}

	class CustomSqlDataSourceView : SqlDataSourceView
	{
		public CustomSqlDataSourceView (SqlDataSource owner,string name,HttpContext context):base(owner,name,context)
		{
		}
		
		public new int ExecuteDelete (global::System.Collections.IDictionary keys, global::System.Collections.IDictionary oldValues)
		{
			return base.ExecuteDelete (keys, oldValues);
		}

		public new int ExecuteInsert (global::System.Collections.IDictionary values)
		{
			return base.ExecuteInsert (values);
		}

		public new global::System.Collections.IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			return base.ExecuteSelect (arguments);
		}

		public new int ExecuteUpdate (global::System.Collections.IDictionary keys, global::System.Collections.IDictionary values, global::System.Collections.IDictionary oldValues)
		{
			return base.ExecuteUpdate (keys, values, oldValues);
		}

		
	}

	[TestFixture]
	public class SqlDataSourceTest
	{
		[SetUp]
		public void SetUp ()
		{
			SqlDataSourceTest.CustomEventParameterCollection = null;
			SqlDataSourceTest.PassedParameters = "";

			WebTest.CopyResource (GetType (), "SqlDataSource_OnInit_Bug572781.aspx", "SqlDataSource_OnInit_Bug572781.aspx");
		}

		[Test]
		public void Defaults ()
		{
			SqlPoker sql = new SqlPoker ();
			Assert.AreEqual ("", sql.CacheKeyDependency, "A1");
			Assert.IsTrue (sql.CancelSelectOnNullParameter, "A2");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "A3");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "A4");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "A5");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "A6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "A7");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A8");
			Assert.AreEqual ("", sql.SqlCacheDependency, "A9");
			Assert.AreEqual ("", sql.SortParameterName, "A10");
			Assert.AreEqual (0, sql.CacheDuration, "A11");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, sql.CacheExpirationPolicy, "A12");
			Assert.IsFalse (sql.EnableCaching, "A13");
			Assert.AreEqual ("", sql.ProviderName, "A14");
			Assert.AreEqual ("", sql.ConnectionString, "A15");
			Assert.AreEqual (SqlDataSourceMode.DataSet, sql.DataSourceMode, "A16");
			Assert.AreEqual ("", sql.DeleteCommand, "A17");
			Assert.IsNotNull (sql.DeleteParameters, "A18");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "A18.1");
			Assert.IsNotNull (sql.FilterParameters, "A19");
			Assert.AreEqual (0, sql.FilterParameters.Count, "A19.1");
			Assert.AreEqual ("", sql.InsertCommand, "A20");
			Assert.IsNotNull (sql.InsertParameters, "A21");
			Assert.AreEqual (0, sql.InsertParameters.Count, "A21.1");
			Assert.AreEqual ("", sql.SelectCommand, "A22");
			Assert.IsNotNull (sql.SelectParameters, "A23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "A23.1");
			Assert.AreEqual ("", sql.UpdateCommand, "A24");
			Assert.IsNotNull (sql.UpdateParameters, "A25");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "A25.1");
			Assert.AreEqual ("", sql.FilterExpression, "A26");
		}

		[Test]
		public void ViewState ()
		{
			SqlPoker sql = new SqlPoker ();

			sql.CacheKeyDependency = "hi";
			sql.CancelSelectOnNullParameter = false;
			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			sql.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
			sql.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
			sql.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
			sql.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
			sql.OldValuesParameterFormatString = "{1}";
			sql.SqlCacheDependency = "hi";
			sql.SortParameterName = "hi";
			sql.CacheDuration = 1;
			sql.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			sql.EnableCaching = true;
			sql.DataSourceMode = SqlDataSourceMode.DataReader;
			sql.DeleteCommand = "DELETE foo";
			sql.InsertCommand = "INSERT foo";
			sql.SelectCommand = "SELECT foo";
			sql.UpdateCommand = "UPDATE foo";
			sql.FilterExpression = "hi";

			Assert.AreEqual ("hi", sql.CacheKeyDependency, "A1");
			Assert.IsFalse (sql.CancelSelectOnNullParameter, "A2");
			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "A3");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.DeleteCommandType, "A4");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.InsertCommandType, "A5");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.SelectCommandType, "A6");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.UpdateCommandType, "A7");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "A8");
			Assert.AreEqual ("hi", sql.SqlCacheDependency, "A9");
			Assert.AreEqual ("hi", sql.SortParameterName, "A10");
			Assert.AreEqual (1, sql.CacheDuration, "A11");
			Assert.AreEqual (DataSourceCacheExpiry.Sliding, sql.CacheExpirationPolicy, "A12");
			Assert.IsTrue (sql.EnableCaching, "A13");
			Assert.AreEqual (SqlDataSourceMode.DataReader, sql.DataSourceMode, "A16");
			Assert.AreEqual ("DELETE foo", sql.DeleteCommand, "A17");
			Assert.AreEqual ("INSERT foo", sql.InsertCommand, "A20");
			Assert.AreEqual ("SELECT foo", sql.SelectCommand, "A22");
			Assert.AreEqual ("UPDATE foo", sql.UpdateCommand, "A24");
			Assert.AreEqual ("hi", sql.FilterExpression, "A26");

			object state = sql.SaveToViewState ();
			Assert.IsNull (state, "ViewState is null");

			sql = new SqlPoker ();
			sql.LoadFromViewState (state);

			Assert.AreEqual ("", sql.CacheKeyDependency, "B1");
			Assert.IsTrue (sql.CancelSelectOnNullParameter, "B2");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "B3");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "B4");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "B5");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "B6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "B7");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "B8");
			Assert.AreEqual ("", sql.SqlCacheDependency, "B9");
			Assert.AreEqual ("", sql.SortParameterName, "B10");
			Assert.AreEqual (0, sql.CacheDuration, "B11");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, sql.CacheExpirationPolicy, "B12");
			Assert.IsFalse (sql.EnableCaching, "B13");
			Assert.AreEqual (SqlDataSourceMode.DataSet, sql.DataSourceMode, "B16");
			Assert.AreEqual ("", sql.DeleteCommand, "B17");
			Assert.IsNotNull (sql.DeleteParameters, "B18");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "B18.1");
			Assert.IsNotNull (sql.FilterParameters, "B19");
			Assert.AreEqual (0, sql.FilterParameters.Count, "B19.1");
			Assert.AreEqual ("", sql.InsertCommand, "B20");
			Assert.IsNotNull (sql.InsertParameters, "B21");
			Assert.AreEqual (0, sql.InsertParameters.Count, "B21.1");
			Assert.AreEqual ("", sql.SelectCommand, "B22");
			Assert.IsNotNull (sql.SelectParameters, "B23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "B23.1");
			Assert.AreEqual ("", sql.UpdateCommand, "B24");
			Assert.IsNotNull (sql.UpdateParameters, "B25");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "B25.1");
			Assert.AreEqual ("", sql.FilterExpression, "B26");
		}

		// Help parameter for Asserts
		private static SqlParameterCollection CustomEventParameterCollection;
		private static string PassedParameters;

		[Test]
		public void ReturnValueParameter ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.OldValuesParameterFormatString = "origin_{0}";

			view.SelectParameters.Add (new Parameter ("ProductID", TypeCode.Int32, "10"));
			Parameter myReturn = new Parameter ("myReturn", TypeCode.Int32);
			myReturn.Direction = ParameterDirection.ReturnValue;
			view.SelectParameters.Add (myReturn);
			
			view.Selecting += new SqlDataSourceSelectingEventHandler (view_Selecting);
			view.Select (new DataSourceSelectArguments ());
			
			Assert.IsNotNull (CustomEventParameterCollection, "Select event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.IsNotNull (CustomEventParameterCollection ["@myReturn"], "Parameter name");
		}
		
		[Test]
		public void ExecuteSelect ()
		{
			SqlPoker sql = new SqlPoker();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.OldValuesParameterFormatString = "origin_{0}";
		
			view.SelectParameters.Add (new Parameter ("ProductID", TypeCode.Int32, "10"));
			view.Selecting += new SqlDataSourceSelectingEventHandler (view_Selecting);
			view.Select (new DataSourceSelectArguments ());
			Assert.IsNotNull (CustomEventParameterCollection, "Select event not fired");
			Assert.AreEqual (1, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@ProductID", CustomEventParameterCollection[0].ParameterName, "Parameter name");
			Assert.AreEqual (10, CustomEventParameterCollection[0].Value, "Parameter value");
		}

		[Test]
		public void ExecuteSelect2 () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			sql.DataSourceMode = SqlDataSourceMode.DataReader;
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.OldValuesParameterFormatString = "origin_{0}";

			view.SelectParameters.Add (new Parameter ("ProductID", TypeCode.Int32, "10"));
			view.Selecting += new SqlDataSourceSelectingEventHandler (view_Selecting);
			view.Select (new DataSourceSelectArguments ());
			Assert.IsNotNull (CustomEventParameterCollection, "Select event not fired");
			Assert.AreEqual (1, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@ProductID", CustomEventParameterCollection [0].ParameterName, "Parameter name");
			Assert.AreEqual (10, CustomEventParameterCollection [0].Value, "Parameter value");
		}

		[Test]
		public void ExecuteUpdate ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE Table1 SET UserName = @UserName WHERE UserId = @UserId";
			view.OldValuesParameterFormatString = "origin_{0}";
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "TestUser"));
			view.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "1"));
			view.Update (null, null, null);
			Assert.IsNotNull (CustomEventParameterCollection, "Update event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual ("TestUser", CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@UserId", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual (1, CustomEventParameterCollection[1].Value, "Parameter value#2");
		}

		[Test]
		public void ExecuteInsert ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.InsertCommandType = SqlDataSourceCommandType.Text;
			view.InsertCommand = "INSERT INTO Table1 (UserId, UserName) VALUES ({0},{1})";
			view.InsertParameters.Add (new Parameter ("UserId", TypeCode.Int32, "15"));
			view.InsertParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.Inserting += new SqlDataSourceCommandEventHandler (view_Inserting);
			view.Insert (null);
			Assert.IsNotNull (CustomEventParameterCollection, "Insert event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserId", CustomEventParameterCollection[0].ParameterName, "Parameter name#2");
			Assert.AreEqual (15, CustomEventParameterCollection[0].Value, "Parameter value#2");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[1].ParameterName, "Parameter name#1");
			Assert.AreEqual ("newuser", CustomEventParameterCollection[1].Value, "Parameter value#1");
		}

		[Test]
		public void ExecuteInsertWithCollection ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.InsertCommandType = SqlDataSourceCommandType.Text;
			view.InsertCommand = "INSERT INTO products (UserId, UserName) VALUES ({0},{1})";
			view.InsertParameters.Add (new Parameter ("UserId", TypeCode.Int32, "15"));
			view.InsertParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.Inserting += new SqlDataSourceCommandEventHandler (view_Inserting);
			Hashtable value = new Hashtable ();
			value.Add ("Description", "TestDescription");
			view.Insert (value);
			Assert.IsNotNull (CustomEventParameterCollection, "Insert event not fired");
			Assert.AreEqual (3, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserId", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual (15, CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual ("newuser", CustomEventParameterCollection[1].Value, "Parameter value#2");
			Assert.AreEqual ("@Description", CustomEventParameterCollection[2].ParameterName, "Parameter name#3");
			Assert.AreEqual ("TestDescription", CustomEventParameterCollection[2].Value, "Parameter value#3");
		}

		[Test]
		public void ExecuteDelete ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductId", TypeCode.Int32, "15"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			view.Delete (null, null);
			Assert.IsNotNull (CustomEventParameterCollection, "Delete event not fired");
			Assert.AreEqual (1, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@ProductId", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual (15, CustomEventParameterCollection[0].Value, "Parameter value#1");
		}

		[Test]
		public void ExecuteDeleteWithOldValues ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductID", TypeCode.Int32, "15"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			Hashtable oldvalue = new Hashtable ();
			oldvalue.Add ("ProductID", 10);
			view.Delete (null,oldvalue );
			Assert.IsNotNull (CustomEventParameterCollection, "Delete event not fired");
			Assert.AreEqual (1, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@origin_ProductID", CustomEventParameterCollection[0].ParameterName, "Parameter name#2");
			Assert.AreEqual (10, CustomEventParameterCollection[0].Value, "Parameter value#2");
		}

		[Test]
		public void ExecuteDeleteWithMergedOldValues ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductId", TypeCode.Int32, "15"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			Hashtable oldvalue = new Hashtable ();
			oldvalue.Add ("Desc", "Description");
			view.Delete (null, oldvalue);
			Assert.IsNotNull (CustomEventParameterCollection, "Delete event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@ProductId", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual (15, CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@origin_Desc", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual ("Description", CustomEventParameterCollection[1].Value, "Parameter value#2");
		}

		[Test]
		public void ExecuteDeleteWithMergedValues ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductId", TypeCode.Int32, "15"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			Hashtable value = new Hashtable ();
			value.Add ("Desc", "Description");
			view.Delete (value, null);
			Assert.IsNotNull (CustomEventParameterCollection, "Delete event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@ProductId", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual (15, CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@origin_Desc", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual ("Description", CustomEventParameterCollection[1].Value, "Parameter value#2");
		}

		[Test]
		public void ExecuteDelete_KeysAndOldValues_OverwriteChanges () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			IDictionary keys;
			IDictionary values;
			IDictionary oldValues;
			InitializeView (view, out keys, out values, out oldValues);

			view.ConflictDetection = ConflictOptions.OverwriteChanges;

			view.Delete (keys, oldValues);

			Assert.IsNotNull (CustomEventParameterCollection, "KeysAndOldValues_OverwriteChanges");
			Assert.AreEqual ("String:@origin_ProductID=k_10", PassedParameters, "KeysAndOldValues_OverwriteChanges Values");
		}

		[Test]
		public void ExecuteDelete_KeysAndOldValues_CompareAllValues () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			IDictionary keys;
			IDictionary values;
			IDictionary oldValues;
			InitializeView (view, out keys, out values, out oldValues);

			view.ConflictDetection = ConflictOptions.CompareAllValues;

			view.Delete (keys, oldValues);

			Assert.IsNotNull (CustomEventParameterCollection, "KeysAndOldValues_CompareAllValues");
			Assert.AreEqual ("String:@origin_ProductID=ov_10, String:@origin_Description=ov_Beautifull, String:@origin_Name=ov_ColorTV", PassedParameters, "KeysAndOldValues_CompareAllValues Values");
		}

		[Test]
		public void ExecuteDelete_KeysAndOldValues_CompareAllValues2 () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			IDictionary keys;
			IDictionary values;
			IDictionary oldValues;
			InitializeView (view, out keys, out values, out oldValues);
			view.DeleteParameters.Add ("origin_ProductID", "po_10");

			view.ConflictDetection = ConflictOptions.CompareAllValues;

			view.Delete (keys, oldValues);

			Assert.IsNotNull (CustomEventParameterCollection, "ExecuteDelete_KeysAndOldValues_CompareAllValues2");
			string [] expectedParams = new string []
						{ 
							"String:@origin_ProductID=ov_10",
							"String:@origin_Name=ov_ColorTV",
							"String:@origin_Description=ov_Beautifull"
						};
			string [] actualValues = PassedParameters.Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 Params count");
			ValidatePassedParams (expectedParams, actualValues, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 expecte '{0}'");
		}

		private static void ValidatePassedParams (string [] expectedParams, string [] actualValues, string errorMessageFormat) 
		{
			foreach (string eps in expectedParams) {
				bool found = false;
				foreach (string aps in actualValues) {
					if (eps == aps) {
						found = true;
						break;
					}
				}
				Assert.IsTrue (found, String.Format (errorMessageFormat, eps));
			}
		}

		private void InitializeView (CustomSqlDataSourceView view, out IDictionary keys, out IDictionary values, out IDictionary oldValues) 
		{
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.SelectParameters.Add (new Parameter ("ProductID", TypeCode.String, "p_10"));
			view.Selecting += new SqlDataSourceSelectingEventHandler (view_Selecting);

			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductID", TypeCode.String, "p_10"));
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);

			view.InsertCommandType = SqlDataSourceCommandType.Text;
			view.InsertCommand = "INSERT INTO products (ProductID, Name, Description) VALUES (@ProductID, @Name, @Description)";
			view.InsertParameters.Add (new Parameter ("ProductID", TypeCode.String, "p_15"));
			view.InsertParameters.Add (new Parameter ("Name", TypeCode.String, "p_NewProduct"));
			view.InsertParameters.Add (new Parameter ("Description", TypeCode.String, "p_Description"));
			view.Inserting += new SqlDataSourceCommandEventHandler (view_Inserting);

			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE products SET Name = @Name, Description = @Description WHERE ProductID = @ProductID";
			view.UpdateParameters.Add (new Parameter ("ProductID", TypeCode.String, "p_15"));
			view.UpdateParameters.Add (new Parameter ("Name", TypeCode.String, "p_UpdatedProduct"));
			view.UpdateParameters.Add (new Parameter ("Description", TypeCode.String, "p_UpdatedDescription"));
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);

			view.OldValuesParameterFormatString = "origin_{0}";

			keys = new Hashtable ();
			values = new Hashtable ();
			oldValues = new Hashtable ();

			keys.Add ("ProductID", "k_10");

			values.Add ("ProductID", "n_10");
			values.Add ("Name", "n_ColorTV");
			values.Add ("Description", "n_Beautifull");

			oldValues.Add ("ProductID", "ov_10");
			oldValues.Add ("Name", "ov_ColorTV");
			oldValues.Add ("Description", "ov_Beautifull");			
		}

		[Test]
		public void ExecuteUpdateWithOldValues ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE Table1 SET UserName = @UserName WHERE UserId = @UserId";
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "TestUser"));
			view.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "1"));
			Hashtable oldvalue = new Hashtable ();
			oldvalue.Add ("UserId", 2);
			view.Update (null, null, oldvalue);
			Assert.IsNotNull (CustomEventParameterCollection, "Update event not fired");
			Assert.AreEqual (3, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual ("TestUser", CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@UserId", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual (1, CustomEventParameterCollection[1].Value, "Parameter value#2");
			Assert.AreEqual ("@origin_UserId", CustomEventParameterCollection[2].ParameterName, "Parameter name#3");
			Assert.AreEqual (2, CustomEventParameterCollection[2].Value, "Parameter value#3");
		}

		[Test]
		public void ExecuteUpdateWithOverwriteParameters ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE Table1 SET UserName = @UserName WHERE UserId = @UserId";
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.OverwriteChanges;
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "TestUser"));
			view.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "1"));
			Hashtable value = new Hashtable ();
			value.Add ("UserId", 2);
			view.Update (value, null, null);
			Assert.IsNotNull (CustomEventParameterCollection, "Update event not fired");
			Assert.AreEqual (2, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual ("TestUser", CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@origin_UserId", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual (2, CustomEventParameterCollection[1].Value, "Parameter value#2");
		}

		[Test]
		public void ExecuteUpdateWithMargeParameters ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE Table1 SET UserName = @UserName WHERE UserId = @UserId";
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.OverwriteChanges;
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "TestUser"));
			view.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "1"));
			Hashtable value = new Hashtable ();
			value.Add ("UserLName", "TestLName");
			view.Update (null, value, null);
			Assert.IsNotNull (CustomEventParameterCollection, "Update event not fired");
			Assert.AreEqual (3, CustomEventParameterCollection.Count, "Parameter count");
			Assert.AreEqual ("@UserName", CustomEventParameterCollection[0].ParameterName, "Parameter name#1");
			Assert.AreEqual ("TestUser", CustomEventParameterCollection[0].Value, "Parameter value#1");
			Assert.AreEqual ("@UserId", CustomEventParameterCollection[1].ParameterName, "Parameter name#2");
			Assert.AreEqual (1, CustomEventParameterCollection[1].Value, "Parameter value#2");
			Assert.AreEqual ("@UserLName", CustomEventParameterCollection[2].ParameterName, "Parameter name#3");
			Assert.AreEqual ("TestLName", CustomEventParameterCollection[2].Value, "Parameter value#3");
		}

		[Test]
		public void ExecuteUpdate_KeysValuesAndOldValues_OverwriteChanges () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			IDictionary keys;
			IDictionary values;
			IDictionary oldValues;
			InitializeView (view, out keys, out values, out oldValues);

			view.ConflictDetection = ConflictOptions.OverwriteChanges;

			view.Update (keys, values, oldValues);

			Assert.IsNotNull (CustomEventParameterCollection, "ExecuteUpdate_KeysValuesAndOldValues_OverwriteChanges");
			string [] expectedParams = new string []
						{ 
							"String:@ProductID=n_10", 
							"String:@Name=n_ColorTV", 
							"String:@Description=n_Beautifull",
							"String:@origin_ProductID=k_10" 
						};
			string [] actualValues = PassedParameters.Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 Params count");
			ValidatePassedParams (expectedParams, actualValues, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 expecte '{0}'");
		}

		[Test]
		public void ExecuteUpdate_KeysValuesAndOldValues_CompareAllValues () 
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			IDictionary keys;
			IDictionary values;
			IDictionary oldValues;
			InitializeView (view, out keys, out values, out oldValues);

			view.ConflictDetection = ConflictOptions.CompareAllValues;

			view.Update (keys, values, oldValues);

			Assert.IsNotNull (CustomEventParameterCollection, "ExecuteUpdate_KeysValuesAndOldValues_CompareAllValues");
			string [] expectedParams = new string []
						{ 
							"String:@ProductID=n_10", 
							"String:@Name=n_ColorTV", 
							"String:@Description=n_Beautifull",
							"String:@origin_ProductID=ov_10", 
							"String:@origin_Name=ov_ColorTV", 
							"String:@origin_Description=ov_Beautifull",
						};
			string [] actualValues = PassedParameters.Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 Params count");
			ValidatePassedParams (expectedParams, actualValues, "ExecuteDelete_KeysAndOldValues_CompareAllValues2 expecte '{0}'");
		}

		void view_Updating (object sender, SqlDataSourceCommandEventArgs e)
		{
			SqlDataSourceTest.CustomEventParameterCollection = (SqlParameterCollection) e.Command.Parameters;
			SqlDataSourceTest.PassedParameters = FormatParameters (SqlDataSourceTest.CustomEventParameterCollection);
			e.Cancel = true;
		}

		void view_Selecting (object sender, SqlDataSourceSelectingEventArgs e)
		{
			SqlDataSourceTest.CustomEventParameterCollection = (SqlParameterCollection) e.Command.Parameters;
			SqlDataSourceTest.PassedParameters = FormatParameters (SqlDataSourceTest.CustomEventParameterCollection);
			e.Cancel = true;
		}

		void view_Inserting (object sender, SqlDataSourceCommandEventArgs e)
		{
			SqlDataSourceTest.CustomEventParameterCollection = (SqlParameterCollection) e.Command.Parameters;
			SqlDataSourceTest.PassedParameters = FormatParameters (SqlDataSourceTest.CustomEventParameterCollection);
			e.Cancel = true;
		}

		void view_Deleting (object sender, SqlDataSourceCommandEventArgs e)
		{
			SqlDataSourceTest.CustomEventParameterCollection = (SqlParameterCollection) e.Command.Parameters;
			SqlDataSourceTest.PassedParameters = FormatParameters (SqlDataSourceTest.CustomEventParameterCollection);
			e.Cancel = true;
		}

		private string FormatParameters (SqlParameterCollection sqlParameterCollection) 
		{
			StringBuilder sb = new StringBuilder ();
			foreach (SqlParameter p in sqlParameterCollection) {
				if (sb.Length > 0) {
					sb.Append (", ");
				}
				sb.AppendFormat ("{0}:{1}={2}", p.DbType, p.ParameterName, p.Value);
			}
			return sb.ToString ();
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
		public void SqlDataSource_DataSourceViewChanged ()
		{
			SqlPoker sql = new SqlPoker ();
			((IDataSource) sql).DataSourceChanged += new EventHandler (SqlDataSourceTest_DataSourceChanged);

			sql.DoRaiseDataSourceChangedEvent ();
			eventAssert.IsTrue ("SqlDataSourceView"); // Assert include counter the first is zero
			sql.CacheKeyDependency = "hi";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.CancelSelectOnNullParameter = false;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.DeleteCommandType = SqlDataSourceCommandType.StoredProcedure;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.InsertCommandType = SqlDataSourceCommandType.StoredProcedure;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.UpdateCommandType = SqlDataSourceCommandType.StoredProcedure;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.OldValuesParameterFormatString = "{1}";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.SqlCacheDependency = "hi";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.SortParameterName = "hi";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.CacheDuration = 1;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.EnableCaching = true;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.DataSourceMode = SqlDataSourceMode.DataReader;
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.DeleteCommand = "DELETE foo";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.InsertCommand = "INSERT foo";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.SelectCommand = "SELECT foo";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.UpdateCommand = "UPDATE foo";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.FilterExpression = "hi";
			eventAssert.IsFalse ("SqlDataSourceView");
		}

		void SqlDataSourceTest_DataSourceChanged (object sender, EventArgs e)
		{
			eventAssert.eventChecker = true;
		}

		//exceptions 
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ExecuteUpdateException ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.Update (null, null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ExecuteDeleteException ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.Delete (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ExecuteInsertException ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.Insert (null);
		}

		[Test]  //ConflictOptions.CompareAllValues must include old value collection
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExecuteUpdateWithOldValuesException ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommand = "UPDATE Table1 SET UserName = @UserName WHERE UserId = @UserId";
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "TestUser"));
			view.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "1"));
			view.Update (null, null, null);
		}

		[Test] //ConflictOptions.CompareAllValues must include old value collection
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExecuteDeleteWithOldValuesException ()
		{
			SqlPoker sql = new SqlPoker ();
			sql.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			sql.ProviderName = "System.Data.SqlClient";
			CustomSqlDataSourceView view = new CustomSqlDataSourceView (sql, "TestView", null);
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.SelectCommand = "SELECT * FROM products WHERE ProductID = @ProductID;";
			view.DeleteCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommand = "DELETE * FROM products WHERE ProductID = @ProductID;";
			view.DeleteParameters.Add (new Parameter ("ProductId", TypeCode.Int32, "15"));
			view.OldValuesParameterFormatString = "origin_{0}";
			view.ConflictDetection = ConflictOptions.CompareAllValues;
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			Hashtable oldvalue = new Hashtable ();
			oldvalue.Add ("ProductID", 10);
			view.Delete (null, null);
		}

		[Test]
		public void SqlDataSource_OnInit_Bug572781 ()
		{
			WebTest t = new WebTest ("SqlDataSource_OnInit_Bug572781.aspx");
			string origHtmlFirst = @" Init: button1. Init: sqlDataSource1<input type=""submit"" name=""button1"" value=""Click me!"" id=""button1"" />";
			string origHtmlSecond = @" Init: button1. Init: sqlDataSource1<input type=""submit"" name=""button1"" value=""You clicked me"" id=""button1"" />";
			string html;
			string renderedHtml;

			html = t.Run ();
			renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlFirst, renderedHtml, "#A1");

			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("button1");
			fr.Controls ["button1"].Value = "Click me!";
			t.Request = fr;
			
			html = t.Run ();
			renderedHtml = HtmlDiff.GetControlFromPageHtml (html);
			HtmlDiff.AssertAreEqual (origHtmlSecond, renderedHtml, "#A1");
		}
	}
}

#endif
