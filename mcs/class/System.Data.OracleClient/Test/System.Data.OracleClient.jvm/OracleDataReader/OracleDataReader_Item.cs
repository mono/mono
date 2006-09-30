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
public class OracleDataReader_Item : ADONetTesterClass 
{
	public static void Main()
	{
		OracleDataReader_Item tc = new OracleDataReader_Item();
		Exception exp = null;
		try
		{
			tc.BeginTest("OracleDataReader_Item");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		Exception exp = null;


		//prepare data



		OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
		OracleCommand cmd = new OracleCommand("select ProductID,ProductName,Discontinued from Products where ProductID=1", con);
		con.Open();
		OracleDataReader rdr =  cmd.ExecuteReader();

		rdr.Read();
		object obj = null;

		switch (ConnectedDataProvider.GetDbType(con)) 
		{
			case DataBaseServer.DB2:
			case DataBaseServer.SQLServer:
			case DataBaseServer.PostgreSQL:
				try
				{
					BeginCase("Column int - type");
					obj = rdr["ProductID"];
					Compare(obj.GetType().FullName , typeof(int).FullName);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
				break;
			case DataBaseServer.Oracle:
			case DataBaseServer.Sybase:
				try
				{
					BeginCase("Column Decimal - type");
					obj = rdr["ProductID"];
					Compare(obj.GetType().FullName , typeof(decimal).FullName);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
				break;
		}

		try
		{
			BeginCase("Column int - value");
			Compare(obj.ToString(),"1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Column string - type");
			obj = rdr["ProductName"];
			Compare(obj.GetType().FullName , typeof(string).FullName);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("Column string - value");
			Compare(obj.ToString(),"Chai");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		switch (ConnectedDataProvider.GetDbType(con)) 
		{
			case DataBaseServer.DB2 :
				try
				{
					BeginCase("Column Int16 - type");
					obj = rdr["Discontinued"];
					Compare(obj.GetType().FullName , typeof(Int16).FullName);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Column Int16 - value");
					Compare(obj.ToString(),"0");
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
				break;
			case DataBaseServer.SQLServer :
				try
				{
					BeginCase("Column bool - type");
					obj = rdr["Discontinued"];
					Compare(obj.GetType().FullName , typeof(bool).FullName);
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}

				try
				{
					BeginCase("Column bool - value");
					Compare(obj.ToString().ToUpper(),"FALSE");
				} 
				catch(Exception ex){exp = ex;}
				finally{EndCase(exp); exp = null;}
				break;
			case DataBaseServer.Oracle :
				//Column type is Decimal - already tested
				break;
		}
		if (con.State == ConnectionState.Open) con.Close();
	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}

}