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
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	class SqlPoker : SqlDataSource {
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
	}

	[TestFixture]
	public class SqlDataSourceTest {
		[Test]
		public void Defaults ()
		{
			SqlPoker sql = new SqlPoker ();

			Assert.AreEqual ("", sql.CacheKeyDependency, "A1");
			Assert.IsTrue (sql.CancelSelectOnNullParameter, "A2");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "A3");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.DeleteCommandType, "A4");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.InsertCommandType, "A5");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.SelectCommandType, "A6");
			Assert.AreEqual (SqlDataSourceCommandType.StoredProcedure, sql.UpdateCommandType, "A7");
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
			sql.DeleteCommandType = SqlDataSourceCommandType.Text;
			sql.InsertCommandType = SqlDataSourceCommandType.Text;
			sql.SelectCommandType = SqlDataSourceCommandType.Text;
			sql.UpdateCommandType = SqlDataSourceCommandType.Text;
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
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "A4");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "A5");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "A6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "A7");
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

			object state = sql.SaveToViewState();

			sql = new SqlPoker ();
			sql.LoadFromViewState (state);

			Assert.AreEqual ("hi", sql.CacheKeyDependency, "B1");
			Assert.IsFalse  (sql.CancelSelectOnNullParameter, "B2");
			Assert.AreEqual (ConflictOptions.CompareAllValues, sql.ConflictDetection, "B3");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.DeleteCommandType, "B4");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.InsertCommandType, "B5");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.SelectCommandType, "B6");
			Assert.AreEqual (SqlDataSourceCommandType.Text, sql.UpdateCommandType, "B7");
			Assert.AreEqual ("{1}", sql.OldValuesParameterFormatString, "B8");
			Assert.AreEqual ("hi", sql.SqlCacheDependency, "B9");
			Assert.AreEqual ("hi", sql.SortParameterName, "B10");
			Assert.AreEqual (1, sql.CacheDuration, "B11");
			Assert.AreEqual (DataSourceCacheExpiry.Sliding, sql.CacheExpirationPolicy, "B12");
			Assert.IsTrue   (sql.EnableCaching, "B13");
			Assert.AreEqual (SqlDataSourceMode.DataReader, sql.DataSourceMode, "B16");
			Assert.AreEqual ("DELETE foo", sql.DeleteCommand, "B17");
			Assert.AreEqual ("INSERT foo", sql.InsertCommand, "B20");
			Assert.AreEqual ("SELECT foo", sql.SelectCommand, "B22");
			Assert.AreEqual ("UPDATE foo", sql.UpdateCommand, "B24");
			Assert.AreEqual ("hi", sql.FilterExpression, "B26");
		}
	}

}

#endif
