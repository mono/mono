//
// SqlParameterTest.cs - NUnit Test Cases for testing the
//                          SqlParameter class
// Author:
//      Senganal T (tsenganal@novell.com)
//      Amit Biswas (amit@amitbiswas.com)
//      Veerapuram Varadhan  (vvaradhan@novell.com)
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
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	public class SqlParameterTest
	{
		SqlConnection conn;
		SqlCommand cmd;
		SqlDataReader rdr;
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[TearDown]
		public void TearDown ()
		{
			if (cmd != null)
				cmd.Dispose ();
			if (rdr != null)
				rdr.Close ();
			ConnectionManager.Instance.Close ();
		}

		[Test] // bug #324840
		[Category("NotWorking")]
		public void ParameterSizeTest ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("Hangs on SQL Server 7.0");

			string longstring = new String('x', 20480);
			SqlParameter prm;
			cmd = new SqlCommand ("create table #text1 (ID int not null, Val1 ntext)", conn);
			cmd.ExecuteNonQuery ();
			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			prm.SqlDbType = SqlDbType.NText; // Comment and enjoy the truncation
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#1");

			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			//prm.SqlDbType = SqlDbType.NText;
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#2");

			cmd.CommandText = "INSERT INTO #text1(ID,Val1) VALUES (@ID,@Val1)";
			prm = new SqlParameter ();
			prm.ParameterName = "@ID";
			prm.Value = 1;
			cmd.Parameters.Add (prm);

			prm = new SqlParameter ();
			prm.ParameterName = "@Val1";
			prm.Value = longstring;
			prm.SqlDbType = SqlDbType.VarChar;
			cmd.Parameters.Add (prm);
			cmd.ExecuteNonQuery ();
			cmd = new SqlCommand ("select datalength(Val1) from #text1", conn);
			Assert.AreEqual (20480 * 2, cmd.ExecuteScalar (), "#3");
			cmd = new SqlCommand ("drop table #text1", conn);
			cmd.ExecuteNonQuery ();
			conn.Close ();
		}

		[Test] // bug #382635
		[Category("NotWorking")]
		public void ParameterSize_compatibility_Test ()
		{
			string longstring = "abcdefghijklmnopqrstuvwxyz";

			cmd = new SqlCommand ("create table #bug382635 (description varchar(20))", conn);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = 
					"CREATE PROCEDURE #sp_bug382635 (@Desc varchar(20)) "
					+ "AS " + Environment.NewLine 
					+ "BEGIN" + Environment.NewLine 
					+ "UPDATE #bug382635 SET description = @Desc" + Environment.NewLine
					+ "END";
			cmd.CommandType = CommandType.Text;
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "INSERT INTO #bug382635 " +
					  "(description) VALUES ('Verifies bug #382635')";
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "#sp_bug382635";
			cmd.CommandType = CommandType.StoredProcedure;

			SqlParameter p1 = new SqlParameter ("@Desc", SqlDbType.NVarChar, 15);
			p1.Value = longstring;
			Assert.AreEqual (longstring, p1.Value);
			cmd.Parameters.Add (p1);
			cmd.ExecuteNonQuery ();

			// Test for truncation
			SqlCommand selectCmd = new SqlCommand ("SELECT DATALENGTH(description), description from #bug382635", conn);

			rdr = selectCmd.ExecuteReader ();
			Assert.IsTrue (rdr.Read (), "#A1");
			Assert.AreEqual (15, rdr.GetValue (0), "#A2");
			Assert.AreEqual (longstring.Substring (0, 15), rdr.GetValue (1), "#A3");
			Assert.AreEqual (longstring, p1.Value, "#A4");
			rdr.Close ();

			// Test to ensure truncation is not done in the Value getter/setter
			p1.Size = 12;
			p1.Value = longstring.Substring (0, 22);
			p1.Size = 14;
			cmd.ExecuteNonQuery ();

			rdr = selectCmd.ExecuteReader ();
			Assert.IsTrue (rdr.Read (), "#B1");
			Assert.AreEqual (14, rdr.GetValue (0), "#B2");
			Assert.AreEqual (longstring.Substring (0, 14), rdr.GetValue (1), "#B3");
			Assert.AreEqual (longstring.Substring (0, 22), p1.Value, "#B4");
			rdr.Close ();

			// Size exceeds size of value
			p1.Size = 40;
			cmd.ExecuteNonQuery ();

			rdr = selectCmd.ExecuteReader ();
			Assert.IsTrue (rdr.Read (), "#C1");
			Assert.AreEqual (14, rdr.GetValue (0), "#C2");
			Assert.AreEqual (longstring.Substring (0, 14), rdr.GetValue (1), "#C3");
			rdr.Close ();
		}

		[Test]
		public void ConversionToSqlTypeInvalid ()
		{
			string insert_data = "insert into datetime_family (id, type_datetime) values (6000, @type_datetime)";
			string delete_data = "delete from datetime_family where id = 6000";

			object [] values = new object [] {
				5,
				true,
				40L,
				"invalid date",
				};

			try {
				for (int i = 0; i < values.Length; i++) {
					object value = values [i];

					cmd = conn.CreateCommand ();
					cmd.CommandText = insert_data;
					SqlParameter param = cmd.Parameters.Add ("@type_datetime", SqlDbType.DateTime);
					param.Value = value;
					cmd.Prepare ();

					try {
						cmd.ExecuteNonQuery ();
						Assert.Fail ("#1:" + i);
					} catch (InvalidCastException) {
						if (value is string)
							Assert.Fail ("#2");
					} catch (FormatException) {
						if (!(value is string))
							Assert.Fail ("#3");
					}
				}
			} finally {
				DBHelper.ExecuteNonQuery (conn, delete_data);
			}
		}

		[Test] // bug #382589
		public void DecimalMaxAsParamValueTest ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("Maximum precision is 28.");

			string create_sp = "CREATE PROCEDURE #sp_bug382539 (@decmax decimal(29,0) OUT)"
				+ "AS " + Environment.NewLine
				+ "BEGIN" + Environment.NewLine
				+ "SET @decmax = 102.34" + Environment.NewLine
				+ "END";

			cmd = new SqlCommand (create_sp, conn);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "[#sp_bug382539]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decmax", Decimal.MaxValue);
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);

			Assert.AreEqual (Decimal.MaxValue, pValue.Value, "Parameter initialization value mismatch");
			cmd.ExecuteNonQuery();

			Assert.AreEqual (102m, pValue.Value, "Parameter value mismatch");
		}

		[Test] // bug #382589
		public void DecimalMinAsParamValueTest ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("Maximum precision is 28.");

			string create_sp = "CREATE PROCEDURE #sp_bug382539 (@decmax decimal(29,0) OUT)"
				+ "AS " + Environment.NewLine
				+ "BEGIN" + Environment.NewLine
				+ "SET @decmax = 102.34" + Environment.NewLine
				+ "END";

			cmd = new SqlCommand (create_sp, conn);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "[#sp_bug382539]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decmax", Decimal.MinValue);
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);

			Assert.AreEqual (Decimal.MinValue, pValue.Value, "Parameter initialization value mismatch");
			cmd.ExecuteNonQuery();

			Assert.AreEqual (102m, pValue.Value, "Parameter value mismatch");
		}

		[Test] // bug #382589
		public void DecimalMaxAsParamValueExceptionTest ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("Maximum precision is 28.");

			string create_sp = "CREATE PROCEDURE #sp_bug382539 (@decmax decimal(29,10) OUT)"
				+ "AS " + Environment.NewLine
				+ "BEGIN" + Environment.NewLine
				+ "SET @decmax = 102.36" + Environment.NewLine
				+ "END";

			cmd = new SqlCommand (create_sp, conn);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "[#sp_bug382539]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decmax", Decimal.MaxValue);
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Error converting data type numeric to decimal
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 16, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (8114, ex.Number, "#6");
				Assert.AreEqual ((byte) 5, ex.State, "#7");
			}
		}

		[Test] // bug# 382589
		public void DecimalMinAsParamValueExceptionTest ()
		{
			if (ClientVersion == 7)
				Assert.Ignore ("Maximum precision is 28.");

			string create_sp = "CREATE PROCEDURE #sp_bug382539 (@decmax decimal(29,10) OUT)"
				+ "AS " + Environment.NewLine
				+ "BEGIN" + Environment.NewLine
				+ "SET @decmax = 102.36" + Environment.NewLine
				+ "END";

			cmd = new SqlCommand (create_sp, conn);
			cmd.ExecuteNonQuery ();

			cmd.CommandText = "[#sp_bug382539]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decmax", Decimal.MinValue);
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);
			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#1");
			} catch (SqlException ex) {
				// Error converting data type numeric to decimal
				Assert.AreEqual (typeof (SqlException), ex.GetType (), "#2");
				Assert.AreEqual ((byte) 16, ex.Class, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
				Assert.AreEqual (8114, ex.Number, "#6");
				Assert.AreEqual ((byte) 5, ex.State, "#7");
			}
		}

		[Test] // bug #526794
		public void ZeroLengthString ()
		{
			cmd = new SqlCommand ("create table #bug526794 (name varchar(20) NULL)", conn);
			cmd.ExecuteNonQuery ();

			SqlParameter param;

			param = new SqlParameter ("@name", SqlDbType.VarChar);
			param.Value = string.Empty;

			cmd = new SqlCommand ("insert into #bug526794 values (@name)", conn);
			cmd.Parameters.Add (param);
			cmd.ExecuteNonQuery ();

			cmd = new SqlCommand ("select * from #bug526794", conn);
			rdr = cmd.ExecuteReader ();
			Assert.IsTrue (rdr.Read (), "#A1");
			Assert.AreEqual (string.Empty, rdr.GetValue (0), "#A2");
			rdr.Close ();

			param = new SqlParameter ("@name", SqlDbType.Int);
			param.Value = string.Empty;

			cmd = new SqlCommand ("insert into #bug526794 values (@name)", conn);
			cmd.Parameters.Add (param);

			try {
				cmd.ExecuteNonQuery ();
				Assert.Fail ("#B1");
			} catch (FormatException ex) {
				// Failed to convert parameter value from a String to a Int32
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf (typeof (string).Name) != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf (typeof (int).Name) != -1, "#B5");

				// Input string was not in a correct format
				Exception inner = ex.InnerException;
				Assert.IsNotNull (inner, "#B6");
				Assert.AreEqual (typeof (FormatException), inner.GetType (), "#B7");
				Assert.IsNull (inner.InnerException, "#B8");
				Assert.IsNotNull (inner.Message, "#B9");
			}
		}

		[Test] // bug #595918
		public void DecimalDefaultScaleTest ()
		{
			string create_tbl = "CREATE TABLE #decimalScaleCheck (decsclcheck DECIMAL (19, 5) null)";
			string create_sp = "CREATE PROCEDURE #sp_bug595918(@decsclcheck decimal(19,5) OUT)"
			        + "AS " + Environment.NewLine
			        + "BEGIN" + Environment.NewLine
			        + "INSERT INTO #decimalScaleCheck values (@decsclcheck)" + Environment.NewLine
			        + "SELECT @decsclcheck=decsclcheck from #decimalScaleCheck" + Environment.NewLine
			        + "END";
			
			cmd = new SqlCommand (create_tbl, conn);
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = create_sp;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "[#sp_bug595918]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decsclcheck", SqlDbType.Decimal);
			pValue.Value = 128.425;
			pValue.Precision = 19;
			pValue.Scale = 3;
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);
			cmd.ExecuteNonQuery();

			Assert.AreEqual (128.425, pValue.Value, "Stored decimal value is incorrect - DS - Bug#595918");
		}
		
		[Test] // bug #595918
		public void DecimalGreaterScaleTest ()
		{
			string create_tbl = "CREATE TABLE #decimalScaleCheck (decsclcheck DECIMAL (19, 5) null)";
			string create_sp = "CREATE PROCEDURE #sp_bug595918(@decsclcheck decimal(19,5) OUT)"
			        + "AS " + Environment.NewLine
			        + "BEGIN" + Environment.NewLine
			        + "INSERT INTO #decimalScaleCheck values (@decsclcheck)" + Environment.NewLine
			        + "SELECT @decsclcheck=decsclcheck from #decimalScaleCheck" + Environment.NewLine
			        + "END";
			
			cmd = new SqlCommand (create_tbl, conn);
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = create_sp;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "[#sp_bug595918]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decsclcheck", SqlDbType.Decimal);
			pValue.Value = 128.425;
			pValue.Precision = 19;
			pValue.Scale = 5;
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);
			cmd.ExecuteNonQuery();

			Assert.AreEqual (128.42500, pValue.Value, "Stored decimal value is incorrect - GS - Bug#595918");
		}

		[Test] // bug #595918
		public void DecimalLesserScaleTest ()
		{
			string create_tbl = "CREATE TABLE #decimalScaleCheck (decsclcheck DECIMAL (19, 5) null)";
			string create_sp = "CREATE PROCEDURE #sp_bug595918(@decsclcheck decimal(19,5) OUT)"
			        + "AS " + Environment.NewLine
			        + "BEGIN" + Environment.NewLine
			        + "INSERT INTO #decimalScaleCheck values (@decsclcheck)" + Environment.NewLine
			        + "SELECT @decsclcheck=decsclcheck from #decimalScaleCheck" + Environment.NewLine
			        + "END";
			
			cmd = new SqlCommand (create_tbl, conn);
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = create_sp;
			cmd.ExecuteNonQuery ();
			
			cmd.CommandText = "[#sp_bug595918]";
			cmd.CommandType = CommandType.StoredProcedure;
			SqlParameter pValue = new SqlParameter("@decsclcheck", SqlDbType.Decimal);
			pValue.Value = 128.425;
			pValue.Precision = 19;
			pValue.Scale = 2;
			pValue.Direction = ParameterDirection.InputOutput;
			cmd.Parameters.Add(pValue);
			cmd.ExecuteNonQuery();

			Assert.AreEqual (128.42, pValue.Value, "Stored decimal value is incorrect - LS - Bug#595918");
		}

		int ClientVersion {
			get {
				return (engine.ClientVersion);
			}
		}
	}
}
