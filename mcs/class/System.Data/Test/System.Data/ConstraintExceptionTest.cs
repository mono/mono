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
using System.Text;
using System.IO;
using System.Data;
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class ConstraintExceptionTest
	{
		[Test] public void Generate()
		{
			DataTable dtParent= DataProvider.CreateParentDataTable(); 
			DataTable dtChild = DataProvider.CreateChildDataTable(); 

			DataSet ds = new DataSet();
			ds.Tables.Add(dtChild);
			ds.Tables.Add(dtParent);

			//------ check UniqueConstraint ---------

			//create unique constraint
			UniqueConstraint uc; 

			//Column type = int
			uc = new UniqueConstraint(dtParent.Columns[0]); 
			dtParent.Constraints.Add(uc);
			// UniqueConstraint Exception - Column type = int
			try 
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
				Assert.Fail("CNE1: Rows.Add failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE2: Rows.Add wrong exception type. Got: " + exc);
			}

			//Column type = DateTime
			dtParent.Constraints.Clear();
			uc = new UniqueConstraint(dtParent.Columns["ParentDateTime"]); 
			dtParent.Constraints.Add(uc);
			// UniqueConstraint Exception - Column type = DateTime
			try 
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
				Assert.Fail("CNE3: Rows.Add failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE4: Rows.Add wrong exception type. Got: " + exc);
			}

			//Column type = double
			dtParent.Constraints.Clear();
			uc = new UniqueConstraint(dtParent.Columns["ParentDouble"]); 
			dtParent.Constraints.Add(uc);
			// UniqueConstraint Exception - Column type = double
			try 
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
				Assert.Fail("CNE5: Rows.Add failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE6: Rows.Add wrong exception type. Got: " + exc);
			}

			//Column type = string
			dtParent.Constraints.Clear();
			uc = new UniqueConstraint(dtParent.Columns["String1"]); 
			dtParent.Constraints.Add(uc);
			// UniqueConstraint Exception - Column type = String
			try 
			{
				//add exisiting value - will raise exception
				dtParent.Rows.Add(dtParent.Rows[0].ItemArray);
				Assert.Fail("CNE7: Rows.Add failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE8: Rows.Add wrong exception type. Got: " + exc);
			}

			//Column type = string, ds.CaseSensitive = false;
			ds.CaseSensitive = false;

			dtParent.Constraints.Clear();
			uc = new UniqueConstraint(dtParent.Columns["String1"]); 
			dtParent.Constraints.Add(uc);
			DataRow dr = dtParent.NewRow();
			dr.ItemArray = dtParent.Rows[0].ItemArray ;
			dr["String1"] = dr["String1"].ToString().ToUpper();

			// UniqueConstraint Exception - Column type = String, CaseSensitive = false;
			try 
			{
				dtParent.Rows.Add(dr);
				Assert.Fail("CNE9: Rows.Add failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE10: Rows.Add wrong exception type. Got: " + exc);
			}

			//Column type = string, ds.CaseSensitive = true;
			ds.CaseSensitive = true;

			dtParent.Constraints.Clear();
			uc = new UniqueConstraint(dtParent.Columns["String1"]); 
			dtParent.Constraints.Add(uc);

			// No UniqueConstraint Exception - Column type = String, CaseSensitive = true;
			dtParent.Rows.Add(dr);

			// Column type = string, ds.CaseSensitive = false;
			// UniqueConstraint Exception - Column type = String, Enable CaseSensitive = true;
			try 
			{
				ds.CaseSensitive = false;
				Assert.Fail("CNE13: CaseSensitive failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE14: CaseSensitive wrong exception type. Got: " + exc);
			}

			dtChild.Constraints.Add(new UniqueConstraint(new DataColumn[] {dtChild.Columns[0],dtChild.Columns[1]}));
			ds.EnforceConstraints = false;
			dtChild.Rows.Add(dtChild.Rows[0].ItemArray);

			// UniqueConstraint Exception - ds.EnforceConstraints 
			try 
			{
				ds.EnforceConstraints = true;
				Assert.Fail("CNE15: EnforceConstraints failed to raise ConstraintException.");
			}
			catch (ConstraintException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("CNE16: EnforceConstraints wrong exception type. Got: " + exc);
			}
		}
	}
}
