//
// Tests for System.Web.UI.WebControls.SqlDataSource
// This test uses Derby, java embedded database.
//
// Author:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//

//
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

#if NET_2_0 && TARGET_JVM

using NUnit.Framework;
using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class SqlDataSourceDerbyTest {
		[TestFixtureSetUp]
		public void setup ()
		{
			if (Directory.Exists (_dataDir))
				Directory.Delete (_dataDir, true);

			string initSql = @"CREATE TABLE Table1 (
				UserId                                  int                 NOT NULL PRIMARY KEY,
				UserName                                varchar(256)        NOT NULL,
				Description                             varchar(256)
			)";

			OleDbConnection connection = new OleDbConnection (_connectionString);
			try {
				connection.Open ();
				DbCommand cmd = connection.CreateCommand ();
				cmd.CommandText = initSql;
				cmd.CommandType = CommandType.Text;
				cmd.ExecuteNonQuery ();
			}
			catch (Exception) {
			}
			finally {
				connection.Close ();
			}
		}

		[Test]
		public void SelectTest1 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1";
			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());

			Assert.AreEqual (10, dataView.Count);
		}

		[Test]
		public void SelectTest2 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1";
			ds.FilterExpression = "UserId > 5";

			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (4, dataView.Count);
		}

		[Test]
		public void SelectTest3 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1";

			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ("Description"));
			Assert.AreEqual ("Description", dataView.Sort);
		}

		[Test]
		public void SelectTest4 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1";
			try {
				DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments (1, 2));
			}
			catch (NotSupportedException) {
				Assert.AreEqual (true, true);
			}
		}

		[Test]
		public void SelectTest5 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1 WHERE UserId = ?";
			ds.SelectParameters.Add (new Parameter ("UserId", TypeCode.Int32, "5"));

			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, dataView.Count);
		}

		[Test]
		public void UpdateTest1 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1 WHERE UserName = ?";
			ds.SelectParameters.Add (new Parameter ("UserName", TypeCode.String, "superuser"));

			ds.UpdateCommand = "UPDATE Table1 SET UserName = ? WHERE UserId = ?";
			ds.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "superuser"));
			ds.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "5"));

			int records = ds.Update ();
			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, dataView.Count);
			Assert.AreEqual (1, records);

		}

		[Test]
		public void UpdateTest2 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1 WHERE UserName = ?";
			ds.SelectParameters.Add (new Parameter ("UserName", TypeCode.String, "SimpleUser"));

			ds.UpdateCommand = "UPDATE Table1 SET UserName = ? WHERE UserId = ?";
			ds.UpdateParameters.Add (new Parameter ("UserName", TypeCode.String, "superuser"));
			ds.UpdateParameters.Add (new Parameter ("UserId", TypeCode.Int32, "5"));
			ds.OldValuesParameterFormatString = "original_{0}";

			SqlDataSourceView view = (SqlDataSourceView) ((IDataSource) ds).GetView ("");

			OrderedDictionary keys = new OrderedDictionary ();
			keys.Add ("UserId", 7);

			OrderedDictionary values = new OrderedDictionary ();
			values.Add ("UserName", "SimpleUser");

			OrderedDictionary oldvalues = new OrderedDictionary ();
			oldvalues.Add ("UserName", "user7");

			int records = view.Update (keys, values, oldvalues);
			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, dataView.Count);
			Assert.AreEqual (1, records);

		}

		[Test]
		public void InsertTest1 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1 WHERE UserName = ?";
			ds.SelectParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser"));

			ds.InsertCommand = "INSERT INTO Table1 (UserId, UserName, Description) VALUES (?, ?, ?)";
			ds.InsertParameters.Add (new Parameter ("UserId", TypeCode.Int32, "15"));
			ds.InsertParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser"));
			ds.InsertParameters.Add (new Parameter ("Description", TypeCode.String, "newuser"));

			int records = ds.Insert ();
			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, dataView.Count);
			Assert.AreEqual (1, records);

		}

		[Test]
		public void InsertTest2 ()
		{
			SqlDataSource ds = CreateDataSource ();
			ds.SelectCommand = "SELECT * FROM Table1 WHERE UserName = ?";
			ds.SelectParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser2"));

			ds.InsertCommand = "INSERT INTO Table1 (UserId, UserName, Description) VALUES (?, ?, ?)";
			ds.InsertParameters.Add (new Parameter ("UserId", TypeCode.Int32, "5"));
			ds.InsertParameters.Add (new Parameter ("UserName", TypeCode.String, "newuser"));
			ds.InsertParameters.Add (new Parameter ("Description", TypeCode.String, "newuser"));

			SqlDataSourceView view = (SqlDataSourceView) ((IDataSource) ds).GetView ("");

			OrderedDictionary values = new OrderedDictionary ();
			values.Add ("UserId", "17");
			values.Add ("UserName", "newuser2");
			values.Add ("Description", "newuser2");

			int records = view.Insert (values);
			DataView dataView = (DataView) ds.Select (new DataSourceSelectArguments ());
			Assert.AreEqual (1, dataView.Count);
			Assert.AreEqual (1, records);

		}


		const string _dataDir = "DataDir";
		const string _connectionString = "JdbcDriverClassName=org.apache.derby.jdbc.EmbeddedDriver;JdbcURL=jdbc:derby:" + _dataDir + ";create=true";
		private SqlDataSource CreateDataSource ()
		{
			SqlDataSource ds = new SqlDataSource ();
			ds.ConnectionString = _connectionString;
			ds.ProviderName = "System.Data.OleDb";
			ds.DataSourceMode = SqlDataSourceMode.DataSet;
			return ds;
		}

		[SetUp]
		public void RestoreData ()
		{
			string insertSql = @"INSERT INTO Table1 VALUES ({0}, '{1}', '{2}')";
			string deleteSql = @"DELETE FROM Table1";

			OleDbConnection connection = new OleDbConnection (_connectionString);
			connection.Open ();
			try {
				DbCommand dc = connection.CreateCommand ();
				dc.CommandText = deleteSql;
				dc.CommandType = CommandType.Text;
				dc.ExecuteNonQuery ();

				for (int i = 0; i < 10; i++) {
					DbCommand ic = connection.CreateCommand ();
					ic.CommandText = string.Format (insertSql, i.ToString (), "user" + i.ToString (), (9 - i).ToString ());
					ic.CommandType = CommandType.Text;
					ic.ExecuteNonQuery ();
				}
			}
			catch (Exception) {
			}
			finally {
				connection.Close ();
			}
		}
	}

}

#endif
