// 
// Copyright (c) 2006 Mainsoft Co.
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
using System.Text;
using System.Data;
using System.Data.OleDb;

using MonoTests.System.Data.Utils;

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbDataReader_GetDateTime : ADONetTesterClass
	{
		private OleDbConnection con;
		private OleDbCommand cmd;
		private OleDbDataReader rdr;
		Exception exp;

		[SetUp]

		[TearDown]
		public void TearDown()
		{
			if (con != null && con.State == ConnectionState.Open) con.Close();
		}

		public static void Main()
		{
			OleDbDataReader_GetDateTime tc = new OleDbDataReader_GetDateTime();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataReader_GetDateTime");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{
			SimpleValue();
			MinDate();
		}

		[Test]
		public void SimpleValue()
		{
			try
			{
				BeginCase("check simple value");
				//prepare data
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

				con.Open();
				cmd = new OleDbCommand("Select BirthDate From Employees where EmployeeID = 100", con);
				rdr = cmd.ExecuteReader();
				rdr.Read();
				DateTime dt = rdr.GetDateTime(0); //will be 1988-May-31 15:33:44
				Compare(dt,new DateTime(1988,5,31,15,33,44,00));
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
				{
					rdr.Close();
				}
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
				EndCase(exp); 
				exp = null;
			}
		}

		[Test]
		public void MinDate()
		{
			BeginCase("Test Min date.");
			exp = null;
			string[] dateColumns;
			DateTime[] expectedValues;
			
			InitMinDates(out dateColumns, out expectedValues);
			try
			{
				con = new OleDbConnection(ConnectedDataProvider.ConnectionString);
				cmd = new OleDbCommand();
				cmd.Connection = con;
				cmd.CommandText = BuildMinDateTimeSelectSql(dateColumns);
				con.Open();
				rdr = cmd.ExecuteReader();
				Compare(true, rdr.HasRows);
				bool b = rdr.Read();
				for (int i=0; i<dateColumns.Length && i<expectedValues.Length; i++)
				{
					int j=-1;
					j = rdr.GetOrdinal(dateColumns[i]);
					//DateTime result = rdr.GetDateTime(j);
					object result = rdr.GetValue(j);
					Compare(result, expectedValues[i]);
				}
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (rdr != null && !rdr.IsClosed)
				{
					rdr.Close();
				}
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
				EndCase(exp);
			}
		}
		
		private void InitMinDates(out string[] columns, out DateTime[] values)
		{
			switch(ConnectedDataProvider.GetDbType())
			{
				case DataBaseServer.SQLServer:
				case DataBaseServer.Sybase:
					columns = new string[] {"T_DATETIME", "T_SMALLDATETIME"};
					values = new DateTime[] {new DateTime(1753, 01, 01,00, 00, 00),		//	01/01/1753 00:00:00.000
																  new DateTime(1900, 01, 01,00, 00, 00)};		//	01/01/1900 00:00
					break;
				case DataBaseServer.Oracle:
					columns = new string[] {"T_DATE"};
					values = new DateTime[] {new DateTime(0001, 01, 01,00, 00, 00)}; //01-Jan-0001 12:00:00 AM
					break;
				case DataBaseServer.PostgreSQL:
					columns = new string[] {"T_TIMESTAMP"};
					values = new DateTime[] {new DateTime(1753, 01, 01,00, 00, 00)}; //01-Jan-1753 12:00:00 AM
					break;
				case DataBaseServer.DB2:
					columns = new string[] {"T_DATE", "T_TIMESTAMP"};
					values = new DateTime[] {new DateTime(0001, 01, 01),		//	1/1/0001
																  new DateTime(0001, 01, 01, 00,00,00,0)};	//	1/1/0001 00:00:00.000
					break;
				default:
					throw new ApplicationException(string.Format("GHT ERROR: Unknown DB server [{0}].",ConnectedDataProvider.GetDbType()));
			}
		}

		private string BuildMinDateTimeSelectSql(string[] dateColumns)
		{
			StringBuilder sqlBuilder = new StringBuilder();
			sqlBuilder.Append("SELECT ");
			foreach(string col in dateColumns)
			{
				sqlBuilder.Append(col + ", ");
			}
			sqlBuilder.Remove(sqlBuilder.Length - 2, 2);
			sqlBuilder.Append(" FROM TYPES_EXTENDED WHERE ID='MIN'");
			return sqlBuilder.ToString();
		}
	}
}
