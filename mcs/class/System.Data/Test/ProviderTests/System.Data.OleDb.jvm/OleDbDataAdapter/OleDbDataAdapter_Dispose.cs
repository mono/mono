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
using System.Data.OleDb ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbDataAdapter_Dispose : GHTBase
{
	public static void Main()
	{
		OleDbDataAdapter_Dispose tc = new OleDbDataAdapter_Dispose();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDbDataAdapter_Dispose");
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
		Exception exp = null;

		DataSet ds = new DataSet();
		string sqlSelect = "Select * from Customers where CustomerID = 'ABC'";
		OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		
		OleDbCommand cmdSelect = new OleDbCommand(sqlSelect,con);
		OleDbCommand cmdDelete = new OleDbCommand("",con);
		OleDbCommand cmdUpdate = new OleDbCommand("",con);
		OleDbCommand cmdInsert = null;

		con.Open();

		OleDbDataAdapter da = new OleDbDataAdapter(cmdSelect);
		da.DeleteCommand = cmdDelete;
		da.UpdateCommand = cmdUpdate;
		da.InsertCommand = new OleDbCommand("",con);

		cmdInsert =	da.InsertCommand;

		da.Fill(ds);

		try
		{
			BeginCase("Dispose - check DataAdapter select command");
			da.Dispose();
			Compare(da.SelectCommand == null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check DataAdapter insert command");
			Compare(da.InsertCommand == null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check DataAdapter Delete Command");
			Compare(da.DeleteCommand == null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check DataAdapter update command");
			Compare(da.UpdateCommand == null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}



		try
		{
			BeginCase("Dispose - check select command object");
			Compare(cmdSelect != null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check delete command object");
			Compare(cmdDelete  != null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check insert command object");
			Compare(cmdInsert != null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check Update command object");
			Compare(cmdUpdate != null, true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check cmdSelect.connection object");
			Compare(cmdSelect.Connection != null ,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check cmdDelete.connection object");
			Compare(cmdDelete.Connection != null ,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check cmdUpdate.connection object");
			Compare(cmdUpdate.Connection != null ,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check cmdInsert.connection object");
			Compare(cmdInsert.Connection != null ,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

		try
		{
			BeginCase("Dispose - check command connection state");
			da.Dispose();
			Compare(cmdSelect.Connection.State ,ConnectionState.Open);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp);exp = null;}

	}
}
}