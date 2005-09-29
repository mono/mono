//
// PagedDataSourceTest.cs
//
// Authors:
// 	Duncan Mak (duncan@novell.com)
// 	Gonzalo Paniagua Javier (gonzalo@novell.com)
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
using System.Collections;
using System.Diagnostics;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace MonoTests.System.Web.UI.WebControls {

	[TestFixture]
	public class PagedDataSourceTest {

		PagedDataSource ds;

		[SetUp]
		public void SetUp ()
		{
			ds = new PagedDataSource ();
		}

		public void SetUpTest ()
		{
			Assert.AreEqual (10, ds.PageSize);
			Assert.IsFalse (ds.AllowPaging);
			Assert.AreEqual (0, ds.CurrentPageIndex);
			Assert.IsFalse (ds.AllowCustomPaging);
			Assert.AreEqual (0, ds.VirtualCount);
		}

		public void Reset ()
		{
			ds.DataSource = null;
			ds.PageSize = 10;
			ds.AllowPaging = false;
			ds.CurrentPageIndex = 0;
			ds.AllowCustomPaging = false;
			ds.VirtualCount = 0;
		}

		void SetSource (IEnumerable source)
		{
			Reset ();
			ds.DataSource = source;
		}

		[Test]
		public void GetItemProperties ()
		{
			PagedDataSource ds = new PagedDataSource ();
			DataTable table = new DataTable ();

			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));
			table.Columns.Add (new DataColumn ("three", typeof (string)));

			ds.DataSource = new DataView (table);
			PropertyDescriptorCollection props = ds.GetItemProperties (null);

			Assert.AreEqual (props.Count, 3, "A1");
			Assert.AreEqual (props [0].Name, "one", "A2");
			Assert.AreEqual (props [1].Name, "two", "A3");
			Assert.AreEqual (props [2].Name, "three", "A4");

			ds.DataSource = new ArrayList ();
			props = ds.GetItemProperties (null);
			Assert.AreEqual (props, null, "A5");
		}

		[Test]
		public void GetEnumeratorTest ()
		{
			// Found out that there are 3 possibilities
			// for GetEnumerator () from this test.
			// One for ICollection, one for IList and otherwise, it uses the DataSource directly

			// Hashtable implements ICollection
			SetSource (new Hashtable ());
			//Console.WriteLine (ds.GetEnumerator ().GetType ().Name);

			// IList implementations
			SetSource (new int [] { 1, 2, 3, 4, 5 });
			//Console.WriteLine (ds.GetEnumerator ().GetType ().Name);

			SetSource (new ArrayList ().ToArray ());
			//Console.WriteLine (ds.GetEnumerator ().GetType ().Name);

			// Default case
			SetSource (new MyEnumerable ());
		}

		public class MyEnumerable : IEnumerable, IEnumerator
		{
			IEnumerator IEnumerable.GetEnumerator () { return this; }
			object IEnumerator.Current { get { return null; } }
			bool IEnumerator.MoveNext () { return false; }
			void IEnumerator.Reset () {}
		}

		[Test]
		public void FirstIndexInPageTest ()
		{
			SetSource (null);
			Assert.AreEqual (0, ds.FirstIndexInPage);
			
			SetSource (new int [] { 1, 2, 3, 4, 5 });
			ds.AllowPaging = false;
			Assert.AreEqual (0, ds.FirstIndexInPage);
			
			ds.AllowCustomPaging = false;
			Assert.AreEqual (0, ds.FirstIndexInPage);
			
			ds.AllowPaging = true;
			ds.CurrentPageIndex = 10;
			ds.PageSize = 5;
			Assert.AreEqual (ds.CurrentPageIndex * ds.PageSize, ds.FirstIndexInPage);
		}

		[Test]
		public void PageCountTest ()
		{
			SetSource (null);
			Assert.AreEqual (0, ds.PageCount, "A1");

			SetSource (new int [] {});
			ds.AllowPaging = false;
			Assert.AreEqual (1, ds.PageCount, "A2");

			ds.PageSize = 0;
			Assert.AreEqual (1, ds.PageCount, "A3");

			SetSource (new int [] { 1, 2, 3, 4, 5 });
			ds.AllowPaging = true;
			ds.PageSize = 10;
			Assert.AreEqual (1, ds.PageCount, "A4");

			SetSource (new int [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
			ds.AllowPaging = true;
			ds.PageSize = 10;
			Assert.AreEqual (2, ds.PageCount, "A5");

			SetSource (new int [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
			ds.PageSize = 5;
			ds.AllowPaging = true;
			Assert.AreEqual (3, ds.PageCount, "A6");

			SetSource (new int [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
			ds.AllowPaging = true;
			ds.PageSize = 3;
			Assert.AreEqual (4, ds.PageCount, "A7");
		}

		[Test]
		public void CountTest ()
		{
			SetSource (null);
			Assert.AreEqual (0, ds.Count);

			SetSource (new int [] { 1, 2, 3, 4, 5 });
			ds.AllowPaging = true;

			ds.AllowCustomPaging = true;
			Assert.AreEqual (ds.PageSize, ds.Count);

			// ds.AllowCustomPaging = false;
			// ds.CurrentPageIndex = ds.PageCount;

			// Assert.AreEqual (ds.FirstIndexInPage - ds.DataSourceCount, ds.Count);

			// ds.AllowPaging = false;
			// Assert.AreEqual (ds.DataSourceCount, ds.Count);
		}

		[Test]
		public void IsFirstPageTest ()
		{
			ds.AllowPaging = false;
			ds.CurrentPageIndex = 100;
			Assert.IsTrue (ds.IsFirstPage);
			
			ds.AllowPaging = true;
			ds.CurrentPageIndex = 0;
			Assert.IsTrue (ds.IsFirstPage);

			ds.CurrentPageIndex = 10;
			Assert.IsFalse (ds.IsFirstPage);
		}
				
		[Test]
		public void IsLastPageTest ()
		{
			Reset ();
			ds.AllowPaging = false;
			Assert.IsTrue (ds.IsLastPage);

			// When PageCount is 0, IsLastPage is false
			ds.AllowPaging = true;
			Assert.AreEqual (0, ds.PageCount);
			Assert.IsFalse (ds.IsLastPage);

			SetSource (new int [] { 1, 2, 3 });
			Assert.IsTrue (ds.IsLastPage);
			Assert.IsTrue (ds.IsLastPage == (ds.CurrentPageIndex == ds.PageCount - 1));
			ds.CurrentPageIndex = 3;
			// Assert.IsTrue (ds.IsLastPage);
			
		}

		// Need mucho grande more here
		public void EnumeratorTester (IEnumerator e, string name)
		{			
			int index = 20;

			while (e.MoveNext ()) {
				Assert.AreEqual (e.Current, index, name + "-A1-" + index);
				index++;
			}
			
			Assert.AreEqual (30, index, name + "-A2");
		}

		public void EnumeratorTester_NoPaging (IEnumerator e, string name)
		{			
			int index = 0;

			while (e.MoveNext ()) {
				Assert.AreEqual (e.Current, index, name + "-A1-" + index);
				index++;
			}
			
			Assert.AreEqual (50, index, name + "-A2");
		}

		[Test]
		public void TestEnumerators ()
		{
			PagedDataSource ds = new PagedDataSource ();
			ds.AllowPaging = true;
			ds.PageSize = 10;
			ds.CurrentPageIndex = 2;


			//
			// Collection Enumerator
			//
			Queue q = new Queue ();
			for (int i = 0; i < 50; i++)
				q.Enqueue (i);
			ds.DataSource = q;
			EnumeratorTester (ds.GetEnumerator (), "collection");
			
			//
			// List Enumerator
			//
			ArrayList l = new ArrayList ();
			for (int i = 0; i < 50; i++)
				l.Add (i);
			EnumeratorTester (ds.GetEnumerator (), "list");
		}

		[Test]
		public void TestEnumerators_NoPaging ()
		{
			PagedDataSource ds = new PagedDataSource ();
			ds.AllowPaging = false;

			//
			// Collection Enumerator
			//
			Queue q = new Queue ();
			for (int i = 0; i < 50; i++)
				q.Enqueue (i);
			ds.DataSource = q;
			EnumeratorTester_NoPaging (ds.GetEnumerator (), "collection");
			
			//
			// List Enumerator
			//
			ArrayList l = new ArrayList ();
			for (int i = 0; i < 50; i++)
				l.Add (i);
			EnumeratorTester_NoPaging (ds.GetEnumerator (), "list");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullSource ()
		{
			PagedDataSource ds = new PagedDataSource ();
			ds.DataSource = null;
			IEnumerator data = ds.GetEnumerator ();
		}

		static void FillTable (DataTable table, int nelems)
		{
			table.Columns.Add (new DataColumn ("one", typeof (string)));
			table.Columns.Add (new DataColumn ("two", typeof (string)));

			for (int i = 0; i < nelems; i++) {
				DataRow row = table.NewRow ();
				row ["one"] = i % 2;
				row ["two"] = i / 2;
				table.Rows.Add (row);
			}
		}

		[Test]
		public void Paging1 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 5;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			Assert.IsTrue (paged.IsFirstPage, "first-1");
			Assert.IsFalse (paged.IsLastPage, "last-1");

			paged.CurrentPageIndex = 100; // no problem setting this.
			Assert.AreEqual (100, paged.CurrentPageIndex, "current-1");
			Assert.IsFalse (paged.IsFirstPage, "first-2");
			Assert.IsFalse (paged.IsLastPage, "last-2");
			IEnumerator rator = paged.GetEnumerator ();
			Assert.IsFalse (rator.MoveNext (), "beyondtheend-1");
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Paging2 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 5;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			paged.CurrentPageIndex = -1;
			Assert.AreEqual (-1, paged.CurrentPageIndex, "current");
			Assert.IsFalse (paged.IsFirstPage, "first");
			Assert.IsFalse (paged.IsLastPage, "last");
			IEnumerator rator = paged.GetEnumerator ();
			Assert.AreEqual (-1, paged.CurrentPageIndex, "current-2");
			Assert.IsTrue (rator.MoveNext (), "beyondtheend");
			DataRowView drv = (DataRowView) rator.Current; // Throws (out of range)
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Paging3 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 5;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			paged.CurrentPageIndex = -7;
			Assert.AreEqual (-7, paged.CurrentPageIndex, "current");
			Assert.IsFalse (paged.IsFirstPage, "first");
			Assert.IsFalse (paged.IsLastPage, "last");
			IEnumerator rator = paged.GetEnumerator ();
			Assert.AreEqual (-7, paged.CurrentPageIndex, "current-2");
			Assert.IsTrue (rator.MoveNext (), "beyondtheend");
			DataRowView drv = (DataRowView) rator.Current; // Throws (out of range)
		}

		[Test]
		public void Paging4 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 5;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			paged.CurrentPageIndex = 1;
			IEnumerator rator = paged.GetEnumerator ();
			Assert.IsTrue (rator.MoveNext (), "beginning-1");
			DataRowView drv = (DataRowView) rator.Current;
			int one = Int32.Parse ((string) drv ["one"]);
			Assert.IsTrue (one == 0 || one == 1, "one-1");
			int res =  one + 2 * Int32.Parse ((string) drv ["two"]);
			Assert.AreEqual (5, res, "five");
		}

		[Test]
		public void Copy1 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);
			object [] data = new object [100];
			paged.CopyTo (data, 0);
			Type t = typeof (DataRowView);
			Assert.AreEqual (t, data [0].GetType ());
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Copy2 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.DataSource = null;
			object [] data = new object [100];
			paged.CopyTo (data, 0);
		}

		[Test]
		public void Copy3 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.DataSource = new object [] {"1", "2"};
			object [] data = new object [100];
			paged.CopyTo (data, 0);
		}

		[Test]
		public void VirtualPager1 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 20;
			paged.VirtualCount = 100;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			int count = 0;
			IEnumerator rator = paged.GetEnumerator ();
			while (rator.MoveNext ())
				count++;
			Assert.AreEqual (20, count, "count");
			Assert.AreEqual (true, paged.IsFirstPage, "first");
			Assert.AreEqual (false, paged.IsLastPage, "last");
		}

		[Test]
		public void VirtualPager2 ()
		{
			PagedDataSource paged = new PagedDataSource ();
			paged.AllowPaging = true;
			paged.PageSize = 100;
			paged.VirtualCount = 50;
			paged.AllowCustomPaging = true;
			DataTable table = new DataTable ();
			FillTable (table, 100);
			paged.DataSource = new DataView (table);

			int count = 0;
			IEnumerator rator = paged.GetEnumerator ();
			while (rator.MoveNext ())
				count++;
			Assert.AreEqual (100, count, "count");
			Assert.AreEqual (true, paged.IsFirstPage, "first");
			Assert.AreEqual (true, paged.IsLastPage, "last");
		}
	}
}

