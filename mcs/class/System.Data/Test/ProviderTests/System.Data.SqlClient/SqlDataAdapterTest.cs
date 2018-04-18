//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                          SqlDataAdapter class
// Author:
//      Umadevi S (sumadevi@novell.com)
//	Sureshkumar T (tsureshkumar@novell.com)
//	Senganal T (tsenganal@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlDataAdapterTest
	{
		SqlDataAdapter adapter;
		SqlDataReader dr;
		DataSet data;
		string connectionString = ConnectionManager.Instance.Sql.ConnectionString;
		SqlConnection conn;
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			if (adapter != null) {
				adapter.Dispose ();
				adapter = null;
			}

			if (dr != null) {
				dr.Close ();
				dr = null;
			}

			if (conn != null) {
				conn.Close ();
				conn = null;
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Update_DeleteRow ()
		{
			conn = new SqlConnection (ConnectionManager.Instance.Sql.ConnectionString);
			conn.Open ();

			DataTable dt = new DataTable ();
			adapter = new SqlDataAdapter ("SELECT * FROM employee", conn);
			SqlCommandBuilder builder = new SqlCommandBuilder (adapter);
			adapter.DeleteCommand = builder.GetDeleteCommand ();
			adapter.Fill (dt);

			DateTime now = DateTime.Now;

			DateTime doj = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);

			DateTime dob = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);
			dob.Subtract (new TimeSpan (20 * 365, 0, 0, 0));

			try {
				DataRow newRow = dt.NewRow ();
				newRow ["id"] = 6002;
				newRow ["fname"] = "boston";
				newRow ["dob"] = dob;
				newRow ["doj"] = doj;
				newRow ["email"] = "mono@novell.com";
				dt.Rows.Add (newRow);
				adapter.Update (dt);

				foreach (DataRow row in dt.Rows)
					if (((int) row ["id"]) == 6002)
						row.Delete ();
				adapter.Update (dt);

				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT id, fname, lname, dob, doj, email FROM employee WHERE id = 6002";
				dr = cmd.ExecuteReader ();
				Assert.IsFalse (dr.Read ());
				dr.Close ();
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Update_InsertRow ()
		{
			conn = new SqlConnection (ConnectionManager.Instance.Sql.ConnectionString);
			conn.Open ();

			DataTable dt = new DataTable ();
			adapter = new SqlDataAdapter ("SELECT * FROM employee", conn);

			SqlCommandBuilder builder = new SqlCommandBuilder (adapter);
			adapter.InsertCommand = builder.GetInsertCommand ();
			adapter.Fill (dt);

			DateTime now = DateTime.Now;

			DateTime doj = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);

			DateTime dob = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);
			dob.Subtract (new TimeSpan (20 * 365, 0, 0, 0));

			try {
				DataRow newRow = dt.NewRow ();
				newRow ["id"] = 6002;
				newRow ["fname"] = "boston";
				newRow ["dob"] = dob;
				newRow ["doj"] = doj;
				newRow ["email"] = "mono@novell.com";
				dt.Rows.Add (newRow);
				adapter.Update (dt);

				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT id, fname, lname, dob, doj, email FROM employee WHERE id = 6002";
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (6002, dr.GetValue (0), "#A2");
				Assert.AreEqual ("boston", dr.GetValue (1), "#A3");
				Assert.AreEqual (DBNull.Value, dr.GetValue (2), "#A4");
				Assert.AreEqual (dob, dr.GetValue (3), "#A5");
				Assert.AreEqual (doj, dr.GetValue (4), "#A6");
				Assert.AreEqual ("mono@novell.com", dr.GetValue (5), "#A7");
				Assert.IsFalse (dr.Read (), "#A8");
				dr.Close ();
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Update_UpdateRow ()
		{
			conn = new SqlConnection (ConnectionManager.Instance.Sql.ConnectionString);
			conn.Open ();

			DataTable dt = new DataTable ();
			adapter = new SqlDataAdapter ("SELECT * FROM employee", conn);
			SqlCommandBuilder builder = new SqlCommandBuilder (adapter);
			adapter.UpdateCommand = builder.GetUpdateCommand ();
			adapter.Fill (dt);

			DateTime now = DateTime.Now;

			DateTime doj = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);

			DateTime dob = new DateTime (now.Year, now.Month, now.Day, now.Hour,
				now.Minute, now.Second);
			dob.Subtract (new TimeSpan (20 * 365, 0, 0, 0));

			try {
				DataRow newRow = dt.NewRow ();
				newRow ["id"] = 6002;
				newRow ["fname"] = "boston";
				newRow ["dob"] = dob;
				newRow ["doj"] = doj;
				newRow ["email"] = "mono@novell.com";
				dt.Rows.Add (newRow);
				adapter.Update (dt);

				foreach (DataRow row in dt.Rows)
					if (((int) row ["id"]) == 6002)
						row ["lname"] = "de Icaza";
				adapter.Update (dt);

				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "SELECT id, fname, lname, dob, doj, email FROM employee WHERE id = 6002";
				dr = cmd.ExecuteReader ();
				Assert.IsTrue (dr.Read (), "#A1");
				Assert.AreEqual (6002, dr.GetValue (0), "#A2");
				Assert.AreEqual ("boston", dr.GetValue (1), "#A3");
				Assert.AreEqual ("de Icaza", dr.GetValue (2), "#A4");
				Assert.AreEqual (dob, dr.GetValue (3), "#A5");
				Assert.AreEqual (doj, dr.GetValue (4), "#A6");
				Assert.AreEqual ("mono@novell.com", dr.GetValue (5), "#A7");
				Assert.IsFalse (dr.Read (), "#A8");
				dr.Close ();
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
			}
		}

		/**
		   The below test will not run everytime, since the region id column is unique
		   so change the regionid if you want the test to pass.
		**/
		/*
		[Test]
		public void UpdateTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				DataTable dt = new DataTable();
				SqlDataAdapter da = null;
				da = new SqlDataAdapter("Select * from employee", conn);
				//SqlCommandBuilder cb = new SqlCommandBuilder (da);
				da.Fill(dt);
				DataRow dr = dt.NewRow();
				dr ["id"] = 6002;
				dr ["fname"] = "boston";
				dr ["dob"] = DateTime.Now.Subtract (new TimeSpan (20*365, 0, 0, 0));
				dr ["doj"] = DateTime.Now;
				dt.Rows.Add(dr);

				da.Update(dt);
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
				ConnectionManager.Singleton.Sql.CloseConnection ();
			}
		}

		private static void OnRowUpdatedTest (object sender, SqlRowUpdatedEventArgs e)
		{
			rowUpdated = true;
		}

		private static void OnRowUpdatingTest (object sender, SqlRowUpdatingEventArgs e)
		{
			rowUpdating = true;
		}

		private static bool rowUpdated = false;
		private static bool rowUpdating = false;
		[Test]
		public void RowUpdatedTest () {
			conn = (SqlConnection) ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				DataTable dt = null;
				DataSet ds = new DataSet ();
				SqlDataAdapter da = null;
				da = new SqlDataAdapter("Select * from employee", conn);
				//SqlCommandBuilder cb = new SqlCommandBuilder (da);
				rowUpdated = false;
				rowUpdating = false;
				da.RowUpdated += new SqlRowUpdatedEventHandler (OnRowUpdatedTest);
				da.RowUpdating += new SqlRowUpdatingEventHandler (OnRowUpdatingTest);
				da.Fill (ds);
				dt = ds.Tables [0];
				dt.Rows[0][0] = 200;
				da.UpdateCommand = new SqlCommand ("Update employee set id = @id");
				da.Update (dt);
				dt.Rows[0][0] = 1;
				da.Update (dt);
				da.RowUpdated -= new SqlRowUpdatedEventHandler (OnRowUpdatedTest);
				da.RowUpdating -= new SqlRowUpdatingEventHandler (OnRowUpdatingTest);
				Assert.AreEqual (true, rowUpdated, "RowUpdated");
				Assert.AreEqual (true, rowUpdating, "RowUpdating");
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
				ConnectionManager.Singleton.Sql.CloseConnection ();
			}
		}
		*/

		/**
		   This needs a errortable created as follows 
		   id uniqueidentifier,name char(10) , with values
		   Guid		name
		   {A12...}	NULL
		   NULL		bbbbbb
		**/
		[Test]
		public void NullGuidTest()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			try {
				DBHelper.ExecuteNonQuery (conn, "create table #tmp_guid_table ( " +
							  " id uniqueidentifier default newid (), " +
							  " name char (10))");
				DBHelper.ExecuteNonQuery (conn, "insert into #tmp_guid_table (name) values (null)");
				DBHelper.ExecuteNonQuery (conn, "insert into #tmp_guid_table (id, name) values (null, 'bbbb')");
				SqlDataAdapter da = new SqlDataAdapter("select * from #tmp_guid_table", conn);
				DataSet ds = new DataSet();
				da.Fill(ds);
				Assert.AreEqual (1, ds.Tables.Count, "#1");
				Assert.AreEqual (DBNull.Value, ds.Tables [0].Rows [1] ["id"], "#2");
			} finally {
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
			// the bug 68804 - is that the fill hangs!
			Assert.AreEqual("Done","Done");
		}

		[Test]
		public void DefaultConstructorTest ()
		{
			adapter = new SqlDataAdapter ();
			Assert.AreEqual (MissingMappingAction.Passthrough,
				adapter.MissingMappingAction,
				"#1 Missing Mapping acttion default to Passthrough");
			Assert.AreEqual (MissingSchemaAction.Add,
				adapter.MissingSchemaAction,
				"#2 Missing Schme action default to Add");
		}

		[Test]
		public void OverloadedConstructorsTest ()
		{
			SqlCommand selCmd = new SqlCommand ("Select * from numeric_family");
			adapter = new SqlDataAdapter (selCmd);
			Assert.AreEqual (MissingMappingAction.Passthrough,
				adapter.MissingMappingAction,
				"#1 Missing Mapping acttion default to Passthrough");
			Assert.AreEqual (MissingSchemaAction.Add,
				adapter.MissingSchemaAction,
				"#2 Missing Schme action default to Add");
			Assert.AreSame (selCmd, adapter.SelectCommand,
				"#3 Select Command shud be a ref to the arg passed");
			
			conn = new SqlConnection (connectionString);
			String selStr = "Select * from numeric_family";
			adapter = new SqlDataAdapter (selStr, conn);
			Assert.AreEqual (MissingMappingAction.Passthrough,
				adapter.MissingMappingAction,
				"#4 Missing Mapping acttion default to Passthrough");
			Assert.AreEqual (MissingSchemaAction.Add,
				adapter.MissingSchemaAction,
				"#5 Missing Schme action default to Add");
			Assert.AreSame (selStr, adapter.SelectCommand.CommandText,
				"#6 Select Command shud be a ref to the arg passed");
			Assert.AreSame (conn, adapter.SelectCommand.Connection,
				"#7 cmd.connection shud be t ref to connection obj");

			selStr = "Select * from numeric_family";
			adapter = new SqlDataAdapter (selStr, connectionString);
			Assert.AreEqual (MissingMappingAction.Passthrough,
				adapter.MissingMappingAction,
				"#8 Missing Mapping action shud default to Passthrough");
			Assert.AreEqual (MissingSchemaAction.Add,
				adapter.MissingSchemaAction,
				"#9 Missing Schema action shud default to Add");
			Assert.AreSame (selStr,
				adapter.SelectCommand.CommandText,
				"#10");
			Assert.AreEqual (connectionString,
				adapter.SelectCommand.Connection.ConnectionString,
				"#11  ");
		}

		[Test]
		public void Fill_Test_ConnState ()
		{
			//Check if Connection State is maintained correctly .. 
			data = new DataSet ("test1");
			adapter = new SqlDataAdapter ("select id from numeric_family where id=1",
					 connectionString);
			SqlCommand cmd = adapter.SelectCommand ; 

			Assert.AreEqual (ConnectionState.Closed,
				cmd.Connection.State, "#1 Connection shud be in closed state");
			adapter.Fill (data);
			Assert.AreEqual (1, data.Tables.Count, "#2 One table shud be populated");
			Assert.AreEqual (ConnectionState.Closed, cmd.Connection.State,
				"#3 Connection shud be closed state");

			data = new DataSet ("test2");
			cmd.Connection.Open ();
			Assert.AreEqual (ConnectionState.Open, cmd.Connection.State,
				"#3 Connection shud be open");
			adapter.Fill (data);
			Assert.AreEqual (1, data.Tables.Count, "#4 One table shud be populated");
			Assert.AreEqual (ConnectionState.Open, cmd.Connection.State,
				"#5 Connection shud be open");
			cmd.Connection.Close ();
 
			// Test if connection is closed when exception occurs
			cmd.CommandText = "select id1 from numeric_family";
			try {
				adapter.Fill (data);
			} catch {
				if (cmd.Connection.State == ConnectionState.Open) {
					cmd.Connection.Close ();
					Assert.Fail ("# Connection Shud be Closed");
				}
			}
		}

		[Test]
		[Category("NotWorking")]
		public void Fill_Test_Data ()
		{
			//Check if a table is created for each resultset 
			String batchQuery = "Select id,type_bit,type_int from numeric_family;";
			batchQuery += "Select type_bit from numeric_family";
			adapter = new SqlDataAdapter (batchQuery, connectionString);
			data = new DataSet ("test1");
			adapter.Fill (data);
			Assert.AreEqual (2, data.Tables.Count,"#1 2 Table shud be created");

			//Check if Table and Col are named correctly for unnamed columns 
			string query = "Select 10,20 from numeric_family;" ;
			query += "Select 10,20 from numeric_family";
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test2");
			adapter.Fill (data);
			Assert.AreEqual (2, data.Tables.Count,
				"#2 2 Tables shud be created");
			Assert.AreEqual ("Table", data.Tables[0].TableName, "#3");
			Assert.AreEqual ("Table1", data.Tables[1].TableName, "#4");
			Assert.AreEqual ("Column1", data.Tables[0].Columns[0].ColumnName, "#5");
			Assert.AreEqual ("Column2", data.Tables[0].Columns[1].ColumnName, "#6");
			Assert.AreEqual ("Column1", data.Tables[1].Columns[0].ColumnName, "#7");
			Assert.AreEqual ("Column2", data.Tables[1].Columns[1].ColumnName, "#8");

			//Check if dup columns are named correctly
			query = "select A.id ,B.id , C.id from numeric_family A, ";
			query += "numeric_family B , numeric_family C";
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test3");
			adapter.Fill (data);

			// NOTE msdotnet contradicts documented behavior
			// as per documentation the column names should be 
			// id1,id2,id3 .. but msdotnet returns id,id1,id2
			Assert.AreEqual ("id", data.Tables[0].Columns[0].ColumnName,
				"#9 if colname is duplicated ,shud be col,col1,col2 etc");
			Assert.AreEqual ("id1", data.Tables[0].Columns[1].ColumnName,
				"#10 if colname is duplicated ,shud be col,col1,col2 etc");
			Assert.AreEqual ("id2", data.Tables[0].Columns[2].ColumnName,
				"#11 if colname is duplicated ,shud be col,col1,col2 etc");

			// Test if tables are created and named accordingly ,
			// but only for those queries returning result sets
			query = "update numeric_family set id=100 where id=50;";
			query += "select * from numeric_family";
			adapter = new SqlDataAdapter (query, connectionString); 
			data = new DataSet ("test4");
			adapter.Fill (data);
			Assert.AreEqual (1 ,data.Tables.Count,
				"#12 Tables shud be named only for queries returning a resultset");
			Assert.AreEqual ("Table", data.Tables[0].TableName,
				"#13 The first resutlset shud have 'Table' as its name");

			// Test behavior with an outerjoin
			query = "select A.id,B.type_bit from numeric_family A LEFT OUTER JOIN "; 
			query += "numeric_family B on A.id = B.type_bit"; 
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test5");
			adapter.Fill (data);
			Assert.AreEqual (0, data.Tables[0].PrimaryKey.Length,
				"#14 Primary Key shudnt be set if an outer join is performed");
			Assert.AreEqual (0, data.Tables[0].Constraints.Count,
				"#15 Constraints shudnt be set if an outer join is performed");
			adapter = new SqlDataAdapter ("select id from numeric_family",
					connectionString);
			data = new DataSet ("test6");
			adapter.Fill (data, 1, 1, "numeric_family");
			Assert.AreEqual (1, data.Tables[0].Rows.Count, "#16"); 
			Assert.AreEqual (2, data.Tables[0].Rows[0][0], "#17");

			// only one test for DataTable.. DataSet tests covers others  
			adapter = new SqlDataAdapter ("select id from numeric_family",
					connectionString);
			DataTable table = new DataTable ("table1");
			adapter.Fill (table);
			Assert.AreEqual (4, table.Rows.Count , "#18");
		}
		
		[Test]
		public void Fill_Test_PriKey ()
		{	      
			// Test if Primary Key & Constraints Collection is correct 
			adapter = new SqlDataAdapter ("select id,type_bit from numeric_family", 
					connectionString);
			adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			data = new DataSet ("test1");
			adapter.Fill (data);
			Assert.AreEqual (1, data.Tables[0].PrimaryKey.Length,
				"#1 Primary Key shud be set");
			Assert.AreEqual (1, data.Tables[0].Constraints.Count,
				"#2 Constraints shud be set");
			Assert.AreEqual (4, data.Tables[0].Rows.Count,
				"#3 No Of Rows shud be 4");
		
			// Test if data is correctly merged 
			adapter.Fill (data);
			Assert.AreEqual (4, data.Tables[0].Rows.Count,
				"#4 No of Row shud still be 4");

			// Test if rows are appended  and not merged 
			// when primary key is not returned in the result-set
			string query = "Select type_int from numeric_family";
			adapter.SelectCommand.CommandText = query;
			data = new DataSet ("test2");
			adapter.Fill (data);
			Assert.AreEqual (4, data.Tables[0].Rows.Count,
				"#5 No of Rows shud be 4");
			adapter.Fill (data);
			Assert.AreEqual (8, data.Tables[0].Rows.Count,
				"#6 No of Rows shud double now");
		}
 	
		[Test]
		public void Fill_Test_Exceptions ()
		{
			adapter = new SqlDataAdapter ("select * from numeric_family",
					connectionString);
			data = new DataSet ("test1");
			try {
				adapter.Fill (data, -1, 0, "numeric_family");
				Assert.Fail ("#1 Exception shud be thrown:Incorrect Arguments"); 
			}catch (AssertionException e){
				throw e;
			}catch (Exception e){
				Assert.AreEqual (typeof(ArgumentException), e.GetType(),
					"#2 Incorrect Exception : "  + e);
			}

			// conn is not closed due to a bug..
			// can be removed later 
			adapter.SelectCommand.Connection.Close (); 

			try {
				adapter.Fill (data , 0 , -1 , "numeric_family");
				Assert.Fail ("#3 Exception shud be thrown:Incorrect Arguments"); 
			}catch (AssertionException e){
				throw e;
			}catch (Exception e){
				Assert.AreEqual (typeof(ArgumentException), e.GetType(),
					"#4 Incorrect Exception : "  + e);
			}
			// conn is curr not closed.. can be removed later 
			adapter.SelectCommand.Connection.Close ();  

			/*
			// NOTE msdotnet contradicts documented behavior
			// InvalidOperationException is expected if table is not valid	
			try {
				adapter.Fill (data , 0 , 0 , "invalid_talbe_name");
			}catch (InvalidOperationException e) {
				ex= e;
			}catch (Exception e){
				Assert.Fail ("#5 Exception shud be thrown : incorrect arugments ");
			}
			Assert.IsNotNull (ex , "#6 Exception shud be thrown : incorrect args ");
			adapter.SelectCommand.Connection.Close (); // tmp .. can be removed once the bug if fixed
			ex=null;
			*/

			try {
				adapter.Fill ( null , 0 , 0 , "numeric_family");
				Assert.Fail ( "#7 Exception shud be thrown : Invalid Dataset");
			}catch (AssertionException e){
				throw e ;
			}catch (ArgumentNullException) {
		       
			}catch (Exception e) {
				Assert.AreEqual (typeof(SystemException), e.GetType(),
					"#8 Incorrect Exception : " + e);
			}
			// conn is currently not being closed.. 
			//need to be removed once behavior is fixed 
			adapter.SelectCommand.Connection.Close (); 

			adapter.SelectCommand.Connection = null; 
			try {
				adapter.Fill (data);
				Assert.Fail ("#9 Exception shud be thrown : Invalid Connection");
			}catch (AssertionException e){
				throw e;
			}catch (Exception e){
				Assert.AreEqual (typeof(InvalidOperationException), e.GetType(),
					"#10 Incorrect Exception : " + e);
			}
		}

		bool FillErrorContinue = false;
		[Test]
		public void Fill_Test_FillErrorTest ()
		{
			string query = "select type_int from numeric_family where id=1 or id=4 ";

			DataSet ds = new DataSet ();
			DataTable table = ds.Tables.Add ("test");
			table.Columns.Add ("col", typeof (short));

			adapter = new SqlDataAdapter (query, connectionString);
			DataTableMapping mapping = adapter.TableMappings.Add ("numeric_family", "test");
			mapping.ColumnMappings.Add ("type_int", "col");

			try {
				adapter.Fill (ds, "numeric_family");
				Assert.Fail ("#A1");
			} catch (OverflowException) {
			} catch (ArgumentException ex) {
				// System.OverflowException: Value was either too large or too
				// small for an Int16
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");

				OverflowException inner = ex.InnerException as OverflowException;
				Assert.IsNotNull (inner, "#A6");
				Assert.AreEqual (typeof (OverflowException), inner.GetType (), "#A7");
				Assert.IsNull (inner.InnerException, "#A8");
				Assert.IsNotNull (inner.Message, "#A9");
			}
			Assert.AreEqual (0, ds.Tables [0].Rows.Count, "#A10");

			adapter.FillError += new FillErrorEventHandler (ErrorHandler);
			FillErrorContinue = false;
			try {
				adapter.Fill (ds, "numeric_family");
				Assert.Fail ("#B1");
			} catch (OverflowException) {
			} catch (ArgumentException ex) {
				// System.OverflowException: Value was either too large or too
				// small for an Int16
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");

				OverflowException inner = ex.InnerException as OverflowException;
				Assert.IsNotNull (inner, "#B6");
				Assert.AreEqual (typeof (OverflowException), inner.GetType (), "#B7");
				Assert.IsNull (inner.InnerException, "#B8");
				Assert.IsNotNull (inner.Message, "#B9");
			}
			Assert.AreEqual (0, ds.Tables [0].Rows.Count, "#B10");

			FillErrorContinue = true;
			int count = adapter.Fill (ds, "numeric_family");
			Assert.AreEqual (1, ds.Tables [0].Rows.Count, "#C1");
			Assert.AreEqual (1, count, "#C2");
		}

		void ErrorHandler (object sender, FillErrorEventArgs args)
		{
			args.Continue = FillErrorContinue;
		}

		[Test]
		public void GetFillParametersTest ()
		{
			string query = "select id, type_bit from numeric_family where id > @param1";
			adapter = new SqlDataAdapter (query, connectionString);
			IDataParameter[] param = adapter.GetFillParameters ();
			Assert.AreEqual (0, param.Length, "#1 size shud be 0");
			
			SqlParameter param1 = new SqlParameter ();
			param1.ParameterName = "@param1";
			param1.Value = 2;
			adapter.SelectCommand.Parameters.Add (param1);
		
			param = adapter.GetFillParameters ();
			Assert.AreEqual (1, param.Length, "#2 count shud be 1");
			Assert.AreEqual (param1, param[0], "#3 Params shud be equal");
		}
		
		[Test]
		public void FillSchemaTest ()
		{
			string query;

			// Test if connection is closed if excepton occurs during fill schema 
			query = "select * from invalid_table"; 
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test");
			try {
				adapter.FillSchema (data , SchemaType.Source);
			} catch {
				if (adapter.SelectCommand.Connection.State != ConnectionState.Closed) {
					Assert.Fail ("#0 Conn shud be closed if exception occurs");
					adapter.SelectCommand.Connection.Close();
				}
			}
		
			// Test Primary Key is set (since primary key column returned)	
			query = "select id, type_int from numeric_family where id=1";	
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test1");
			adapter.FillSchema (data , SchemaType.Source);

			Assert.AreEqual (1, data.Tables[0].PrimaryKey.Length,
				"#1 Primary Key property must be set");
	
			// Test Primary Key is not set (since primary key column is returned)	
			query = "select type_bit, type_int from numeric_family where id=1"; 	
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test2");
			adapter.FillSchema (data, SchemaType.Source);
			Assert.AreEqual (0, data.Tables[0].PrimaryKey.Length,
				"#2 Primary Key property should not be set");

			// Test multiple tables are created for a batch query
			query = "Select id ,type_bit from numeric_family;" ;
			query += "Select id,type_bit,type_int from numeric_family;"; 
			data = new DataSet ("test3");
			adapter = new SqlDataAdapter (query, connectionString);
			adapter.FillSchema (data , SchemaType.Source);
			Assert.AreEqual (2 , data.Tables.Count , "#3 A table shud be created for each Result Set");
			Assert.AreEqual (2 , data.Tables[0].Columns.Count , "#4 should have 2 columns");
			Assert.AreEqual (3 , data.Tables[1].Columns.Count , "#5 Should have 3 columns");

			// Test if table names and column names  are filled correctly
			query = "select 10,20 from numeric_family;" ;
			query += "select 10,20 from numeric_family;";
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test4");
			try {
				adapter.FillSchema (data , SchemaType.Source);
			}catch (Exception e){
				Assert.Fail ("#3 Unexpected Exception : " + e); 
			}
			Assert.AreEqual ( "Table", data.Tables[0].TableName);
			Assert.AreEqual ( "Table1", data.Tables[1].TableName);
			Assert.AreEqual ( "Column1", data.Tables[0].Columns[0].ColumnName,
				"#6 Unnamed col shud be named as 'ColumnN'");
			Assert.AreEqual ( "Column2", data.Tables[0].Columns[1].ColumnName,
				"#7 Unnamed col shud be named as 'ColumnN'");
			Assert.AreEqual ( "Column1", data.Tables[1].Columns[0].ColumnName,
				"#8 Unnamed col shud be named as 'ColumnN'");
			Assert.AreEqual ( "Column2", data.Tables[1].Columns[1].ColumnName,
				"#9 Unnamed col shud be named as 'ColumnN'");
			Assert.AreEqual (ConnectionState.Closed, adapter.SelectCommand.Connection.State,
				"#10 Connection shud be closed");
			
			// Test if mapping works correctly  
			// doesent work in both mono and msdotnet
			// gotto check if something is wrong 
			/*
			query = "select id,type_bit from numeric_family";  
			adapter = new SqlDataAdapter (query, connectionString);
			data = new DataSet ("test");
			DataTable table = data.Tables.Add ("numeric_family_1");
			table.Columns.Add ("id");
			table.Columns.Add ("type_bit");
			DataTableMapping map = adapter.TableMappings.Add("numeric_family_1",
							"numeric_family");
			map.ColumnMappings.Add ("id", "id_1");
			map.ColumnMappings.Add ("type_bit", "type_bit_1");
			adapter.FillSchema (data, SchemaType.Source, "numeric_family");
			foreach (DataTable tab in data.Tables){
				Console.WriteLine ("Table == {0}",tab.TableName);
				foreach (DataColumn col in tab.Columns)
					Console.WriteLine (" Col = {0} " , col.ColumnName);
			}
			*/
		}

		[Test]
		[Category("NotWorking")]
		public void MissingSchemaActionTest ()
		{
			adapter = new SqlDataAdapter (
					"select id,type_bit,type_int from numeric_family where id<=4",
					 connectionString);
			data = new DataSet ();
			Assert.AreEqual (MissingSchemaAction.Add, adapter.MissingSchemaAction,
					 "#1 Default Value");

			adapter.Fill (data);
			Assert.AreEqual (1, data.Tables.Count , "#1 One table shud be populated");
			Assert.AreEqual (3, data.Tables[0].Columns.Count, "#2 Missing cols are added");
			Assert.AreEqual (0, data.Tables[0].PrimaryKey.Length, "#3 Default Value");

			adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
			data.Reset();
			adapter.Fill (data);
			Assert.AreEqual (3, data.Tables[0].Columns.Count,
				"#4 Missing cols are added");
			Assert.AreEqual (1, data.Tables[0].PrimaryKey.Length, "#5 Default Value");

			adapter.MissingSchemaAction = MissingSchemaAction.Ignore ;
			data.Reset ();
			adapter.Fill (data);
			Assert.AreEqual (0, data.Tables.Count, "#6 Data shud be ignored");
			
			adapter.MissingSchemaAction = MissingSchemaAction.Error ; 
			data.Reset();
			try {
				adapter.Fill (data);
				Assert.Fail ("#8 Exception shud be thrown: Schema Mismatch");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof(InvalidOperationException), ex.GetType(),
					"#9");
			}

			// Test for invalid MissingSchema Value
			try {
				adapter.MissingSchemaAction = (MissingSchemaAction)(-5000);
				Assert.Fail ("#10 Exception shud be thrown: Invalid Value");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#11");
			}

			// Tests if Data is filled correctly if schema is defined 
			// manually and MissingSchemaAction.Error is set 
			adapter.MissingSchemaAction = MissingSchemaAction.Error;
			data.Reset();
			DataTable table = data.Tables.Add ("Table");
			table.Columns.Add ("id");
			table.Columns.Add ("type_bit");
			table.Columns.Add ("type_int");
			adapter.Fill (data);
			Assert.AreEqual (1, data.Tables.Count, "#12");
			Assert.AreEqual (4, data.Tables[0].Rows.Count, "#13");
		}
		
		[Test]
		[Category("NotWorking")]
		public void MissingMappingActionTest ()
		{
			adapter = new SqlDataAdapter ("select id,type_bit from numeric_family where id=1",
					connectionString);
			data = new DataSet ();
			Assert.AreEqual (adapter.MissingMappingAction,
				MissingMappingAction.Passthrough,
				"#1 Default Value");
			adapter.Fill(data);
			Assert.AreEqual (1, data.Tables.Count,
				"#2 One Table shud be created");
			Assert.AreEqual (2, data.Tables[0].Columns.Count,
				"#3 Two Cols shud be created");

			adapter.MissingMappingAction = MissingMappingAction.Ignore;
			data.Reset ();
			adapter.Fill (data);
			Assert.AreEqual (0, data.Tables.Count, "#4 No table shud be created");
			
			adapter.MissingMappingAction = MissingMappingAction.Error;
			data.Reset ();
			try {
				adapter.Fill (data);
				Assert.Fail ("#5 Exception shud be thrown : Mapping is missing");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof(InvalidOperationException), ex.GetType(),
					"#6");
			}

			try {
				adapter.MissingMappingAction = (MissingMappingAction)(-5000);
				Assert.Fail ("#7 Exception shud be thrown : Invalid Value");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (),
					"#8");
			}

			// Test if mapping the column and table names works correctly	
			adapter.MissingMappingAction = MissingMappingAction.Error;
			data.Reset ();
			DataTable table = data.Tables.Add ("numeric_family_1");
			table.Columns.Add ("id_1");
			table.Columns.Add ("type_bit_1");
			table.Columns.Add ("type_int_1");
			DataTableMapping tableMap = adapter.TableMappings.Add ("numeric_family",
							 "numeric_family_1");
			tableMap.ColumnMappings.Add ("id", "id_1");
			tableMap.ColumnMappings.Add ("type_bit", "type_bit_1");
			tableMap.ColumnMappings.Add ("type_int", "type_int_1");
			adapter.Fill (data,"numeric_family");
			Assert.AreEqual (1, data.Tables.Count ,
				"#8 The DataTable shud be correctly mapped");
			Assert.AreEqual (3, data.Tables[0].Columns.Count,
				"#9 The DataColumns shud be corectly mapped");
			Assert.AreEqual (1, data.Tables[0].Rows.Count,
				"#10 Data shud be populated if mapping is correct");
		}

		[Test] // bug #76433
		public void FillSchema_ValuesTest()
		{
			using (SqlConnection conn = new SqlConnection(connectionString)) {
				conn.Open();
				IDbCommand command = conn.CreateCommand();

				// Create Temp Table
				String cmd = "Create Table #tmp_TestTable (" ;
				cmd += "Field1 DECIMAL (10) NOT NULL,";
				cmd += "Field2 DECIMAL(19))";
				command.CommandText = cmd; 
				command.ExecuteNonQuery();

				DataSet dataSet = new DataSet();
				string selectString = "SELECT * FROM #tmp_TestTable";
				IDbDataAdapter dataAdapter = new SqlDataAdapter (
									selectString, conn);
				dataAdapter.FillSchema(dataSet, SchemaType.Mapped);

				Assert.AreEqual (1, dataSet.Tables.Count, "#1");
				Assert.IsFalse (dataSet.Tables[0].Columns[0].AllowDBNull,"#2");
				Assert.IsTrue (dataSet.Tables[0].Columns[1].AllowDBNull,"#3");
			}
		}

		[Test]
		public void Fill_CheckSchema ()
		{
			using (SqlConnection conn = new SqlConnection(connectionString)) {
				conn.Open();

				IDbCommand command = conn.CreateCommand();

				// Create Temp Table
				String cmd = "Create Table #tmp_TestTable (" ;
				cmd += "id int primary key,";
				cmd += "field int not null)";
				command.CommandText = cmd; 
				command.ExecuteNonQuery();

				DataSet dataSet = new DataSet();
				string selectString = "SELECT * from #tmp_TestTable";
				IDbDataAdapter dataAdapter = new SqlDataAdapter (
									selectString,conn);
				dataAdapter.Fill (dataSet);
				Assert.AreEqual (1, dataSet.Tables.Count, "#A1");
				Assert.AreEqual (2, dataSet.Tables [0].Columns.Count, "#A2");
				Assert.IsTrue (dataSet.Tables [0].Columns [1].AllowDBNull, "#A3");
				Assert.AreEqual (0, dataSet.Tables [0].PrimaryKey.Length, "#A4");

				dataSet.Reset ();
				dataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				dataAdapter.Fill (dataSet);
				Assert.AreEqual (1, dataSet.Tables.Count, "#B1");
				Assert.AreEqual (2, dataSet.Tables [0].Columns.Count, "#B2");
				Assert.IsFalse (dataSet.Tables [0].Columns [1].AllowDBNull, "#B3");
				if (ClientVersion == 7)
					Assert.AreEqual (0, dataSet.Tables [0].PrimaryKey.Length, "#B4");
				else
					Assert.AreEqual (1, dataSet.Tables [0].PrimaryKey.Length, "#B4");
			}
		}

		[Test]
		public void FillSchema_CheckSchema ()
		{
			using (SqlConnection conn = new SqlConnection(connectionString)) {
				conn.Open();

				IDbCommand command = conn.CreateCommand();

				// Create Temp Table
				String cmd = "Create Table #tmp_TestTable (" ;
				cmd += "id int primary key,";
				cmd += "field int not null)";
				command.CommandText = cmd; 
				command.ExecuteNonQuery();

				DataSet dataSet = new DataSet();
				string selectString = "SELECT * from #tmp_TestTable";
				IDbDataAdapter dataAdapter = new SqlDataAdapter (
									selectString,conn);

				dataAdapter.FillSchema (dataSet, SchemaType.Mapped);
				Assert.IsFalse (dataSet.Tables[0].Columns[1].AllowDBNull, "#1");

				dataSet.Reset ();
				dataAdapter.MissingSchemaAction = MissingSchemaAction.Add;
				dataAdapter.FillSchema (dataSet, SchemaType.Mapped);
				Assert.IsFalse (dataSet.Tables[0].Columns[1].AllowDBNull, "#2");

				dataSet.Reset ();
				dataAdapter.MissingSchemaAction = MissingSchemaAction.Ignore;
				dataAdapter.FillSchema (dataSet, SchemaType.Mapped);
				Assert.AreEqual (0, dataSet.Tables.Count, "#3");

				dataSet.Reset ();
				dataAdapter.MissingSchemaAction = MissingSchemaAction.Error;
				try {
					dataAdapter.FillSchema (dataSet, SchemaType.Mapped);
					Assert.Fail ("#4 Error should be thrown");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#4");
				}
			}
		}

		[Test]
		[Ignore("TODO: Set SSPI Connection String")]
		public void CreateViewSSPITest ()
		{
			var conn = ConnectionManager.Instance.Sql.Connection;

			string sql = "create view MONO_TEST_VIEW as select * from Numeric_family";

			SqlCommand dbcmd = new SqlCommand( sql, conn );
			dbcmd.ExecuteNonQuery();

			sql = "drop view MONO_TEST_VIEW";

			dbcmd = new SqlCommand( sql, conn );
			dbcmd.ExecuteNonQuery();

			conn.Close();
		}

		[Test]
		public void Fill_RelatedTables ()
		{
			SqlConnection conn = new SqlConnection(connectionString);
			using (conn) {
				conn.Open();
				IDbCommand command = conn.CreateCommand();

				DataSet dataSet = new DataSet();
				string selectString = "SELECT id, type_int from numeric_family where id < 3";
				DbDataAdapter dataAdapter = new SqlDataAdapter (selectString,conn);

				DataTable table2 = dataSet.Tables.Add ("table2");
				DataColumn ccol1 = table2.Columns.Add ("id", typeof (int));
				DataColumn ccol2 = table2.Columns.Add ("type_int", typeof (int));

				DataTable table1 = dataSet.Tables.Add ("table1");
				DataColumn pcol1 = table1.Columns.Add ("id", typeof (int));
				DataColumn pcol2 = table1.Columns.Add ("type_int", typeof (int));

				table2.Constraints.Add ("fk", pcol1, ccol1);
				//table1.Constraints.Add ("fk1", pcol2, ccol2);

				dataSet.EnforceConstraints = false;
				dataAdapter.Fill (dataSet, "table1");
				dataAdapter.Fill (dataSet, "table2");

				//Should not throw an exception
				dataSet.EnforceConstraints = true;

				Assert.AreEqual (2, table1.Rows.Count, "#1");
				Assert.AreEqual (2, table2.Rows.Count, "#2");
			}
		}

		[Test]
		public void UpdateBatchSizeTest ()
		{
			adapter = new SqlDataAdapter();
			Assert.AreEqual (1, adapter.UpdateBatchSize, "#1 The default value should be 1");
			adapter.UpdateBatchSize = 3;
			Assert.AreEqual (3, adapter.UpdateBatchSize, "#2 The value should be 3 after setting the property UpdateBatchSize to 3");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void UpdateBatchSizeArgumentOutOfRangeTest ()
		{
			adapter = new SqlDataAdapter();
			adapter.UpdateBatchSize = -2;
		}

		int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}
	}

	[TestFixture]
	[Category ("sqlserver")]
	public class SqlDataAdapterInheritTest : DbDataAdapter
	{
		SqlConnection conn = null;

		[Test]
		public void FillDataAdapterTest ()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			try
			{
				DataTable dt = new DataTable();
				SqlCommand command = new SqlCommand ();
				command.CommandText = "Select * from employee;";
				command.Connection = conn;
				SelectCommand = command;
				Fill (dt, command.ExecuteReader ());
				Assert.AreEqual (4, dt.Rows.Count, "#1");
				Assert.AreEqual (6, dt.Columns.Count, "#2");
			} finally {
				DBHelper.ExecuteSimpleSP (conn, "sp_clean_employee_table");
				ConnectionManager.Instance.Sql.CloseConnection ();
			}
		}
	}
}
