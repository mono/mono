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
using System.Threading;
using System.Data;
using NUnit.Framework;
using MonoTests.System.Data.Utils;

using System.Data.OleDb;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbConnection_InfoMessage : GHTBase
	{
		private int errorCounter=0;
		public static void Main()
		{
			OleDbConnection_InfoMessage tc = new OleDbConnection_InfoMessage();
			Exception exp = null;
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("OleDbConnection_InfoMessage");
				tc.run();
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				// Every Test must End with EndTest
				tc.EndTest(exp);
			}
		}

		public void run()
		{
			Exception exp = null;

			// Start Sub Test
			try
			{
				test();
				
				
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				// Every Sub Test must end with EndCase
				EndCase(exp);
				exp = null;
			}
			// End Sub Test
		}

		[Test]
#if JAVA
		[Category("NotWorking")]
#endif
		public void test()
		{
			BeginCase("InfoMessage testing");
			OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();
			con.InfoMessage+=new OleDbInfoMessageEventHandler(con_InfoMessage);
			generateError(con);
			con.Close();
		}
		private void generateError(OleDbConnection con)
		{
			string errorString = string.Empty;
			OleDbCommand cmd = new OleDbCommand(string.Empty,con); 

			switch (ConnectedDataProvider.GetDbType(con))
			{
				case DataBaseServer.SQLServer:
				case DataBaseServer.Sybase:
				{
					cmd.CommandText  = "Raiserror ('A sample SQL informational message',10,1)";
					break;
				}
				case DataBaseServer.Oracle:
				case DataBaseServer.PostgreSQL:
				{
					cmd.CommandText = "GH_ERROR";
					//cmd.CommandText = "print 'This is a warning.'";
					//cmd.CommandText = "select   count(SUPPLIERID) from GHTDB.PRODUCTS";
					cmd.CommandType = CommandType.StoredProcedure;
					break;
				}

				case DataBaseServer.DB2:
				{
					cmd.CommandText = "SIGNAL SQLSTATE '99999' SET MESSAGE_TEXT ='Blah Blah';";
					break;
				}

				default:
				{
					throw new NotImplementedException(string.Format("GHT: Test is not implemented for {0}", ConnectedDataProvider.GetDbType(con))); 
				}
			}

			
			//cmd.CommandType = CommandType.StoredProcedure;
			
				cmd.ExecuteNonQuery();
		
		
		
//				cmd.CommandText = "TestInfoMessage";
//				cmd.CommandType = CommandType.StoredProcedure;

			
			if (errorCounter == 0)
			{
				Thread.Sleep(5000);	
			}
			Compare(errorCounter,1);
		}

		private void con_InfoMessage(object sender, OleDbInfoMessageEventArgs e)
		{
			
			foreach(OleDbError err in e.Errors)
			{
				errorCounter++;
				
			}

		}

		//Activate This Construntor to log All To Standard output
		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}

		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

		
	}
}