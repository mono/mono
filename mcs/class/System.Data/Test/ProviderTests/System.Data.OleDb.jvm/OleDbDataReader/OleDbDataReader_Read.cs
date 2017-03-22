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


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbDataReader_Read : ADONetTesterClass 
	{
		OleDbConnection con;
		OleDbCommand cmd;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				cmd = new OleDbCommand("", con);
				con.Open();
				//prepare data
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null && con.State != ConnectionState.Closed)
				con.Close();
		}

		public static void Main()
		{
			OleDbDataReader_Read tc = new OleDbDataReader_Read();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataReader_Read");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void CommandBehaviorSingleRow ()
		{
			Exception exp = null;

			cmd.CommandText = "Select * From Employees";
			OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.SingleRow);

			try
			{
				BeginCase ("CommandBehaviorSingleRow");
				int i = 0;
				while (rdr.Read ())
					i++;
				Compare(i, 1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
		}

		[Test]
		public void run()
		{
			Exception exp = null;

			cmd.CommandText = "Select EmployeeID, LastName, FirstName, Title, BirthDate From Employees where EmployeeID in (100,200) order by EmployeeID asc";
			OleDbDataReader rdr = cmd.ExecuteReader();

			try
			{
				BeginCase("first row");
				bool read = rdr.Read();
				Compare(read, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("first row - value");
				object obj = rdr.GetValue(0);
				Compare(obj.ToString(), "100");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("Second row");
				bool read = rdr.Read();
				Compare(read, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Second row - value");
				object obj = rdr.GetValue(0);
				Compare(obj.ToString(), "200");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("End of data");
				bool read = rdr.Read();
				Compare(read, false);
				rdr.Close();
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Read return false");
				cmd.CommandText= "select * from Orders where OrderID=-909";
				rdr = cmd.ExecuteReader();
				Compare(rdr.Read(),false);
				rdr.Close();
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}
}