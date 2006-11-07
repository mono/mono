//
// Tests for System.Web.UI.WebControls.AccessDataSource
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
using System.Data.OleDb;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	class AccessPoker : AccessDataSource {
		public AccessPoker ()
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

		public DbProviderFactory GetFactory ()
		{
			return GetDbProviderFactory ();
		}
	}

	[TestFixture]
	public class AccessDataSourceTest {
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ProviderName ()
		{
			AccessPoker sql = new AccessPoker ();
			sql.ProviderName = "foo";
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SqlCacheDependency1 ()
		{
			AccessPoker sql = new AccessPoker ();
			Assert.AreEqual ("", sql.SqlCacheDependency, "A1");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void SqlCacheDependency2 ()
		{
			AccessPoker sql = new AccessPoker ();
			sql.SqlCacheDependency = "hi";
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConnectionString1 ()
		{
			AccessPoker sql = new AccessPoker ();
			sql.ConnectionString = "hi";
		}

		[Test]
		public void ConnectionString2 ()
		{
			AccessPoker sql = new AccessPoker ();

			sql.DataFile = "";
			Assert.AreEqual ("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=", sql.ConnectionString, "A1");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConnectionString3 ()
		{
			AccessPoker sql = new AccessPoker ();

			sql.DataFile = "hi there";
			Assert.AreEqual ("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=", sql.ConnectionString, "A1");
		}

#if notyet
		// XXX enable this test once mono gets System.Data.OleDb.OleDbFactory
		//
		[Test]
		public void ProviderFactory ()
		{
			AccessPoker sql = new AccessPoker ();

			Assert.AreEqual (typeof (OleDbFactory), sql.GetFactory ().GetType());
		}
#endif

		[Test]
        [Category ("NotWorking")]
		public void Defaults ()
		{
			AccessPoker sql = new AccessPoker ();

			Assert.AreEqual ("", sql.CacheKeyDependency, "A1");
			Assert.IsTrue (sql.CancelSelectOnNullParameter, "A2");
			Assert.AreEqual (ConflictOptions.OverwriteChanges, sql.ConflictDetection, "A3");
            Assert.AreEqual(SqlDataSourceCommandType.Text, sql.DeleteCommandType, "A4");
            Assert.AreEqual(SqlDataSourceCommandType.Text, sql.InsertCommandType, "A5");
            Assert.AreEqual(SqlDataSourceCommandType.Text, sql.SelectCommandType, "A6");
            Assert.AreEqual(SqlDataSourceCommandType.Text, sql.UpdateCommandType, "A7");
			Assert.AreEqual ("{0}", sql.OldValuesParameterFormatString, "A8");

			// SqlCacheDependency access should raise an exception
			//			Assert.AreEqual ("", sql.SqlCacheDependency, "A9");
			Assert.AreEqual ("", sql.SortParameterName, "A10");
			Assert.AreEqual (0, sql.CacheDuration, "A11");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, sql.CacheExpirationPolicy, "A12");
			Assert.IsFalse (sql.EnableCaching, "A13");
			Assert.AreEqual ("System.Data.OleDb", sql.ProviderName, "A14");
			Assert.AreEqual ("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=", sql.ConnectionString, "A15");
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
			Assert.AreEqual ("", sql.DataFile, "A27");
		}
    }
}

#endif
