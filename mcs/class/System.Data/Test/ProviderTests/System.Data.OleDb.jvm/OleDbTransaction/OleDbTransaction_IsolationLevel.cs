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
	public class OleDbTransaction_IsolationLevel : ADONetTesterClass
	{
		public static void Main()
		{
			OleDbTransaction_IsolationLevel tc = new OleDbTransaction_IsolationLevel();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbTransaction_IsolationLevel");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
#if JAVA
		[Category("NotWorking")]
#endif
		public void IsolationLevelChaos() {
			Exception exp = null;


			MonoTests.System.Data.Utils.DataBaseServer dbServer = ConnectedDataProvider.GetDbType(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			OleDbTransaction txn = null;

			//supported only in SQLServer
			if (dbServer == MonoTests.System.Data.Utils.DataBaseServer.SQLServer) {
				try {
					BeginCase("IsolationLevel = Chaos");
					con.Open();
					txn=con.BeginTransaction(IsolationLevel.Chaos);
					Compare(txn.IsolationLevel,IsolationLevel.Chaos);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}
			}
		}

		[Test]
		public void run()
		{
			Exception exp = null;


			MonoTests.System.Data.Utils.DataBaseServer dbServer = ConnectedDataProvider.GetDbType(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			OleDbConnection con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			OleDbTransaction txn = null;
		
			try
			{
				BeginCase("IsolationLevel = ReadCommitted");
				con.Open();
				txn=con.BeginTransaction();
				Compare(txn.IsolationLevel,IsolationLevel.ReadCommitted);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}

			//not supported in Oracle
			if (dbServer != MonoTests.System.Data.Utils.DataBaseServer.Oracle) 
			{
				try
				{
					BeginCase("IsolationLevel = ReadUncommitted");
					con.Open();
					txn=con.BeginTransaction(IsolationLevel.ReadUncommitted );
					Compare(txn.IsolationLevel,IsolationLevel.ReadUncommitted);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}
			}

			//not supported in Oracle
			if (dbServer != MonoTests.System.Data.Utils.DataBaseServer.Oracle) 
			{
				try
				{
					BeginCase("IsolationLevel = RepeatableRead");
					con.Open();
					txn=con.BeginTransaction(IsolationLevel.RepeatableRead);
					Compare(txn.IsolationLevel,IsolationLevel.RepeatableRead);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}
			}

			try
			{
				BeginCase("IsolationLevel = Serializable");
				con.Open();
				txn=con.BeginTransaction(IsolationLevel.Serializable);
				Compare(txn.IsolationLevel,IsolationLevel.Serializable);
				txn.Rollback();
				txn=con.BeginTransaction();
				txn.Rollback();
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}

			// not supported in DB2,MSSQL,Oracle,sybase and guess what... Postgres.
			if (dbServer != MonoTests.System.Data.Utils.DataBaseServer.DB2 
				&& dbServer != MonoTests.System.Data.Utils.DataBaseServer.SQLServer
				&& dbServer != MonoTests.System.Data.Utils.DataBaseServer.Oracle 
				&& dbServer != DataBaseServer.PostgreSQL
				&& dbServer != MonoTests.System.Data.Utils.DataBaseServer.Sybase ) 
			{
				try
				{
					BeginCase("IsolationLevel = Unspecified");
					con.Open();
					txn=con.BeginTransaction(IsolationLevel.Unspecified );
					Compare(txn.IsolationLevel,IsolationLevel.Unspecified );
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null; if (con.State == ConnectionState.Open) con.Close();}
			}



		}
	}
}