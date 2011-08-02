//
// Tests for System.Web.UI.WebControls.SqlDataSourceView
//
// Author:
//	Chris Toshok (toshok@novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data.SqlClient;
using System.Text;

namespace MonoTests.System.Web.UI.WebControls
{
	class SqlViewPoker : SqlDataSourceView {
		public SqlViewPoker (SqlDataSource ds, string name, HttpContext context)
			: base (ds, name, context)
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

		public void DoOnDataSourceViewChanged ()
		{
			base.OnDataSourceViewChanged (new EventArgs());
		}
	}

	[TestFixture]
	public class SqlDataSourceViewTest 
	{
		[SetUp]
		public void Setup () 
		{
			eventsCalled = null;
		}

		[Test]
		public void Defaults ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			Assert.IsTrue (sql.CancelSelectOnNullParameter, "A1");
			Assert.IsFalse (sql.CanDelete,"A2");
			Assert.IsFalse (sql.CanInsert,"A3");
			Assert.IsFalse (sql.CanPage,"A4");
			Assert.IsFalse (sql.CanRetrieveTotalRowCount,"A5");
			Assert.IsTrue (sql.CanSort,"A6");
			Assert.IsFalse (sql.CanUpdate,"A7");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "A8");
			Assert.AreEqual ("", sql.DeleteCommand, "A9");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "A10");
			Assert.IsNotNull (sql.DeleteParameters, "A11");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "A12");
			Assert.AreEqual ("", sql.FilterExpression, "A13");
			Assert.IsNotNull (sql.FilterParameters, "A14");
			Assert.AreEqual (0, sql.FilterParameters.Count, "A15");
			Assert.AreEqual ("", sql.InsertCommand, "A16");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "A17");
			Assert.IsNotNull (sql.InsertParameters, "A18");
			Assert.AreEqual (0, sql.InsertParameters.Count, "A19");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A20");
			Assert.AreEqual ("", sql.SelectCommand, "A21");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "A22");
			Assert.IsNotNull (sql.SelectParameters, "A23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "A24");
			Assert.AreEqual ("", sql.SortParameterName, "A25");
			Assert.AreEqual ("", sql.UpdateCommand, "A26");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "A27");
			Assert.IsNotNull (sql.UpdateParameters, "A28");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "A29");
		}

		[Test]
		public void ViewStateSupport () 
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker view = new SqlViewPoker (ds, "DefaultView", null);

			ds.ID = "SqlDataSource1";
			ds.SelectCommand = "Select";

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
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			/* XXX test parameters */

			sql.CancelSelectOnNullParameter = false;
			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			sql.DeleteCommandType = SqlDataSourceCommandType.Text;
			sql.DeleteCommand = "delete command";
			sql.FilterExpression = "filter expression";
			sql.InsertCommand = "insert command";
			sql.InsertCommandType = SqlDataSourceCommandType.Text;
			sql.OldValuesParameterFormatString = "{1}";
			sql.SelectCommand = "select command";
			sql.SelectCommandType = SqlDataSourceCommandType.Text;
			sql.SortParameterName = "sort parameter";
			sql.UpdateCommand = "update command";
			sql.UpdateCommandType = SqlDataSourceCommandType.Text;

			Assert.IsFalse (sql.CancelSelectOnNullParameter, "A1");
			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "A2");
			Assert.AreEqual ("delete command", sql.DeleteCommand, "A3");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "A4");
			Assert.AreEqual ("filter expression", sql.FilterExpression, "A5");
			Assert.AreEqual ("insert command", sql.InsertCommand, "A6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "A7");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "A8");
			Assert.AreEqual ("select command", sql.SelectCommand, "A9");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "A10");
			Assert.AreEqual ("sort parameter", sql.SortParameterName, "A11");
			Assert.AreEqual ("update command", sql.UpdateCommand, "A12");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "A13");

			object state = sql.SaveToViewState ();
			Assert.IsNull (state, "ViewState is null");

			sql = new SqlViewPoker (ds, "DefaultView", null);
			sql.LoadFromViewState (state);

			Assert.IsTrue (sql.CancelSelectOnNullParameter, "B1");
			Assert.IsFalse (sql.CanDelete, "B2");
			Assert.IsFalse (sql.CanInsert, "B3");
			Assert.IsFalse (sql.CanPage, "B4");
			Assert.IsFalse (sql.CanRetrieveTotalRowCount, "B5");
			Assert.IsTrue (sql.CanSort, "B6");
			Assert.IsFalse (sql.CanUpdate, "B7");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "B8");
			Assert.AreEqual ("", sql.DeleteCommand, "B9");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "B10");
			Assert.IsNotNull (sql.DeleteParameters, "B11");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "B12");
			Assert.AreEqual ("", sql.FilterExpression, "B13");
			Assert.IsNotNull (sql.FilterParameters, "B14");
			Assert.AreEqual (0, sql.FilterParameters.Count, "B15");
			Assert.AreEqual ("", sql.InsertCommand, "B16");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "B17");
			Assert.IsNotNull (sql.InsertParameters, "B18");
			Assert.AreEqual (0, sql.InsertParameters.Count, "B19");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "B20");
			Assert.AreEqual ("", sql.SelectCommand, "B21");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "B22");
			Assert.IsNotNull (sql.SelectParameters, "B23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "B24");
			Assert.AreEqual ("", sql.SortParameterName, "B25");
			Assert.AreEqual ("", sql.UpdateCommand, "B26");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "B27");
			Assert.IsNotNull (sql.UpdateParameters, "B28");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "B29");
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
		public void SqlDataSourceView_DataSourceViewChanged ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);
			sql.DataSourceViewChanged += new EventHandler (sql_DataSourceViewChanged);
			sql.DoOnDataSourceViewChanged ();
			eventAssert.IsTrue ("SqlDataSourceView"); // Assert include counter the first is zero
			/* XXX test parameters */

			sql.CancelSelectOnNullParameter = false;
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.ConflictDetection = ConflictOptions.CompareAllValues;
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.DeleteCommandType = SqlDataSourceCommandType.Text;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.DeleteCommand = "delete command";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.FilterExpression = "filter expression";
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.InsertCommand = "insert command";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.InsertCommandType = SqlDataSourceCommandType.Text;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.OldValuesParameterFormatString = "{1}";
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.SelectCommand = "select command";
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.SelectCommandType = SqlDataSourceCommandType.Text;
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.SortParameterName = "sort parameter";
			eventAssert.IsTrue ("SqlDataSourceView");
			sql.UpdateCommand = "update command";
			eventAssert.IsFalse ("SqlDataSourceView");
			sql.UpdateCommandType = SqlDataSourceCommandType.Text;
			eventAssert.IsFalse ("SqlDataSourceView");
		}

		void sql_DataSourceViewChanged (object sender, EventArgs e)
		{
			eventAssert.eventChecker = true;
		}

		[Test]
		public void CanDelete ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			sql.DeleteCommand = "DELETE from foo";
			Assert.IsTrue (sql.CanDelete, "A1");

			sql.DeleteCommand = "";
			Assert.IsFalse (sql.CanDelete, "A2");

			sql.DeleteCommand = null;
			Assert.IsFalse (sql.CanDelete, "A3");
		}

		[Test]
		public void CanInsert ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			sql.InsertCommand = "INSERT into foo";
			Assert.IsTrue (sql.CanInsert, "A1");

			sql.InsertCommand = "";
			Assert.IsFalse (sql.CanInsert, "A2");

			sql.InsertCommand = null;
			Assert.IsFalse (sql.CanInsert, "A3");
		}

		[Test]
		public void CanUpdate ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			sql.UpdateCommand = "UPDATE foo";
			Assert.IsTrue (sql.CanUpdate, "A1");

			sql.UpdateCommand = "";
			Assert.IsFalse (sql.CanUpdate, "A2");

			sql.UpdateCommand = null;
			Assert.IsFalse (sql.CanUpdate, "A3");
		}

		[Test]
		public void CanSort ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			sql.SortParameterName = "foo";
			Assert.IsTrue (sql.CanSort, "A1");

			sql.SortParameterName = null;
			Assert.IsTrue (sql.CanSort, "A2");

			sql.SortParameterName = "";
			Assert.IsTrue (sql.CanSort, "A3");

			sql.SortParameterName = "foo";

			ds.DataSourceMode = SqlDataSourceMode.DataReader;
			Assert.IsTrue (sql.CanSort, "A4");

			ds.DataSourceMode = SqlDataSourceMode.DataSet;
			Assert.IsTrue (sql.CanSort, "A5");

			sql.SortParameterName = "";

			ds.DataSourceMode = SqlDataSourceMode.DataReader;
			Assert.IsFalse (sql.CanSort, "A6");

			ds.DataSourceMode = SqlDataSourceMode.DataSet;
			Assert.IsTrue (sql.CanSort, "A7");
		}

		[Test]
		public void OldValuesParameterFormatString ()
		{
			SqlDataSource ds = new SqlDataSource ();
			
			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A1");

			ds.OldValuesParameterFormatString = "hi {0}";

			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A2");

			ds.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A3");

			ds.OldValuesParameterFormatString = "{0}";
			sql.OldValuesParameterFormatString = "hi {0}";

			Assert.AreEqual ("{0}", ds.OldValuesParameterFormatString, "A4");
		}

		[Test]
		public void CancelSelectOnNullParameter ()
		{
			SqlDataSource ds = new SqlDataSource ();

			ds.CancelSelectOnNullParameter = false;

			SqlViewPoker sql = new SqlViewPoker (ds, "DefaultView", null);

			Assert.IsTrue (sql.CancelSelectOnNullParameter, "A1");

			ds.CancelSelectOnNullParameter = true;
			sql.CancelSelectOnNullParameter = false;

			Assert.IsTrue (ds.CancelSelectOnNullParameter, "A2");

			sql.CancelSelectOnNullParameter = false;
			ds.CancelSelectOnNullParameter = true;
			Assert.IsFalse (sql.CancelSelectOnNullParameter, "A3");
		}

		public class AlwaysChangingParameter : Parameter
		{
			int evaluateCount;

			public AlwaysChangingParameter (string name, TypeCode type, string defaultValue)
				: base (name, type, defaultValue) {
				evaluateCount = 0;
			}

#if NET_4_0
			internal
#endif
			protected override object Evaluate (HttpContext context, Control control) {
				evaluateCount++;
				return String.Format ("{0}{1}", DefaultValue, evaluateCount);
			}
		}

		enum InitViewType
		{
			MatchParamsToValues,
			MatchParamsToOldValues,
			DontMatchParams,
		}

		[Test]
		public void SelectCommand_DataSourceViewChanged2 ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker view = new SqlViewPoker (ds, "DefaultView", null);
			view.DataSourceViewChanged += new EventHandler (view_DataSourceViewChanged);

			Assert.AreEqual ("", view.SelectCommand);
			view.SelectCommand = null;
			Assert.AreEqual (1, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0]);
			Assert.AreEqual ("", view.SelectCommand);

			view.SelectCommand = null;
			Assert.AreEqual (2, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [1]);
			Assert.AreEqual ("", view.SelectCommand);

			view.SelectCommand = "";
			Assert.AreEqual (2, eventsCalled.Count);
		}

		[Test]
		public void SelectCommand_DataSourceViewChanged1 ()
		{
			SqlDataSource ds = new SqlDataSource ();
			SqlViewPoker view = new SqlViewPoker (ds, "DefaultView", null);
			view.DataSourceViewChanged+=new EventHandler(view_DataSourceViewChanged);

			view.SelectCommand = "select 1";
			Assert.AreEqual (1, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled[0]);
			
			view.SelectCommand = "select 2";
			Assert.AreEqual (2, eventsCalled.Count);
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [1]);

			view.SelectCommand = "select 2";
			Assert.AreEqual (2, eventsCalled.Count);

			view.SelectCommandType = SqlDataSourceCommandType.StoredProcedure;
			Assert.AreEqual (2, eventsCalled.Count);

			view.SelectCommandType = SqlDataSourceCommandType.Text;
			Assert.AreEqual (2, eventsCalled.Count);
		}

		private static SqlViewPoker InitializeView (InitViewType initType, ConflictOptions conflictDetection, out Hashtable keys, out Hashtable old_value, out Hashtable new_value) 
		{
			SqlDataSource ds = new SqlDataSource ();
			ds.ConnectionString = "Data Source=fake\\SQLEXPRESS;Initial Catalog=Northwind;User ID=sa";
			ds.ProviderName = "System.Data.SqlClient";
			SqlViewPoker view = new SqlViewPoker (ds, "DefaultView", null);

			view.ConflictDetection = conflictDetection;
			view.OldValuesParameterFormatString = "oldvalue_{0}";
			view.SelectCommandType = SqlDataSourceCommandType.Text;
			view.InsertCommandType = SqlDataSourceCommandType.Text;
			view.UpdateCommandType = SqlDataSourceCommandType.Text;
			view.DeleteCommandType = SqlDataSourceCommandType.Text;

			view.SelectCommand = "SELECT * FROM Customers WHERE ID = @ID";
			view.InsertCommand = "INSERT INTO Customers (ID) VALUES (@ID)";
			view.UpdateCommand = "UPDATE Customers SET ID = @ID WHERE ID = @oldvalue_ID";
			view.DeleteCommand = "DELETE * FROM Customers WHERE ID = @ID";

			Parameter selectParameter = null;
			Parameter insertParameter = null;
			Parameter updateParameter = null;
			Parameter deleteParameter = null;

			selectParameter = new AlwaysChangingParameter ("ID", TypeCode.String, "p_ValueSelect");
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

			view.Selecting += new SqlDataSourceSelectingEventHandler (view_Selecting);
			view.Inserting += new SqlDataSourceCommandEventHandler (view_Inserting);
			view.Updating += new SqlDataSourceCommandEventHandler (view_Updating);
			view.Deleting += new SqlDataSourceCommandEventHandler (view_Deleting);
			return view;
		}

		static void view_Selecting (object source, SqlDataSourceSelectingEventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add (e.Arguments.ToString ());
			eventsCalled.Add ("view_Selecting");
			eventsCalled.Add (FormatParameters ((SqlParameterCollection)e.Command.Parameters));
			e.Cancel = true;
		}

		static void view_Inserting (object source, SqlDataSourceCommandEventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("view_Inserting");
			eventsCalled.Add (FormatParameters ((SqlParameterCollection) e.Command.Parameters));
			e.Cancel = true;
		}

		static void view_Updating (object source, SqlDataSourceCommandEventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("view_Updating");
			eventsCalled.Add (FormatParameters ((SqlParameterCollection) e.Command.Parameters));
			e.Cancel = true;
		}

		static void view_Deleting (object source, SqlDataSourceCommandEventArgs e) 
		{
			if (eventsCalled == null) {
				eventsCalled = new ArrayList ();
			}
			eventsCalled.Add ("view_Deleting");
			eventsCalled.Add (FormatParameters ((SqlParameterCollection) e.Command.Parameters));
			e.Cancel = true;
		}

		private static string FormatParameters (SqlParameterCollection sqlParameterCollection) 
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
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Select (DataSourceSelectArguments.Empty);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (5, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("view_DataSourceViewChanged", eventsCalled [0], "view_DataSourceViewChanged");
			Assert.AreEqual ("SelectParameters_ParametersChanged", eventsCalled [1], "SelectParameters_ParametersChanged");
			Assert.AreEqual ("System.Web.UI.DataSourceSelectArguments", eventsCalled [2], "DataSourceSelectArguments");
			Assert.AreEqual ("view_Selecting", eventsCalled [3], "view_Selecting");
			string [] expectedParams = new string []
						{ 
							"String:@ID=p_ValueSelect1"
						};
			string [] actualValues = ((string)eventsCalled [4]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_Select Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_Select expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchInsert () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("view_Inserting", eventsCalled [1], "view_Inserting");
			string [] expectedParams = new string []
						{ 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchInsert Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchInsert expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchInsertAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("view_Inserting", eventsCalled [1], "view_Inserting");
			string [] expectedParams = new string []
						{ 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchInsert Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchInsert expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldInsert () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("view_Inserting", eventsCalled [1], "view_Inserting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=p_OldValueInsert1", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchOldInsert Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchOldInsert expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldInsertAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Insert (new_values);
			
			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("view_Inserting", eventsCalled [1], "view_Inserting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=p_OldValueInsert1", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchOldInsert Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchOldInsert expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchInsert () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.DontMatchParams, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Insert (new_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("InsertParameters_ParametersChanged", eventsCalled [0], "InsertParameters_ParametersChanged");
			Assert.AreEqual ("view_Inserting", eventsCalled [1], "view_Inserting");
			string [] expectedParams = new string []
						{ 
							"String:@OtherValue=p_OtherValueInsert1", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_DontMatchInsert Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_DontMatchInsert expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchUpdate () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("view_Updating", eventsCalled [1], "view_Updating");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchUpdate Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchUpdate expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchUpdateAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("view_Updating", eventsCalled [1], "view_Updating");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=ov_1001", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchUpdate Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchUpdate expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldUpdate () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("view_Updating", eventsCalled [1], "view_Updating");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchUpdate Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchUpdate expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldUpdateAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("view_Updating", eventsCalled [1], "view_Updating");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=ov_1001", 
							"String:@ID=n_1001"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchUpdate Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchUpdate expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchUpdate () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.DontMatchParams, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Update (keys, new_values, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("UpdateParameters_ParametersChanged", eventsCalled [0], "UpdateParameters_ParametersChanged");
			Assert.AreEqual ("view_Updating", eventsCalled [1], "view_Updating");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
							"String:@ID=n_1001",
							"String:@OtherValue=p_OtherValueUpdate1"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_DontMatchUpdate Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_DontMatchUpdate expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchDelete () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("view_Deleting", eventsCalled [1], "view_Deleting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchDelete Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchDelete expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchDeleteAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("view_Deleting", eventsCalled [1], "view_Deleting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=ov_1001", 
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchDelete Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchDelete expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldDelete () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("view_Deleting", eventsCalled [1], "view_Deleting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchOldDelete Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchOldDelete expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_MatchOldDeleteAllValues () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.MatchParamsToOldValues, ConflictOptions.CompareAllValues, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("view_Deleting", eventsCalled [1], "view_Deleting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=ov_1001", 
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchOldDelete Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchOldDelete expecte '{0}'");
		}

		[Test]
		public void ParametersAndViewChangedEvent_DontMatchDelete () 
		{
			Hashtable keys = null;
			Hashtable old_values = null;
			Hashtable new_values = null;
			SqlViewPoker view = InitializeView (InitViewType.DontMatchParams, ConflictOptions.OverwriteChanges, out keys, out old_values, out new_values);

			view.Delete (keys, old_values);

			Assert.IsNotNull (eventsCalled, "Events not raized");
			Assert.AreEqual (3, eventsCalled.Count, "Events Count");
			Assert.AreEqual ("DeleteParameters_ParametersChanged", eventsCalled [0], "DeleteParameters_ParametersChanged");
			Assert.AreEqual ("view_Deleting", eventsCalled [1], "view_Deleting");
			string [] expectedParams = new string []
						{ 
							"String:@oldvalue_ID=k_1001", 
							"String:@OtherValue=p_OtherValueDelete1"
						};
			string [] actualValues = ((string) eventsCalled [2]).Split (new string [] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			Assert.AreEqual (expectedParams.Length, actualValues.Length, "ParametersAndViewChangedEvent_MatchOldDelete Params count");
			ValidatePassedParams (expectedParams, actualValues, "ParametersAndViewChangedEvent_MatchOldDelete expecte '{0}'");
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
	}

}

#endif
