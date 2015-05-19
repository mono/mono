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
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class GridColumnStylesCollectionTest : TestHelper
	{
		private bool eventhandled;
		private object Element;
		private CollectionChangeAction Action;
		private int times;


		[Test]
		public void TestDefaultValues ()
		{
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;

			Assert.AreEqual (false, sc.IsSynchronized, "IsSynchronized property");
			Assert.AreEqual (0, sc.Count, "Count");
			Assert.AreEqual (sc, sc.SyncRoot, "SyncRoot property");
			Assert.AreEqual (false, ((IList)sc).IsFixedSize, "IsFixedSize property");
			Assert.AreEqual (false, sc.IsReadOnly, "IsReadOnly property");
		}

		[Test]
		public void TestAdd ()
		{
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;			
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single
			ResetEventData ();
			DataGridTextBoxColumn col1 = new DataGridTextBoxColumn ();
			col1.MappingName = "Column1";
			sc.Add (col1);
			Assert.AreEqual (true, eventhandled);
			Assert.AreEqual (col1, Element);
			Assert.AreEqual (CollectionChangeAction.Add, Action);

			// Add multiple
			ResetEventData ();
			DataGridTextBoxColumn elem1 = new DataGridTextBoxColumn ();
			DataGridTextBoxColumn elem2 = new DataGridTextBoxColumn ();
			sc.AddRange (new DataGridTextBoxColumn [] {elem1, elem2});
			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (CollectionChangeAction.Add, Action, "A2");
			Assert.AreEqual (elem2, Element, "A3");
			
		}

		// The idea of this test is to have a complete DataGrid system
		// and assert that the Add method of the colleciton is _not_ forcing a 
		// call to the protected CheckValidSource method in the new column style class.
		// Bug #465019.
		[Test]
		public void TestAddWithBindingContext ()
		{
			DataGrid datagrid = new DataGrid ();
			datagrid.BindingContext = new BindingContext ();
			DataTable table = new DataTable ();
			datagrid.DataSource = table;

			DataGridTableStyle ts = new DataGridTableStyle ();
			datagrid.TableStyles.Add (ts);

			DataGridTextBoxColumn col1 = new DataGridTextBoxColumn ();
			col1.MappingName = "Column1"; // Not valid mapping
			ts.GridColumnStyles.Add (col1);

			// More important: we should _not_ throw an exc here.
			Assert.AreEqual (ts, col1.DataGridTableStyle, "#A1");
			Assert.AreEqual (1, ts.GridColumnStyles.Count, "#A2");
		}

		[Test]
		public void TestAddRange ()
		{
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;			
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			ResetEventData ();
			DataGridTextBoxColumn col1 = new DataGridTextBoxColumn ();
			col1.MappingName = "Column1";

			DataGridTextBoxColumn col2 = new DataGridTextBoxColumn ();
			col2.MappingName = "Column2";
			sc.AddRange (new DataGridColumnStyle[] {col1, col2});

			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (col2, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Add, Action, "A3");
			Assert.AreEqual (2, times, "A4");
		}

		[Test]
		public void TestRemove ()
		{
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;			
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single			
			DataGridTextBoxColumn col1 = new DataGridTextBoxColumn ();
			col1.MappingName = "Column1";
			sc.Add (col1);
			
			DataGridTextBoxColumn col2 = new DataGridTextBoxColumn ();
			col2.MappingName = "Column2";
			sc.Add (col2);
			
			DataGridTextBoxColumn col3 = new DataGridTextBoxColumn ();
			col3.MappingName = "Column3";
			sc.Add (col3);

			ResetEventData ();
			sc.Remove (col2);
			Assert.AreEqual (true, eventhandled, "A1");
			Assert.AreEqual (col2, Element, "A2");
			Assert.AreEqual (CollectionChangeAction.Remove, Action, "A3");
			Assert.AreEqual (2, sc.Count, "A4");

			ResetEventData ();
			sc.RemoveAt (0);
			Assert.AreEqual (true, eventhandled, "A5");
			Assert.AreEqual (col1, Element, "A6");
			Assert.AreEqual (CollectionChangeAction.Remove, Action, "A6");
			Assert.AreEqual (1, sc.Count, "A7");

			ResetEventData ();
			sc.Clear ();
			Assert.AreEqual (null, Element, "A8");
			Assert.AreEqual (CollectionChangeAction.Refresh, Action, "A9");

		}

		[Test]
		public void TestIndexContains ()
		{
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;			
			sc.CollectionChanged += new CollectionChangeEventHandler (OnCollectionEventHandler);

			// Add single			
			DataGridTextBoxColumn col1 = new DataGridTextBoxColumn ();
			col1.MappingName = "Column1";
			sc.Add (col1);
			
			DataGridTextBoxColumn col2 = new DataGridTextBoxColumn ();
			col2.MappingName = "Column2";
			sc.Add (col2);
			
			DataGridTextBoxColumn col3 = new DataGridTextBoxColumn ();
			col3.MappingName = "Column3";
			sc.Add (col3);

			ResetEventData ();
			IList ilist = (IList) sc;
			Assert.AreEqual (1, ilist.IndexOf (col2), "A1");
			Assert.AreEqual (false, sc.Contains ("nothing"), "A2");
			Assert.AreEqual (true, sc.Contains (col3), "A3");
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
	            	eventhandled = true;
	            	Element = e.Element;
			Action = e.Action;
			times++;
	        }

		
	}

}
