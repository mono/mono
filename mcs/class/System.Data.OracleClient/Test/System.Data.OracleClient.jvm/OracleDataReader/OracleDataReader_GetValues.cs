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
	public class OracleDataReader_GetValues : ADONetTesterClass 
	{
		public static void Main()
		{
			OracleDataReader_GetValues tc = new OracleDataReader_GetValues();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataReader_GetValues");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
			int intValuesCount = 0;

			//prepare data
			base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();
			OracleCommand cmd = new OracleCommand("Select CustomerID, ContactName , CompanyName From Customers where CustomerID = 'GH100'", con);
			OracleDataReader rdr = cmd.ExecuteReader();
			rdr.Read();

			object [] values = null;


			//------ check big array
			try
			{
				BeginCase("GetValues - bigger array - check count");
				values = new object[50];
				intValuesCount = rdr.GetValues(values);
				Compare(intValuesCount ,3 );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - bigger array - check CustomerID");
				Compare(values[0].ToString().Trim() ,"GH100" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - bigger array - check CompanyName");
				Compare(values[2].ToString() ,"Company100" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - bigger array - check DBNull");
				Compare(values[3] == null ,true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			//------ check small array
			try
			{
				BeginCase("GetValues - smaller array - check count");
				values = new object[2];
				intValuesCount = rdr.GetValues(values);
				Compare(intValuesCount ,2 );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - smaller array - check CustomerID");
				Compare(values[0].ToString().Trim() ,"GH100" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			//------ check exact array
			try
			{
				BeginCase("GetValues - exact array - check count");
				values = new object[3];
				intValuesCount = rdr.GetValues(values);
				Compare(intValuesCount ,3 );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - exact array - check CustomerID");
				Compare(values[0].ToString().Trim() ,"GH100" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("GetValues - exact array - check CompanyName");
				Compare(values[2].ToString() ,"Company100" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			if (con.State == ConnectionState.Open) con.Close();


		}
	}
}