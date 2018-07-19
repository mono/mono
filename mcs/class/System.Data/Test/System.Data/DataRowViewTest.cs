//
// DataRowViewTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2005 Novell Inc,
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowViewTest 
	{
		private DataView CreateTestView ()
		{
			DataTable dt1 = new DataTable ("table1");
			DataColumn c1 = new DataColumn ("col");
			dt1.Columns.Add (c1);
			dt1.Rows.Add (new object [] {"1"});
			return new DataView (dt1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChildViewNullStringArg ()
		{
			DataView dv = CreateTestView ();
			DataRowView dvr = dv [0];
			dvr.CreateChildView ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChildViewNullDataRelationArg ()
		{
			DataView dv = CreateTestView ();
			DataRowView dvr = dv [0];
			dvr.CreateChildView ((DataRelation) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateChildViewNonExistentName ()
		{
			DataView dv = CreateTestView ();
			DataRowView dvr = dv [0];
			dvr.CreateChildView ("nothing");
		}

		[Test]
		public void CreateChildViewSimple ()
		{
			DataSet ds = new DataSet ();

			DataTable dt1 = new DataTable ("table1");
			ds.Tables.Add (dt1);
			DataColumn c1 = new DataColumn ("col");
			dt1.Columns.Add (c1);
			dt1.Rows.Add (new object [] {"1"});

			DataTable dt2 = new DataTable ("table2");
			ds.Tables.Add (dt2);
			DataColumn c2 = new DataColumn ("col");
			dt2.Columns.Add (c2);
			dt2.Rows.Add (new object [] {"1"});

			DataRelation dr = new DataRelation ("dr", c1, c2);

			DataView dv = new DataView (dt1);
			DataRowView dvr = dv [0];
			DataView v = dvr.CreateChildView (dr);
			Assert.AreEqual ("", v.RowFilter, "RowFilter");
			Assert.AreEqual ("", v.Sort, "Sort");
		}

		[Test]
		public void IsEdit ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col");
			dt.Rows.Add ((new object [] {"val"}));

			DataView dv = new DataView (dt);
			DataRowView drv = dv [0];
			dt.Rows [0].BeginEdit ();
			Assert.AreEqual (true, drv.IsEdit, "DataView.Item");

			drv = dv.AddNew ();
			drv.Row ["col"] = "test";
			drv.Row.CancelEdit ();
			Assert.AreEqual (false, drv.IsEdit, "AddNew");
		}

		[Test]
		public void Item ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col");
			dt.Rows.Add ((new object [] {"val"}));
			DataView dv = new DataView (dt);
			DataRowView drv = dv [0];
			dt.Rows [0].BeginEdit ();
			Assert.AreEqual ("val", drv ["col"], "DataView.Item");
		}

		[Test]
		[ExpectedException (typeof (RowNotInTableException))]
		public void ItemException ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col");
			dt.Rows.Add ((new object [] {"val"}));
			DataView dv = new DataView (dt);
			DataRowView drv = dv.AddNew ();
			drv.Row ["col"] = "test";
			drv.Row.CancelEdit ();
			object o = drv ["col"];
		}

		[Test]
		public void RowVersion1 ()
		{
			// I guess we could write better tests.
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col");
			dt.Rows.Add (new object [] {1});
			DataView dv = new DataView (dt);
			DataRowView drv = dv.AddNew ();
			Assert.AreEqual (DataRowVersion.Current, drv.RowVersion);
			Assert.AreEqual (DataRowVersion.Current, dv [0].RowVersion);
			drv ["col"] = "mod";
			Assert.AreEqual (DataRowVersion.Current, drv.RowVersion);
			Assert.AreEqual (DataRowVersion.Current, dv [0].RowVersion);
			dt.AcceptChanges ();
			Assert.AreEqual (DataRowVersion.Current, drv.RowVersion);
			Assert.AreEqual (DataRowVersion.Current, dv [0].RowVersion);
			drv.EndEdit ();
			dv [0].EndEdit ();
			Assert.AreEqual (DataRowVersion.Current, drv.RowVersion);
			Assert.AreEqual (DataRowVersion.Current, dv [0].RowVersion);
		}
	}
}
