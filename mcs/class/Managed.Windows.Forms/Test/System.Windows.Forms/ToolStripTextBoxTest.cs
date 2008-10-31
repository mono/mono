//
// ToolStripTextBoxTests.cs
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
	public class ToolStripTextBoxTests : TestHelper
	{
		[Test]
		public void Constructor ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();

			Assert.AreEqual (false, tsi.AcceptsReturn, "A1");
			Assert.AreEqual (false, tsi.AcceptsTab, "A2");
			Assert.AreEqual ("System.Windows.Forms.AutoCompleteStringCollection", tsi.AutoCompleteCustomSource.GetType ().ToString (), "A3");
			Assert.AreEqual (AutoCompleteMode.None, tsi.AutoCompleteMode, "A4");
			Assert.AreEqual (AutoCompleteSource.None, tsi.AutoCompleteSource, "A5");
			Assert.AreEqual (BorderStyle.Fixed3D, tsi.BorderStyle, "A6");
			Assert.AreEqual (false, tsi.CanUndo, "A7");
			Assert.AreEqual (CharacterCasing.Normal, tsi.CharacterCasing, "A8");
			Assert.AreEqual (true, tsi.HideSelection, "A9");
			Assert.AreEqual ("System.String[]", tsi.Lines.GetType ().ToString (), "A10");
			Assert.AreEqual (32767, tsi.MaxLength, "A11");
			//Bug in TextBox
			//Assert.AreEqual (false, tsi.Modified, "A12");
			Assert.AreEqual (false, tsi.ReadOnly, "A13");
			Assert.AreEqual (string.Empty, tsi.SelectedText, "A14");
			Assert.AreEqual (0, tsi.SelectionLength, "A15");
			Assert.AreEqual (0, tsi.SelectionStart, "A16");
			Assert.AreEqual (true, tsi.ShortcutsEnabled, "A17");
			Assert.AreEqual ("System.Windows.Forms.ToolStripTextBox+ToolStripTextBoxControl", tsi.TextBox.GetType ().ToString (), "A18");
			Assert.AreEqual (HorizontalAlignment.Left, tsi.TextBoxTextAlign, "A19");
			Assert.AreEqual (0, tsi.TextLength, "A20");

			tsi = new ToolStripTextBox ("Bob");
			Assert.AreEqual ("Bob", tsi.Name, "A21");
			Assert.AreEqual (string.Empty, tsi.Control.Name, "A22");
		}
	
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConstructorNSE ()
		{
			new ToolStripTextBox (new TextBox ());
		}
		
		[Test]
		public void ProtectedProperties ()
		{
			ExposeProtectedProperties epp = new ExposeProtectedProperties ();

			Assert.AreEqual (new Padding (1, 0, 1, 0), epp.DefaultMargin, "C1");
			Assert.AreEqual (new Size (100, 22), epp.DefaultSize, "C2");
		}

		[Test]
		public void PropertyAcceptsReturn ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AcceptsReturn = true;
			Assert.AreEqual (true, tsi.AcceptsReturn, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AcceptsReturn = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAcceptsTab ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AcceptsTab = true;
			Assert.AreEqual (true, tsi.AcceptsTab, "B1");
			Assert.AreEqual ("AcceptsTabChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.AcceptsTab = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAutoCompleteCustomSource ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			AutoCompleteStringCollection acsc = new AutoCompleteStringCollection ();
			acsc.AddRange (new string[] { "Apple", "Banana" });

			tsi.AutoCompleteCustomSource = acsc;
			Assert.AreSame (acsc, tsi.AutoCompleteCustomSource, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoCompleteCustomSource = acsc;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAutoCompleteMode ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AutoCompleteMode = AutoCompleteMode.Append;
			Assert.AreEqual (AutoCompleteMode.Append, tsi.AutoCompleteMode, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoCompleteMode = AutoCompleteMode.Append;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyAutoCompleteSource ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.AutoCompleteSource = AutoCompleteSource.RecentlyUsedList;
			Assert.AreEqual (AutoCompleteSource.RecentlyUsedList, tsi.AutoCompleteSource, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.AutoCompleteSource = AutoCompleteSource.RecentlyUsedList;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyBorderStyle ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.BorderStyle = BorderStyle.None;
			Assert.AreEqual (BorderStyle.None, tsi.BorderStyle, "B1");
			Assert.AreEqual ("BorderStyleChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.BorderStyle = BorderStyle.None;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyCharacterCasing ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.CharacterCasing = CharacterCasing.Lower;
			Assert.AreEqual (CharacterCasing.Lower, tsi.CharacterCasing, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.CharacterCasing = CharacterCasing.Lower;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyHideSelection ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.HideSelection = false;
			Assert.AreEqual (false, tsi.HideSelection, "B1");
			Assert.AreEqual ("HideSelectionChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.HideSelection = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[Ignore ("Problems in TextBox")]
		public void PropertyLines ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			string[] lines = new string[] {"Apple", "Banana"};
			tsi.Lines = lines;
			
			Assert.AreEqual (lines, tsi.Lines, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.Lines = lines;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyMaxLength ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.MaxLength = 15;
			Assert.AreEqual (15, tsi.MaxLength, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.MaxLength = 15;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[Ignore ("TextBox does not raise ModifiedChanged")]
		// When this works, also uncomment A12 in Constructor test above please
		public void PropertyModified ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Modified = true;
			Assert.AreEqual (true, tsi.Modified, "B1");
			Assert.AreEqual ("ModifiedChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.Modified = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyReadOnly ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ReadOnly = true;
			Assert.AreEqual (true, tsi.ReadOnly, "B1");
			Assert.AreEqual ("ReadOnlyChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.ReadOnly = true;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		[Ignore ("TextBox does not raise ModifiedChanged")]
		public void PropertySelectedText ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Crumbelievable";
			tsi.SelectedText = "lie";

			Assert.AreEqual (string.Empty, tsi.SelectedText, "B1");
			Assert.AreEqual ("ModifiedChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectedText = "lie";
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectionLength ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Crumbelievable";
			tsi.SelectionLength = 6;
			Assert.AreEqual (6, tsi.SelectionLength, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectionLength = 6;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertySelectionStart ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.Text = "Crumbelievable";
			tsi.SelectionStart = 4;
			Assert.AreEqual (4, tsi.SelectionStart, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.SelectionStart = 4;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyShortcutsEnabled ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.ShortcutsEnabled = false;
			Assert.AreEqual (false, tsi.ShortcutsEnabled, "B1");
			Assert.AreEqual (string.Empty, ew.ToString (), "B2");

			ew.Clear ();
			tsi.ShortcutsEnabled = false;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		[Test]
		public void PropertyTextBoxTextAlign ()
		{
			ToolStripTextBox tsi = new ToolStripTextBox ();
			EventWatcher ew = new EventWatcher (tsi);

			tsi.TextBoxTextAlign = HorizontalAlignment.Right;
			Assert.AreEqual (HorizontalAlignment.Right, tsi.TextBoxTextAlign, "B1");
			Assert.AreEqual ("TextBoxTextAlignChanged", ew.ToString (), "B2");

			ew.Clear ();
			tsi.TextBoxTextAlign = HorizontalAlignment.Right;
			Assert.AreEqual (string.Empty, ew.ToString (), "B3");
		}

		private class EventWatcher
		{
			private string events = string.Empty;
			
			public EventWatcher (ToolStripTextBox tsi)
			{
				tsi.AcceptsTabChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("AcceptsTabChanged;"); });
				tsi.BorderStyleChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("BorderStyleChanged;"); });
				tsi.HideSelectionChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("HideSelectionChanged;"); });
				tsi.ModifiedChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ModifiedChanged;"); });
				tsi.ReadOnlyChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("ReadOnlyChanged;"); });
				tsi.TextBoxTextAlignChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("TextBoxTextAlignChanged;"); });
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
		
		private class ExposeProtectedProperties : ToolStripTextBox
		{
			public ExposeProtectedProperties () : base () {}

			public new Padding DefaultMargin { get { return base.DefaultMargin; } }
			public new Size DefaultSize { get { return base.DefaultSize; } }
		}
	}
}
#endif