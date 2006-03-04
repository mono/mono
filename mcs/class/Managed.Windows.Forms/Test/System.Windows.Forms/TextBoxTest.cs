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
		TextBox textBox;

		[SetUp]
		public void SetUp()
		{
			textBox = new TextBox();
		}

		[Test]
		public void TextBoxBasePropertyTest ()
		{
			Assert.AreEqual (false, textBox.AcceptsTab, "#1a");
			textBox.Multiline = true;
			textBox.AcceptsTab = true;
			SendKeys.SendWait ("^%");
			Assert.AreEqual (true, textBox.AcceptsTab, "#1b");
			Assert.AreEqual (true, textBox.AutoSize, "#2");
			Assert.AreEqual ("Window", textBox.BackColor.Name, "#3a");
			textBox.BackColor = Color.White;
			Assert.AreEqual ("White", textBox.BackColor.Name, "#3b");
			Assert.AreEqual (null, textBox.BackgroundImage, "#4a");
			string gif = "M.gif";
			textBox.BackgroundImage = Image.FromFile (gif);
			// comparing image objects fails on MS .Net so using Size property
			Assert.AreEqual (Image.FromFile(gif, true).Size, textBox.BackgroundImage.Size, "#4b");
			
			Assert.AreEqual (BorderStyle.Fixed3D, textBox.BorderStyle, "#5");
			Assert.AreEqual (false, textBox.CanUndo, "#6a");
			textBox.Paste ();
			Assert.AreEqual (true, textBox.CanUndo, "#6b");
			textBox.ClearUndo ();
			Assert.AreEqual (false, textBox.CanUndo, "#6c");
			Assert.AreEqual ("WindowText", textBox.ForeColor.Name, "#7");
			Assert.AreEqual (true, textBox.HideSelection, "#8");
			Assert.AreEqual (1, textBox.Lines.Length, "#9");
			Assert.AreEqual (32767, textBox.MaxLength, "#10");
			Assert.AreEqual (true, textBox.Modified, "#11");
			Assert.AreEqual (true, textBox.Multiline, "#12a");
			textBox.WordWrap = false;
			Assert.AreEqual (true, textBox.Multiline, "#12b");
			textBox.AcceptsReturn = true;
			Assert.AreEqual (true, textBox.Multiline, "#12c");
			Assert.AreEqual (20, textBox.PreferredHeight, "#13");
			Assert.AreEqual (false, textBox.ReadOnly, "#14");
			Assert.AreEqual ("", textBox.SelectedText, "#15");
			textBox.Text = "sample TextBox";
			Assert.AreEqual (0, textBox.SelectionLength, "#16b");
			Assert.AreEqual (0, textBox.SelectionStart, "#17");
			textBox.WordWrap = false;
			textBox.AcceptsReturn = true;
			Assert.AreEqual ("sample TextBox", textBox.Text, "#18");
			Assert.AreEqual (14, textBox.TextLength, "#19");
			Assert.AreEqual (false, textBox.WordWrap, "#20");
		}

		[Test]
		public void TextBoxPropertyTest ()
		{
			Assert.AreEqual (false, textBox.AcceptsReturn, "#21");
			Assert.AreEqual (CharacterCasing.Normal, textBox.CharacterCasing, "#22");
			Assert.AreEqual ('\0', textBox.PasswordChar, "#23");
			textBox.PasswordChar = '*';
			Assert.AreEqual ('*', textBox.PasswordChar, "#23b");
			Assert.AreEqual (ScrollBars.None, textBox.ScrollBars, "#24");
			Assert.AreEqual (-1, textBox.SelectionLength, "#25");
			Assert.AreEqual (HorizontalAlignment.Left , textBox.TextAlign, "#26");
		}

#if NET_2_0
		[Test]
		public void UseSystemPasswordCharDefault()
		{
			Assert.IsFalse(textBox.UseSystemPasswordChar);
		}

		[Test]
		public void UseSystemPasswordCharOverridesPasswordChar()
		{
			textBox.PasswordChar = '!';
			textBox.UseSystemPasswordChar = true;
			Assert.AreEqual('*', textBox.PasswordChar);
		}
#endif

		[Test]
		public void AppendTextTest ()
		{   
			Form f = new Form (); 
			f.Visible = true;
			textBox.Visible = true;
			textBox.Text = "TextBox1";
			TextBox textBox2 = new TextBox ();
			textBox2.Visible = true;
			f.Controls.Add (textBox);
			f.Controls.Add (textBox2);
			textBox2.AppendText (textBox.Text);
			Assert.AreEqual ("TextBox1", textBox2.Text, "#27");
		}

		[Test]
		public void ClearTest ()
		{
			textBox.Text = "TextBox1";
			Assert.AreEqual ("TextBox1", textBox.Text, "#28a" );
			textBox.Clear ();
			Assert.AreEqual ("", textBox.Text, "#28b");
		}

		[Test]
		public void ClearUndoTest ()
		{
			textBox.Text = "TextBox1";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			Assert.AreEqual ("Text", textBox.SelectedText, "#29a");
			textBox.Paste ();
			Assert.AreEqual (true, textBox.CanUndo, "#29b");
			textBox.ClearUndo ();
			Assert.AreEqual (false, textBox.CanUndo, "#29c");
		}

		[Test]
		public void CopyTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			Assert.AreEqual ("ABCD", textBox.SelectedText, "#30");
		}

		[Test]
		public void CutTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Cut ();
			Assert.AreEqual ("E", textBox.Text, "#31");
		}

		[Test]
		public void PasteTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength;
			textBox.Paste ();
			Assert.AreEqual ("ABCDABCD", textBox.Text, "#32");
		}

		[Test]
		public void SelectTest ()
		{
			textBox.Text = "This is a sample test.";
			textBox.Select (0, 4);
			Assert.AreEqual ("This", textBox.SelectedText, "#33");
		}

		[Test]
		public void SelectAllTest ()
		{
			textBox.Text = "This is a sample test.";
			textBox.SelectAll ();
			Assert.AreEqual ("This is a sample test.", textBox.SelectedText, "#34");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("System.Windows.Forms.TextBox, Text: ", textBox.ToString(), "#35");
		}

		[Test]
		public void UndoTest1 ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength;
			textBox.Paste ();
			textBox.Undo ();
			Assert.AreEqual ("ABCDE", textBox.Text, "#36");
		}
	}
}
