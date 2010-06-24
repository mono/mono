//
// Tests for System.Web.UI.WebControls.ListItemCollection.cs 
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//

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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class ListItemCollectionTest 
	{
		[Test]
		public void Methods ()
		{
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			Assert.AreEqual (0, c.Count, "T1");

			i = new ListItem("Item 1", "10");
			c.Add(i);
			Assert.AreEqual (1, c.Count, "T2");

			i = new ListItem("This is item 2", "20");
			c.Add(i);
			Assert.AreEqual (2, c.Count, "T3");

			Assert.AreEqual (null, c.FindByText(" is "), "T4");
			Assert.AreEqual (i.Text, c.FindByText("This is item 2").Text, "T5");
			Assert.AreSame (i, c.FindByText("This is item 2"), "T6");
			Assert.AreEqual (1, c.IndexOf(c.FindByText("This is item 2")), "T7");
			Assert.AreEqual (1, c.IndexOf(c.FindByValue("20")), "T8");

			i = new ListItem("Item 3", "30");
			Assert.IsFalse(c.Contains(i), "T9");
			c.Add(i);
			Assert.IsTrue(c.Contains(i), "T10");

			i = new ListItem("Forth", "40");
			i2 = new ListItem("Fifth", "50");
			c.AddRange(new ListItem[] {i, i2});
			Assert.AreEqual (5, c.Count, "T11");

			c.RemoveAt(1);
			Assert.AreEqual (4, c.Count, "T12");
			Assert.AreEqual (null, c.FindByText("This is item 2"), "T13");

			c.Clear();
			Assert.AreEqual (0, c.Count, "T13");
		}

		[Test]
		public void IListTest () 
		{
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			Assert.AreEqual(0, c.Count, "I1");
			((IList)c).Add(i);
			Assert.AreEqual(1, c.Count, "I2");
			((IList)c).Clear();
			Assert.AreEqual(0, c.Count, "I3");

			((IList)c).Add(i);
			((IList)c).Add(i2);
			Assert.AreEqual(1, ((IList)c).IndexOf(i2), "I4");

			((IList)c).RemoveAt(1);
			Assert.AreEqual(1, c.Count, "I5");

			Assert.AreEqual(true, ((IList)c).Contains(i), "I6");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void RemoveInvalidTest () 
		{
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add(i);
			c.Add(i2);

			c.RemoveAt(20);
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void AddInvalidTest () 
		{
			ListItemCollection	c;

			c = new ListItemCollection();

			((IList)c).Add(5);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void AccessInvalidTest () {
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			i = c[3];
		}

		[Test]
		public void AddTest () 
		{
			ListItemCollection	c;
			ListItem		i;

			c = new ListItemCollection();

			c.Add("string");

			Assert.AreEqual("string", c.FindByText("string").ToString(), "A1");

			c.Add((string)null);
			c.Add((ListItem)null);
			Assert.AreEqual(3, c.Count, "A2");
		}

		[Test]
		public void AssignmentTest () {
			ListItemCollection	c;
			ListItem		i;

			c = new ListItemCollection();
			i = new ListItem("Text", "Value");

			c.Add(i);

			i = new ListItem("Blah", "Argl");
			((IList)c)[0] = i;

			Assert.AreEqual("Blah", c.FindByText("Blah").ToString(), "AS1");
			Assert.AreEqual(1, c.Count, "AS2");
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void AssignmentExceptionTest () {
			ListItemCollection	c;
			ListItem		i;

			c = new ListItemCollection();
			i = new ListItem("Text", "Value");

			c.Add(i);

			((IList)c)[0] = 5;
		}

		[Test]
		public void ContainsTest ()
		{
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add(i);
			c.Add(i2);

			i2 = new ListItem("Item 1", "1");

			// test same vs equal
			Assert.AreEqual (true, c.Contains(i), "C1");
			Assert.AreEqual (true, c.Contains(i2), "C2");
		}

		[Test]
		public void IndexOfTest () {
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add(i);
			c.Add(i2);

			i = new ListItem("Item 2", "2");

			// test same vs equal
			Assert.AreEqual (1, c.IndexOf(i), "IO1");
			Assert.AreEqual (1, c.IndexOf(i2), "IO2");
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void ContainsTypeTest () {
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add(i);
			c.Add(i2);

			Assert.AreEqual (false, ((IList)c).Contains(5), "CT1");
		}

		[Test]
		public void RemoveTest () {
			ListItemCollection	c;
			ListItem		i;
			ListItem		i2;

			c = new ListItemCollection();
			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add(i);
			c.Add(i2);

			i = new ListItem("Item 2", "2");

			// test same vs equal
			c.Remove(i);
			Assert.AreEqual (1, c.Count, "R1");
		}

		[Test]
		public void ViewState () 
		{
			ListItemCollection	c;
			ListItemCollection	c2;
			ListItem		i;
			ListItem		i2;
			object			state;

			c = new ListItemCollection();
			state = ((IStateManager) c).SaveViewState ();
			Assert.IsNull (state, "#A1");

			i = new ListItem("Item 1", "1");
			i2 = new ListItem("Item 2", "2");

			c.Add (i);
			c.Add (i2);
			state = ((IStateManager) c).SaveViewState ();
			Assert.IsNull (state, "#A2");

			c = new ListItemCollection ();
			((IStateManager)c).TrackViewState();

			c.Add(i);
			c.Add(i2);
			Assert.AreEqual (2, c.Count, "V1");

			state = ((IStateManager)c).SaveViewState();
			Assert.IsNotNull (state, "#A3");

			c2 = new ListItemCollection();
			((IStateManager)c2).LoadViewState(state);
			Assert.AreEqual (2, c2.Count, "V2");
			Assert.AreEqual ("Item 1", c2.FindByText("Item 1").ToString(), "V3");
			Assert.AreEqual ("Item 2", c2.FindByText("Item 2").ToString(), "V4");
			Assert.AreEqual (false, c2.IndexOf(i) == c2.IndexOf(i2), "V5");
		}
	}
}
