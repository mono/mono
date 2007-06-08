//
// ComboBoxTest.cs: Test cases for ComboBox.
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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//   	Ritvik Mayank <mritvik@novell.com>
//	Jordi Mas i Hernandez <jordi@ximian.com>

using System;
using System.Data;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ComboBoxTest
	{
		private CultureInfo _originalCulture;

		[SetUp]
		public void SetUp ()
		{
			_originalCulture = Thread.CurrentThread.CurrentCulture;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = _originalCulture;
		}
		
		[Test]
		public void ContextMenuTest ()
		{
			ComboBox cmb = new ComboBox ();
			ContextMenu cm = new ContextMenu ();
			
			Assert.IsNull (cmb.ContextMenu, "#1");
			cmb.ContextMenu = cm;
			Assert.AreSame (cmb.ContextMenu, cm, "#2");
			cmb.DropDownStyle = ComboBoxStyle.DropDown;
			Assert.AreSame (cmb.ContextMenu, cm, "#3");
			cmb.DropDownStyle = ComboBoxStyle.DropDownList;
			Assert.AreSame (cmb.ContextMenu, cm, "#4");
			cmb.DropDownStyle = ComboBoxStyle.Simple;
			Assert.AreSame (cmb.ContextMenu, cm, "#5");
			
		}
		
		[Test] // bug 80794
		public void DataBindingTest ()
		{
			string table = 
@"<?xml version=""1.0"" standalone=""yes""?>
<DOK>
<DOK>
<klient>287</klient>
</DOK>
</DOK>
";
			string lookup = 
@"<?xml version=""1.0"" standalone=""yes""?>
<klient>
<klient>
<nimi>FAILED</nimi>
<kood>316</kood>
</klient>
<klient>
<nimi>SUCCESS</nimi>
<kood>287</kood>
</klient>
</klient>";
			
			using (Form frm = new Form ()) {
				frm.ShowInTaskbar = false;
				DataSet dsTable = new DataSet ();
				dsTable.ReadXml (new StringReader (table));
				DataSet dsLookup = new DataSet ();
				dsLookup.ReadXml (new StringReader (lookup));
				ComboBox cb = new ComboBox ();
				cb.DataSource = dsLookup.Tables [0];
				cb.DisplayMember = "nimi";
				cb.ValueMember = "kood";
				cb.DataBindings.Add ("SelectedValue", dsTable.Tables [0], "klient");
				frm.Controls.Add (cb);
				Assert.AreEqual ("", cb.Text, "#01");
				frm.Show ();
				Assert.AreEqual ("SUCCESS", cb.Text, "#02");
			}
		}
		
		[Test]
		public void ComboBoxPropertyTest ()
		{
			ComboBox mycmbbox = new ComboBox ();
			Assert.AreEqual (DrawMode.Normal, mycmbbox.DrawMode, "#1");
			Assert.AreEqual (ComboBoxStyle.DropDown, mycmbbox.DropDownStyle, "#2");
			Assert.AreEqual (false, mycmbbox.DroppedDown, "#4");
			Assert.AreEqual (true, mycmbbox.IntegralHeight, "#5");
			Assert.AreEqual (0, mycmbbox.Items.Count, "#6");
			//Assert.AreEqual (15, mycmbbox.ItemHeight, "#7"); 	// Note: Item height depends on the current font.
			Assert.AreEqual (8, mycmbbox.MaxDropDownItems, "#8");
			Assert.AreEqual (0, mycmbbox.MaxLength, "#9");
			//Assert.AreEqual (20, mycmbbox.PreferredHeight, "#10");
			// Note: Item height depends on the current font.
			Assert.AreEqual (-1, mycmbbox.SelectedIndex, "#11");
			Assert.AreEqual (null, mycmbbox.SelectedItem, "#12");
			Assert.AreEqual ("", mycmbbox.SelectedText, "#13");
			Assert.AreEqual (0, mycmbbox.SelectionLength, "#14");
			Assert.AreEqual (0, mycmbbox.SelectionStart, "#15");
			Assert.AreEqual (false, mycmbbox.Sorted, "#16");
			Assert.AreEqual ("", mycmbbox.Text, "#17");
#if NET_2_0
			Assert.AreEqual (true, mycmbbox.AutoCompleteCustomSource != null, "#18");
			Assert.AreEqual (AutoCompleteMode.None, mycmbbox.AutoCompleteMode, "#19");
			Assert.AreEqual (AutoCompleteSource.None, mycmbbox.AutoCompleteSource, "#20");

			mycmbbox.AutoCompleteCustomSource = null;
			Assert.AreEqual (true, mycmbbox.AutoCompleteCustomSource != null, "#21");
			
			Assert.AreEqual (ImageLayout.Tile, mycmbbox.BackgroundImageLayout, "#22");
			Assert.AreEqual (null, mycmbbox.DataSource, "#23");
			Assert.AreEqual (106, mycmbbox.DropDownHeight, "#24");
			Assert.AreEqual (FlatStyle.Standard, mycmbbox.FlatStyle, "#25");
			Assert.AreEqual ("{Width=0, Height=0}", mycmbbox.MaximumSize.ToString (), "#26");
			Assert.AreEqual ("{Width=0, Height=0}", mycmbbox.MinimumSize.ToString (), "#27");
			Assert.AreEqual ("{Left=0,Top=0,Right=0,Bottom=0}", mycmbbox.Padding.ToString (), "#28");
			
#endif
		}

#if NET_2_0
		[Test]
		public void ResetTextTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			Assert.AreEqual ("", cmbbox.Text, "#01");
			cmbbox.Text = "abc";
			Assert.AreEqual ("abc", cmbbox.Text, "#02");
			cmbbox.ResetText ();
			Assert.AreEqual ("", cmbbox.Text, "#03");
		}
		
		[Test]
		public void BackgroundImageLayoutTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.BackgroundImageLayout = ImageLayout.Stretch;
			Assert.AreEqual (ImageLayout.Stretch, cmbbox.BackgroundImageLayout, "#01");
		}
		
		[Test]
		public void DropDownHeightTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownHeight = 225;
			Assert.AreEqual (225, cmbbox.DropDownHeight, "#01");
			cmbbox.DropDownHeight = 1;
			Assert.AreEqual (1, cmbbox.DropDownHeight, "#02");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DropDownHeightExceptionTest1 ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownHeight = -225;	
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DropDownHeightExceptionTest2 ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownHeight = 0;
		}
		
		[Test]
		public void FlatStyleTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.FlatStyle = FlatStyle.Popup;
			Assert.AreEqual (FlatStyle.Popup, cmbbox.FlatStyle, "#01");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void FlatStyleExceptionTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.FlatStyle = (FlatStyle) (-123);
		}
	
		[Test]
		public void MaximumSizeTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.MaximumSize = new Size (25, 25);
			Assert.AreEqual ("{Width=25, Height=0}", cmbbox.MaximumSize.ToString (), "#01");
			cmbbox.MaximumSize = new Size (50, 75);
			Assert.AreEqual ("{Width=50, Height=0}", cmbbox.MaximumSize.ToString (), "#02");
		}
	
		[Test]
		public void MinumumSizeTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.MinimumSize = new Size (25, 25);
			Assert.AreEqual ("{Width=25, Height=0}", cmbbox.MinimumSize.ToString (), "#1");
			cmbbox.MinimumSize = new Size (50, 75);
			Assert.AreEqual ("{Width=50, Height=0}", cmbbox.MinimumSize.ToString (), "#2");	
		}
		
		[Test]
		public void PaddingTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Padding = new Padding (21);
			Assert.AreEqual ("{Left=21,Top=21,Right=21,Bottom=21}", cmbbox.Padding.ToString (), "#01");
		}
#endif

		[Test]
		public void BeginEndUpdateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.Add ("A");
			cmbbox.Visible = true;
			myform.Controls.Add (cmbbox);
			cmbbox.BeginUpdate ();
			for (int x = 1 ; x < 5000 ; x++) {
				cmbbox.Items.Add ("Item " + x.ToString ());
			}
			cmbbox.EndUpdate ();
			myform.Dispose ();
		}

		[Test]
		public void FindString ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

			ComboBox cmbbox = new ComboBox ();
			Assert.AreEqual (-1, cmbbox.FindString ("Hello"), "#A1");
			Assert.AreEqual (-1, cmbbox.FindString (string.Empty), "#A2");
			Assert.AreEqual (-1, cmbbox.FindString (null), "#A3");
			Assert.AreEqual (-1, cmbbox.FindString ("Hola", -5), "#A4");
			Assert.AreEqual (-1, cmbbox.FindString ("Hola", 40), "#A5");

			cmbbox.Items.AddRange (new object [] { "in", "BADTest", "IN", "BAD", "Bad", "In" });
			Assert.AreEqual (2, cmbbox.FindString ("I"), "#B1");
			Assert.AreEqual (0, cmbbox.FindString ("in"), "#B2");
			Assert.AreEqual (1, cmbbox.FindString ("BAD"), "#B3");
			Assert.AreEqual (1, cmbbox.FindString ("Bad"), "#B4");
			Assert.AreEqual (1, cmbbox.FindString ("b"), "#B5");
			Assert.AreEqual (0, cmbbox.FindString (string.Empty), "#B6");
			Assert.AreEqual (-1, cmbbox.FindString (null), "#B7");

			Assert.AreEqual (3, cmbbox.FindString ("b", 2), "#C1");
			Assert.AreEqual (5, cmbbox.FindString ("I", 3), "#C2");
			Assert.AreEqual (4, cmbbox.FindString ("B", 3), "#C3");
			Assert.AreEqual (1, cmbbox.FindString ("B", 4), "#C4");
			Assert.AreEqual (5, cmbbox.FindString ("I", 4), "#C5");
			Assert.AreEqual (4, cmbbox.FindString ("BA", 3), "#C6");
			Assert.AreEqual (0, cmbbox.FindString ("i", -1), "#C7");
			Assert.AreEqual (3, cmbbox.FindString (string.Empty, 2), "#C8");
			Assert.AreEqual (-1, cmbbox.FindString (null, 1), "#C9");

			cmbbox.Items.Add (string.Empty);
			Assert.AreEqual (0, cmbbox.FindString (string.Empty), "#D1");
			Assert.AreEqual (-1, cmbbox.FindString (null), "#D2");

			Assert.AreEqual (4, cmbbox.FindString (string.Empty, 3), "#E1");
			Assert.AreEqual (-1, cmbbox.FindString (null, 99), "#E2");
			Assert.AreEqual (-1, cmbbox.FindString (null, -5), "#E3");
		}

		[Test]
		public void FindString_StartIndex_ItemCount ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "BA", "BB" });
#if NET_2_0
			Assert.AreEqual (0, cmbbox.FindString ("b", 1));
#else
			try {
				cmbbox.FindString ("b", 1);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
#endif
		}

		[Test]
		public void FindString_StartIndex_Min ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "ACBD", "ABDC", "ACBD", "ABCD" });
			try {
				cmbbox.FindString ("Hola", -2);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void FindString_StartIndex_Max ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "ACBD", "ABDC", "ACBD", "ABCD" });
			try {
				cmbbox.FindString ("Hola", 4);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void FindStringExact ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

			ComboBox cmbbox = new ComboBox ();
			Assert.AreEqual (-1, cmbbox.FindStringExact ("Hello"), "#A1");
			Assert.AreEqual (-1, cmbbox.FindStringExact (string.Empty), "#A2");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null), "#A3");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("Hola", -5), "#A4");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("Hola", 40), "#A5");

			cmbbox.Items.AddRange (new object [] { "in", "BADTest", "IN", "BAD", "Bad", "In" });
			Assert.AreEqual (2, cmbbox.FindStringExact ("IN"), "#B1");
			Assert.AreEqual (0, cmbbox.FindStringExact ("in"), "#B2");
			Assert.AreEqual (3, cmbbox.FindStringExact ("BAD"), "#B3");
			Assert.AreEqual (3, cmbbox.FindStringExact ("bad"), "#B4");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("B"), "#B5");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("NonExistant"), "#B6");
			Assert.AreEqual (-1, cmbbox.FindStringExact (string.Empty), "#B7");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null), "#B8");

			Assert.AreEqual (2, cmbbox.FindStringExact ("In", 1), "#C1");
			Assert.AreEqual (5, cmbbox.FindStringExact ("In", 2), "#C2");
			Assert.AreEqual (4, cmbbox.FindStringExact ("BaD", 3), "#C3");
			Assert.AreEqual (3, cmbbox.FindStringExact ("bad", -1), "#C4");
			Assert.AreEqual (5, cmbbox.FindStringExact ("In", 4), "#C5");
			Assert.AreEqual (3, cmbbox.FindStringExact ("bad", 4), "#C6");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("B", 4), "#C7");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("BADNOT", 0), "#C8");
			Assert.AreEqual (-1, cmbbox.FindStringExact ("i", -1), "#C9");
			Assert.AreEqual (-1, cmbbox.FindStringExact (string.Empty, 2), "#C10");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null, 1), "#C11");

			cmbbox.Items.Add (string.Empty);
			Assert.AreEqual (6, cmbbox.FindStringExact (string.Empty), "#D1");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null), "#D2");

			Assert.AreEqual (6, cmbbox.FindStringExact (string.Empty, 3), "#E1");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null, 99), "#E2");
			Assert.AreEqual (-1, cmbbox.FindStringExact (null, -5), "#E3");
		}

		[Test]
		public void FindStringExact_StartIndex_ItemCount ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "AB", "BA", "AB", "BA" });
#if NET_2_0
			Assert.AreEqual (1, cmbbox.FindStringExact ("BA", 3));
#else
			try {
				cmbbox.FindString ("BA", 3);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
#endif
		}

		[Test]
		public void FindStringExact_StartIndex_Min ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "ACBD", "ABDC", "ACBD", "ABCD" });
			try {
				cmbbox.FindStringExact ("Hola", -2);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void FindStringExact_StartIndex_Max ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange (new object [] { "ACBD", "ABDC", "ACBD", "ABCD" });
			try {
				cmbbox.FindStringExact ("Hola", 4);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("startIndex", ex.ParamName, "#6");
			}
		}

		[Test]
		public void GetItemHeightTest ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.Add ("ABC");
			cmbbox.Items.Add ("BCD");
			cmbbox.Items.Add ("DEF");
			int x = -1;
			x = cmbbox.GetItemHeight (x);
			Assert.IsTrue (cmbbox.ItemHeight > 0, "#21");
		}

		//
		// Exceptions
		//

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DropDownStyleException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyle = (ComboBoxStyle) 10;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DrawModeException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DrawMode = (DrawMode) 10;
		}

		[Test]
		public void DropDownWidth ()
		{
			ComboBox cmbbox = new ComboBox ();
			Assert.AreEqual (121, cmbbox.DropDownWidth, "#A1");
			cmbbox.DropDownWidth = 1;
			Assert.AreEqual (1, cmbbox.DropDownWidth, "#A2");

			try {
				cmbbox.DropDownWidth = 0;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("DropDownWidth", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
		}

		[Test]
		public void ItemHeight ()
		{
			ComboBox cmbbox = new ComboBox ();
			Assert.IsTrue (cmbbox.ItemHeight >= 1, "#A1");
			cmbbox.ItemHeight = 1;
			Assert.AreEqual (1, cmbbox.ItemHeight, "#A2");

			try {
				cmbbox.ItemHeight = 0;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNotNull (ex.ParamName, "#B4");
				Assert.AreEqual ("ItemHeight", ex.ParamName, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException ()
		{
			ComboBox cmbbox = new ComboBox ();
			cmbbox.SelectedIndex = -2;
		}

		//
		// Events
		//
		private bool eventFired;
		private DrawItemEventArgs drawItemsArgs;
		private void DrawItemEventH (object sender,  DrawItemEventArgs e)
		{
			eventFired = true;
			drawItemsArgs = e;
		}

		private void GenericHandler (object sender,  EventArgs e)
		{
			eventFired = true;
		}

		[Ignore ("Bugs in X11 prevent this test to run properly")]
		public void DrawItemEventTest ()
		{
			eventFired = false;
			drawItemsArgs = null;
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyle = ComboBoxStyle.Simple;
			cmbbox.DrawMode = DrawMode.OwnerDrawFixed;
			cmbbox.DrawItem += new DrawItemEventHandler (DrawItemEventH);

			myform.Controls.Add (cmbbox);
			cmbbox.Items.AddRange(new object[] {"Item1"});

			myform.Visible = true;
			cmbbox.Visible = true;
			cmbbox.Refresh ();

			Assert.AreEqual (true, eventFired, "DW1");
			Assert.AreEqual (0, drawItemsArgs.Index, "DW2");
			myform.Dispose ();
		}

		[Test]
		public void DropDownStyleEventTest ()
		{
			eventFired = false;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.DropDownStyleChanged += new EventHandler (GenericHandler);
			cmbbox.DropDownStyle = ComboBoxStyle.Simple;

			Assert.AreEqual (true, eventFired, "DI1");
		}

		[Test]
		public void SelectedIndexTest ()
		{
			eventFired = false;
			ComboBox cmbbox = new ComboBox ();
			cmbbox.Items.AddRange(new object[] {"Item1", "Item2"});
			cmbbox.SelectedIndexChanged += new EventHandler (GenericHandler);
			cmbbox.SelectedIndex = 1;
			Assert.AreEqual (true, eventFired, "SI1");
		}

		[Test]
		public void SelectionWithAdd()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.Items.Add("Item 3");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 4");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA1");
			Assert.AreEqual(false, eventFired, "SWA2");
			cb.Sorted = true;
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 5");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA3");
			Assert.AreEqual("Item 3", cb.SelectedItem, "SWA4");
			Assert.AreEqual(false, eventFired, "SWA5");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Items.Add("Item 2");
			Assert.AreEqual(1, cb.SelectedIndex, "SWA6");
			Assert.AreEqual("Item 2", cb.SelectedItem, "SWA7");
			Assert.AreEqual(false, eventFired, "SWA8");
		}

		[Test]
		public void SelectionWithInsert()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.SelectedIndex = 0;
			eventFired = false;
			cb.Items.Insert(0, "Item 2");
			Assert.AreEqual(0, cb.SelectedIndex, "SWI1");
			Assert.AreEqual(false, eventFired, "SWI2");
		}

		[Test]
		public void SelectionWithClear()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.SelectedIndex = 0;
			eventFired = false;
			cb.Items.Clear();
			Assert.AreEqual(-1, cb.SelectedIndex, "SWC1");
			Assert.AreEqual(false, eventFired, "SWC2");
		}

		[Test]
		public void SortedTest()
		{
			ComboBox mycb = new ComboBox();
			Assert.AreEqual(false, mycb.Sorted, "#1");
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			Assert.AreEqual("Item 2", mycb.Items[0], "#2");
			Assert.AreEqual("Item 1", mycb.Items[1], "#3");
			mycb.Sorted = true;
			Assert.AreEqual(true, mycb.Sorted, "#4");
			Assert.AreEqual("Item 1", mycb.Items[0], "#5");
			Assert.AreEqual("Item 2", mycb.Items[1], "#6");
			mycb.Sorted = false;
			Assert.AreEqual(false, mycb.Sorted, "#7");
			Assert.AreEqual("Item 1", mycb.Items[0], "#8");
			Assert.AreEqual("Item 2", mycb.Items[1], "#9");
		}

		[Test]
		public void SortedAddTest()
		{
			ComboBox mycb = new ComboBox();
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			mycb.Sorted = true;
			Assert.AreEqual("Item 1", mycb.Items[0], "#I1");
			Assert.AreEqual("Item 2", mycb.Items[1], "#I2");
		}

		[Test]
		public void SortedInsertTest()
		{
			ComboBox mycb = new ComboBox();
			mycb.Items.Add("Item 2");
			mycb.Items.Add("Item 1");
			mycb.Sorted = true;
			mycb.Items.Insert (0, "Item 3");
			Assert.AreEqual("Item 1", mycb.Items[0], "#J1");
			Assert.AreEqual("Item 2", mycb.Items[1], "#J2");
			Assert.AreEqual("Item 3", mycb.Items[2], "#J3");
		}

		[Test]
		public void SortedSelectionInteractions()
		{
			ComboBox cb = new ComboBox();
			cb.SelectedIndexChanged += new EventHandler(GenericHandler);
			cb.Items.Add("Item 1");
			cb.Items.Add("Item 2");
			cb.Items.Add("Item 3");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = true;
			Assert.AreEqual(-1, cb.SelectedIndex, "#SSI1");
			Assert.AreEqual(true, eventFired, "#SSI2");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = true;
			Assert.AreEqual(1, cb.SelectedIndex, "#SSI3");
			Assert.AreEqual(false, eventFired, "#SSI4");
			cb.SelectedIndex = 1;
			eventFired = false;
			cb.Sorted = false;
			Assert.AreEqual(-1, cb.SelectedIndex, "#SSI5");
			Assert.AreEqual(true, eventFired, "#SSI6");
		}

		[Test]
		public void Text_DropDown ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("tr-TR");

			ComboBox cmbbox = new ComboBox ();
			Assert.IsNotNull (cmbbox.Text, "#A1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#A2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#A3");

			cmbbox.Items.Add ("Another");
			cmbbox.Items.Add ("Bad");
			cmbbox.Items.Add ("IN");
			cmbbox.Items.Add ("Combobox");
			cmbbox.Items.Add ("BAD");
			cmbbox.Items.Add ("iN");
			cmbbox.Items.Add ("Bad");

			Assert.IsNotNull (cmbbox.Text, "#B1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#B2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#B3");

			cmbbox.SelectedIndex = 3;
			Assert.IsNotNull (cmbbox.Text, "#C1");
			Assert.AreEqual ("Combobox", cmbbox.Text, "#C2");
			Assert.AreEqual (3, cmbbox.SelectedIndex, "#C3");

			cmbbox.Text = string.Empty;
			Assert.IsNotNull (cmbbox.Text, "#D1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#D2");
			Assert.AreEqual (3, cmbbox.SelectedIndex, "#D3");

			cmbbox.SelectedIndex = 1;
			Assert.IsNotNull (cmbbox.Text, "#E1");
			Assert.AreEqual ("Bad", cmbbox.Text, "#E2");
			Assert.AreEqual (1, cmbbox.SelectedIndex, "#E3");

			cmbbox.Text = null;
			Assert.IsNotNull (cmbbox.Text, "#F1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#F2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#F3");

			cmbbox.SelectedIndex = 0;
			cmbbox.Text = "Q";
			Assert.IsNotNull (cmbbox.Text, "#G1");
			Assert.AreEqual ("Q", cmbbox.Text, "#G2");
			Assert.AreEqual (0, cmbbox.SelectedIndex, "#G3");

			cmbbox.Text = "B";
			Assert.IsNotNull (cmbbox.Text, "#H1");
			Assert.AreEqual ("B", cmbbox.Text, "#H2");
			Assert.AreEqual (0, cmbbox.SelectedIndex, "#H3");

			cmbbox.Text = "BAD";
			Assert.IsNotNull (cmbbox.Text, "#I1");
			Assert.AreEqual ("BAD", cmbbox.Text, "#I2");
			Assert.AreEqual (4, cmbbox.SelectedIndex, "#I3");

			cmbbox.Text = "BAD";
			Assert.IsNotNull (cmbbox.Text, "#J1");
			Assert.AreEqual ("BAD", cmbbox.Text, "#J2");
			Assert.AreEqual (4, cmbbox.SelectedIndex, "#J3");

			cmbbox.Text = "baD";
			Assert.IsNotNull (cmbbox.Text, "#K1");
			Assert.AreEqual ("Bad", cmbbox.Text, "#K2");
			Assert.AreEqual (1, cmbbox.SelectedIndex, "#K3");

			cmbbox.SelectedIndex = -1;
			cmbbox.Text = "E";
			Assert.IsNotNull (cmbbox.Text, "#L1");
			Assert.AreEqual ("E", cmbbox.Text, "#L2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#L3");

			cmbbox.Text = "iN";
			Assert.IsNotNull (cmbbox.Text, "#M1");
			Assert.AreEqual ("iN", cmbbox.Text, "#M2");
			Assert.AreEqual (5, cmbbox.SelectedIndex, "#M3");

			cmbbox.Text = "In";
			Assert.IsNotNull (cmbbox.Text, "#N1");
			Assert.AreEqual ("IN", cmbbox.Text, "#N2");
			Assert.AreEqual (2, cmbbox.SelectedIndex, "#N3");

			cmbbox.Text = "Badd";
			Assert.IsNotNull (cmbbox.Text, "#O1");
			Assert.AreEqual ("Badd", cmbbox.Text, "#O2");
			Assert.AreEqual (2, cmbbox.SelectedIndex, "#O3");

			cmbbox.SelectedIndex = -1;
			Assert.IsNotNull (cmbbox.Text, "#P1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#P2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#P3");

			cmbbox.Text = "Something";
			Assert.IsNotNull (cmbbox.Text, "#Q1");
			Assert.AreEqual ("Something", cmbbox.Text, "#Q2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#Q3");

			cmbbox.SelectedIndex = -1;
			Assert.IsNotNull (cmbbox.Text, "#R1");
			Assert.AreEqual ("Something", cmbbox.Text, "#R2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#R3");

			cmbbox.Text = null;
			Assert.IsNotNull (cmbbox.Text, "#S1");
			Assert.AreEqual (string.Empty, cmbbox.Text, "#S2");
			Assert.AreEqual (-1, cmbbox.SelectedIndex, "#S3");
		}

#if NET_2_0
		[Test]
		public void MethodScaleControl ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			f.Show ();

			PublicComboBox gb = new PublicComboBox ();
			gb.Location = new Point (5, 10);
			f.Controls.Add (gb);

			Assert.AreEqual (new Rectangle (5, 10, 121, 21), gb.Bounds, "A1");

			gb.PublicScaleControl (new SizeF (2.0f, 2.0f), BoundsSpecified.All);
			Assert.AreEqual (new Rectangle (10, 20, 238, 21), gb.Bounds, "A2");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Location);
			Assert.AreEqual (new Rectangle (5, 10, 238, 21), gb.Bounds, "A3");

			gb.PublicScaleControl (new SizeF (.5f, .5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 121, 21), gb.Bounds, "A4");

			gb.PublicScaleControl (new SizeF (3.5f, 3.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 414, 21), gb.Bounds, "A5");

			gb.PublicScaleControl (new SizeF (2.5f, 2.5f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 1029, 21), gb.Bounds, "A6");

			gb.PublicScaleControl (new SizeF (.2f, .2f), BoundsSpecified.Size);
			Assert.AreEqual (new Rectangle (5, 10, 209, 21), gb.Bounds, "A7");

			f.Dispose ();
		}

		private class PublicComboBox : ComboBox
		{
			public void PublicScaleControl (SizeF factor, BoundsSpecified specified)
			{
				base.ScaleControl (factor, specified);
			}
		}
#endif
	}

	[TestFixture]
	public class ComboBoxObjectCollectionTest
	{
		[Test]
		public void ComboBoxObjectCollectionPropertyTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			Assert.AreEqual (false, col.IsReadOnly, "#B1");
			Assert.AreEqual (false, ((ICollection)col).IsSynchronized, "#B2");
			Assert.AreEqual (col, ((ICollection)col).SyncRoot, "#B3");
			Assert.AreEqual (false, ((IList)col).IsFixedSize, "#B4");
		}

		[Test]
		public void AddTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (2, col.Count, "#C1");
		}

		[Test]
		public void Add_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			try {
				col.Add (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("item", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AddRange_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			try {
				col.AddRange (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("items", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ClearTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Clear ();
			Assert.AreEqual (0, col.Count, "#D1");
		}

		[Test]
		public void ContainsTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			object obj = "Item1";
			col.Add (obj);
			Assert.AreEqual (true, col.Contains ("Item1"), "#E1");
			Assert.AreEqual (false, col.Contains ("Item2"), "#E2");
		}

		[Test]
		public void Contains_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			try {
				col.Contains (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
#if NET_2_0
				Assert.AreEqual ("value", ex.ParamName, "#6");
#endif
			}
		}

		[Test]
		public void IndexOfTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (1, col.IndexOf ("Item2"), "#F1");
		}

		[Test]
		public void IndexOf_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			try {
				col.IndexOf (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
#if NET_2_0
				Assert.AreEqual ("value", ex.ParamName, "#6");
#endif
			}
		}

		[Test]
		public void Insert_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			try {
				col.Insert (0, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("item", ex.ParamName, "#6");
			}
		}

		[Test]
		public void RemoveTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.Remove ("Item1");
			Assert.AreEqual (1, col.Count, "#1");
			col.Remove (null);
			Assert.AreEqual (1, col.Count, "#2");
		}

		[Test]
		public void RemoveAtTest ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			col.Add ("Item2");
			col.RemoveAt (0);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.AreEqual (true, col.Contains ("Item2"), "#H1");
		}

		[Test]
		public void Indexer_Null ()
		{
			ComboBox.ObjectCollection col = new ComboBox.ObjectCollection (new ComboBox ());
			col.Add ("Item1");
			try {
				col [0] = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("value", ex.ParamName, "#6");
			}
		}
	}

}
