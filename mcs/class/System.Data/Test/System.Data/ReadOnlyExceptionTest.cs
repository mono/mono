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
	[TestFixture] public class ReadOnlyExceptionTest
	{
		[Test] public void Generate()
		{
			Exception tmpEx = new Exception() ;

			DataTable tbl = DataProvider.CreateParentDataTable();
			tbl.Columns[0].ReadOnly = true;

			//chaeck for int column
			// ReadOnlyException - EndEdit
			//tbl.Rows[0].BeginEdit();   // this throw an exception but according to MSDN it shouldn't !!!
			//tbl.Rows[0][0] = 99 ;
			try 
			{
				tbl.Rows[0][0] = 99 ;
				//tbl.Rows[0].EndEdit();
				Assert.Fail("ROE1: Rows Indexer failed to raise ReadOnlyException.");
			}
			catch (ReadOnlyException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("ROE2: Rows Indexer wrong exception type. Got: " + exc);
			}

			// ReadOnlyException - ItemArray
			try 
			{
				tbl.Rows[0].ItemArray = new object[] {99,"value","value"};
				Assert.Fail("ROE3: Rows[0].ItemArray failed to raise ReadOnlyException.");
			}
			catch (ReadOnlyException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("ROE4: Rows[0].ItemArray wrong exception type. Got: " + exc);
			}

			//chaeck for string column
			tbl.Columns[0].ReadOnly = false;
			tbl.Columns[1].ReadOnly = true;

			// ReadOnlyException - EndEdit
			//tbl.Rows[0].BeginEdit();   // this throw an exception but according to MSDN it shouldn't !!!
			//tbl.Rows[0][0] = 99 ;
			try 
			{
				tbl.Rows[0][1] = "NewValue" ;
				//tbl.Rows[0].EndEdit();
				Assert.Fail("ROE5: Rows Indexer failed to raise ReadOnlyException.");
			}
			catch (ReadOnlyException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("ROE6: Rows Indexer wrong exception type. Got: " + exc);
			}

			// ReadOnlyException - ItemArray
			try 
			{
				tbl.Rows[0].ItemArray = new object[] {99,"value","value"};
				Assert.Fail("ROE7: Rows[0].ItemArray failed to raise ReadOnlyException.");
			}
			catch (ReadOnlyException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("ROE8: Rows[0].ItemArray wrong exception type. Got: " + exc);
			}
		}
	}
}
