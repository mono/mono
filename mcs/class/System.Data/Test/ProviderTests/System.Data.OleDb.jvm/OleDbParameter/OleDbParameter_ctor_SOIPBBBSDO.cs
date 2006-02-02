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
public class OleDbParameter_ctor_SOIPBBBSDO : GHTBase
{
	public static void Main()
	{
		OleDbParameter_ctor_SOIPBBBSDO tc = new OleDbParameter_ctor_SOIPBBBSDO();
		Exception exp = null;
		try
		{
			tc.BeginTest("OleDbParameter_ctor_SOIPBBBSDO");
			tc.run();
		}
		catch(Exception ex){exp = ex;}
		finally	{tc.EndTest(exp);}
	}

	[Test]
	public void run()
	{
		Exception exp = null;

		OleDbParameter param = new OleDbParameter("myParam",		//param name
												OleDbType.Double,	//type
												5,					//size
												ParameterDirection.Input, //direction
												true,				//nillbale
												1,					//precision 
												1,					//scale	
												"Column1",			//source column
												DataRowVersion.Current, //datarow version
												590.456);			//value
		try
		{
			BeginCase("ParameterName");
			Compare(param.ParameterName , "myParam");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}

		try
		{
			BeginCase("OleDbType");
			Compare(param.OleDbType ,OleDbType.Double );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Size");
			Compare(param.Size  , 5);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("ParameterDirection");
			Compare(param.Direction  , ParameterDirection.Input );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("IsNullable");
			Compare(param.IsNullable , true);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Precision");
			Compare(param.Precision , (byte)1);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Scale");
			Compare(param.Scale  , (byte)1);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("SourceColumn");
			Compare(param.SourceColumn  ,"Column1");
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("SourceVersion");
			Compare(param.SourceVersion ,DataRowVersion.Current );
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
		try
		{
			BeginCase("Value");
			Compare(param.Value  ,590.456);
		} 
		catch(Exception ex){exp = ex;}
		finally{EndCase(exp); exp = null;}
	}


	//public TestClass():base(true){}

	//Activate this constructor to log Failures to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, false){}

	//Activate this constructor to log All to a log file
	//public TestClass(System.IO.TextWriter tw):base(tw, true){}

	//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

}
}