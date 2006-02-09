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
public class OracleDataReader_GetName_I : GHTBase
{
	public static void Main()
	{
		OracleDataReader_GetName_I tc = new OracleDataReader_GetName_I();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleDataReader_NextResult");
				tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
#if !TARGET_JVM
	[Category("NotWorking")]
#endif
	public void run()
	{
		Exception exp = null;
		OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		OracleCommand cmd = new OracleCommand("Customers",  con);
		cmd.CommandType = CommandType.TableDirect;

		con.Open();

		OracleDataReader rdr = cmd.ExecuteReader();
		rdr.Read();


		try
		{
			BeginCase("GetName field 0");
			string str = rdr.GetName(0);
			Compare(str.ToUpper(),"CUSTOMERID" );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("GetName last field ");
			string str = rdr.GetName(rdr.FieldCount -1);
			Compare(str.ToUpper(),"FAX" );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		if (con.State == ConnectionState.Open) con.Close();
	}
}











}