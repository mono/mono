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
	public class OleDbDataReader_GetChars : ADONetTesterClass 
	{
		OleDbConnection con;
		char [] Result;

		[SetUp]
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
				//prepare data
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

				con = new OleDbConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				Result = new char[100];
				con.Open();
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		[TearDown]
		public void TearDown()
		{
			if (con.State == ConnectionState.Open) con.Close();
		}

		public static void Main()
		{
			OleDbDataReader_GetChars tc = new OleDbDataReader_GetChars();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDbDataReader_GetChars");
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
			long rdrResults = 0;

			OleDbCommand cmd = new OleDbCommand("Select LastName From Employees Where EmployeeID = 100", con);
			OleDbDataReader rdr = cmd.ExecuteReader();
			rdr.Read();

			//LastName should be "Last100"
	
			try
			{
				BeginCase("check result length");
				rdrResults = rdr.GetChars(0, 0, Result, 0, Result.Length);
				Compare(rdrResults,(long)"Last100".Length  );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check result - char[0]");
				Compare(Result[0] ,'L');
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("check result - char[last char index]");
				Compare(Result["Last100".Length-1] ,'0');
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}
}