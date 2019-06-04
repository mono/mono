//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//      Rolf Bjarne Kvinge  (RKvinge@novell.com)
//


using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using NUnit.Framework;
using System.Globalization;
using Thread=System.Threading.Thread;
using CategoryAttribute=NUnit.Framework.CategoryAttribute;
using System.Reflection;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MaskedTextBoxTest : TestHelper
	{
		[SetUp]
		protected override void SetUp () {
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("en-US");
			base.SetUp ();
		}

		[Test]
		public void InitialProperties ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.AcceptsTab, "#A1");
			Assert.AreEqual (true, mtb.AllowPromptAsInput, "#A2");
			Assert.AreEqual (false, mtb.AsciiOnly, "#A3");
			Assert.AreEqual (false, mtb.BeepOnError, "#B1");
			Assert.AreEqual (false, mtb.CanUndo, "#C1");
			Assert.IsNotNull (mtb.Culture, "#C3");
			Assert.AreEqual (Thread.CurrentThread.CurrentCulture.Name, mtb.Culture.Name, "#4");
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#C5");
			Assert.IsNull (mtb.FormatProvider, "#F1");
			Assert.AreEqual (false, mtb.HidePromptOnLeave, "#H1");
			Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#I1");
			Assert.AreEqual (false, mtb.IsOverwriteMode, "#I2");
			Assert.AreEqual (0, mtb.Lines.Length, "#L1");
			Assert.AreEqual ("", mtb.Mask, "#M1");
			Assert.AreEqual (true, mtb.MaskCompleted, "#M2");
			Assert.IsNull (mtb.MaskedTextProvider, "#M3");
			Assert.AreEqual (true, mtb.MaskFull, "#M4");
			Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#M5");
			Assert.AreEqual (false, mtb.Multiline, "#M6");
			Assert.AreEqual ('\0', mtb.PasswordChar, "#P1");
			Assert.AreEqual ('_', mtb.PromptChar, "#P2");
			Assert.AreEqual (false, mtb.ReadOnly, "#R1");
			Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#R2");
			Assert.AreEqual (true, mtb.ResetOnPrompt, "#R3");
			Assert.AreEqual (true, mtb.ResetOnSpace, "#R4");
			Assert.AreEqual ("", mtb.SelectedText, "#S1");
			Assert.AreEqual (true, mtb.SkipLiterals, "#S2");
			Assert.AreEqual ("", mtb.Text, "#T1");
			Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#T2");
			Assert.AreEqual (0, mtb.TextLength, "#T3");
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#T4");
			Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#U1");
			Assert.IsNull (mtb.ValidatingType, "#V1");
			Assert.AreEqual (false, mtb.WordWrap, "#W1");
			
			mtb.Dispose ();
		}
		
		[Test]
		public void WordWrapTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.WordWrap, "#W1");
			mtb.WordWrap = true;
			Assert.AreEqual (false, mtb.WordWrap, "#W2");
			
			mtb.Dispose ();
		}
		
		[Test]
		public void ValidatingTypeTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.IsNull (mtb.ValidatingType, "#V1");
			mtb.ValidatingType = typeof(int);
			Assert.IsNotNull (mtb.ValidatingType, "#V2");
			Assert.AreSame (typeof(int), mtb.ValidatingType, "#V3");
			mtb.Dispose ();
		}
		
		[Test]
		public void UseSystemPasswordCharTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#U1");
			mtb.UseSystemPasswordChar = true;
			Assert.AreEqual (true, mtb.UseSystemPasswordChar, "#U2");
			mtb.Dispose ();
		}
		
		[Test]
		public void TextMaskFormatTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#T1");
			mtb.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
			Assert.AreEqual (MaskFormat.ExcludePromptAndLiterals, mtb.TextMaskFormat, "#T2");
			mtb.TextMaskFormat = MaskFormat.IncludePromptAndLiterals;
			Assert.AreEqual (MaskFormat.IncludePromptAndLiterals, mtb.TextMaskFormat, "#T3");
			mtb.TextMaskFormat = MaskFormat.IncludePrompt;
			Assert.AreEqual (MaskFormat.IncludePrompt, mtb.TextMaskFormat, "#T4");
			mtb.TextMaskFormat = MaskFormat.IncludeLiterals;
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#T4");
			mtb.Dispose ();
		}
		
		[Test]
		[ExpectedException (typeof(InvalidEnumArgumentException))]
		public void TextMaskFormatExceptionTestException ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.TextMaskFormat = (MaskFormat) 123;
			mtb.Dispose ();
		}
		
		[Test]
		public void TextLengthTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (0, mtb.TextLength, "#T1");
			mtb.Text = "abc";
			Assert.AreEqual (3, mtb.TextLength, "#T2");
			
			mtb.Dispose ();
		}
		
		[Test]
		public void TextAlignTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#T1");
			mtb.TextAlign = HorizontalAlignment.Center;
			Assert.AreEqual (HorizontalAlignment.Center, mtb.TextAlign, "#T2");
			mtb.TextAlign = HorizontalAlignment.Right;
			Assert.AreEqual (HorizontalAlignment.Right, mtb.TextAlign, "#T3");
			mtb.TextAlign = HorizontalAlignment.Left;
			Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#T4");
			mtb.Dispose ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void TextAlignExceptionTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.TextAlign = (HorizontalAlignment) 123;
			mtb.Dispose ();
		}
		
		[Test]
		public void TextTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual ("", mtb.Text, "#T1");
			mtb.Text = "abc";
			Assert.AreEqual ("abc", mtb.Text, "#T2");
			mtb.Text = "ABC";
			Assert.AreEqual ("ABC", mtb.Text, "#T3");
			mtb.Mask = "abc";
			mtb.Text = "abc";
			Assert.AreEqual ("abc", mtb.Text, "#T4");
			mtb.Text = "ABC";
			Assert.AreEqual ("Abc", mtb.Text, "#T5");
			mtb.Text = "123";
			Assert.AreEqual ("1bc", mtb.Text, "#T6");
			mtb.Dispose ();
		}

		[Test]
		public void TextTest2 ()
		{
			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.Mask = "99 99";

			mtb.Text = "23 34";
			Assert.AreEqual ("23 34", mtb.Text, "#T1");

			mtb.RejectInputOnFirstFailure = true;
			mtb.Text = "23 34";
			Assert.AreEqual ("23 34", mtb.Text, "#T2");

			mtb.Dispose ();
		}

		[Test]
		public void TextTest3 ()
		{
			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.Mask = "00-00";
			mtb.Text = "12 3";
			Assert.AreEqual ("12- 3", mtb.Text, "#T1");

			mtb.Text = "b31i4";
			Assert.AreEqual ("31-4", mtb.Text, "#T2");

			mtb.Text = "1234";
			Assert.AreEqual ("12-34", mtb.Text, "#T3");

			mtb.Dispose ();
		}

		[Test]
		public void SkipLiteralsTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.SkipLiterals, "#S1");
			mtb.SkipLiterals = false;
			Assert.AreEqual (false, mtb.SkipLiterals, "#S2");
			mtb.Dispose ();
		}
		
		[Test]
		public void SelectedTextTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual ("", mtb.SelectedText, "#S1");
			mtb.Text = "abc";
			Assert.AreEqual ("", mtb.SelectedText, "#S2");
			mtb.SelectAll ();
			Assert.AreEqual ("abc", mtb.SelectedText, "#S3");
			mtb.Dispose ();
		}
		
		[Test]
		public void ResetOnSpaceTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.ResetOnSpace, "#R1");
			mtb.ResetOnSpace = false;
			Assert.AreEqual (false, mtb.ResetOnSpace, "#R2");
			mtb.Dispose ();
		}
		
		[Test]
		public void ResetOnPromptTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.ResetOnPrompt, "#R1");
			mtb.ResetOnPrompt = false;
			Assert.AreEqual (false, mtb.ResetOnPrompt, "#R2");
			mtb.Dispose ();
		}
		
		[Test]
		public void RejectInputOnFirstFailureTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#R1");
			mtb.RejectInputOnFirstFailure = true;
			Assert.AreEqual (true, mtb.RejectInputOnFirstFailure, "#R2");
			mtb.Dispose ();
		}
		
		[Test]
		public void ReadOnlyTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.ReadOnly, "#R1");
			mtb.ReadOnly = true;
			Assert.AreEqual (true, mtb.ReadOnly, "#R2");
			mtb.Dispose ();
		}
		
		[Test]
		public void PromptCharTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual ('_', mtb.PromptChar, "#P1");
			mtb.PromptChar = '*';
			Assert.AreEqual ('*', mtb.PromptChar, "#P2");
			mtb.Dispose ();
		}
		
		[Test]
		public void PasswordCharTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual ('\0', mtb.PasswordChar, "#P1");
			mtb.PasswordChar = '*';
			Assert.AreEqual ('*', mtb.PasswordChar, "#P2");
			mtb.Dispose ();
		}

		[Test]
		public void MultilineTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.Multiline, "#M1");
			mtb.Multiline = true;
			Assert.AreEqual (false, mtb.Multiline, "#M2");
			mtb.Dispose ();
		}
		
		[Test]
		public void MaskFullTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.MaskFull, "#M1");
			mtb.Mask = "abc";
			Assert.AreEqual (false, mtb.MaskFull, "#M2");
			mtb.Text = "abc";
			Assert.AreEqual (true, mtb.MaskFull, "#M3");
			mtb.Dispose ();
		}
		
		[Test]
		public void MaskedTextProviderTest ()
		{

			MaskedTextBox mtb;
			MaskedTextProvider mtp;

			mtb = new MaskedTextBox ();
			Assert.IsNull (mtb.MaskedTextProvider, "#M1");
			mtb.Mask = "abc";
			Assert.IsNotNull (mtb.MaskedTextProvider, "#M2");
			Assert.AreEqual ("abc", mtb.MaskedTextProvider.Mask, "#M3");
			// We always get a clone of the current mtp.
			Assert.IsTrue (mtb.MaskedTextProvider != mtb.MaskedTextProvider, "#M4");
			mtb.Dispose ();

			mtp = new MaskedTextProvider ("Z");
			mtb = new MaskedTextBox (mtp);
			Assert.IsNotNull (mtb.MaskedTextProvider, "#M5");
			Assert.AreEqual ("Z", mtb.MaskedTextProvider.Mask, "#6");
			Assert.IsTrue (mtb.MaskedTextProvider != mtb.MaskedTextProvider, "#M7");
			Assert.IsTrue (mtb.MaskedTextProvider != mtp, "#M8");
			mtb.Dispose ();
		}

		[Test]
		public void MaskCompletedTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.MaskCompleted, "#M1");
			mtb.Mask = "abcABCZZZ";
			Assert.AreEqual (false, mtb.MaskCompleted, "#M2");
			mtb.Text = "abcabcabc";
			Assert.AreEqual (true, mtb.MaskCompleted, "#M3");
			mtb.Dispose ();
		}

		[Test]
		public void MaskTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual ("", mtb.Mask, "#M1");
			mtb.Mask = "abc";
			Assert.AreEqual ("abc", mtb.Mask, "#M2");
			mtb.Mask = "";
			Assert.AreEqual ("", mtb.Mask, "#M3");
			mtb.Mask = null;
			Assert.AreEqual ("", mtb.Mask, "#M4");
			mtb.Mask = "";
			Assert.AreEqual ("", mtb.Mask, "#M5");
			mtb.Dispose ();
		}

		[Test]
		public void LinesTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (0, mtb.Lines.Length, "#L1");
			mtb.Text = "abc";
			Assert.AreEqual (1, mtb.Lines.Length, "#L2");
			Assert.AreEqual ("abc", mtb.Lines [0], "#L2a");
			mtb.Text = "abc\nabc";
			Assert.AreEqual (2, mtb.Lines.Length, "#L3");
			Assert.AreEqual ("abc", mtb.Lines [0], "#L3a");
			Assert.AreEqual ("abc", mtb.Lines [1], "#L3b");
			mtb.Dispose ();
		}

		[Test]
		public void IsOverwriteModeTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.IsOverwriteMode, "#I1");
			mtb.Dispose ();
		}

		[Test]
		public void InsertKeyModeTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#I1");
			mtb.InsertKeyMode = InsertKeyMode.Insert;
			Assert.AreEqual (InsertKeyMode.Insert, mtb.InsertKeyMode, "#I2");
			mtb.InsertKeyMode = InsertKeyMode.Overwrite;
			Assert.AreEqual (InsertKeyMode.Overwrite, mtb.InsertKeyMode, "#I3");
			mtb.InsertKeyMode = InsertKeyMode.Default;
			Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#I4");
			mtb.Dispose ();
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void InsertKeyModeExceptionTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.InsertKeyMode = (InsertKeyMode) 123;
			mtb.Dispose ();
		}
		
		[Test]
		public void HidePromptOnLeaveTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.HidePromptOnLeave, "#H1");
			mtb.HidePromptOnLeave = true;
			Assert.AreEqual (true, mtb.HidePromptOnLeave, "#H1");
			mtb.Dispose ();
		}
		
		[Test]
		public void FormatProviderTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.IsNull (mtb.FormatProvider, "#F1");
			mtb.FormatProvider = CultureInfo.CurrentCulture.NumberFormat;
			Assert.IsNotNull (mtb.FormatProvider, "#F2");
			Assert.AreSame (CultureInfo.CurrentCulture.NumberFormat, mtb.FormatProvider, "#F3");
			mtb.Dispose ();
		}
		
		[Test]
		public void CutCopyMaskFormatTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#C1");
			mtb.CutCopyMaskFormat = MaskFormat.ExcludePromptAndLiterals;
			Assert.AreEqual (MaskFormat.ExcludePromptAndLiterals, mtb.CutCopyMaskFormat, "#C2");
			mtb.CutCopyMaskFormat = MaskFormat.IncludeLiterals;
			Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#C3");
			mtb.CutCopyMaskFormat = MaskFormat.IncludePrompt;
			Assert.AreEqual (MaskFormat.IncludePrompt, mtb.CutCopyMaskFormat, "#C4");
			mtb.CutCopyMaskFormat = MaskFormat.IncludePromptAndLiterals;
			Assert.AreEqual (MaskFormat.IncludePromptAndLiterals, mtb.CutCopyMaskFormat, "#C5");
			mtb.Dispose ();
		}
		
		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void CutCopyMaskFormatExceptionTest ()
		{

			MaskedTextBox mtb;

			mtb = new MaskedTextBox ();
			mtb.CutCopyMaskFormat = (MaskFormat) 123;
			mtb.Dispose ();
		}
		
		[Test]
		public void CultureTest ()
		{
			MaskedTextBox mtb;
			MaskedTextProvider mtp;
			try {
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("en-US");
				mtb = new MaskedTextBox ();
				Assert.IsNotNull (mtb.Culture, "#A1");
				Assert.AreEqual ("en-US", mtb.Culture.Name, "#A2");
				mtb.Dispose ();

				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("es-ES");
				mtb = new MaskedTextBox ();
				Assert.IsNotNull (mtb.Culture, "#B1");
				Assert.AreEqual ("es-ES", mtb.Culture.Name, "#B2");
				mtb.Dispose ();

				mtp = new MaskedTextProvider ("abc");
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("es-AR");
				mtb = new MaskedTextBox (mtp);
				Assert.IsNotNull (mtb.Culture, "#C1");
				Assert.AreEqual ("es-ES", mtb.Culture.Name, "#C2");
				mtb.Dispose ();
				
				mtb = new MaskedTextBox ();
				mtb.Culture = CultureInfo.GetCultureInfo ("de-DE");
				Assert.AreEqual ("de-DE", mtb.Culture.Name, "#D1");
				
			} finally {
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("en-US");
			}
		}
		
		[Test]
		public void CanUndoTest ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			TextBoxBase tbb = mtb;
			Assert.AreEqual (false, mtb.CanUndo, "#A1");
			Assert.AreEqual (false, tbb.CanUndo, "#A2");
			mtb.Dispose ();
		}
		
		[Test]
		public void BeepOnErrorTest ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.BeepOnError, "#A1");
			mtb.BeepOnError = true;
			Assert.AreEqual (true, mtb.BeepOnError, "#A2");
			mtb.BeepOnError = false;
			Assert.AreEqual (false, mtb.BeepOnError, "#A3");
			mtb.Dispose ();
		}

		[Test]
		public void AsciiOnlyTest ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.AsciiOnly, "#A1");
			mtb.AsciiOnly = true;
			Assert.AreEqual (true, mtb.AsciiOnly, "#A2");
			mtb.AsciiOnly = false;
			Assert.AreEqual (false, mtb.AsciiOnly, "#A3");
			mtb.Dispose ();
		}
		
		[Test]
		public void AllowPromptAsInputTest ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			Assert.AreEqual (true, mtb.AllowPromptAsInput, "#A1");
			mtb.AllowPromptAsInput = true;
			Assert.AreEqual (true, mtb.AllowPromptAsInput, "#A2");
			mtb.AllowPromptAsInput = false;
			Assert.AreEqual (false, mtb.AllowPromptAsInput, "#A3");
			mtb.Dispose ();
		}
		
		[Test]
		public void AcceptsTabTest ()
		{
			MaskedTextBox mtb = new MaskedTextBox ();
			Assert.AreEqual (false, mtb.AcceptsTab, "#A1");
			mtb.AcceptsTab = true;
			Assert.AreEqual (false, mtb.AcceptsTab, "#A2");
			mtb.AcceptsTab = false;
			Assert.AreEqual (false, mtb.AcceptsTab, "#A3");
			mtb.Dispose ();
		}
		
		[Test]
		public void ConstructorTest ()
		{
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				Assert.AreEqual (false, mtb.AcceptsTab, "#A_A1");
				Assert.AreEqual (true, mtb.AllowPromptAsInput, "#A_A2");
				Assert.AreEqual (false, mtb.AsciiOnly, "#A_A3");
				Assert.AreEqual (false, mtb.BeepOnError, "#A_B1");
				Assert.AreEqual (false, mtb.CanUndo, "#A_C1");
				Assert.IsNotNull (mtb.Culture, "#A_C3");
				Assert.AreEqual (Thread.CurrentThread.CurrentCulture.Name, mtb.Culture.Name, "#A_4");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#A_C5");
				Assert.IsNull (mtb.FormatProvider, "#A_F1");
				Assert.AreEqual (false, mtb.HidePromptOnLeave, "#A_H1");
				Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#A_I1");
				Assert.AreEqual (false, mtb.IsOverwriteMode, "#A_I2");
				Assert.AreEqual (0, mtb.Lines.Length, "#A_L1");
				Assert.AreEqual ("", mtb.Mask, "#A_M1");
				Assert.AreEqual (true, mtb.MaskCompleted, "#A_M2");
				Assert.IsNull (mtb.MaskedTextProvider, "#A_M3");
				Assert.AreEqual (true, mtb.MaskFull, "#A_M4");
				Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#A_M5");
				Assert.AreEqual (false, mtb.Multiline, "#A_M6");
				Assert.AreEqual ('\0', mtb.PasswordChar, "#A_P1");
				Assert.AreEqual ('_', mtb.PromptChar, "#A_P2");
				Assert.AreEqual (false, mtb.ReadOnly, "#A_R1");
				Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#A_R2");
				Assert.AreEqual (true, mtb.ResetOnPrompt, "#A_R3");
				Assert.AreEqual (true, mtb.ResetOnSpace, "#A_R4");
				Assert.AreEqual ("", mtb.SelectedText, "#A_S1");
				Assert.AreEqual (true, mtb.SkipLiterals, "#A_S2");
				Assert.AreEqual ("", mtb.Text, "#A_T1");
				Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#A_T2");
				Assert.AreEqual (0, mtb.TextLength, "#A_T3");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#A_T4");
				Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#A_U1");
				Assert.IsNull (mtb.ValidatingType, "#A_V1");
				Assert.AreEqual (false, mtb.WordWrap, "#A_W1");
			}

			using (MaskedTextBox mtb = new MaskedTextBox ("abc")) {
				Assert.AreEqual (false, mtb.AcceptsTab, "#B_A1");
				Assert.AreEqual (true, mtb.AllowPromptAsInput, "#B_A2");
				Assert.AreEqual (false, mtb.AsciiOnly, "#B_A3");
				Assert.AreEqual (false, mtb.BeepOnError, "#B_B1");
				Assert.AreEqual (false, mtb.CanUndo, "#B_C1");
				Assert.IsNotNull (mtb.Culture, "#B_C3");
				Assert.AreEqual (Thread.CurrentThread.CurrentCulture.Name, mtb.Culture.Name, "#B_4");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#B_C5");
				Assert.IsNull (mtb.FormatProvider, "#B_F1");
				Assert.AreEqual (false, mtb.HidePromptOnLeave, "#B_H1");
				Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#B_I1");
				Assert.AreEqual (false, mtb.IsOverwriteMode, "#B_I2");
				Assert.AreEqual (1, mtb.Lines.Length, "#B_L1");
				Assert.AreEqual ("abc", mtb.Mask, "#B_M1");
				Assert.AreEqual (true, mtb.MaskCompleted, "#B_M2");
				Assert.IsNotNull (mtb.MaskedTextProvider, "#B_M3");
				Assert.AreEqual (false, mtb.MaskFull, "#B_M4");
				Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#B_M5");
				Assert.AreEqual (false, mtb.Multiline, "#B_M6");
				Assert.AreEqual ('\0', mtb.PasswordChar, "#B_P1");
				Assert.AreEqual ('_', mtb.PromptChar, "#B_P2");
				Assert.AreEqual (false, mtb.ReadOnly, "#B_R1");
				Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#B_R2");
				Assert.AreEqual (true, mtb.ResetOnPrompt, "#B_R3");
				Assert.AreEqual (true, mtb.ResetOnSpace, "#B_R4");
				Assert.AreEqual ("", mtb.SelectedText, "#B_S1");
				Assert.AreEqual (true, mtb.SkipLiterals, "#B_S2");
				Assert.AreEqual (" bc", mtb.Text, "#B_T1");
				Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#B_T2");
				Assert.AreEqual (3, mtb.TextLength, "#B_T3");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#B_T4");
				Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#B_U1");
				Assert.IsNull (mtb.ValidatingType, "#B_V1");
				Assert.AreEqual (false, mtb.WordWrap, "#B_W1");
			}

			using (MaskedTextBox mtb = new MaskedTextBox ("<>")) {
				Assert.AreEqual (false, mtb.AcceptsTab, "#C_A1");
				Assert.AreEqual (true, mtb.AllowPromptAsInput, "#C_A2");
				Assert.AreEqual (false, mtb.AsciiOnly, "#C_A3");
				Assert.AreEqual (false, mtb.BeepOnError, "#C_B1");
				Assert.AreEqual (false, mtb.CanUndo, "#C_C1");
				Assert.IsNotNull (mtb.Culture, "#C_C3");
				Assert.AreEqual (Thread.CurrentThread.CurrentCulture.Name, mtb.Culture.Name, "#C_4");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#C_C5");
				Assert.IsNull (mtb.FormatProvider, "#C_F1");
				Assert.AreEqual (false, mtb.HidePromptOnLeave, "#C_H1");
				Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#C_I1");
				Assert.AreEqual (false, mtb.IsOverwriteMode, "#C_I2");
				Assert.AreEqual (0, mtb.Lines.Length, "#C_L1");
				Assert.AreEqual ("<>", mtb.Mask, "#C_M1");
				Assert.AreEqual (true, mtb.MaskCompleted, "#C_M2");
				Assert.IsNotNull (mtb.MaskedTextProvider, "#C_M3");
				Assert.AreEqual (true, mtb.MaskFull, "#C_M4");
				Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#C_M5");
				Assert.AreEqual (false, mtb.Multiline, "#C_M6");
				Assert.AreEqual ('\0', mtb.PasswordChar, "#C_P1");
				Assert.AreEqual ('_', mtb.PromptChar, "#C_P2");
				Assert.AreEqual (false, mtb.ReadOnly, "#C_R1");
				Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#C_R2");
				Assert.AreEqual (true, mtb.ResetOnPrompt, "#C_R3");
				Assert.AreEqual (true, mtb.ResetOnSpace, "#C_R4");
				Assert.AreEqual ("", mtb.SelectedText, "#C_S1");
				Assert.AreEqual (true, mtb.SkipLiterals, "#C_S2");
				Assert.AreEqual ("", mtb.Text, "#C_T1");
				Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#C_T2");
				Assert.AreEqual (0, mtb.TextLength, "#C_T3");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#C_T4");
				Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#C_U1");
				Assert.IsNull (mtb.ValidatingType, "#C_V1");
				Assert.AreEqual (false, mtb.WordWrap, "#C_W1");
			}

			using (MaskedTextBox mtb = new MaskedTextBox ("abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMOPQRSTUVWXYZ1234567890ÑñæøåÆØÅ!\\ºª\"·$%&/()='?¡¿`^+*´¨Çç-_.:,;}{][")) {
				Assert.AreEqual (false, mtb.AcceptsTab, "#D_A1");
				Assert.AreEqual (true, mtb.AllowPromptAsInput, "#D_A2");
				Assert.AreEqual (false, mtb.AsciiOnly, "#D_A3");
				Assert.AreEqual (false, mtb.BeepOnError, "#D_B1");
				Assert.AreEqual (false, mtb.CanUndo, "#D_C1");
				Assert.IsNotNull (mtb.Culture, "#D_C3");
				Assert.AreEqual (Thread.CurrentThread.CurrentCulture.Name, mtb.Culture.Name, "#D_4");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#D_C5");
				Assert.IsNull (mtb.FormatProvider, "#D_F1");
				Assert.AreEqual (false, mtb.HidePromptOnLeave, "#D_H1");
				Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#D_I1");
				Assert.AreEqual (false, mtb.IsOverwriteMode, "#D_I2");
				Assert.AreEqual (1, mtb.Lines.Length, "#D_L1");
				Assert.AreEqual ("abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMOPQRSTUVWXYZ1234567890ÑñæøåÆØÅ!\\ºª\"·$%&/()='?¡¿`^+*´¨Çç-_.:,;}{][", mtb.Mask, "#D_M1");
				Assert.AreEqual (false, mtb.MaskCompleted, "#D_M2");
				Assert.IsNotNull (mtb.MaskedTextProvider, "#D_M3");
				Assert.AreEqual (false, mtb.MaskFull, "#D_M4");
				Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#D_M5");
				Assert.AreEqual (false, mtb.Multiline, "#D_M6");
				Assert.AreEqual ('\0', mtb.PasswordChar, "#D_P1");
				Assert.AreEqual ('_', mtb.PromptChar, "#D_P2");
				Assert.AreEqual (false, mtb.ReadOnly, "#D_R1");
				Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#D_R2");
				Assert.AreEqual (true, mtb.ResetOnPrompt, "#D_R3");
				Assert.AreEqual (true, mtb.ResetOnSpace, "#D_R4");
				Assert.AreEqual ("", mtb.SelectedText, "#D_S1");
				Assert.AreEqual (true, mtb.SkipLiterals, "#D_S2");
				Assert.AreEqual (" bcdefghijklmopqrstuvwxyz B DEFGHIJK MOPQRSTUVWXYZ12345678  ÑñæøåÆØÅ!ºª\"·$% /()=' ¡¿`^+*´¨Çç-_.:,;}{][", mtb.Text, "#D_T1");
				Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#D_T2");
				Assert.AreEqual (102, mtb.TextLength, "#D_T3");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#D_T4");
				Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#D_U1");
				Assert.IsNull (mtb.ValidatingType, "#D_V1");
				Assert.AreEqual (false, mtb.WordWrap, "#D_W1");
			}

			MaskedTextProvider mtp = new MaskedTextProvider ("abcd", CultureInfo.GetCultureInfo ("es-AR"), false, '>', '^', false);
			using (MaskedTextBox mtb = new MaskedTextBox (mtp)) {
				Assert.AreEqual (false, mtb.AcceptsTab, "#E_A1");
				Assert.AreEqual (false, mtb.AllowPromptAsInput, "#E_A2");
				Assert.AreEqual (false, mtb.AsciiOnly, "#E_A3");
				Assert.AreEqual (false, mtb.BeepOnError, "#E_B1");
				Assert.AreEqual (false, mtb.CanUndo, "#E_C1");
				Assert.IsNotNull (mtb.Culture, "#E_C3");
				Assert.AreEqual ("es-AR", mtb.Culture.Name, "#E_4");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.CutCopyMaskFormat, "#E_C5");
				Assert.IsNull (mtb.FormatProvider, "#E_F1");
				Assert.AreEqual (false, mtb.HidePromptOnLeave, "#E_H1");
				Assert.AreEqual (InsertKeyMode.Default, mtb.InsertKeyMode, "#E_I1");
				Assert.AreEqual (false, mtb.IsOverwriteMode, "#E_I2");
				Assert.AreEqual (1, mtb.Lines.Length, "#E_L1");
				Assert.AreEqual ("abcd", mtb.Mask, "#E_M1");
				Assert.AreEqual (true, mtb.MaskCompleted, "#E_M2");
				Assert.IsNotNull (mtb.MaskedTextProvider, "#E_M3");
				Assert.IsFalse (mtb.MaskedTextProvider == mtp, "#E_M3-b");
				Assert.AreEqual (false, mtb.MaskFull, "#E_M4");
				Assert.AreEqual (Int16.MaxValue, mtb.MaxLength, "#E_M5");
				Assert.AreEqual (false, mtb.Multiline, "#E_M6");
				Assert.AreEqual ('^', mtb.PasswordChar, "#E_P1");
				Assert.AreEqual ('>', mtb.PromptChar, "#E_P2");
				Assert.AreEqual (false, mtb.ReadOnly, "#E_R1");
				Assert.AreEqual (false, mtb.RejectInputOnFirstFailure, "#E_R2");
				Assert.AreEqual (true, mtb.ResetOnPrompt, "#E_R3");
				Assert.AreEqual (true, mtb.ResetOnSpace, "#E_R4");
				Assert.AreEqual ("", mtb.SelectedText, "#E_S1");
				Assert.AreEqual (true, mtb.SkipLiterals, "#E_S2");
				Assert.AreEqual (" bcd", mtb.Text, "#E_T1");
				Assert.AreEqual (HorizontalAlignment.Left, mtb.TextAlign, "#E_T2");
				Assert.AreEqual (4, mtb.TextLength, "#E_T3");
				Assert.AreEqual (MaskFormat.IncludeLiterals, mtb.TextMaskFormat, "#E_T4");
				Assert.AreEqual (false, mtb.UseSystemPasswordChar, "#E_U1");
				Assert.IsNull (mtb.ValidatingType, "#E_V1");
				Assert.AreEqual (false, mtb.WordWrap, "#E_W1");
			}
			
		}
		
		[Test]
		public void UndoTest ()
		{
			MaskedTextBox mtb;
			
			mtb = new MaskedTextBox ();
			mtb.Text = "abcdef";
			Assert.AreEqual (false, mtb.CanUndo, "#A0-c");
			mtb.Undo ();
			Assert.AreEqual ("abcdef", mtb.Text, "#A1");
			Assert.AreEqual (false, mtb.CanUndo, "#A1-c");
			mtb.Text = "cdef";
			mtb.ClearUndo ();
			Assert.AreEqual ("cdef", mtb.Text, "#A2");
			Assert.AreEqual (false, mtb.CanUndo, "#A2-c");
			
			mtb.Dispose ();
		}
		
		[Test]
		public void CreateHandleTest ()
		{
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				Assert.AreEqual (false, mtb.IsHandleCreated, "#A1");
				typeof (MaskedTextBox).GetMethod ("CreateHandle", global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic).Invoke (mtb, new object [] { });
				Assert.AreEqual (true, mtb.IsHandleCreated, "#A2");
			}
		}
		
		[Test]
		public void GetFirstCharIndexFromLineTest ()
		{
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				for (int i = -100; i < 100; i++) { 
					Assert.AreEqual (0, mtb.GetFirstCharIndexFromLine (i), "#A" + i.ToString ());
				}
				mtb.Text = "alñsdh gaph" + Environment.NewLine + "asldg";
				for (int i = -100; i < 100; i++) {
					Assert.AreEqual (0, mtb.GetFirstCharIndexFromLine (i), "#B" + i.ToString ());
				}
			}
		}


		[Test]
		public void GetFirstCharIndexOfCurrentLineTest ()
		{
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				Assert.AreEqual (0, mtb.GetFirstCharIndexOfCurrentLine (), "#A1");
				mtb.Text = "alñsdh gaph" + Environment.NewLine + "asldg";
				Assert.AreEqual (0, mtb.GetFirstCharIndexOfCurrentLine (), "#B1");
			}
		}

		[Test]
		public void GetLineFromCharIndexTest ()
		{
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				for (int i = -100; i < 100; i++) {
					Assert.AreEqual (0, mtb.GetLineFromCharIndex (i), "#A" + i.ToString ());
				}
				mtb.Text = "alñsdh gaph" + Environment.NewLine + "asldg";
				for (int i = -100; i < 100; i++) {
					Assert.AreEqual (0, mtb.GetLineFromCharIndex (i), "#B" + i.ToString ());
				}
			}
		}
		
		[Test]
		public void IsInputKeyTest ()
		{
			using (Form f = new Form ()) {
			using (MaskedTextBox mtb = new MaskedTextBox ()) {
				f.Controls.Add (mtb);
				f.Show ();
				MethodInfo IsInputKey = typeof (MaskedTextBox).GetMethod ("IsInputKey", global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Instance);
				
				for (int i = 0; i <= 0xFF; i++) {
					Keys key = (Keys) i;
					Keys key_ALT = key | Keys.Alt;
					Keys key_SHIFT = key | Keys.Shift;
					Keys key_CTRL = key | Keys.Control;
					Keys key_ALT_SHIFT = key | Keys.Alt | Keys.Shift;
					Keys key_ALT_CTRL = key | Keys.Alt | Keys.Control;
					Keys key_SHIFT_CTLR = key | Keys.Shift | Keys.Control;
					Keys key_ALT_SHIFT_CTLR = key | Keys.Alt | Keys.Shift | Keys.Control;

					bool is_input = false;
					
					switch (key) {
					case Keys.PageDown:
					case Keys.PageUp:
					case Keys.End:
					case Keys.Home:
					case Keys.Left:
					case Keys.Right:
					case Keys.Up:
					case Keys.Down:
					case Keys.Back:
						is_input = true;
						break;
					}

					Assert.AreEqual (is_input, (bool)IsInputKey.Invoke (mtb, new object [] { key }));
					Assert.AreEqual (false, (bool)IsInputKey.Invoke (mtb, new object [] { key_ALT }));
					Assert.AreEqual (is_input, (bool)IsInputKey.Invoke (mtb, new object [] { key_SHIFT }));
					Assert.AreEqual (is_input, (bool)IsInputKey.Invoke (mtb, new object [] { key_CTRL }));
					Assert.AreEqual (false, (bool)IsInputKey.Invoke (mtb, new object [] { key_ALT_SHIFT }));
					Assert.AreEqual (false, (bool)IsInputKey.Invoke (mtb, new object [] { key_ALT_CTRL }));
					Assert.AreEqual (is_input, (bool)IsInputKey.Invoke (mtb, new object [] { key_SHIFT_CTLR }));
					Assert.AreEqual (false, (bool)IsInputKey.Invoke (mtb, new object [] { key_ALT_SHIFT_CTLR }));
				}
			}
			}
		}
		
		[Test]
		public void ValidateTextTest ()
		{
			Assert.Ignore ("Pending implementation");
		}
		
		[Test]
		public void ToStringTest ()
		{
			Assert.Ignore ("Pending implementation");
		}
	}
}

