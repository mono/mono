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
using System.Data;
using System.Data.OleDb;

using MonoTests.System.Data.Utils;

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbDataReader_GetGuiid_I : ADONetTesterClass 
	{

		private const string GUID_COLUMN_NAME = "T_UNIQUEIDENTIFIER";
		private const string GUID_TABLE_NAME = ConnectedDataProvider.SPECIFIC_TYPES_TABLE_NAME;
		private const string TEST_GUID_STRING = "239A3C5E-8D41-11D1-B675-00C04FA3C554";

		public static void Main()
		{
			OleDbDataReader_GetGuiid_I tc = new OleDbDataReader_GetGuiid_I();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataReader_GetGuiid_I");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		
		public void run()
		{
			
		
			TestUsingSQLTextOnly();
			TestUsingParametersArray();
		}

		[Test]
		public void TestUsingSQLTextOnly()
		{
			//Only apply to MSSQL
			if ( (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)) {
				return;
			}

			Exception exp = null;
			OleDbDataReader rdr = null;
			OleDbConnection con = null;
			OleDbCommand cmdDelete = null;
			try
			{
				BeginCase("Test using SQL text only.");
				string rowId = "43973_" + TestCaseNumber.ToString();
				string insertText = string.Format("INSERT INTO {0} (ID, {1}) VALUES ('{2}', '{{{3}}}')", GUID_TABLE_NAME, GUID_COLUMN_NAME, rowId, TEST_GUID_STRING);
				string selectText = string.Format("SELECT {0} FROM {1} WHERE ID='{2}'", GUID_COLUMN_NAME, GUID_TABLE_NAME, rowId);
				string deleteText = string.Format("DELETE FROM {0} WHERE ID='{1}'", GUID_TABLE_NAME, rowId);
				con = new OleDbConnection(ConnectedDataProvider.ConnectionString);
				OleDbCommand cmdInsert = new OleDbCommand(insertText, con);
				OleDbCommand cmdSelect = new OleDbCommand(selectText, con);
				cmdDelete = new OleDbCommand(deleteText, con);

				con.Open();
				cmdInsert.ExecuteNonQuery();
				rdr = cmdSelect.ExecuteReader();
				rdr.Read();
				Guid  guidValue = rdr.GetGuid (0);
				Guid origGuid = new Guid(TEST_GUID_STRING);
				Compare(guidValue, origGuid);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if ( (rdr != null) && (!rdr.IsClosed) )
				{
					rdr.Close();
				}
				if (cmdDelete != null)
				{
					cmdDelete.ExecuteNonQuery();
				}
				if ( (con != null) && (con.State != ConnectionState.Closed) )
				{
					con.Close();
				}
				EndCase(exp);
				exp = null;
			}
		}

		[Test]
		public void TestUsingParametersArray()
		{
			//Only apply to MSSQL
			if ( (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer))
			{
				return;
			}
			Exception exp = null;
			OleDbDataReader rdr = null;
			OleDbConnection con = null;
			DbTypeParametersCollection row = new DbTypeParametersCollection(GUID_TABLE_NAME);
			string rowId = string.Empty;
			try
			{
				BeginCase("Test using parameters array");
				rowId = "43973_" + TestCaseNumber.ToString();
				row.Add("UNIQUEIDENTIFIER", new Guid(TEST_GUID_STRING));
				row.ExecuteInsert(rowId);
				row.ExecuteSelectReader(rowId, out rdr, out con);
				rdr.Read();
				Guid  guidValue = rdr.GetGuid (0);
				Compare(guidValue, row[GUID_COLUMN_NAME].Value);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if ( (rdr != null) && (!rdr.IsClosed) )
				{
					rdr.Close();
				}
				if (rowId != String.Empty)
				{
					row.ExecuteDelete(rowId);
				}
				if ( (con != null) && (con.State != ConnectionState.Closed) )
				{
					con.Close();
				}
				EndCase(exp);
				exp = null;
				
			}
		}
	}
	

}