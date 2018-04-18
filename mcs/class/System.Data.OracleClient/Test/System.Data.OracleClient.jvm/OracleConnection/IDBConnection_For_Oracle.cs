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
	class IDBConnection_For_Oracle : GHTBase
	{
		string _ConnectionString = "";
		[SetUp]
		public void SetUp()
		{
			_ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
		}

		[TearDown]
		public void TearDown()
		{
		}

		public static void Main()
		{
			IDBConnection_For_Oracle tc = new IDBConnection_For_Oracle();
			Exception exp = null;
			try
			{
				tc.BeginTest("IDBConnection_For_Oracle");
				tc.SetUp();
				tc.run();
				tc.TearDown();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
			IDbConnection ICon = new OracleConnection();

			try
			{
				BeginCase("check IDbConnection is null");
				Compare(ICon != null, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check IDbConnection type");
				Compare(ICon.GetType().FullName ,typeof(OracleConnection).FullName);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			ICon = new OracleConnection(_ConnectionString);

			try
			{
				BeginCase("check IDbConnection connection string");
				Compare(ICon.ConnectionString ,_ConnectionString);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check IDbConnection ConnectionTimeout");
				Compare(ICon.ConnectionTimeout ,15);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check IDbConnection state - closed");
				Compare(ICon.State ,ConnectionState.Closed);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			ICon.Open();

			try
			{
				BeginCase("check IDbConnection - open");
				Compare(ICon.State ,ConnectionState.Open );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check IDbConnection CreateCommand");
				IDbCommand cmd = ICon.CreateCommand();
				Compare(cmd.GetType().FullName ,typeof(OracleCommand).FullName);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (ICon.State == ConnectionState.Open) ICon.Close();

		}
	}

}