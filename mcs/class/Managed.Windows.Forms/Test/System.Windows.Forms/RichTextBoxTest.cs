//
// RichTextBoxTest.cs: Test cases for RichTextBox.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class RichTextBoxBaseTest
	{
#if not
		[Test]
		public void RichTextBoxPropertyTest ()
		{
			RichTextBox rTBox = new RichTextBox ();
			
			// A
			Assert.AreEqual (false, rTBox.AllowDrop, "#A1");
			rTBox.Multiline = true;
			rTBox.AcceptsTab = true;
			SendKeys.SendWait ("^%");
			Assert.AreEqual (false, rTBox.AutoSize, "#A2");
			Assert.AreEqual (false, rTBox.AutoWordSelection, "#A3");
			
			
			// B
			rTBox.BackColor = Color.White;
			Assert.AreEqual (null, rTBox.BackgroundImage, "#B1");
			string gif = "M.gif";
			rTBox.BackgroundImage = Image.FromFile (gif);
			// comparing image objects fails on MS .Net so using Size property
			Assert.AreEqual (Image.FromFile(gif, true).Size, rTBox.BackgroundImage.Size, "#B2");
			Assert.AreEqual (0, rTBox.BulletIndent, "#B3");
			
			// C
			Assert.AreEqual (false, rTBox.CanRedo, "#C1");
			rTBox.Paste ();
			Assert.AreEqual (false, rTBox.CanRedo, "#C2");
			rTBox.ClearUndo ();
			Assert.AreEqual (false, rTBox.CanRedo, "#C3");

			// D
			Assert.AreEqual (true, rTBox.DetectUrls, "#D1");

			// F 
			Assert.AreEqual (FontStyle.Regular, rTBox.Font.Style, "#F1");
			Assert.AreEqual ("WindowText", rTBox.ForeColor.Name, "#F2");

			//M
			Assert.AreEqual (2147483647, rTBox.MaxLength, "#M1");
			Assert.AreEqual (true, rTBox.Multiline, "#M2");
			rTBox.WordWrap = false;
			Assert.AreEqual (true, rTBox.Multiline, "#M3");
			
			// R
			Assert.AreEqual ("", rTBox.RedoActionName, "#R1");
			Assert.AreEqual (0, rTBox.RightMargin, "#R2");
			
			// [MonoTODO ("Assert.AreEqual (false, rTBox.Rtf, "#R3");") ]
						
			// S
	
			// [MonoTODO ("Assert.AreEqual (ScrollBars.Both, rTBox.ScrollBars, "#S1");")]
			Assert.AreEqual (RichTextBoxScrollBars.Both, rTBox.ScrollBars, "#S1");
			Assert.AreEqual ("", rTBox.SelectedText, "#S3");
			rTBox.Text = "sample TextBox";
			Assert.AreEqual (HorizontalAlignment.Left, rTBox.SelectionAlignment, "#S5");
			Assert.AreEqual (false, rTBox.SelectionBullet, "#S6");
			Assert.AreEqual (0, rTBox.SelectionCharOffset, "#S7");
			//Assert.AreEqual (Color.Black, rTBox.SelectionColor, "#S8"); // Random color
			Assert.AreEqual ("Courier New", rTBox.SelectionFont.Name, "#S9a");
			Assert.AreEqual (FontStyle.Regular, rTBox.SelectionFont.Style, "#S9b");
			Assert.AreEqual (0, rTBox.SelectionHangingIndent, "#S10");
			Assert.AreEqual (0, rTBox.SelectionIndent, "#S11");
			Assert.AreEqual (0, rTBox.SelectionLength, "#S12");
			Assert.AreEqual (false, rTBox.SelectionProtected, "#S13");
			Assert.AreEqual (0, rTBox.SelectionRightIndent, "#S14");
			Assert.AreEqual (false, rTBox.ShowSelectionMargin, "#S15");
			// [MonoTODO ("Assert.AreEqual (, rTBox.SelectionTabs, "#S16");")]
			// [MonoTODO("Assert.AreEqual (TypeCode.Empty, rTBox.SelectionType, "#S17");")] 
			
			// T
			Assert.AreEqual ("sample TextBox", rTBox.Text, "#T1");
			Assert.AreEqual (14, rTBox.TextLength, "#T2");
			
			// UVW 
			Assert.AreEqual ("", rTBox.UndoActionName, "#U1");

			// XYZ
			Assert.AreEqual (1, rTBox.ZoomFactor, "#Z1");

		}

		[Test]
		public void CanPasteTest ()
		{
			RichTextBox rTextBox = new RichTextBox ();
			Bitmap myBitmap = new Bitmap ("M.gif");
            		Clipboard.SetDataObject (myBitmap);
			DataFormats.Format myFormat = DataFormats.GetFormat (DataFormats.Bitmap);
			Assert.AreEqual (true, rTextBox.CanPaste (myFormat), "#Mtd1");
		}
#endif
		
		[Test]
		public void FindCharTest ()
		{
			RichTextBox rTextBox = new RichTextBox ();
			rTextBox.Text = "something";
			Assert.AreEqual (2, rTextBox.Find (new char [] {'m'}), "#Mtd3");
			Assert.AreEqual (-1, rTextBox.Find (new char [] {'t'},5), "#Mtd3a");
			Assert.AreEqual (4, rTextBox.Find (new char [] {'t'},4,5), "#Mtd3b");
		}
		
		[Test]
		public void FindStringTest ()
		{
			RichTextBox rTextBox = new RichTextBox ();
			rTextBox.Text = "sample text for richtextbox";
			int indexToText1 = rTextBox.Find ("for");
			Assert.AreEqual (12, indexToText1, "#Mtd4");
			int indexToText2 = rTextBox.Find ("for", 0, 14, RichTextBoxFinds.MatchCase);
			Assert.AreEqual (-1, indexToText2, "#Mtd5");
			int indexToText3 = rTextBox.Find ("for", 0, 15, RichTextBoxFinds.MatchCase);
			Assert.AreEqual (12, indexToText3, "#Mtd6");
			int indexToText4 = rTextBox.Find ("richtextbox", 0, RichTextBoxFinds.MatchCase);
			Assert.AreEqual (16, indexToText4, "#Mtd7");
			int indexToText5 = rTextBox.Find ("text", RichTextBoxFinds.MatchCase);
			Assert.AreEqual (7, indexToText5, "#Mtd8");
		}

		[Test]
		public void FindTest() {
			RichTextBox t = new RichTextBox();

			t.Text = "Testtext and arglblah may not be what we're looking for\n, but blah Blah is";

			Assert.AreEqual(t.Find(new char[] {'b', 'l', 'a', 'h'}), 9, "Find1");
			Assert.AreEqual(t.Find(new char[] {'b', 'l', 'a', 'h'}, 20), 20, "Find2");
			Assert.AreEqual(t.Find(new char[] {'b', 'l', 'a', 'h'}, 25, 30), -1, "Find3");
			Assert.AreEqual(t.Find("blah"), 17, "Find4");
			Assert.AreEqual(t.Find("blah", 10, 30, RichTextBoxFinds.None), 17, "Find5");
			Assert.AreEqual(t.Find("blah", 10, 30, RichTextBoxFinds.WholeWord), -1, "Find6");
			Assert.AreEqual(t.Find("blah", 10, 30, RichTextBoxFinds.MatchCase), 17, "Find7");
			Assert.AreEqual(t.Find("blah", 10, 70, RichTextBoxFinds.Reverse), 62, "Find8");
			Assert.AreEqual(t.Find("blah", 10, 73, RichTextBoxFinds.Reverse), 67, "Find9");
			Assert.AreEqual(t.Find("blah", 10, 73, RichTextBoxFinds.Reverse | RichTextBoxFinds.MatchCase), 62, "Find10");
			Assert.AreEqual(t.Find("blah", 10, RichTextBoxFinds.None), 17, "Find11");
			Assert.AreEqual(t.Find("blah", 10, RichTextBoxFinds.WholeWord), 62, "Find12");
			Assert.AreEqual(t.Find("blah", 10, RichTextBoxFinds.MatchCase), 17, "Find13");
			Assert.AreEqual(t.Find("blah", 10, RichTextBoxFinds.Reverse), 67, "Find14");
			Assert.AreEqual(t.Find("blah", 10, RichTextBoxFinds.Reverse | RichTextBoxFinds.MatchCase), 62, "Find15");
			Assert.AreEqual(t.Find("blah", RichTextBoxFinds.Reverse), 67, "Find16");
			Assert.AreEqual(t.Find("blah", RichTextBoxFinds.MatchCase), 17, "Find17");
			Assert.AreEqual(t.Find("blah", RichTextBoxFinds.WholeWord), 62, "Find18");

			// Special cases
			AreEqual(t.Find("blah", 10, 11, RichTextBoxFinds.None), -1, "Find19");	// Range to short to ever match
			AreEqual(t.Find("blah", 17, 18, RichTextBoxFinds.None), -1, "Find20");	// Range to short to ever match, but starts matching
			AreEqual(t.Find("is", RichTextBoxFinds.WholeWord), 72, "Find21");	// Last word in document
			AreEqual(t.Find("for", RichTextBoxFinds.WholeWord), 52, "Find22");	// word followed by \n
			AreEqual(t.Find("Testtext", RichTextBoxFinds.WholeWord), 0, "Find23");	// First word in document
			AreEqual(t.Find("Testtext", RichTextBoxFinds.WholeWord | RichTextBoxFinds.Reverse), 0, "Find24");	// First word in document, searched in reverse
		}

		[Test]
		public void GetCharFromPositionTest ()
		{
			Form myForm = new Form ();
			RichTextBox rTextBox = new RichTextBox ();
			rTextBox.Text = "sample text for richtextbox";
			myForm.Controls.Add (rTextBox);
			Assert.AreEqual ('m', rTextBox.GetCharFromPosition (new Point (10, 10)), "#21");
		}
		//[MonoTODO("Add test for method Paste (Visual Test)")]
	}
}
