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
using MonoTests.System.Data.Utils;

namespace MonoTests.System.Data
{
	[TestFixture] public class RowNotInTableExceptionTest
	{
		[Test]
		public void Generate()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(DataProvider.CreateParentDataTable());
			ds.Tables.Add(DataProvider.CreateChildDataTable());
			ds.Relations.Add(new DataRelation("myRelation",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));

			DataRow drParent = ds.Tables[0].Rows[0];
			DataRow drChild = ds.Tables[1].Rows[0];
			drParent.Delete();
			drChild.Delete();
			ds.AcceptChanges();

			// RowNotInTableException - AcceptChanges
			try
			{
				drParent.AcceptChanges();
				Assert.Fail("RNT1: AcceptChanges failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: AcceptChanges wrong exception type. Got: " + exc);
			}

			// RowNotInTableException - GetChildRows
			try
			{
				drParent.GetChildRows("myRelation");
				Assert.Fail("RNT1: GetChildRows failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: GetChildRows wrong exception type. Got: " + exc);
			}

			// RowNotInTableException - ItemArray
			object[] o = null;
			try
			{
				o = drParent.ItemArray ;
				Assert.Fail("RNT1: ItemArray failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: ItemArray wrong exception type. Got: " + exc);
			}

			// **********	don't throw exception (should be according to MSDN)	***********************
			//		// RowNotInTableException - GetParentRow
			//		try
			//		{
			//			DataRow dr = null;
			//			dr = drChild.GetParentRow("myRelation"); 
			//			Assert.Fail("RNT1: GetParentRow failed to raise (RowNotInTableException.");
			//		}
			//		catch (RowNotInTableException) {}
			//		catch (AssertionException) { throw; }
			//		catch (Exception exc)
			//		{
			//			Assert.Fail("RNT2: GetParentRow wrong exception type. Got: " + exc);
			//		}

			// RowNotInTableException - GetParentRows
			DataRow[] dr = null;
			try
			{
				dr = drChild.GetParentRows("myRelation"); 
				Assert.Fail("RNT1: GetParentRows failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: GetParentRows wrong exception type. Got: " + exc);
			}

			// RowNotInTableException - RejectChanges
			try
			{
				drParent.RejectChanges();
				Assert.Fail("RNT1: RejectChanges failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: RejectChanges wrong exception type. Got: " + exc);
			}

			// RowNotInTableException - SetParentRow
			try
			{
				drChild.SetParentRow(ds.Tables[0].Rows[1]);
				Assert.Fail("RNT1: SetParentRow failed to raise (RowNotInTableException.");
			}
			catch (RowNotInTableException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("RNT2: SetParentRow wrong exception type. Got: " + exc);
			}
		}
	}
}
