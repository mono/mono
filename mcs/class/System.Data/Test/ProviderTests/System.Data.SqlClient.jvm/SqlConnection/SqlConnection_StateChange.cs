using System;
using System.Data;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlConnection_StateChange : GHTBase
	{

		public static void Main()
		{
			SqlConnection_StateChange tc = new SqlConnection_StateChange();
			Exception exp = null;
			try
			{
				tc.BeginTest("SqlConnection_StateChange");
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
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}
			
			Exception exp = null;

			SqlConnection  con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
	        
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