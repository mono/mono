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
using System.Data.OracleClient;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
[TestFixture]
public class OracleConnection_StateChange : GHTBase
{
	public static void Main()
	{
		OracleConnection_StateChange tc = new OracleConnection_StateChange();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleConnection_StateChange");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}


	bool blnEventRaised = false;
	ConnectionState OriginalState,CurrentState;

	[Test]
	public void run()
	{
		
		Exception exp = null;

		OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
        
		// ----------- reserved for future versions of the product ---------------
		//Broken	The connection to the data source is broken. This can occur only after the connection has been opened. A connection in this state may be closed and then re-opened. (This value is reserved for future versions of the product).
		//Connecting  The connection object is connecting to the data source. (This value is reserved for future versions of the product.) 2 
		//Executing The connection object is executing a command. (This value is reserved for future versions of the product.) 4 
		//Fetching  The connection object is retrieving data. (This value is reserved for future versions of the product.) 8 

		//-------------- checking only the following: ----------------
		//Closed  The connection is closed.  
		//Open  The connection is open. 


		//add event handler
		con.StateChange +=new StateChangeEventHandler(con_StateChange);

		con.Open();
		try
		{
			BeginCase("ConnectionState Closed");
			Compare(blnEventRaised,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("OriginalState Closed");
			Compare(OriginalState,ConnectionState.Closed );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("CurrentState Open");
			Compare(CurrentState,ConnectionState.Open );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		blnEventRaised = false;
		con.Close();
		try
		{
			BeginCase("ConnectionState Open");
			Compare(blnEventRaised,true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("OriginalState Open");
			Compare(OriginalState,ConnectionState.Open );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("CurrentState Close");
			Compare(CurrentState,ConnectionState.Closed  );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		if (con.State == ConnectionState.Open) con.Close();
	}

	void con_StateChange(Object sender, StateChangeEventArgs e)
	{
		CurrentState = e.CurrentState ;
		OriginalState = e.OriginalState ;
		blnEventRaised = true;


	}

	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}