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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Olivier Dufour	olivier.duff@free.fr
//  Alan McGovern alan.mcgovern@gmail.com
//
#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	public class BindingNavigatorTest : TestHelper
	{
		private bool flag = false;
		private BindingNavigator navigator;

		private void SetFlag(object a, EventArgs e)
		{
			flag = true;
		}

		[SetUp]
		protected override void SetUp ()
		{
			IntThing test = new IntThing(50);
			BindingSource s = new BindingSource();
			s.DataSource = test;
			navigator = new BindingNavigator(s);
			flag = false;
			base.SetUp ();
		}

		[Test]
		public void AddNewItemTest()
		{
			navigator.ItemAdded += new ToolStripItemEventHandler(SetFlag);
			navigator.Items.Add("Test Item");
			Assert.IsTrue(flag, "#1");
		}

		[Test]
		public void AddStandardItems()
		{
			BindingNavigator navigator = new BindingNavigator();
			navigator.AddStandardItems();
			CheckStandardItems(navigator);
		}

		[Test]
		public void BeginInitTest()
		{
			navigator.RefreshItems += new EventHandler(SetFlag);
			navigator.ItemAdded += new ToolStripItemEventHandler(SetFlag);
			navigator.Paint += new PaintEventHandler(SetFlag);

			navigator.BeginInit();

			navigator.Invalidate();
			navigator.AddNewItem = new ToolStripButton();
			navigator.Refresh();

			Assert.IsFalse(flag, "#1");
			navigator.EndInit();
			Assert.IsTrue(flag, "#2");
		}

		[Test]
		public void BindingSourceTest()
		{
			navigator.BindingSource.PositionChanged += new EventHandler(SetFlag);
			navigator.BindingSource.Position = 5;
			Assert.IsTrue(flag, "#1");
			Assert.AreEqual("6", navigator.PositionItem.Text, "#2");
		}

		[Test]
		[Ignore("Bug in setting the textbox width breaks this test")]
		public void Constructor()
		{
			BindingNavigator navigator = new BindingNavigator(true);
			Assert.AreEqual(11, navigator.Items.Count, "count");
			CheckStandardItems(navigator);

			navigator = new BindingNavigator(false);
			Assert.IsTrue(navigator.Items.Count == 0, "#01");

			IntThing test = new IntThing(50);
			BindingSource s = new BindingSource();
			s.DataSource = test;

			navigator = new BindingNavigator((BindingSource)null);
			Assert.AreEqual(11, navigator.Items.Count, "#02");

			Assert.AreEqual(50, ((ToolStripTextBox)navigator.PositionItem).TextBox.Width, "#03");
		}

		[Test]
		public void ControlDisposedTest()
		{
			ToolStripItem existing = navigator.AddNewItem;
			navigator.AddNewItem = new ToolStripButton();
			Assert.IsFalse(existing.IsDisposed, "#1");
		}


		private void CheckStandardItems(BindingNavigator navigator)
		{
			Assert.IsNotNull(navigator.AddNewItem, "*1");
			Assert.IsNotNull(navigator.MoveFirstItem, "*2");
			Assert.IsNotNull(navigator.MoveLastItem, "*3");
			Assert.IsNotNull(navigator.MoveNextItem, "*4");
			Assert.IsNotNull(navigator.MovePreviousItem, "*5");
			Assert.IsNotNull(navigator.DeleteItem, "*6");
			Assert.IsNotNull(navigator.CountItem, "*7");
			Assert.IsNotNull(navigator.PositionItem, "*8");
			Assert.IsNotNull(navigator.AddNewItem, "*9");
			Assert.IsNotNull(navigator.AddNewItem, "*10");
			Assert.IsNull(navigator.BindingSource, "*11");

			Assert.IsTrue(navigator.AddNewItem is ToolStripButton, "#1");
			Assert.IsTrue(navigator.MoveFirstItem is ToolStripButton, "#2");
			Assert.IsTrue(navigator.MoveLastItem is ToolStripButton, "#3");
			Assert.IsTrue(navigator.MoveNextItem is ToolStripButton, "#4");
			Assert.IsTrue(navigator.MovePreviousItem is ToolStripButton, "#5");
			Assert.IsTrue(navigator.DeleteItem is ToolStripButton, "#6");
			Assert.IsTrue(navigator.CountItem is ToolStripLabel, "#7");
			Assert.IsTrue(navigator.PositionItem is ToolStripTextBox, "#8");
			Assert.IsTrue(navigator.AddNewItem is ToolStripButton, "#9");
			Assert.IsTrue(navigator.AddNewItem is ToolStripButton, "#10");
			Assert.AreEqual("of {0}", navigator.CountItemFormat, "#11");
			Assert.AreEqual(11, navigator.Items.Count, "#12");
		}

		[Test]
		public void ManuallyReplaceItemsTest()
		{
			ToolStripButton newButton = new ToolStripButton();
			ToolStripItem oldItem = navigator.AddNewItem;
			navigator.AddNewItem = newButton;
			Assert.AreEqual(11, navigator.Items.Count, "#1");
			Assert.IsFalse(navigator.Items.Contains(newButton), "#2");
			Assert.IsTrue(navigator.Items.Contains(oldItem), "#3");
		}

		[Test]
		public void OnRefreshItems()
		{
			navigator.RefreshItems += new EventHandler(SetFlag);
			navigator.AddNewItem = new ToolStripButton();
			Assert.IsTrue(flag, "#1");
		}

		[Test]
		[Ignore("Not working yet")]
		public void PositionItemTest()
		{
			navigator.BindingSource.PositionChanged += new EventHandler(SetFlag);

			int position = int.Parse(navigator.PositionItem.Text);
			navigator.PositionItem.Text = "aaa";
			Assert.IsFalse(flag, "#1");
			RefreshNav();
			Assert.AreEqual(position.ToString(), navigator.PositionItem.Text, "#2");

			navigator.PositionItem.Text = "-1";
			RefreshNav();
			Assert.IsFalse(flag, "#3");
			flag = false;
			Assert.AreEqual("1", navigator.PositionItem.Text, "#4");

			navigator.PositionItem.Text = "7";
			RefreshNav();
			Assert.IsFalse(flag, "#5");
			Assert.AreEqual("1", navigator.PositionItem.Text, "#6");
			Assert.AreEqual(0, navigator.BindingSource.Position, "#7");
		}

		[Test]
		public void RefreshItemsCore()
		{
			navigator.RefreshItems += new EventHandler(SetFlag);
			navigator.AddNewItem = new ToolStripButton();
			Assert.IsTrue(flag, "#1");

		}


		private void RefreshNav()
		{
			navigator.BeginInit();
			navigator.EndInit();
		}

		[Test]
		[Ignore("Not working")]
		public void RemoveItemTest()
		{
			navigator.BindingSource.Position = 5;
			navigator.BindingSource.ListChanged += new ListChangedEventHandler(SetFlag);
			navigator.BindingSource.Remove(5);
			Assert.IsFalse(navigator.BindingSource.Contains(5), "#1");
			Assert.AreEqual("6", navigator.PositionItem.Text, "#2");
			Assert.AreEqual(6, (navigator.BindingSource.Current), "#3");
		}

		[Test]
		public void SetControlNullTest()
		{
			navigator.AddNewItem = null;
			Assert.IsTrue(navigator.AddNewItem == null, "#1");
			Assert.AreEqual(11, navigator.Items.Count, "#2");
		}


		private class IntThing : BindingList<int>
		{
			int Number;
			public IntThing(int number)
				: base()
			{
				for (int i = 0; i < number; i++)
					this.Add(i);

				number = 6;
			}

			protected override bool SupportsSearchingCore
			{
				get { return true; }
			}
			
			protected override int FindCore(PropertyDescriptor prop, object key)
			{
				return this.Items.IndexOf((int)key);
				return -1;
			}
		}
	}
}
#endif
