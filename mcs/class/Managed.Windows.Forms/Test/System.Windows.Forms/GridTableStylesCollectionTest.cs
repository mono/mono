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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	class GridTableStylesCollectionTests : Assertion
	{
		private bool eventhandled;
		private object Element;
		private CollectionChangeAction Action;

		[TearDown]
		public void Clean() {}

		[SetUp]
		public void GetReady ()
		{
		}

		[Test]
		public void TestDefaultValues ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;

			AssertEquals ("IsSynchronized property", false, sc.IsSynchronized);
			AssertEquals ("SyncRoot property", false, sc.IsSynchronized);
			AssertEquals ("IsReadOnly  property", false, sc.IsSynchronized);
		}

		[Test]
		public void TestAdd ()
		{			
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single
			ResetEventData ();
			DataGridTableStyle ts = new DataGridTableStyle ();
			ts.MappingName = "Table1";
			sc.Add (ts);
			AssertEquals (true, eventhandled);
			AssertEquals (ts, Element);
			AssertEquals (CollectionChangeAction.Add, Action);

			// Add multiple
			ResetEventData ();
			sc.AddRange (new DataGridTableStyle [] {new DataGridTableStyle (), new DataGridTableStyle ()});
			AssertEquals (true, eventhandled);
			AssertEquals (null, Element);
			AssertEquals (CollectionChangeAction.Refresh, Action);
		}

		[Test]
		public void TestRemove ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single
			DataGridTableStyle ts1 = new DataGridTableStyle ();
			ts1.MappingName = "Table1";
			sc.Add (ts1);

			DataGridTableStyle ts2 = new DataGridTableStyle ();
			ts2.MappingName = "Table2";
			sc.Add (ts2);

			DataGridTableStyle ts3 = new DataGridTableStyle ();
			ts3.MappingName = "Table3";
			sc.Add (ts3);

			ResetEventData ();
			sc.Remove (ts2);
			AssertEquals (true, eventhandled);
			AssertEquals (ts2, Element);
			AssertEquals (CollectionChangeAction.Remove, Action);
			AssertEquals (2, sc.Count);

			ResetEventData ();
			sc.RemoveAt (0);
			AssertEquals (true, eventhandled);
			AssertEquals (ts1, Element);
			AssertEquals (CollectionChangeAction.Remove, Action);
			AssertEquals (1, sc.Count);

			ResetEventData ();
			sc.Clear ();
			AssertEquals (null, Element);
			AssertEquals (CollectionChangeAction.Refresh, Action);

		}

		[Test]
		public void TestIndexContains ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single
			DataGridTableStyle ts1 = new DataGridTableStyle ();
			ts1.MappingName = "Table1";
			sc.Add (ts1);

			DataGridTableStyle ts2 = new DataGridTableStyle ();
			ts2.MappingName = "Table2";
			sc.Add (ts2);

			DataGridTableStyle ts3 = new DataGridTableStyle ();
			ts3.MappingName = "Table3";
			sc.Add (ts3);

			ResetEventData ();
			IList ilist = (IList) sc;
			AssertEquals (1, ilist.IndexOf (ts2));
			AssertEquals (false, sc.Contains ("nothing"));
			AssertEquals (true, sc.Contains (ts3));
		}

		private void ResetEventData ()
		{
			eventhandled = false;
			Element = null;
			Action = (CollectionChangeAction) 0;
		}

		private void OnEventHandler (object sender, EventArgs e)
	        {
	            	eventhandled = true;
	        }

	        private void OnCollectionEventHandler (object sender, CollectionChangeEventArgs e)
	        {
	            	eventhandled = true;
	            	Element = e.Element;
			Action = e.Action;			
	        }
	}
}
