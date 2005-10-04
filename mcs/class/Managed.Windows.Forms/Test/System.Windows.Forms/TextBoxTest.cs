//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TextBoxBaseTest
	{
		[Test]
		public void TextBoxBasePropertyTest ()
		{
			TextBox tb = new TextBox ();
			Assert.AreEqual (false, tb.AcceptsTab, "#1a");
			tb.Multiline = true;
			tb.AcceptsTab = true;
			SendKeys.SendWait ("^%");
			Assert.AreEqual (true, tb.AcceptsTab, "#1b");
			Assert.AreEqual (true, tb.AutoSize, "#2");
			Assert.AreEqual ("Window", tb.BackColor.Name, "#3a");
			tb.BackColor = Color.White;
			Assert.AreEqual ("White", tb.BackColor.Name, "#3b");
			Assert.AreEqual (null, tb.BackgroundImage, "#4a");
			string gif = "M.gif";
			tb.BackgroundImage = Image.FromFile (gif);
			// comparing image objects fails on MS .Net so using Size property
			Assert.AreEqual (Image.FromFile(gif, true).Size, tb.BackgroundImage.Size, "#4b");
			
			Assert.AreEqual (BorderStyle.Fixed3D, tb.BorderStyle, "#5");
			Assert.AreEqual (false, tb.CanUndo, "#6a");
			tb.Paste ();
			Assert.AreEqual (true, tb.CanUndo, "#6b");
			tb.ClearUndo ();
			Assert.AreEqual (false, tb.CanUndo, "#6c");
			Assert.AreEqual ("WindowText", tb.ForeColor.Name, "#7");
			Assert.AreEqual (true, tb.HideSelection, "#8");
			Assert.AreEqual (1, tb.Lines.Length, "#9");
			Assert.AreEqual (32767, tb.MaxLength, "#10");
			Assert.AreEqual (true, tb.Modified, "#11");
			Assert.AreEqual (true, tb.Multiline, "#12a");
			tb.WordWrap = false;
			Assert.AreEqual (true, tb.Multiline, "#12b");
			tb.AcceptsReturn = true;
			Assert.AreEqual (true, tb.Multiline, "#12c");
			Assert.AreEqual (20, tb.PreferredHeight, "#13");
			Assert.AreEqual (false, tb.ReadOnly, "#14");
			Assert.AreEqual ("", tb.SelectedText, "#15");
			tb.Text = "sample TextBox";
			Assert.AreEqual (0, tb.SelectionLength, "#16b");
			Assert.AreEqual (0, tb.SelectionStart, "#17");
			tb.WordWrap = false;
			tb.AcceptsReturn = true;
			Assert.AreEqual ("sample TextBox", tb.Text, "#18");
			Assert.AreEqual (14, tb.TextLength, "#19");
			Assert.AreEqual (false, tb.WordWrap, "#20");
		}

		[Test]
		public void TextBoxPropertyTest ()
		{
			TextBox tb = new TextBox ();
			Assert.AreEqual (false, tb.AcceptsReturn, "#21");
			Assert.AreEqual (CharacterCasing.Normal, tb.CharacterCasing, "#22");
			Assert.AreEqual ('\0', tb.PasswordChar, "#23");
			tb.PasswordChar = '*';
			Assert.AreEqual ('*', tb.PasswordChar, "#23b");
			Assert.AreEqual (ScrollBars.None, tb.ScrollBars, "#24");
			Assert.AreEqual (-1, tb.SelectionLength, "#25");
			Assert.AreEqual (HorizontalAlignment.Left , tb.TextAlign, "#26");
		}

		[Test]
		public void AppendTextTest ()
		{   
			Form f = new Form (); 
			f.Visible = true;
			TextBox tb1 = new TextBox ();
			tb1.Visible = true;
			tb1.Text = "TextBox1";
			TextBox tb2 = new TextBox ();
			tb2.Visible = true;
			f.Controls.Add (tb1);
			f.Controls.Add (tb2);
			tb2.AppendText (tb1.Text);
			Assert.AreEqual ("TextBox1", tb2.Text, "#27");
		}

		[Test]
		public void ClearTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "TextBox1";
			Assert.AreEqual ("TextBox1", tb1.Text, "#28a" );
			tb1.Clear ();
			Assert.AreEqual ("", tb1.Text, "#28b");
		}

		[Test]
		public void ClearUndoTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "TextBox1";
			tb1.SelectionLength = 4;
			tb1.Copy ();
			Assert.AreEqual ("Text", tb1.SelectedText, "#29a");
			tb1.Paste ();
			Assert.AreEqual (true, tb1.CanUndo, "#29b");
			tb1.ClearUndo ();
			Assert.AreEqual (false, tb1.CanUndo, "#29c");
		}

		[Test]
		public void CopyTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "ABCDE";
			tb1.SelectionLength = 4;
			tb1.Copy ();
			Assert.AreEqual ("ABCD", tb1.SelectedText, "#30");
		}

		[Test]
		public void CutTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "ABCDE";
			tb1.SelectionLength = 4;
			tb1.Cut ();
			Assert.AreEqual ("E", tb1.Text, "#31");
		}

		[Test]
		public void PasteTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "ABCDE";
			tb1.SelectionLength = 4;
			tb1.SelectionStart = tb1.SelectionStart + tb1.SelectionLength;
			tb1.Paste ();
			Assert.AreEqual ("ABCDABCD", tb1.Text, "#32");
		}

		[Test]
		public void SelectTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "This is a sample test.";
			tb1.Select (0, 4);
			Assert.AreEqual ("This", tb1.SelectedText, "#33");
		}

		[Test]
		public void SelectAllTest ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "This is a sample test.";
			tb1.SelectAll ();
			Assert.AreEqual ("This is a sample test.", tb1.SelectedText, "#34");
		}

		[Test]
		public void ToStringTest ()
		{
			TextBox tb1 = new TextBox ();
			Assert.AreEqual ("System.Windows.Forms.TextBox, Text: ", tb1.ToString(), "#35");
		}

		[Test]
		public void UndoTest1 ()
		{
			TextBox tb1 = new TextBox ();
			tb1.Text = "ABCDE";
			tb1.SelectionLength = 4;
			tb1.Copy ();
			tb1.SelectionStart = tb1.SelectionStart + tb1.SelectionLength;
			tb1.Paste ();
			tb1.Undo ();
			Assert.AreEqual ("ABCDE", tb1.Text, "#36");
		}

	}
}
