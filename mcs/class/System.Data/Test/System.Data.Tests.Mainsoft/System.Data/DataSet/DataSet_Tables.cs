// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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

using NUnit.Framework;


using System;
using System.Data;

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
[TestFixture] public class DataSet_Tables : GHTBase
{
	[Test] public void Main()
	{
		DataSet_Tables tc = new DataSet_Tables();
		Exception exp = null;
		try
		{
			tc.BeginTest("DataSet_Tables");
			tc.run();
		}
		catch(Exception ex)
		{
			exp = ex;
		}
		finally
		{
			tc.EndTest(exp);
		}
	}

	//Activate This Construntor to log All To Standard output
	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}


	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

	public void run()
	{

	//References by name to tables and relations in a DataSet are case-sensitive. Two or more tables or relations can exist in a DataSet that have the same name, but that differ in case. For example you can have Table1 and table1. In this situation, a reference to one of the tables by name must match the case of the table name exactly, otherwise an exception is thrown. For example, if the DataSet myDS contains tables Table1 and table1, you would reference Table1 by name as myDS.Tables["Table1"], and table1 as myDS.Tables ["table1"]. Attempting to reference either of the tables as myDS.Tables ["TABLE1"] would generate an exception.
	//The case-sensitivity rule does not apply if only one table or relation exists with a particular name. That is, if no other table or relation object in the DataSet matches the name of that particular table or relation object, even by a difference in case, you can reference the object by name using any case and no exception is thrown. For example, if the DataSet has only Table1, you can reference it using myDS.Tables["TABLE1"].
	//The CaseSensitive property of the DataSet does not affect this behavior. The CaseSensitive property 

		Exception exp = null;
		DataSet ds = new DataSet();

		DataTable dt1 = new DataTable();
		DataTable dt2 = new DataTable();
		DataTable dt3 = new DataTable();
		dt3.TableName = "Table3";
		DataTable dt4 = new DataTable(dt3.TableName.ToUpper());
		
	
		try
		{
			base.BeginCase("Checking Tables - default value");
			//Check default
			base.Compare(ds.Tables.Count  ,0);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		ds.Tables.Add(dt1);
		ds.Tables.Add(dt2);
		ds.Tables.Add(dt3);

		try
		{
			base.BeginCase("Checking Tables Count");
			base.Compare(ds.Tables.Count  ,3);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking Tables Value 1");
			base.Compare(ds.Tables[0] ,dt1);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking Tables Value 2");
			base.Compare(ds.Tables[1] ,dt2);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking Tables Value 3");
			base.Compare(ds.Tables[2] ,dt3);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking get table by name.ToUpper");
			base.Compare(ds.Tables[dt3.TableName.ToUpper()] ,dt3);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking get table by name.ToLower");
			base.Compare(ds.Tables[dt3.TableName.ToLower()] ,dt3);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


		try
		{
			base.BeginCase("Checking Tables add with name case insensetive");
			ds.Tables.Add(dt4); //same name as Table3, but different case
			base.Compare(ds.Tables.Count  ,4);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking get table by name");
			base.Compare(ds.Tables[dt4.TableName] ,dt4);
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}

		try
		{
			base.BeginCase("Checking get table by name with diferent case, ArgumentException");
			try
			{
				DataTable tmp = ds.Tables[dt4.TableName.ToLower()];
			}
			catch (ArgumentException ex) {exp = ex;}
			base.Compare(exp == null , false);
			exp=null;
		}
		catch(Exception ex)	{exp = ex;}
		finally	{EndCase(exp); exp = null;}


	}
}
}