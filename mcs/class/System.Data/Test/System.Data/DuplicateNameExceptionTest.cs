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
	[TestFixture] public class DuplicateNameExceptionTest
	{
		[Test] public void Generate()
		{
			DataSet ds = new DataSet();
			ds.Tables.Add(new DataTable("Table"));
			ds.Tables.Add(new DataTable("Table1"));
			ds.Tables[0].Columns.Add(new DataColumn("Column"));
			ds.Tables[0].Columns.Add(new DataColumn("Column1"));
			ds.Tables[0].Columns.Add(new DataColumn("Column2"));
			ds.Tables[1].Columns.Add(new DataColumn("Column"));

			ds.Relations.Add(new DataRelation("Relation",ds.Tables[0].Columns[0],ds.Tables[1].Columns[0]));
			ds.Tables[0].Constraints.Add(new UniqueConstraint("Constraint",ds.Tables[0].Columns[1]));

			// DuplicateNameException - tables 
			try 
			{
				ds.Tables.Add(new DataTable("Table"));
				Assert.Fail("DNE1: Tables.Add failed to raise DuplicateNameException.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DNE2: Tables.Add wrong exception type. Got: " + exc);
			}

			// DuplicateNameException - Column 
			try 
			{
				ds.Tables[0].Columns.Add(new DataColumn("Column"));
				Assert.Fail("DNE3: Tables[0].Columns.Add failed to raise DuplicateNameException.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DNE4: Tables[0].Columns.Add wrong exception type. Got: " + exc);
			}

			// DuplicateNameException - Constraints 
			try 
			{
				ds.Tables[0].Constraints.Add(new UniqueConstraint("Constraint",ds.Tables[0].Columns[2]));
				Assert.Fail("DNE5: Tables[0].Constraints.Add failed to raise DuplicateNameException.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DNE6: Tables[0].Constraints.Add wrong exception type. Got: " + exc);
			}

			// DuplicateNameException - Relations 
			try 
			{
				ds.Relations.Add(new DataRelation("Relation",ds.Tables[0].Columns[1],ds.Tables[1].Columns[0]));
				Assert.Fail("DNE7: Relations.Add failed to raise DuplicateNameException.");
			}
			catch (DuplicateNameException) {}
			catch (AssertionException) { throw; }
			catch (Exception exc)
			{
				Assert.Fail("DNE8: Relations.Add wrong exception type. Got: " + exc);
			}
		}
	}
}
