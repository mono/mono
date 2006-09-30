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
using System.Data.OracleClient ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleConnection_BeginTransaction : ADONetTesterClass
	{
		Exception exp = null;
		OracleConnection con = null;
		OracleTransaction tran = null;

		public static void Main()
		{
			OracleConnection_BeginTransaction tc = new OracleConnection_BeginTransaction();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleConnection_BeginTransaction");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[SetUp]
		public void SetUp() {
			con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();
		}

		[TearDown]
		public void TearDown() {
			if (con != null && con.State == ConnectionState.Open) con.Close();
		}

		[Test]
#if JAVA
		[Category("NotWorking")]
#endif
		public void TestBeginTransactionChaos() {

			DataBaseServer dbType = ConnectedDataProvider.GetDbType(con);
			// not supported on DB2 and Oracle and Sybase
			if (dbType != DataBaseServer.Oracle && dbType != DataBaseServer.DB2 && dbType != DataBaseServer.Sybase) {
				con.Close();
				con.Open();
				try {
					BeginCase("BeginTransaction - IsolationLevel Chaos");
					tran = con.BeginTransaction(IsolationLevel.Chaos);
					Compare(tran == null, false);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
			}
			/*	not supported by MSSQL,DB2,Oracle
				con.Close();
				con.Open();
				try
				{
					BeginCase("BeginTransaction - IsolationLevel Unspecified");
					tran = con.BeginTransaction(IsolationLevel.Unspecified );
					Compare(tran == null, false);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
			*/
			
		}

		[Test]
		public void run()
		{
			try
			{
				BeginCase("BeginTransaction");
				tran = con.BeginTransaction();
				Compare(tran == null, false);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			con.Close();
			con.Open();
			try
			{
				BeginCase("BeginTransaction - IsolationLevel ReadCommitted");
				tran = con.BeginTransaction(IsolationLevel.ReadCommitted);
				Compare(tran == null, false);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		
			DataBaseServer dbType = ConnectedDataProvider.GetDbType(con);

			//Not supported by JDBC driver for oracle
			if (dbType != DataBaseServer.Oracle) 
			{
				con.Close();
				con.Open();
				try
				{
					BeginCase("BeginTransaction - IsolationLevel ReadUncommitted");
					tran = con.BeginTransaction(IsolationLevel.ReadUncommitted );
					Compare(tran == null, false);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}

				con.Close();
				con.Open();
				try
				{
					BeginCase("BeginTransaction - IsolationLevel RepeatableRead");
					tran = con.BeginTransaction(IsolationLevel.RepeatableRead);
					Compare(tran == null, false);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
				con.Close();
				con.Open();
				try
				{
					BeginCase("BeginTransaction - IsolationLevel Serializable");
					tran = con.BeginTransaction(IsolationLevel.Serializable );
					Compare(tran == null, false);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
			}
		}
	}
}