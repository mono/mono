//
// Tests for System.Web.UI.WebControls.DataGridItemCollection
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
	public class DataGridItemCollectionTest {
		[Test]
		public void Defaults ()
		{
			DataGridItemCollection	c;
			ArrayList		list;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			c = new DataGridItemCollection(list);

			Assert.AreEqual(1, c.Count, "D1");
			Assert.AreEqual(item, c[0], "D2");

			// Copy or ref?
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			Assert.AreEqual(2, c.Count, "D3");
			Assert.AreEqual(ListItemType.Header, c[1].ItemType, "D4");
		}

		[Test]
		public void Copy () {
			DataGridItemCollection	c;
			ArrayList		list;
			DataGridItem[]		copy;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			item = new DataGridItem(2, 2, ListItemType.Footer);
			list.Add(item);

			c = new DataGridItemCollection(list);

			copy = new DataGridItem[3];
			Assert.AreEqual(3, c.Count, "C1");
			c.CopyTo(copy, 0);
			Assert.AreEqual(3, copy.Length, "C2");

			copy = new DataGridItem[4];
			c.CopyTo(copy, 1);
			Assert.AreEqual(4, copy.Length, "C3");
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void OutOfBounds () {
			DataGridItemCollection	c;
			ArrayList		list;
			DataGridItem[]		copy;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			item = new DataGridItem(2, 2, ListItemType.Footer);
			list.Add(item);

			c = new DataGridItemCollection(list);

			copy = new DataGridItem[2];
			c.CopyTo(copy, 0);
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void OutOfBounds2 () {
			DataGridItemCollection	c;
			ArrayList		list;
			DataGridItem[]		copy;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			item = new DataGridItem(2, 2, ListItemType.Footer);
			list.Add(item);

			c = new DataGridItemCollection(list);

			copy = new DataGridItem[3];
			c.CopyTo(copy, 1);
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void BadTypeCopy () {
			DataGridItemCollection	c;
			ArrayList		list;
			Array			copy;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			item = new DataGridItem(2, 2, ListItemType.Footer);
			list.Add(item);

			c = new DataGridItemCollection(list);

			copy = new Array[2];
			c.CopyTo(copy, 0);
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void BadTypeCopy2 () {
			DataGridItemCollection	c;
			ArrayList		list;
			string[]		copy;
			DataGridItem		item;

			list = new ArrayList();
			item = new DataGridItem(0, 0, ListItemType.Item);
			list.Add(item);
			item = new DataGridItem(1, 1, ListItemType.Header);
			list.Add(item);
			item = new DataGridItem(2, 2, ListItemType.Footer);
			list.Add(item);

			c = new DataGridItemCollection(list);

			copy = new string[2];
			c.CopyTo(copy, 0);
		}

		[Test]
		[ExpectedException(typeof(InvalidCastException))]
		public void WrongType () {
			DataGridItemCollection	c;
			ArrayList		list;

			list = new ArrayList();
			list.Add("blah");
			list.Add("argl");

			c = new DataGridItemCollection(list);

			Assert.AreEqual("blah", c[0], "E1");
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void BadIndex () {
			DataGridItemCollection	c;
			ArrayList		list;

			list = new ArrayList();
			list.Add("blah");
			list.Add("argl");

			c = new DataGridItemCollection(list);

			Assert.AreEqual("blah", c[3], "E2");
		}
	}
}
