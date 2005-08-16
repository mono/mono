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

namespace MonoTests_System.Data
{
	[TestFixture] public class DeletedRowInaccessibleExceptionTest
	{
		[Test] public void Generate()
		{
			DataTable dtParent;
			dtParent= DataProvider.CreateParentDataTable(); 

			DataRow dr = dtParent.Rows[0];
			dr.Delete();

			// DeletedRowInaccessible Exception (BeginEdit)
			try 
			{
				dr.BeginEdit();
				Assert.Fail("DRIE1: BeginEdit failed to raise DeletedRowInaccessibleException.");
			}
			catch (DeletedRowInaccessibleException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DRIE2: Indexer wrong exception type. Got: " + exc);
			}

			// DeletedRowInaccessible Exception (Item)
			try 
			{
				string s = dr[0].ToString();
				Assert.Fail("DRIE3: Indexer failed to raise DeletedRowInaccessibleException.");
			}
			catch (DeletedRowInaccessibleException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DRIE4: Indexer wrong exception type. Got: " + exc);
			}

			// DeletedRowInaccessible Exception (ItemArray)
			try 
			{
				object[] o = dr.ItemArray;
				Assert.Fail("DRIE5: Indexer failed to raise DeletedRowInaccessibleException.");
			}
			catch (DeletedRowInaccessibleException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DRIE6: Indexer wrong exception type. Got: " + exc);
			}
		}
	}
}
