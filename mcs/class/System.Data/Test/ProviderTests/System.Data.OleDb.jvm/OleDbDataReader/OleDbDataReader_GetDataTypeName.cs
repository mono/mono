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
	public class OleDbDataReader_GetDataTypeName : ADONetTesterClass
	{
		OleDbConnection con;
		OleDbCommand cmd;
		OleDbDataReader rdr;
		string typeName;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				cmd = new OleDbCommand("Select EmployeeID From Employees Where FirstName = 'Oved'",  con);
				cmd.CommandType = CommandType.Text;
				con.Open();

				rdr = cmd.ExecuteReader();
				rdr.Read();

				typeName = rdr.GetDataTypeName(0);
				con.Close();
			}
			catch(Exception ex){exp = ex;}
			finally	{EndCase(exp);}
		}

		[TearDown]
		public void TearDown()
		{
		}

		public static void Main()
		{
			OleDbDataReader_GetDataTypeName tc = new OleDbDataReader_GetDataTypeName();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataReader_GetDataTypeName");
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

			//make database specific test
			switch (ConnectedDataProvider.GetDbType(con))
			{
				case DataBaseServer.SQLServer:
					try
					{
						BeginCase("check type name");
						Compare(typeName == "DBTYPE_I4" ,true);
					} 
					catch(Exception ex){exp = ex;}
					finally{EndCase(exp); exp = null;}
					break;
				case DataBaseServer.Oracle:
					try
					{
						BeginCase("check type name");
						Compare(typeName == "DBTYPE_NUMERIC" || typeName == "NUMBER" ,true);
					} 
					catch(Exception ex){exp = ex;}
					finally{EndCase(exp); exp = null;}
					break;
				case DataBaseServer.DB2:
					try
					{
						BeginCase("check type name");
						Compare(typeName == "DBTYPE_I4" || typeName == "INTEGER" ,true);
					} 
					catch(Exception ex){exp = ex;}
					finally{EndCase(exp); exp = null;}
					break;
				case DataBaseServer.Unknown:
					break;
			}

		}
	}

}