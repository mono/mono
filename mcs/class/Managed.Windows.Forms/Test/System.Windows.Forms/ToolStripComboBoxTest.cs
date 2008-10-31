//
// ToolStripComboBoxTests.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Drawing;
using System.Windows.Forms;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolStripComboBoxTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();

			//Assert.AreEqual ("System.Windows.Forms.AutoCompleteStringCollection", tsi.AutoCompleteCustomSource.GetType ().ToString (), "A1");
			//Assert.AreEqual (AutoCompleteMode.None, tsi.AutoCompleteMode, "A2");
			//Assert.AreEqual (AutoCompleteSource.None, tsi.AutoCompleteSource, "A3");
			Assert.AreEqual ("System.Windows.Forms.ToolStripComboBox+ToolStripComboBoxControl", tsi.ComboBox.GetType ().ToString (), "A4");
			//Assert.AreEqual (106, tsi.DropDownHeight, "A5");
			Assert.AreEqual (ComboBoxStyle.DropDown, tsi.DropDownStyle, "A6");
			Assert.AreEqual (121, tsi.DropDownWidth, "A7");
			Assert.AreEqual (false, tsi.DroppedDown, "A8");
			Assert.AreEqual (FlatStyle.Popup, tsi.FlatStyle, "A9");
			Assert.AreEqual (true, tsi.IntegralHeight, "A10");
			Assert.AreEqual ("System.Windows.Forms.ComboBox+ObjectCollection", tsi.Items.ToString (), "A11");
			Assert.AreEqual (8, tsi.MaxDropDownItems, "A12");
			Assert.AreEqual (0, tsi.MaxLength, "A13");
			Assert.AreEqual (-1, tsi.SelectedIndex, "A14");
			Assert.AreEqual (null, tsi.SelectedItem, "A15");
			Assert.AreEqual (string.Empty, tsi.SelectedText, "A16");
			Assert.AreEqual (0, tsi.SelectionLength, "A17");
			Assert.AreEqual (0, tsi.SelectionStart, "A18");
			Assert.AreEqual (false, tsi.Sorted, "A19");

			tsi = new ToolStripComboBox ("Bob");
			Assert.AreEqual ("Bob", tsi.Name, "A20");
			Assert.AreEqual (string.Empty, tsi.Control.Name, "A21");
		}
	
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConstructorNSE ()
		{
			new ToolStripComboBox (new ComboBox ());
		}
		
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (1, 0, 1, 0), epp.DefaultMargin, "C1");
			Assert.AreEqual (new Size (100, 22), epp.DefaultSize, "C2");
		}

		//[Test]
		//public void PropertyAutoCompleteCustomSource ()
		//{
		//        ToolStripComboBox tsi = new ToolStripComboBox ();
		//        EventWatcher ew = new EventWatcher (tsi);

		//        AutoCompleteStringCollection acsc = new AutoCompleteStringCollection ();
		//        acsc.AddRange (new string[] { "Apple", "Banana" });

		//        tsi.AutoCompleteCustomSource = acsc;
		//        Assert.AreSame (acsc, tsi.AutoCompleteCustomSource, "B1");
		//        Assert.AreEqual (string.Empty, ew.ToString (), "B2");

		//        ew.Clear ();
		//        tsi.AutoCompleteCustomSource = acsc;
		//        Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		//}

		[Test]
		public void PropertyDropDownHeight ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DropDownHeight = 42;
			Assert.AreEqual (42, tsi.DropDownHeight, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.DropDownHeight = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDropDownStyle ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DropDownStyle = ComboBoxStyle.Simple;
			Assert.AreEqual (ComboBoxStyle.Simple, tsi.DropDownStyle, "B1");
			Assert.AreEqual ("DropDownStyleChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.DropDownStyle = ComboBoxStyle.Simple;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDropDownWidth ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DropDownWidth = 42;
			Assert.AreEqual (42, tsi.DropDownWidth, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.DropDownWidth = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyDroppedDown ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.DroppedDown = true;
			Assert.AreEqual (true, tsi.DroppedDown, "B1");
			Assert.AreEqual ("DropDown", ew.ToString (), "B2");

			ew.Clear ();
			tsi.DroppedDown = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyFlatStyle ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.FlatStyle = FlatStyle.System;
			Assert.AreEqual (FlatStyle.System, tsi.FlatStyle, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.FlatStyle = FlatStyle.System;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyIntegralHeight ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.IntegralHeight = false;
			Assert.AreEqual (false, tsi.IntegralHeight, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.IntegralHeight = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMaxDropDownItems ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MaxDropDownItems = 12;
			Assert.AreEqual (12, tsi.MaxDropDownItems, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MaxDropDownItems = 12;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMaxLength ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MaxLength = 42;
			Assert.AreEqual (42, tsi.MaxLength, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MaxLength = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectedIndex ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Items.Add ("A");
			tsi.Items.Add ("B");

			tsi.SelectedIndex = 1;
			Assert.AreEqual (1, tsi.SelectedIndex, "B1");
			Assert.AreEqual ("SelectedIndexChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectedIndex = 1;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void PropertySelectedIndexAOORE ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();

			tsi.SelectedIndex = 42;
		}

		[Test]
		public void PropertySelectedItem ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Items.Add ("A");
			tsi.Items.Add ("B");
			
			tsi.SelectedItem = "B";
			Assert.AreEqual ("B", tsi.SelectedItem, "B1");
			Assert.AreEqual ("SelectedIndexChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectedItem = "B";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectedItem2 ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.SelectedItem = "B";
			Assert.AreEqual (null, tsi.SelectedItem, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectedItem = "B";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[Ignore ("Need TextUpdate event implemented in 2.0 ComboBox")]
		public void PropertySelectedText ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.SelectedText = "Hi";
			Assert.AreEqual (string.Empty, tsi.SelectedText, "B1");
			Assert.AreEqual ("TextUpdate", ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectedText = string.Empty;
			Assert.AreEqual ("TextUpdate", ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectionLength ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.SelectionLength = 42;
			Assert.AreEqual (0, tsi.SelectionLength, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectionLength = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectionStart ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.SelectionStart = 42;
			Assert.AreEqual (0, tsi.SelectionStart, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectionStart = 42;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySorted ()
		{
			ToolStripComboBox tsi = new ToolStripComboBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Sorted = true;
			Assert.AreEqual (true, tsi.Sorted, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Sorted = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripComboBox tsi)
			{
				tsi.DropDown += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DropDown;"); });
				tsi.DropDownClosed += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DropDownClosed;"); });
				tsi.DropDownStyleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("DropDownStyleChanged;"); });
				tsi.SelectedIndexChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("SelectedIndexChanged;"); });
				tsi.TextUpdate += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TextUpdate;"); });
			}

			public override string ToString ()
			{
				return events.TrimEnd (';');
			}
			
			public void Clear ()
			{
				events = string.Empty;
			}
		}
		
		private class ExposeProtectedProperties : ToolStripComboBox
		{
			public ExposeProtectedProperties () : base () {}

			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
		}
	}
}
#endif