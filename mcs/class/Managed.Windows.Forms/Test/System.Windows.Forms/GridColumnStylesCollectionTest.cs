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
	class GridColumnStylesCollectionTests : Assertion
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
			DataGridTableStyle ts = new DataGridTableStyle ();
			GridColumnStylesCollection sc = ts.GridColumnStyles;

			AssertEquals ("IsSynchronized property", false, sc.IsSynchronized);
			AssertEquals ("SyncRoot property", false, sc.IsSynchronized);
			AssertEquals ("IsReadOnly  property", false, sc.IsSynchronized);
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
			AssertEquals (true, eventhandled);
			AssertEquals (col1, Element);
			AssertEquals (CollectionChangeAction.Add, Action);

			// Add multiple
			ResetEventData ();
			DataGridTextBoxColumn elem1 = new DataGridTextBoxColumn ();
			DataGridTextBoxColumn elem2 = new DataGridTextBoxColumn ();
			sc.AddRange (new DataGridTextBoxColumn [] {elem1, elem2});
			AssertEquals (true, eventhandled);
			AssertEquals (CollectionChangeAction.Add, Action);
			AssertEquals (elem2, Element);
			
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
			AssertEquals (true, eventhandled);
			AssertEquals (col2, Element);
			AssertEquals (CollectionChangeAction.Remove, Action);
			AssertEquals (2, sc.Count);

			ResetEventData ();
			sc.RemoveAt (0);
			AssertEquals (true, eventhandled);
			AssertEquals (col1, Element);
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
			AssertEquals (1, ilist.IndexOf (col2));
			AssertEquals (false, sc.Contains ("nothing"));
			AssertEquals (true, sc.Contains (col3));
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
