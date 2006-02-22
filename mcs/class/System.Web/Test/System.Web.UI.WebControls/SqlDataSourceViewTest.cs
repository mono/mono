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

using NUnit.Framework;
using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
	}

	[TestFixture]
	public class SqlDataSourceViewTest {
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
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.DeleteCommandType, "A10");
			Assert.IsNotNull (sql.DeleteParameters, "A11");
			Assert.AreEqual (0, sql.DeleteParameters.Count, "A12");
			Assert.AreEqual ("", sql.FilterExpression, "A13");
			Assert.IsNotNull (sql.FilterParameters, "A14");
			Assert.AreEqual (0, sql.FilterParameters.Count, "A15");
			Assert.AreEqual ("", sql.InsertCommand, "A16");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.InsertCommandType, "A17");
			Assert.IsNotNull (sql.InsertParameters, "A18");
			Assert.AreEqual (0, sql.InsertParameters.Count, "A19");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A20");
			Assert.AreEqual ("", sql.SelectCommand, "A21");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.SelectCommandType, "A22");
			Assert.IsNotNull (sql.SelectParameters, "A23");
			Assert.AreEqual (0, sql.SelectParameters.Count, "A24");
			Assert.AreEqual ("", sql.SortParameterName, "A25");
			Assert.AreEqual ("", sql.UpdateCommand, "A26");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.UpdateCommandType, "A27");
			Assert.IsNotNull (sql.UpdateParameters, "A28");
			Assert.AreEqual (0, sql.UpdateParameters.Count, "A29");
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

			object state = sql.SaveToViewState();

			sql = new SqlViewPoker (ds, "DefaultView", null);
			sql.LoadFromViewState (state);

			Assert.IsFalse (sql.CancelSelectOnNullParameter, "B1");
			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "B2");
			Assert.AreEqual ("delete command", sql.DeleteCommand, "B3");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "B4");
			Assert.AreEqual ("filter expression", sql.FilterExpression, "B5");
			Assert.AreEqual ("insert command", sql.InsertCommand, "B6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "B7");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "B8");
			Assert.AreEqual ("select command", sql.SelectCommand, "B9");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "B10");
			Assert.AreEqual ("sort parameter", sql.SortParameterName, "B11");
			Assert.AreEqual ("update command", sql.UpdateCommand, "B12");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "B13");
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
	}

}
