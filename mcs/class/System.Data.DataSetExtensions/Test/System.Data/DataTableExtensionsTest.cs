//
// DataTableExtensionsTest.cs
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
using System.Linq;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableExtensionsTest
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // for no rows
		public void CopyToDataTableNoArgNoRows ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("CID", typeof (int));
			dt.Columns.Add ("CName", typeof (string));
			dt.AsEnumerable ().CopyToDataTable<DataRow> ();
		}

		[Test]
		public void CopyToDataTableNoArg ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("CID", typeof (int));
			dt.Columns.Add ("CName", typeof (string));
			dt.Rows.Add (new object [] {1, "foo"});
			DataTable dst = dt.AsEnumerable ().CopyToDataTable<DataRow> ();
			Assert.AreEqual (1, dst.Rows.Count, "#1");
			Assert.AreEqual ("foo", dst.Rows [0] ["CName"], "#2");
		}

		[Test]
		// no error for empty table this time.
		[Category ("NotWorking")] // some DataTableReader internal issues
		public void CopyToDataTableTableArgNoRows ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("CID", typeof (int));
			dt.Columns.Add ("CName", typeof (string));
			DataTable dst = new DataTable ();
			dt.AsEnumerable ().CopyToDataTable<DataRow> (dst, LoadOption.PreserveChanges);
		}

		[Test]
		public void AsEnumerable ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/testdataset1.xml"));
			DataTable dt = ds.Tables [0];
			Assert.AreEqual ("ScoreList", dt.TableName, "TableName");
			var dv = dt.AsEnumerable ();
			Assert.AreEqual (4, dv.Count (), "#0");
			var i = dv.GetEnumerator ();
			Assert.IsTrue (i.MoveNext (), "#1");
			Assert.AreEqual (1, i.Current ["ID"], "#2");
			Assert.IsTrue (i.MoveNext (), "#3");
			Assert.AreEqual (2, i.Current ["ID"], "#4");
		}

#if !COREFX //LinqDataView is not supported yet
		[Test]
		public void AsDataView ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/testdataset1.xml"));
			DataTable dt = ds.Tables [0];
			var dv = dt.AsEnumerable ().Where<DataRow> ((DataRow r) => (int) r ["Score"] > 60).AsDataView<DataRow> ();
			Assert.AreEqual (1, dv [0] ["ID"], "#1");
			Assert.AreEqual (4, dv [1] ["ID"], "#2");
		}
#endif
	}
}
