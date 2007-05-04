//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//      Rolf Bjarne Kvinge  (RKvinge@novell.com)
//

#if NET_2_0

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

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	[Ignore ("Pending MTB implementation")]
	public class MaskedTextBoxTest
	{
		[SetUp]
		public void Setup ()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("en-US");
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
	}
}

#endif