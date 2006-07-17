// DataViewTest_IBindingListView.cs - Nunit Test Cases for for testing the DataView
// class for IBindingListView Implementation
//
// Authors:
//      Senganal T <tsenganal@novell.com>
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

#if NET_2_0

using System;
using System.IO;
using System.Data;
using System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.Data
{	
	[TestFixture]
	public class DataViewTest_IBindingListView
	{
		DataTable table = null;
		
		[TestFixtureSetUp]
		public void TestFixtureSetUp ()
		{
			table = new DataTable ("table");
			table.Columns.Add ("col1", typeof(int));
			table.Columns.Add ("col2", typeof(int));
			table.Columns.Add ("col3", typeof(int));
			
			table.Rows.Add (new object[] { 1, 1, 1 });
			table.Rows.Add (new object[] { 1, 1, 2 });
			table.Rows.Add (new object[] { 1, 2, 1 });
			table.Rows.Add (new object[] { 1, 2, 2 });
			table.Rows.Add (new object[] { 2, 0, 0 });
			table.Rows.Add (new object[] { 2, 1, 0 });
			table.Rows.Add (new object[] { 2, 1, 1 });
			table.Rows.Add (new object[] { 2, 1, 2 });
			table.Rows.Add (new object[] { 2, 2, 1 });
			table.Rows.Add (new object[] { 2, 2, 2 });
		}
		
		[Test]
		public void FilterTest()
		{
			IBindingListView view = (IBindingListView) new DataView (table);
			
			view.Filter = "";
			Assert.AreEqual (table.Rows.Count, view.Count, "#1");
			
			view.Filter = null;
			Assert.AreEqual (table.Rows.Count, view.Count, "#2");
			
			view.Filter = "col1 <> 1";
			Assert.AreEqual (view.Filter, ((DataView)view).RowFilter, "#4");
			Assert.AreEqual (6, view.Count, "#5");
			
			//RemoveFilter Test
			view.RemoveFilter ();
			Assert.AreEqual ("", view.Filter, "#6");
			Assert.AreEqual ("", ((DataView)view).RowFilter, "#7");
			Assert.AreEqual (table.Rows.Count, view.Count, "#8");
		}
		
		[Test]
		public void SortDescriptionTest()
		{
			IBindingListView view = (IBindingListView)new DataView (table);
			
			ListSortDescriptionCollection col = view.SortDescriptions;
			
			((DataView)view).Sort = "";
			col = view.SortDescriptions;
			Assert.AreEqual (0, col.Count, "#1");
			
			((DataView)view).Sort = null;
			col = view.SortDescriptions;
			Assert.AreEqual (0, col.Count, "#2");
			
			((DataView)view).Sort = "col1 DESC, col2 ASC";
			col = view.SortDescriptions;
			Assert.AreEqual (2, col.Count, "#3");
			Assert.AreEqual ("col1", col[0].PropertyDescriptor.Name, "#4");
			Assert.AreEqual (ListSortDirection.Descending, col[0].SortDirection, "#5");
			Assert.AreEqual ("col2", col[1].PropertyDescriptor.Name, "#6");
			Assert.AreEqual (ListSortDirection.Ascending, col[1].SortDirection, "#7");
			
			//ApplySort Test
			IBindingListView view1 = (IBindingListView)new DataView (table);
			
			Assert.IsFalse (view.Equals(view1), "#8");
			view1.ApplySort (col);
			Assert.AreEqual ("[col1] DESC,[col2]", ((DataView)view1).Sort, "#9");
			for (int i = 0; i < view.Count; ++i)
				Assert.AreEqual (((DataView)view)[i].Row, ((DataView)view1)[i].Row, "#10"+i);      
		}
		
		[Test]
		public void ReadOnlyPropTest()
		{
			IBindingListView view = (IBindingListView)new DataView (table);
			Assert.IsTrue (view.SupportsAdvancedSorting, "#1");
			Assert.IsTrue (view.SupportsFiltering, "#2");
		}
	}
}
#endif
