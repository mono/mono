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

using System;
using System.Data;

using GHTUtils;
using GHTUtils.Base;
using NUnit.Framework;

namespace tests.system_data_dll.System_Data
{
	[TestFixture] public class DataSet_MergeFailed : GHTBase
	{
		public void SetUp()
		{
			Exception exp = null;
			BeginCase("Setup");
			try
			{
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		public void TearDown()
		{
		}

		[Test] public void Main()
		{
			DataSet_MergeFailed tc = new DataSet_MergeFailed();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataSet_MergeFailed");
				tc.SetUp();
				tc.run();
				tc.TearDown();
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

		private bool EventRaised = false;
		public void run()
		{
			Exception exp = null;
			DataSet ds1,ds2;
			ds1 = new DataSet();
			ds1.Tables.Add(GHTUtils.DataProvider.CreateParentDataTable());
			//add primary key to the FIRST column
			ds1.Tables[0].PrimaryKey = new DataColumn[] {ds1.Tables[0].Columns[0]};

			//create target dataset which is a copy of the source
			ds2 = ds1.Copy();
			//clear the data
			ds2.Clear();
			//add primary key to the SECOND columnn
			ds2.Tables[0].PrimaryKey = new DataColumn[] {ds2.Tables[0].Columns[1]};
			//add a new row that already exists in the source dataset
			//ds2.Tables[0].Rows.Add(ds1.Tables[0].Rows[0].ItemArray);

			//enforce constraints
			ds2.EnforceConstraints = true;
			ds1.EnforceConstraints = true;

			// Add MergeFailed event handler for the table.
			ds2.MergeFailed += new MergeFailedEventHandler( Merge_Failed );

			try
			{
				ds2.Merge(ds1); //will raise MergeFailed event 
			}
			catch {}

			try
			{
				BeginCase("MergeFailed event");
				Compare(EventRaised ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}
		private void Merge_Failed( object sender, MergeFailedEventArgs e )
		{
			EventRaised = true;
		}
	}
}
