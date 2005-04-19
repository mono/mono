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
using System.Data.OleDb;

using GHTUtils;
using GHTUtils.Base;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataSet_ReadXml_Strm2 : GHTBase
	{
		[Test] public void Main()
		{
			DataSet_ReadXml_Strm2 tc = new DataSet_ReadXml_Strm2();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataSet_ReadXml_Strm2");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{
			Exception exp = null;

			string input = string.Empty;

			System.IO.StringReader sr;
			DataSet ds = new DataSet();

			input += "<?xml version=\"1.0\"?>";
			input += "<Stock name=\"MSFT\">";
			input += "		<Company name=\"Microsoft Corp.\"/>";
			input += "		<Price type=\"high\">";
			input += "			<Value>10.0</Value>";		
			input += "			<Date>01/20/2000</Date>";
			input += "		</Price>";
			input += "		<Price type=\"low\">";
			input += "			<Value>1.0</Value>";
			input += "			<Date>03/21/2002</Date>";
			input += "		</Price>";
			input += "		<Price type=\"current\">";
			input += "			<Value>3.0</Value>";
			input += "			<Date>TODAY</Date>";
			input += "		</Price>";
			input += "</Stock>";

			sr = new System.IO.StringReader(input);

	
			ds.ReadXml(sr);

			try
			{
				BeginCase("Relation Count");
				Compare(ds.Relations.Count ,2 );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("RelationName 1");
				Compare(ds.Relations[0].RelationName ,"Stock_Company" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("RelationName 2");
				Compare(ds.Relations[1].RelationName ,"Stock_Price" );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables count ");
				Compare(ds.Tables.Count ,3);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[0] ChildRelations count ");
				Compare(ds.Tables[0].ChildRelations.Count ,2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[0] ChildRelations[0] name ");
				Compare(ds.Tables[0].ChildRelations[0].RelationName ,"Stock_Company");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[0] ChildRelations[1] name ");
				Compare(ds.Tables[0].ChildRelations[1].RelationName ,"Stock_Price");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[1] ChildRelations count ");
				Compare(ds.Tables[1].ChildRelations.Count ,0);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[2] ChildRelations count ");
				Compare(ds.Tables[2].ChildRelations.Count ,0);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("Tables[0] ParentRelations count ");
				Compare(ds.Tables[0].ParentRelations.Count ,0);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
	
			try
			{
				BeginCase("Tables[1] ParentRelations count ");
				Compare(ds.Tables[1].ParentRelations.Count ,1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[1] ParentRelations[0] name ");
				Compare(ds.Tables[1].ParentRelations[0].RelationName ,"Stock_Company");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("Tables[2] ParentRelations count ");
				Compare(ds.Tables[2].ParentRelations.Count ,1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("Tables[2] ParentRelations[0] name ");
				Compare(ds.Tables[2].ParentRelations[0].RelationName ,"Stock_Price");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}
}