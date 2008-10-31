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
	public class GridTableStylesCollectionTest : TestHelper
	{
		private bool eventhandled;
		private object Element;
		private CollectionChangeAction Action;
		private int times;

		[Test]
		public void TestDefaultValues ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;

			Assert.AreEqual (false, sc.IsSynchronized, "IsSynchronized property");
			Assert.AreEqual (0, sc.Count, "Count");
			Assert.AreEqual (sc, sc.SyncRoot, "SyncRoot property");
			Assert.AreEqual (false, ((IList)sc).IsFixedSize, "IsFixedSize property");
			Assert.AreEqual (false, sc.IsReadOnly, "IsReadOnly property");
		}

		[Test]
		public void TestMappingNameChanged ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single
			DataGridTableStyle ts = new DataGridTableStyle ();
			ts.MappingName = "Table1";
			sc.Add (ts);
			ResetEventData ();
			ts.MappingName = "Table2";

			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (null, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Refresh, Action, "A3");
		}

		// Fails due to:
		// "The TableStyles collection already has a TableStyle with this mapping name"
		// There is a TODO in DataGrid about allowing TableStyles to have the same mapping name.
		[Test]
		[NUnit.Framework.Category ("NotWorking")]
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
			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (ts, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Add, Action, "A3");

			// Add multiple
			ResetEventData ();
			sc.AddRange (new DataGridTableStyle [] {new DataGridTableStyle (), new DataGridTableStyle ()});
			Assert.AreEqual (true, eventhandled, "A4");
			Assert.AreEqual (null, Element, "A5");
			Assert.AreEqual (CollectionChangeAction.Refresh, Action, "A6");
		}

		[Test]
		public void TestAddRange ()
		{
			DataGrid grid = new DataGrid ();
			GridTableStylesCollection sc = grid.TableStyles;		
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			ResetEventData ();
			DataGridTableStyle ts1 = new DataGridTableStyle ();
			ts1.MappingName = "Table1";

			DataGridTableStyle ts2 = new DataGridTableStyle ();
			ts2.MappingName = "Table2";
			sc.AddRange (new DataGridTableStyle[] {ts1, ts2});

			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (null, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Refresh, Action, "A3");
			Assert.AreEqual (1, times, "A4");
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
			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (ts2, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Remove, Action, "A3");
			Assert.AreEqual (2, sc.Count, "A4");

			ResetEventData ();
			sc.RemoveAt (0);
			Assert.AreEqual (true, eventhandled, "A5");
			Assert.AreEqual (ts1, Element, "A6");
			Assert.AreEqual (CollectionChangeAction.Remove, Action, "A7");
			Assert.AreEqual (1, sc.Count, "A8");

			ResetEventData ();
			sc.Clear ();
			Assert.AreEqual (null, Element, "A9");
			Assert.AreEqual (CollectionChangeAction.Refresh, Action, "A10");

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
			Assert.AreEqual (1, ilist.IndexOf (ts2), "A1");
			Assert.AreEqual (false, sc.Contains ("nothing"), "A2");
			Assert.AreEqual (true, sc.Contains (ts3), "A3");
		}

		private void ResetEventData ()
		{
			times = 0;
			eventhandled = false;
			Element = null;
			Action = (CollectionChangeAction) 0;
		}

		//private void OnEventHandler (object sender, EventArgs e)
		//{
		//        eventhandled = true;
		//}

	        private void OnCollectionEventHandler (object sender, CollectionChangeEventArgs e)
	        {
			times++;
	            	eventhandled = true;
	            	Element = e.Element;
			Action = e.Action;
	        }

	}
}
