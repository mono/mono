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
using System.Data.Common;
using System.Data.OleDb;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbDataAdapter_TableMappings : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbDataAdapter_TableMappings tc = new OleDbDataAdapter_TableMappings();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataAdapter_TableMappings");
				tc.run();
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				tc.EndTest(exp);
			}
		}


		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}


		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

		[Test]
		public void run()
		{ 
			// open a transaction for PostgreSQL
			OleDbDataAdapter oleDBda = new OleDbDataAdapter();
			oleDBda.SelectCommand = new OleDbCommand("",new OleDbConnection());

			DataAdapter_TableMappings((DbDataAdapter)oleDBda);
		}
		
		public void DataAdapter_TableMappings(DbDataAdapter dbDA)
		{
			Exception exp = null;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			IConn.Open();
			ICmd.Transaction = IConn.BeginTransaction();

			//--- Default value ---
        
			try
			{
				BeginCase("TableMappings Default value");
				Compare(dbDA.TableMappings.Count ,0);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//init dataset
			DataSet ds = new DataSet();
			ds.Tables.Add("CustTable");
			ds.Tables.Add("EmplTable");

			//load the tables
			dbDA.TableMappings.Add("Table","CustTable");
			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";
			dbDA.Fill(ds);

			dbDA.TableMappings.Clear();
			dbDA.TableMappings.Add("Table","EmplTable");
			ICmd.CommandText = " SELECT EmployeeID, LastName, FirstName, Title FROM Employees where EmployeeID > 0";
			dbDA.Fill(ds);

			try
			{
				BeginCase("TableMappings.Count");
				Compare(ds.Tables.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Customers rows count");
				Compare(ds.Tables["CustTable"].Rows.Count > 0,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Employees rows count");
				Compare(ds.Tables["EmplTable"].Rows.Count > 0,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Employees Columns");
				Compare(ds.Tables["EmplTable"].Columns.IndexOf("EmployeeID") >= 0,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try
			{
				BeginCase("Customer Columns");
				Compare(ds.Tables["CustTable"].Columns.IndexOf("CustomerID") >= 0,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//Another checking 
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.DB2) {

			BeginCase("Check table mappings with stored procedure of another owner");
			try
			{
				DataSet ds1 = new DataSet();
				ds1.Tables.Add("EmplTable");
				dbDA.TableMappings.Clear();
				dbDA.TableMappings.Add("Table","EmplTable");
				ICmd.CommandType = CommandType.StoredProcedure;
	
				switch (ConnectedDataProvider.GetDbType(ConnectedDataProvider.ConnectionString))
				{
					case MonoTests.System.Data.Utils.DataBaseServer.Oracle:
						// On ORACLE, the Scheam is the user is the owner.
						ICmd.CommandText = "GHTDB.GH_DUMMY";
						break;
					case MonoTests.System.Data.Utils.DataBaseServer.PostgreSQL:
						ICmd.CommandText = "public.gh_dummy";
						break;
					default:
						ICmd.CommandText = "mainsoft.GH_DUMMY";
						break;
				}

				// investigate the SP parameters
				OleDbCommandBuilder.DeriveParameters((OleDbCommand)ICmd);
				Compare(2,ICmd.Parameters.Count);

				// add one numeric parameter, and Fill
				ICmd.Parameters.Clear();
				ICmd.Parameters.Add(new OleDbParameter("EmployeeIDPrm",1));
				dbDA.Fill(ds1);
				Compare(ds1.Tables.Count,1);
				Compare(ds1.Tables[0].Rows.Count >0,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			}
		

			// 
			((IDbDataAdapter)dbDA).SelectCommand.Transaction.Commit();

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}
	}
}