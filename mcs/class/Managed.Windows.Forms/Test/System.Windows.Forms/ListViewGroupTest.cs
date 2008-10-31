//
//  ListViewGroupTest.cs
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
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListViewGroupTest : TestHelper
	{
		ListView lv = null;

		[SetUp]
		protected override void SetUp () {
			lv = new ListView ();
			base.SetUp ();
		}

		[Test]
		public void DefaultProperties ()
		{
			//default ListView properties for groups
			Assert.AreEqual (true, lv.ShowGroups, "#A1");
			Assert.AreEqual (true, (lv.Groups != null), "#A2");
			Assert.AreEqual (0, lv.Groups.Count, "#A3");

			//default ListViewGroup properties
			ListViewGroup lg1 = new ListViewGroup ();
			Assert.AreEqual ("ListViewGroup", lg1.Header, "#A4");
			Assert.AreEqual (null, lg1.Name, "#A5");
			Assert.AreEqual (HorizontalAlignment.Left, lg1.HeaderAlignment, "#A6");
			Assert.AreEqual (0, lg1.Items.Count, "#A7");
			Assert.AreEqual (null, lg1.ListView, "#A8");
			Assert.AreEqual (null, lg1.Tag, "#A9");
			Assert.AreEqual (lg1.Header, lg1.ToString(), "#A10");
		}

		[Test]
		public void AddTest ()
		{
			ListViewGroup lg1 = new ListViewGroup ();
			lg1.Items.Add ("Item1");
			Assert.AreEqual (1, lg1.Items.Count, "#B1");
			Assert.AreEqual (null, lg1.Items[0].ListView, "#B2");

			lv.Groups.Add (lg1);
			Assert.AreEqual (null, lg1.Items[0].ListView, "#B3");
			Assert.AreEqual (false, lv.Items.Contains(lg1.Items[0]), "#B4");

			ListViewItem lvi = lg1.Items.Add ("Item1");
			Assert.AreEqual (null, lvi.ListView, "#C1");
			Assert.AreEqual (lg1, lvi.Group, "#C2");
		}

        	[Test]
        	public void RemoveTest ()
        	{
            		ListViewGroup lg1 = new ListViewGroup ();
            		lg1.Items.Add ("Item1");
            		lv.Groups.Add (lg1);
            		lv.Groups.Remove (lg1);

            		Assert.AreEqual (1, lg1.Items.Count, "#C1");
            		Assert.AreEqual (0, lv.Items.Count, "#C2");
			Assert.AreEqual (false, lv.Items.Contains (lg1.Items [0]), "#C3");

			lg1.Items.Clear ();
			lv.Groups.Add (lg1);
			ListViewItem lvi = lv.Items.Add ("Item1");
			lg1.Items.Add (lvi);

			Assert.AreEqual (1, lg1.Items.Count, "#D1");
			Assert.AreEqual (1, lv.Items.Count, "#D2");
			Assert.AreEqual (lv, lvi.ListView, "#D3");
			Assert.AreEqual (lg1, lvi.Group, "#D4");
				
			lg1.Items.Remove (lvi);
				
			Assert.AreEqual (0, lg1.Items.Count, "#E1");
			Assert.AreEqual (1, lv.Items.Count, "#E2");
			Assert.AreEqual (lv, lvi.ListView, "#E3");
			Assert.AreEqual (null, lvi.Group, "#E4");
        	}
	}
}
#endif
