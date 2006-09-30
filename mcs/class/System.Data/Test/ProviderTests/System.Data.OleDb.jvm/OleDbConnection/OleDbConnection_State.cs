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
public class OleDbConnection_State : GHTBase
{
	public static void Main()
	{
		OleDbConnection_State tc = new OleDbConnection_State();
		Exception exp = null;
		try
		{
			tc.BeginTest("NoName");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		Exception exp = null;

		OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
        
		// ----------- reserved for future versions of the product ---------------
		//Broken	The connection to the data source is broken. This can occur only after the connection has been opened. A connection in this state may be closed and then re-opened. (This value is reserved for future versions of the product).
		//Connecting  The connection object is connecting to the data source. (This value is reserved for future versions of the product.) 2 
		//Executing The connection object is executing a command. (This value is reserved for future versions of the product.) 4 
		//Fetching  The connection object is retrieving data. (This value is reserved for future versions of the product.) 8 

		//-------------- checking only the following: ----------------
		//Closed  The connection is closed.  
		//Open  The connection is open. 

		try
		{
			BeginCase("ConnectionState Closed");
			Compare(con.State ,ConnectionState.Closed );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("ConnectionState Open");
			con.Open();
			Compare(con.State ,ConnectionState.Open  );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		con.Close();

		try
		{
			BeginCase("ConnectionState Closed after open");
			Compare(con.State ,ConnectionState.Closed );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}