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
	[TestFixture] public class DataColumnCollection_Remove_D : GHTBase
	{
		[Test] public void Main()
		{
			DataColumnCollection_Remove_D tc = new DataColumnCollection_Remove_D();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataColumnCollection_Remove_D");
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
			Exception exp = null;


			//prepare a DataSet with DataTable to be checked
			DataTable dtSource = new DataTable();
			dtSource.Columns.Add("Col_0", typeof(int)); 
			dtSource.Columns.Add("Col_1", typeof(int)); 
			dtSource.Columns.Add("Col_2", typeof(int)); 
			dtSource.Rows.Add(new object[] {0,1,2}); 

			DataTable dt = null;

			//------Check Remove first column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[0]); 
			try
			{
				BeginCase("Remove first column - check column count");
				Compare(dt.Columns.Count , 2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove first column - check column removed");
				Compare(dt.Columns.Contains("Col_0"),false);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove first column - check column 0 data");
				Compare(dt.Rows[0][0],1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove first column - check column 1 data");
				Compare(dt.Rows[0][1],2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}


			//------Check Remove middle column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[1]); 
			try
			{
				BeginCase("Remove middle column - check column count");
				Compare(dt.Columns.Count , 2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove middle column - check column removed");
				Compare(dt.Columns.Contains("Col_1"),false);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove middle column - check column 0 data");
				Compare(dt.Rows[0][0],0);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove middle column - check column 1 data");
				Compare(dt.Rows[0][1],2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}


			//------Check Remove last column---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);

			dt.Columns.Remove(dt.Columns[2]); 
			try
			{
				BeginCase("Remove last column - check column count");
				Compare(dt.Columns.Count , 2);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove last column - check column removed");
				Compare(dt.Columns.Contains("Col_2"),false);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove last column - check column 0 data");
				Compare(dt.Rows[0][0],0);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			try
			{
				BeginCase("Remove last column - check column 1 data");
				Compare(dt.Rows[0][1],1);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}

			//------Check Remove column exception---------
			dt = dtSource.Clone();
			dt.ImportRow(dtSource.Rows[0]);
			try
			{
				BeginCase("Check Remove column exception - Column name not exists");
				try
				{
					DataColumn dc = new DataColumn();
					dt.Columns.Remove(dc); 
				}
				catch (Exception ex)
				{
					exp = ex;
				}
				Compare(exp.GetType().FullName ,typeof(System.ArgumentException).FullName);
				exp=null;
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp);exp = null;}
		}
	}
}