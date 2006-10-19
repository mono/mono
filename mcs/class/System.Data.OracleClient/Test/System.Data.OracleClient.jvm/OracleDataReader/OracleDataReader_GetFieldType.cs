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
	public class OracleDataReader_GetFieldType : ADONetTesterClass
	{
		OracleConnection con;
		OracleCommand cmd;
		OracleDataReader rdr;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				con.Open();
				cmd = new OracleCommand("Select OrderID, CustomerID, OrderDate From Orders", con);
				rdr = cmd.ExecuteReader();
				rdr.Read();

			}
			catch(Exception ex){exp = ex;}
			finally	{EndCase(exp);}
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null && con.State == ConnectionState.Open) con.Close();
		}

		public static void Main()
		{
			OracleDataReader_GetFieldType tc = new OracleDataReader_GetFieldType();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataReader_GetFieldType");
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

			try
			{
				BeginCase("check type string");
				Compare(rdr.GetFieldType(1).FullName,typeof(string).FullName );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check type date");
				Compare(rdr.GetFieldType(2).FullName,typeof(DateTime).FullName );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
		}
	}
}