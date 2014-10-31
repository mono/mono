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
	[TestFixture] public class NoNullAllowedExceptionTest
	{
		[Test] public void Generate()
		{
			DataTable tbl = DataProvider.CreateParentDataTable();

			// ----------- check with columnn type int -----------------------
			tbl.Columns[0].AllowDBNull = false;

			//add new row with null value
			// NoNullAllowedException - Add Row
			try 
			{
				tbl.Rows.Add(new object[] {null,"value","value",new DateTime(0),0.5,true});
				Assert.Fail("NNAE1: Rows.Add failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE2: Rows.Add wrong exception type. Got: " + exc);
			}

			//add new row with DBNull value
			// NoNullAllowedException - Add Row
			try 
			{
				tbl.Rows.Add(new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true});
				Assert.Fail("NNAE3: Rows.Add failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE4: Rows.Add wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - ItemArray
			try 
			{
				tbl.Rows[0].ItemArray = new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true};
				Assert.Fail("NNAE5: Rows Indexer failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE6: Rows Indexer wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - Add Row - LoadDataRow
			try 
			{
				tbl.LoadDataRow(new object[] {DBNull.Value,"value","value",new DateTime(0),0.5,true},true);
				Assert.Fail("NNAE7: LoadDataRow failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE8: LoadDataRow wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - EndEdit
			tbl.Rows[0].BeginEdit();
			tbl.Rows[0][0] = DBNull.Value ;
			try 
			{
				tbl.Rows[0].EndEdit();
				Assert.Fail("NNAE7: Rows Indexer failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE8: Rows Indexer wrong exception type. Got: " + exc);
			}

			// ----------- add new column -----------------------
			tbl.Columns[0].AllowDBNull = true;
			tbl.Columns.Add(new DataColumn("bolCol",typeof(bool)));

			// add new column
			try 
			{
				tbl.Columns[tbl.Columns.Count-1].AllowDBNull = false;
				Assert.Fail("NNAE9: Columns failed to raise DataException.");
			}
			catch (DataException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE10: Columns wrong exception type. Got: " + exc);
			}

			//clear table data in order to add the new column
			tbl.Rows.Clear();
			tbl.Columns[tbl.Columns.Count-1].AllowDBNull = false;
			tbl.Rows.Add(new object[] {99,"value","value",new DateTime(0),0.5,true,false}); //missing last value - will be null

			//add new row with null value
			// NoNullAllowedException - Add Row
			try 
			{
				tbl.Rows.Add(new object[] {99,"value","value",new DateTime(0),0.5,true}); //missing last value - will be null
				Assert.Fail("NNAE11: Rows.Add failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE12: Rows.Add wrong exception type. Got: " + exc);
			}

			//add new row with DBNull value
			// NoNullAllowedException - Add Row
			try 
			{
				tbl.Rows.Add(new object[] {1,"value","value",new DateTime(0),0.5,true,DBNull.Value});
				Assert.Fail("NNAE13: Rows.Add failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE14: Rows.Add wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - ItemArray
			try 
			{
				tbl.Rows[0].ItemArray = new object[] {77,"value","value",new DateTime(0),0.5,true,DBNull.Value };
				Assert.Fail("NNAE15: Rows Indexer failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE16: Rows Indexer wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - Add Row - LoadDataRow
			try 
			{
				tbl.LoadDataRow(new object[] {66,"value","value",new DateTime(0),0.5,true},true);
				Assert.Fail("NNAE17: LoadDataRow failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE18: Rows.LoadDataRow wrong exception type. Got: " + exc);
			}

			// NoNullAllowedException - EndEdit
			tbl.Rows[0].BeginEdit();
			tbl.Rows[0][tbl.Columns.Count-1] = DBNull.Value ;
			try 
			{
				tbl.Rows[0].EndEdit();
				Assert.Fail("NNAE19: Rows[0].EndEdit failed to raise NoNullAllowedException.");
			}
			catch (NoNullAllowedException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("NNAE20: Rows[0].EndEdit wrong exception type. Got: " + exc);
			}
		}
	}
}
