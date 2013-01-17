//
// RichTextBoxTest.cs: Test cases for RichTextBox.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class RichTextBoxTest : TestHelper
	{
		[Test]
		public void RichTextBoxPropertyTest ()
		{
			RichTextBox rTBox = new RichTextBox ();
			
			// A
			Assert.AreEqual (false, rTBox.AllowDrop, "#A1");
			rTBox.Multiline = true;
			rTBox.AcceptsTab = true;
			//SendKeys.SendWait ("^%");
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
			//Assert.AreEqual (false, rTBox.CanRedo, "#C1");
			//rTBox.Paste ();
			//Assert.AreEqual (false, rTBox.CanRedo, "#C2");
			//rTBox.ClearUndo ();
			//Assert.AreEqual (false, rTBox.CanRedo, "#C3");

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
			Assert.AreEqual ("", rTBox.SelectedText, "#S3");
			rTBox.Text = "sample TextBox";
			Assert.AreEqual (HorizontalAlignment.Left, rTBox.SelectionAlignment, "#S5");
			Assert.AreEqual (false, rTBox.SelectionBullet, "#S6");
			Assert.AreEqual (0, rTBox.SelectionCharOffset, "#S7");
			//Assert.AreEqual (Color.Black, rTBox.SelectionColor, "#S8"); // Random color
			//Assert.AreEqual ("Courier New", rTBox.SelectionFont.Name, "#S9a");
			Assert.AreEqual (FontStyle.Regular, rTBox.SelectionFont.Style, "#S9b");
			Assert.AreEqual (0, rTBox.SelectionHangingIndent, "#S10");
			Assert.AreEqual (0, rTBox.SelectionIndent, "#S11");
			//Assert.AreEqual (0, rTBox.SelectionLength, "#S12");
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

#if not
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
		public void BackColor ()
		{
			RichTextBox rtb = new RichTextBox ();
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#1");
			rtb.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, rtb.BackColor, "#2");
			rtb.BackColor = Color.Empty;
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#3");
		}

		[Test] // bug #80626
		public void BorderStyle_None ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.BorderStyle = BorderStyle.None;
			Assert.AreEqual (96, rtb.Height, "#1");
			rtb.CreateControl ();
			Assert.AreEqual (96, rtb.Height, "#2");
		}

		[Test] // bug #80620
		public void ClientRectangle_Borders ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.CreateControl ();
			Assert.AreEqual (rtb.ClientRectangle, new RichTextBox ().ClientRectangle);
		}

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
			Assert.AreEqual(t.Find("blah", 10, 11, RichTextBoxFinds.None), -1, "Find19");	// Range to short to ever match
			Assert.AreEqual(t.Find("blah", 17, 18, RichTextBoxFinds.None), -1, "Find20");	// Range to short to ever match, but starts matching
			Assert.AreEqual(t.Find("is", RichTextBoxFinds.WholeWord), 72, "Find21");	// Last word in document
			Assert.AreEqual(t.Find("for", RichTextBoxFinds.WholeWord), 52, "Find22");	// word followed by \n
			Assert.AreEqual(t.Find("Testtext", RichTextBoxFinds.WholeWord), 0, "Find23");	// First word in document
			Assert.AreEqual(t.Find("Testtext", RichTextBoxFinds.WholeWord | RichTextBoxFinds.Reverse), 0, "Find24");	// First word in document, searched in reverse
		}

		[Test] // bug #80301
		[Ignore ("Depends on specific DPI")]
		public void PreferredHeight ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.Font = new Font ("Arial", 14);
			Assert.AreEqual (29, rtb.PreferredHeight, "#A1");
			rtb.Font = new Font ("Arial", 16);
			Assert.AreEqual (32, rtb.PreferredHeight, "#A2");
			rtb.Font = new Font ("Arial", 17);
			Assert.AreEqual (34, rtb.PreferredHeight, "#A3");

			rtb.BorderStyle = BorderStyle.None;

			Assert.AreEqual (27, rtb.PreferredHeight, "#B1");
			rtb.Font = new Font ("Arial", 14);
			Assert.AreEqual (22, rtb.PreferredHeight, "#B2");
			rtb.Font = new Font ("Arial", 16);
			Assert.AreEqual (25, rtb.PreferredHeight, "#B3");
		}

		[Test]
		public void ReadOnly_BackColor_NotSet ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.ReadOnly = true;
			Assert.IsTrue (rtb.ReadOnly, "#A1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#A2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#A2");
#endif

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (rtb);
			form.Show ();

			Assert.IsTrue (rtb.ReadOnly, "#B1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#B2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#B2");
#endif

			rtb.ResetBackColor ();
			Assert.IsTrue (rtb.ReadOnly, "#C1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#C2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#C2");
#endif

			rtb.ReadOnly = false;
			Assert.IsFalse (rtb.ReadOnly, "#D1");
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#D2");

			rtb.ReadOnly = true;
			Assert.IsTrue (rtb.ReadOnly, "#E1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#E2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#E2");
#endif

			rtb.BackColor = Color.Red;
			Assert.IsTrue (rtb.ReadOnly, "#F1");
			Assert.AreEqual (Color.Red, rtb.BackColor, "#F2");

			rtb.ReadOnly = false;
			Assert.IsFalse (rtb.ReadOnly, "#G1");
			Assert.AreEqual (Color.Red, rtb.BackColor, "#G2");

			rtb.ReadOnly = true;
			Assert.IsTrue (rtb.ReadOnly, "#H1");
			Assert.AreEqual (Color.Red, rtb.BackColor, "#H2");

			rtb.ResetBackColor ();
			Assert.IsTrue (rtb.ReadOnly, "#I1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#I2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#I2");
#endif
			form.Close ();
		}

		[Test]
		public void Modified ()
		{
			RichTextBox rtb = new RichTextBox ();
			Assert.AreEqual (false, rtb.Modified, "#A1");

			rtb.SelectedText = "mono";
			Assert.AreEqual (true, rtb.Modified, "#B1");

			rtb.Modified = false;
			Assert.AreEqual (false, rtb.Modified, "#C1");

			// Only SelectedText seems to cause a change in Modified, as opposed to Text
			rtb.Text = "moon";
			Assert.AreEqual (false, rtb.Modified, "#D1");
		}

		[Test]
		public void ReadOnly_BackColor_Set ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.BackColor = Color.Blue;
			rtb.ReadOnly = true;
			Assert.IsTrue (rtb.ReadOnly, "#A1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#A2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (rtb);
			form.Show ();

			Assert.IsTrue (rtb.ReadOnly, "#B1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#B2");

			rtb.ReadOnly = false;
			Assert.IsFalse (rtb.ReadOnly, "#C1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#C2");

			rtb.ReadOnly = true;
			Assert.IsTrue (rtb.ReadOnly, "#D1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#D2");

			rtb.BackColor = Color.Red;
			Assert.IsTrue (rtb.ReadOnly, "#E1");
			Assert.AreEqual (Color.Red, rtb.BackColor, "#E2");

			rtb.ReadOnly = false;
			Assert.IsFalse (rtb.ReadOnly, "#F1");
			Assert.AreEqual (Color.Red, rtb.BackColor, "#F2");

			rtb.ReadOnly = true;
			rtb.ResetBackColor ();
			Assert.IsTrue (rtb.ReadOnly, "#G1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, rtb.BackColor, "#G2");
#else
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#G2");
#endif

			form.Dispose ();

			rtb = new RichTextBox ();
			rtb.ReadOnly = true;
			rtb.BackColor = Color.Blue;
			Assert.IsTrue (rtb.ReadOnly, "#H1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#H2");

			form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (rtb);
			form.Show ();

			Assert.IsTrue (rtb.ReadOnly, "#I1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#I2");

			rtb.ReadOnly = false;
			Assert.IsFalse (rtb.ReadOnly, "#J1");
			Assert.AreEqual (Color.Blue, rtb.BackColor, "#J2");

			rtb.ResetBackColor ();
			Assert.IsFalse (rtb.ReadOnly, "#K1");
			Assert.AreEqual (SystemColors.Window, rtb.BackColor, "#K2");
			
			form.Close ();
		}

		[Test]
		public void ScrollBarsTest ()
		{
			RichTextBox rtb = new RichTextBox ();
			Assert.AreEqual (RichTextBoxScrollBars.Both, rtb.ScrollBars, "#1");
			rtb.ScrollBars = RichTextBoxScrollBars.Vertical;
			Assert.AreEqual (RichTextBoxScrollBars.Vertical, rtb.ScrollBars, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void ScrollBars_Invalid ()
		{
			RichTextBox rtb = new RichTextBox ();
			rtb.ScrollBars = (RichTextBoxScrollBars) 666;
		}

		[Test]
		public void SelectionFontTest ()
		{
			RichTextBox t = new RichTextBox();
			t.Text = "123";
			t.SelectionStart = 1;
			t.SelectionLength = 1;
			Font f = new Font(FontFamily.GenericMonospace, 120);
			t.SelectionFont = f;

			Assert.AreEqual (t.SelectionFont.Size, f.Size, "A1");
		}

		[Test]
		public void SelectionLength_Negative ()
		{
			RichTextBox rtb = new RichTextBox ();
			try {
				rtb.SelectionLength = -1;
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("SelectionLength", ex.ParamName, "#6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}

		[Test]
		public void SelectionStart_Negative ()
		{
			RichTextBox rtb = new RichTextBox ();
			try {
				rtb.SelectionStart = -1;
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("SelectionStart", ex.ParamName, "#6");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
#endif
		}
		
		RichTextBox rtb;
		
		[Test]	// This test passes if it doesn't throw an NRE.
		public void Bug351886 ()
		{
			Form form = new Form ();
			rtb = new RichTextBox ();
			rtb.Dock = DockStyle.Fill;

			rtb.SelectionFont = new Font (FontFamily.GenericSansSerif, 72f);
			rtb.AppendText ("Left and make this very long so that it can be word wrapped ");

			form.Load += new EventHandler (Bug351886_Load);

			form.Controls.Add (rtb);
			form.Show ();
			
			form.Close ();
			form.Dispose ();
		}

		void Bug351886_Load (object sender, EventArgs e)
		{
			rtb.SelectAll ();
			rtb.SelectionAlignment = HorizontalAlignment.Center;
		}

	}
}
