//
// DataRowComparerTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
//

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
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowComparerTest
	{
		[Test]
		[Category ("NotWorking")]
		public void GetHashCodeWithVersions ()
		{
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ("MyTable");
			ds.Tables.Add (dt);
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			DataRow r1 = dt.Rows.Add (new object [] {"foo", "bar"});
			DataRow r2 = dt.Rows.Add (new object [] {"foo", "bar"});
			ds.AcceptChanges ();
			DataRowComparer<DataRow> c = DataRowComparer.Default;
			Assert.IsTrue (c.GetHashCode (r1) == c.GetHashCode (r2), "#1");
			/*
			// LAMESPEC: .NET fails here
			r2 ["col2"] = "baz";
			r2.AcceptChanges ();
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r2), "#2");
			ds.AcceptChanges (); // now r2 original value is "baz"
			r2 ["col2"] = "bar";
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r2), "#3");
			// LAMESPEC: .NET fails here
			DataRow r3 = dt.Rows.Add (new object [] {"foo", "baz"});
			Assert.IsFalse (c.GetHashCode (r1) == c.GetHashCode (r3), "#4");
			*/
		}
	}
}
