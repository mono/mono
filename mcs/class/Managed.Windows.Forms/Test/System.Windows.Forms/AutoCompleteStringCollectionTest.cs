//
//  AutoCompleteStringCollectionTest.cs
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
// Copyright (c) 2006 Daniel Nauck
//
// Author:
//      Daniel Nauck    (dna(at)mono-project(dot)de)

#if NET_2_0

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class AutoCompleteStringCollectionTest : TestHelper
	{
		AutoCompleteStringCollection autoCol = null;
		private int add_event = 0;
		private int refresh_event = 0;
		private int remove_event = 0;
		private string value_event = null;

		[SetUp]
		protected override void SetUp ()
		{
			autoCol = new AutoCompleteStringCollection ();
			autoCol.CollectionChanged += new CollectionChangeEventHandler (AutoColChanged);
			add_event = 0;
			refresh_event = 0;
			remove_event = 0;
			value_event = "unknown item";
			base.SetUp ();
		}

		private void AutoColChanged(object sender, CollectionChangeEventArgs e)
		{
			switch (e.Action)
			{
				case CollectionChangeAction.Add:
					add_event++;
					value_event = (string)e.Element;
					break;
				case CollectionChangeAction.Refresh:
					refresh_event++;
					value_event = (string)e.Element;
					break;
				case CollectionChangeAction.Remove:
					remove_event++;
					value_event = (string)e.Element;
					break;
			}
		}

		[Test]
		public void DefaultProperties ()
		{
			Assert.AreEqual (false, autoCol.IsReadOnly, "#A1");
			Assert.AreEqual (false, ((IList)autoCol).IsFixedSize, "#A2");
			Assert.AreEqual (false, autoCol.IsSynchronized, "#A3");
			Assert.AreEqual (autoCol, autoCol.SyncRoot, "#A4");
			Assert.AreEqual (0, autoCol.Count, "#A5");
		}
        
		[Test]
		public void AddTest ()
		{
			int item1 = autoCol.Add ("Item1");
			Assert.AreEqual (1, add_event, "#B1");
			Assert.AreEqual ("Item1", value_event, "#B2");
			int item2 = autoCol.Add ("Item2");
			Assert.AreEqual (2, add_event, "#B3");
			Assert.AreEqual ("Item2", value_event, "#B4");

			Assert.AreEqual (2, autoCol.Count, "#B5");
			Assert.AreEqual ("Item1", autoCol[item1], "#B6");
			Assert.AreEqual ("Item2", autoCol[item2], "#B7");
		}
        
		[Test]
		public void ClearTest ()
		{
			autoCol.Add ("Item1");
			autoCol.Add ("Item2");
			autoCol.Clear ();
			Assert.AreEqual (1, refresh_event, "#C1");
			Assert.AreEqual (null, value_event, "#C2");
			Assert.AreEqual (0, autoCol.Count, "#C3");
		}
        
		[Test]
		public void ContainsTest ()
		{
			autoCol.Add ("Item1");
			Assert.AreEqual (true, autoCol.Contains("Item1"), "#D1");
			Assert.AreEqual (false, autoCol.Contains("Item2"), "#D2");
		}
        
		[Test]
		public void IndexOfTest ()
		{
			autoCol.Add ("Item1");
			autoCol.Add ("Item2");
			Assert.AreEqual (1, autoCol.IndexOf("Item2"), "#E1");
		}
        
		[Test]
		public void RemoveTest ()
		{
			autoCol.Add ("Item1");
			autoCol.Add ("Item2");
			autoCol.Remove ("Item1");
			Assert.AreEqual(1, remove_event, "#F1");
            		Assert.AreEqual("Item1", value_event, "#F2");
			Assert.AreEqual (1, autoCol.Count, "#F3");
			Assert.AreEqual ("Item2", autoCol[0], "#F4");
		}
        
		[Test]
		public void RemoveAtTest ()
		{
			autoCol.Add ("Item1");
			autoCol.Add ("Item2");
			autoCol.RemoveAt (0);
			Assert.AreEqual(1, remove_event, "#G1");
            		Assert.AreEqual("Item1", value_event, "#G2");
			Assert.AreEqual (1, autoCol.Count, "#G3");
			Assert.AreEqual (true, autoCol.Contains("Item2"), "#G4");
		}
        
		[Test]
		public void AddRangeTest ()
		{
			string[] values = new string[] { "Item1", "Item2", "Item3", "Item4" };
			autoCol.Add ("Item5");
			autoCol.AddRange (values);
			Assert.AreEqual(1, refresh_event, "#E1");
            		Assert.AreEqual(null, value_event, "#E2");
			Assert.AreEqual (5, autoCol.Count, "#E3");
			Assert.AreEqual (true, autoCol.Contains("Item1"), "#E4");
			Assert.AreEqual (true, autoCol.Contains("Item2"), "#E5");
			Assert.AreEqual (true, autoCol.Contains("Item3"), "#E6");
			Assert.AreEqual (true, autoCol.Contains("Item4"), "#E7");
			Assert.AreEqual (true, autoCol.Contains("Item5"), "#E8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddRangeNullTest ()
		{
			string[] values = null;
			autoCol.AddRange (values);
		}

		[Test]
		public void AddTest_Junk ()
		{
			autoCol.Add (null);
			Assert.AreEqual (1, autoCol.Count, "#F1");
			autoCol.Add (null);
			Assert.AreEqual (2, autoCol.Count, "#F2");
			Assert.AreEqual (true, autoCol.Contains(null), "#F3");
			Assert.AreEqual (0, autoCol.IndexOf(null), "#F4");
			autoCol.Remove (null);
			Assert.AreEqual (1, autoCol.Count, "#F5");
			autoCol[0] = "Item1";
			autoCol[0] = null;
			Assert.AreEqual (null, autoCol[0], "#F6");
		}

		[Test]
		public void IndexerTest()
		{
			autoCol.Add("Item1");
			autoCol[0] = "NewItem1";

			Assert.AreEqual(1, remove_event, "#G1");
			Assert.AreEqual(2, add_event, "#G2");
		}
	}
}
#endif