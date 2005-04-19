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
	[TestFixture] public class DataRow_HasVersion_D : GHTBase
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
			DataRow_HasVersion_D tc = new DataRow_HasVersion_D();
			Exception exp = null;
			try
			{
				tc.BeginTest("DataRow_HasVersion_D");
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


		public void run()
		{
			Exception exp = null;
	

			DataTable t = new DataTable("atable");
			t.Columns.Add("id", typeof(int));
			t.Columns.Add("name", typeof(string));
			t.Columns[0].DefaultValue = 1;
			t.Columns[1].DefaultValue = "something";


			// row r is detached
			DataRow r = t.NewRow();
		
			try
			{
				BeginCase("HasVersion Test #10");
				Compare(r.HasVersion(DataRowVersion.Current) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #11");
				Compare(r.HasVersion(DataRowVersion.Original) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #12");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #13");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}		
	
					
			r[0] = 4; 
			r[1] = "four";

			try
			{
				BeginCase("HasVersion Test #20");
				Compare(r.HasVersion(DataRowVersion.Current) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #21");
				Compare(r.HasVersion(DataRowVersion.Original) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #22");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #23");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	
			t.Rows.Add(r);
			// now it is "added"

			try
			{
				BeginCase("HasVersion Test #30");
				Compare(r.HasVersion(DataRowVersion.Current) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #31");
				Compare(r.HasVersion(DataRowVersion.Original) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #32");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #33");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	

			t.AcceptChanges();
			// now it is "unchanged"
	
			try
			{
				BeginCase("HasVersion Test #40");
				Compare(r.HasVersion(DataRowVersion.Current) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #41");
				Compare(r.HasVersion(DataRowVersion.Original) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #42");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #43");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	
		
			r.BeginEdit();
			r[1] = "newvalue";
	
			try
			{
				BeginCase("HasVersion Test #50");
				Compare(r.HasVersion(DataRowVersion.Current) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #51");
				Compare(r.HasVersion(DataRowVersion.Original) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #52");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #53");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	
					
			r.EndEdit();
			// now it is "modified"
			try
			{
				BeginCase("HasVersion Test #60");
				Compare(r.HasVersion(DataRowVersion.Current) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #61");
				Compare(r.HasVersion(DataRowVersion.Original) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #62");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #63");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	
			// this or t.AcceptChanges
			r.AcceptChanges(); 
			// now it is "unchanged" again
			try
			{
				BeginCase("HasVersion Test #70");
				Compare(r.HasVersion(DataRowVersion.Current) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #71");
				Compare(r.HasVersion(DataRowVersion.Original) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #72");
				Compare(r.HasVersion(DataRowVersion.Default) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #73");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	
			r.Delete();
			// now it is "deleted"
	
			try
			{
				BeginCase("HasVersion Test #80");
				Compare(r.HasVersion(DataRowVersion.Current) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #81");
				Compare(r.HasVersion(DataRowVersion.Original) ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #82");
				Compare(r.HasVersion(DataRowVersion.Default) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #83");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
	

			r.AcceptChanges();
			// back to detached
			try
			{
				BeginCase("HasVersion Test #90");
				Compare(r.HasVersion(DataRowVersion.Current) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #91");
				Compare(r.HasVersion(DataRowVersion.Original) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #92");
				Compare(r.HasVersion(DataRowVersion.Default) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try
			{
				BeginCase("HasVersion Test #93");
				Compare(r.HasVersion(DataRowVersion.Proposed) ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}	
		
	

		}
	}
}
