//
// Tests for System.Web.UI.WebControls.DataGridItem 
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
	public class DataGridItemTest {
		public class DataGridItemTestClass : DataGridItem {
			public DataGridItemTestClass(int itemIndex, int dataSetIndex, ListItemType itemType) : base(itemIndex, dataSetIndex, itemType) {
			}

			public void SetType (ListItemType type) {
				base.SetItemType(type);
			}
		}

		[Test]
		public void Defaults ()
		{
			DataGridItem	i;
			string		s;

			i = new DataGridItem(123, 456, ListItemType.Pager);
			s = "blah";

			i.DataItem = s;

			Assert.AreEqual(ListItemType.Pager, i.ItemType, "D1");
			Assert.AreEqual(456, i.DataSetIndex, "D2");
			Assert.AreEqual(123, i.ItemIndex, "D3");
			Assert.AreEqual(s, i.DataItem, "D4");
			Assert.AreEqual("blah", i.DataItem, "D5");
		}


		[Test]
		public void Methods () {
			DataGridItemTestClass	i;
			string			s;

			i = new DataGridItemTestClass(123, 456, ListItemType.Pager);
			s = "blah";

			i.DataItem = s;

			Assert.AreEqual(ListItemType.Pager, i.ItemType, "M1");
			i.SetType(ListItemType.Header);
			Assert.AreEqual(ListItemType.Header, i.ItemType, "M2");

			
		}

		[Test]
		public void ValidEnum () {
			DataGridItemTestClass	i;
			string			s;

			i = new DataGridItemTestClass(123, 456, (ListItemType)27051977);
		}
	}
}
