//
// System.ComponentModel.MaskedTextProvider test cases
//
// Authors:
// 	Rolf Bjarne Kvinge (RKvinge@novell.com)
//
// (c) 2007 Novell, Inc.
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class MaskedTextProviderTest
	{	
		
		private CultureInfo current_culture;
		
		[SetUp ()]
		public void SetUp ()
		{
			current_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("es-ES");
		}
		
		[TearDown ()]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = current_culture;
		}
		
		[Test]
		public void PasswordTest ()
		{

			MaskedTextProvider mtp = new MaskedTextProvider ("abcd", CultureInfo.GetCultureInfo ("es-AR"), false, '>', '^', false); 
			Assert.AreEqual (" bcd", mtp.ToString (), "#A1");
			
		}
		[Test]
		public void DefaultCultureTest ()
		{
			CultureInfo currentUI = Thread.CurrentThread.CurrentUICulture;
			CultureInfo current = Thread.CurrentThread.CurrentCulture;

			try {
				Thread.CurrentThread.CurrentUICulture = new CultureInfo ("en-US");
				Thread.CurrentThread.CurrentCulture = new CultureInfo ("es-ES");
				MaskedTextProvider mtp = new MaskedTextProvider ("mask");
				Assert.AreEqual ("es-ES", mtp.Culture.Name, "#01");
			} finally {
				Thread.CurrentThread.CurrentCulture = current;
				Thread.CurrentThread.CurrentUICulture = currentUI;
			}
		}

		[Test]
		public void GetOperationResultFromHintTest ()
		{
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.AlphanumericCharacterExpected), "#01");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.AsciiCharacterExpected), "#02");
			Assert.AreEqual (true, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.CharacterEscaped), "#03");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.DigitExpected), "#04");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.InvalidInput), "#05");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.LetterExpected), "#06");
			Assert.AreEqual (true, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.NoEffect), "#07");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.NonEditPosition), "#08");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.PositionOutOfRange), "#09");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.PromptCharNotAllowed), "#10");
			Assert.AreEqual (true, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.SideEffect), "#11");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.SignedDigitExpected), "#12");
			Assert.AreEqual (true, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.Success), "#13");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.UnavailableEditPosition), "#14");
			Assert.AreEqual (false, MaskedTextProvider.GetOperationResultFromHint (MaskedTextResultHint.Unknown), "#15");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Set_string_TestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			mtp.Set (null);
		}

		[Test]
		public void StaticPropertiesTest ()
		{
			Assert.AreEqual ('*', MaskedTextProvider.DefaultPasswordChar, "#D1");
			Assert.AreEqual (-1, MaskedTextProvider.InvalidIndex, "#I1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddStringExceptionTest1 ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			mtp.Add (null);
		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddStringExceptionTest2 ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			int tp;
			MaskedTextResultHint rh;
			mtp.Add (null, out tp, out rh);
		}

		[Test]
		public void CloneTest ()
		{
			MaskedTextProvider mtp;
			MaskedTextProvider mtp2;
			int counter = 0;

			mtp = new MaskedTextProvider ("mask");
			mtp2 = (MaskedTextProvider)mtp.Clone ();
			Assert.AreEqual (mtp.Mask, mtp2.Mask, "#" + (counter++).ToString ());
			AssertProperties (mtp2, "CloneTest", counter++, mtp.AllowPromptAsInput, mtp.AsciiOnly, mtp.AssignedEditPositionCount, mtp.AvailableEditPositionCount, mtp.Culture, mtp.EditPositionCount, mtp.IncludeLiterals, mtp.IncludePrompt, mtp.IsPassword, mtp.LastAssignedPosition, mtp.Length, mtp.Mask, mtp.MaskCompleted, mtp.MaskFull, mtp.PasswordChar, mtp.PromptChar, mtp.ResetOnPrompt, mtp.ResetOnSpace, mtp.SkipLiterals, mtp.ToString (), mtp.ToString (true), mtp.ToString (false), mtp.ToString (true, true), mtp.ToString (true, false), mtp.ToString (false, true), mtp.ToString (false, false));
		}

		[Test]
		public void EditPositionsTest ()
		{
			MaskedTextProvider mtp;
			string sep = ";";

			mtp = new MaskedTextProvider ("mask");

			Assert.AreEqual ("1", join (mtp.EditPositions, sep), "#01");
		}
		[Test]
		public void InsertAt_charTest ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;

			// insert space over space
			mtp = new MaskedTextProvider ("aaa");
			mtp.Add (" ");
			Assert.AreEqual (true, mtp.InsertAt (' ', 0, out Int32_out, out MaskedTextResultHint_out), "#A1");
			Assert.AreEqual ("", mtp.ToString (), "A2");
			Assert.AreEqual (0, Int32_out, "A3");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "A4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_A", 1, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @"", @"", @"", @"___", @"___", @"", @"");

			// insert space over normal char.
			mtp = new MaskedTextProvider ("aaa");
			mtp.Add ("a");
			Assert.AreEqual (true, mtp.InsertAt (' ', 0, out Int32_out, out MaskedTextResultHint_out), "#B1");
			Assert.AreEqual (" a", mtp.ToString (), "B2");
			Assert.AreEqual (0, Int32_out, "B3");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "B4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_B", 1, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @" a", @" a", @" a", @"_a_", @"_a_", @" a", @" a");

			// insert space over empty position.
			mtp = new MaskedTextProvider ("aaa");
			Assert.AreEqual (true, mtp.InsertAt (' ', 0, out Int32_out, out MaskedTextResultHint_out), "#C1");
			Assert.AreEqual ("", mtp.ToString (), "C2");
			Assert.AreEqual (0, Int32_out, "C3");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "C4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_C", 1, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @"", @"", @"", @"___", @"___", @"", @"");

			// insert space over empty position with other characters later in the string already inserted..
			mtp = new MaskedTextProvider ("aaa");
			mtp.InsertAt ('z', 2);
			Assert.AreEqual (true, mtp.InsertAt (' ', 0, out Int32_out, out MaskedTextResultHint_out), "#D1");
			Assert.AreEqual ("  z", mtp.ToString (), "D2");
			Assert.AreEqual (0, Int32_out, "D3");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "D4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_D", 1, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 2, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @"  z", @"  z", @"  z", @"__z", @"__z", @"  z", @"  z");

			// insert space over non-empty position with other characters later in the string already inserted..
			mtp = new MaskedTextProvider ("aaa");
			mtp.InsertAt ('z', 0);
			mtp.InsertAt ('z', 1);
			Assert.AreEqual (true, mtp.InsertAt (' ', 0, out Int32_out, out MaskedTextResultHint_out), "#E1");
			Assert.AreEqual (" zz", mtp.ToString (), "E2");
			Assert.AreEqual (0, Int32_out, "E3");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "E4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_E", 1, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 2, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @" zz", @" zz", @" zz", @"_zz", @"_zz", @" zz", @" zz");

			// Insert number over empty position with other characters later in the string.
			mtp = new MaskedTextProvider (@"aaa");
			mtp.InsertAt ('\x33', 2);
			Assert.AreEqual (true, mtp.InsertAt ('\x34', 0, out Int32_out, out MaskedTextResultHint_out), "#F1");
			Assert.AreEqual ("4 3", mtp.ToString (), "F2");
			Assert.AreEqual (0, Int32_out, "F3");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "F4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_F", 1, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 2, 3, @"aaa", true, false, '\x0', '\x5F', true, true, true, @"4 3", @"4 3", @"4 3", @"4_3", @"4_3", @"4 3", @"4 3");

			// insert space over literal with filled in positions later on (no more available edit positions)
			mtp = new MaskedTextProvider ("aba");
			mtp.InsertAt ('z', 0);
			mtp.InsertAt ('z', 1);
			mtp.InsertAt ('z', 2);
			Assert.AreEqual (false, mtp.InsertAt (' ', 1, out Int32_out, out MaskedTextResultHint_out), "#F1");
			Assert.AreEqual ("zbz", mtp.ToString (), "F2");
			Assert.AreEqual (3, Int32_out, "F3");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "F4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_F", 1, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"zbz", @"zbz", @"zbz", @"zbz", @"zz", @"zbz", @"zz");

			// insert space over literal with filled in positions later on ( more available edit positions)
			mtp = new MaskedTextProvider ("abaa");
			mtp.InsertAt ('z', 0);
			mtp.InsertAt ('z', 2);
			Assert.AreEqual (true, mtp.InsertAt (' ', 1, out Int32_out, out MaskedTextResultHint_out), "#G1");
			Assert.AreEqual ("zb z", mtp.ToString (), "G2");
			Assert.AreEqual (2, Int32_out, "G3");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "G4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_G", 1, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 4, @"abaa", true, false, '\x0', '\x5F', true, true, true, @"zb z", @"zb z", @"zb z", @"zb_z", @"z_z", @"zb z", @"z z");

			// insert space over literal with only more literals later on
			mtp = new MaskedTextProvider ("abb");
			Assert.AreEqual (false, mtp.InsertAt (' ', 1, out Int32_out, out MaskedTextResultHint_out), "#G1");
			Assert.AreEqual (" bb", mtp.ToString (), "G2");
			Assert.AreEqual (3, Int32_out, "G3");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "G4");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_charTest_G", 1, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abb", true, false, '\x0', '\x5F', true, true, true, @" bb", @" bb", @" bb", @"_bb", @"_", @" bb", @"");

		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException), ExpectedMessage = "-1")]
		public void ItemTestExceptionNegative1 ()
		{
			MaskedTextProvider mtp;
			object value;
			mtp = new MaskedTextProvider ("a");
			value = mtp [-1];
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException), ExpectedMessage = "4")]
		public void ItemTestExceptionLength ()
		{
			MaskedTextProvider mtp;
			object value;
			mtp = new MaskedTextProvider ("a><|b");
			value = mtp [mtp.Mask.Length - 1];
		}
		[Test]
		public void MaskCompletedTest ()
		{
			MaskedTextProvider mtp;
			string mask;

			mask = @"0";
			mtp = new MaskedTextProvider (mask);
			Assert.IsFalse (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"9";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"#";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"L";
			mtp = new MaskedTextProvider (mask);
			Assert.IsFalse (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsFalse (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"?";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"&";
			mtp = new MaskedTextProvider (mask);
			Assert.IsFalse (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"C";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"A";
			mtp = new MaskedTextProvider (mask);
			Assert.IsFalse (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"a";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @".";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @",";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @":";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"/";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"$";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"<";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @">";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"|";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);

			mask = @"\\";
			mtp = new MaskedTextProvider (mask);
			Assert.IsTrue (mtp.MaskCompleted, "#A" + mask);
			mtp.Add ("1");
			Assert.IsTrue (mtp.MaskCompleted, "#B" + mask);
			mtp.Add ("a");
			Assert.IsTrue (mtp.MaskCompleted, "#C" + mask);
		}
		[Test]
		public void RemoveAtTest ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aaaaaaaaaaaa");
			mtp.Add ("123456789");
			mtp.RemoveAt (3, 6, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (3, Int32_out, "#A1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#A2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_A", 1, true, false, 5, 7, CultureInfo.GetCultureInfo ("es-ES"), 12, true, false, false, 4, 12, @"aaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"12389", @"12389", @"12389", @"12389_______", @"12389_______", @"12389", @"12389");

			mtp = new MaskedTextProvider (@"La");
			mtp.Add ("z1");
			mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, Int32_out, "#B1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#B2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_B", 1, true, false, 0, 2, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, -1, 2, @"La", false, false, '\x0', '\x5F', true, true, true, @"", @"", @"", @"__", @"__", @"", @"");

			mtp = new MaskedTextProvider (@"La");
			mtp.Add ("z1");
			mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, Int32_out, "#B1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#B2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_B", 1, true, false, 0, 2, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, -1, 2, @"La", false, false, '\x0', '\x5F', true, true, true, @"", @"", @"", @"__", @"__", @"", @"");

			// Remove non-assigned character with assigned characters later on.
			mtp = new MaskedTextProvider (@"aaaaaaaaaaaa");
			mtp.InsertAt ("1", 1);
			mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, Int32_out, "#C1");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#C2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_C", 1, true, false, 1, 11, CultureInfo.GetCultureInfo ("es-ES"), 12, true, false, false, 0, 12, @"aaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"1", @"1", @"1", @"1___________", @"1___________", @"1", @"1");

			// Remove assigned+non-assigned character with assigned characters later on.
			mtp = new MaskedTextProvider (@"aaaaaaaaaaaa");
			mtp.InsertAt ("1", 1);
			mtp.InsertAt ("4", 4);
			mtp.RemoveAt (1, 2, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (1, Int32_out, "#D1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#D2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_D", 1, true, false, 1, 11, CultureInfo.GetCultureInfo ("es-ES"), 12, true, false, false, 2, 12, @"aaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"  4", @"  4", @"  4", @"__4_________", @"__4_________", @"  4", @"  4");

			// Remove non-assigned+assigned character with assigned characters later on.
			mtp = new MaskedTextProvider (@"aaaaaaaaaaaa");
			mtp.InsertAt ("1", 1);
			mtp.InsertAt ("4", 4);
			mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, Int32_out, "#E1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#E2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_E", 1, true, false, 1, 11, CultureInfo.GetCultureInfo ("es-ES"), 12, true, false, false, 2, 12, @"aaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"  4", @"  4", @"  4", @"__4_________", @"__4_________", @"  4", @"  4");

			// Remove 2 characters with a assigned character just after that cannot be removed 1 character.
			mtp = new MaskedTextProvider (@"aaaLaaaaaaaa");
			mtp.Add ("012Z4");
			mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, Int32_out, "#F1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#F2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAtTest_F", 1, true, false, 3, 9, CultureInfo.GetCultureInfo ("es-ES"), 12, true, false, false, 2, 12, @"aaaLaaaaaaaa", false, false, '\x0', '\x5F', true, true, true, @"2Z4", @"2Z4", @"2Z4", @"2Z4_________", @"2Z4_________", @"2Z4", @"2Z4");

		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Replace_string_int_int_int_MaskedTextResultHintTestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			int th;
			MaskedTextResultHint rh;
			mtp.Replace (null, 1, 2, out th, out rh);
		}
		[Test]
		public void ReplaceTest ()
		{
			MaskedTextProvider mtp;
			int Int32_out;
			MaskedTextResultHint resultHint;

			mtp = new MaskedTextProvider ("00aa");
			mtp.Add ("11zz");
			Assert.AreEqual (true, mtp.Replace ("2x", 1, 2, out Int32_out, out resultHint), "#A1");
			Assert.AreEqual (2, Int32_out, "#A2");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "#A3");
			MaskedTextProviderTest.AssertProperties (mtp, "ReplaceTest_A", 3, true, false, 4, 0, CultureInfo.GetCultureInfo ("es-ES"), 4, true, false, false, 3, 4, @"00aa", true, true, '\x0', '\x5F', true, true, true, @"12xz", @"12xz", @"12xz", @"12xz", @"12xz", @"12xz", @"12xz");

			mtp = new MaskedTextProvider ("aaaaaaaaaaaaaaa");
			mtp.Add ("abcdefghijk");
			Assert.AreEqual (true, mtp.Replace ("ZZ", 2, 6, out Int32_out, out resultHint), "#B1");
			Assert.AreEqual (3, Int32_out, "#B2");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "#B3");
			MaskedTextProviderTest.AssertProperties (mtp, "ReplaceTest_B", 3, true, false, 8, 7, CultureInfo.GetCultureInfo ("es-ES"), 15, true, false, false, 7, 15, @"aaaaaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"abZZhijk", @"abZZhijk", @"abZZhijk", @"abZZhijk_______", @"abZZhijk_______", @"abZZhijk", @"abZZhijk");

			mtp = new MaskedTextProvider ("aaaaaaaaaaaaaaa");
			mtp.Add ("abcdefghijk");
			Assert.AreEqual (true, mtp.Replace ('Z', 2, 6, out Int32_out, out resultHint), "#C1");
			Assert.AreEqual (2, Int32_out, "#C2");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "#C3");
			MaskedTextProviderTest.AssertProperties (mtp, "ReplaceTest_C", 3, true, false, 7, 8, CultureInfo.GetCultureInfo ("es-ES"), 15, true, false, false, 6, 15, @"aaaaaaaaaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @"abZhijk", @"abZhijk", @"abZhijk", @"abZhijk________", @"abZhijk________", @"abZhijk", @"abZhijk");

			// Replacing over a space.
			// This causes the replacement character to be INSERTED at the first edit position.
			// only for Replace (string, *), not for Replace (char, *).
			mtp = new MaskedTextProvider ("a aaa");
			mtp.Add ("123");
			Assert.AreEqual (true, mtp.Replace ("4", 1, 1, out Int32_out, out resultHint), "#D1");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "#D3");
			Assert.AreEqual (2, Int32_out, "#D2");
			MaskedTextProviderTest.AssertProperties (mtp, "ReplaceTest_D", 3, true, false, 4, 0, CultureInfo.GetCultureInfo ("es-ES"), 4, true, false, false, 4, 5, @"a aaa", true, true, '\x0', '\x5F', true, true, true, @"1 423", @"1 423", @"1 423", @"1 423", @"1423", @"1 423", @"1423");

		}
		
		
		[Test]
		public void Add_string_int_MaskedTextResultHint_Test00001 ()
		{
			MaskedTextProvider mtp;
			int testPosition;
			MaskedTextResultHint resultHint;
			bool result;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			result = mtp.Add (@"", out testPosition, out resultHint);
			Assert.AreEqual (true, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#0");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1");
			Assert.AreEqual (1, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#2");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");

		}
		[Test]
		public void Add_string_int_MaskedTextResultHint_Test00012 ()
		{
			MaskedTextProvider mtp;
			int testPosition;
			MaskedTextResultHint resultHint;
			bool result;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			result = mtp.Add (@"abc", out testPosition, out resultHint);
			Assert.AreEqual (true, result, "Add_string_int_MaskedTextResultHint_Test#0");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "Add_string_int_MaskedTextResultHint_Test#1");
			Assert.AreEqual (2, testPosition, "Add_string_int_MaskedTextResultHint_Test#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Add_string_int_MaskedTextResultHint_Test", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");

		}
		[Test]
		public void Add_string_int_MaskedTextResultHint_Test00029 ()
		{
			MaskedTextProvider mtp;
			int testPosition;
			bool result;
			MaskedTextResultHint resultHint;
			mtp = new MaskedTextProvider (@"a?b?c");
			result = mtp.Add (@"頽鏢⺸綉䤔퍽ࡡ㉌ꌉΩ㞜帤萸ẏ璜퐨ᄑ鍾ⰵ楯⾹뺤䵁ɳ⨵"/**/, out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1092");
			Assert.AreEqual (MaskedTextResultHint.LetterExpected, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1093");
			Assert.AreEqual (3, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1094");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1095, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"퍕པ녧抟闍Ķ鶣遌鄭爯탖奩竳", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1096");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1097");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1098");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1099, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"⹻概蹕︾֡⪺邅柅痹凱書⻍搩⃳訅䚡ꥇ쐰", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1100");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1101");
			Assert.AreEqual (0, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1102");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1103, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"鎵剂퓼懤峮蹃懹䀺㨔녂˅ľ唢뻫ﳑ", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1104");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1105");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1106");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1107, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"뎫殐饛죖Ⴍ⻕퉨㥺潙꾶訵뀡뛂޲髅ᝀ류鱙ꢳ䐥Ɂ葖᏿ꡖៜ떶Ⰸ拪쯐⊋铀䧏꧌ႄ署襫쑏㌏誅괚Ừ㎷秏똅觳奔ﬓ", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1108");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1109");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1110");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1111, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"㷡֓士吞뭕녪蕛ⁿ礞Ꙡ횏ꎈ贫卩䁥ٔ꘾ᑋ", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1112");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1113");
			Assert.AreEqual (1, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1114");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1115, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"跔楊ḭ霋傟騰跩묶ヵﱱ路᳸㯕弚భ瓾棫쭾맰횋筢Ꚍ♦疟莞", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1116");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1117");
			Assert.AreEqual (1, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1118");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1119, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"옍뀍ႉ㗰⑊츒隚⸮櫧뷨畫ᖘ", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1120");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1121");
			Assert.AreEqual (0, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1122");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1123, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"ន䪴៝䜣ࠞ⭬ⓨ⦋꽙㨉쁿柵ꨒ珊粱੊曵䨭㙤౮", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1124");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1125");
			Assert.AreEqual (3, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1126");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1127, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"뿀쨬鄇疒觔㪽悗ﰈ溸ﱭ旉㙙໒⫫", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1128");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1129");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1130");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1131, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"ഝ骠謤䁇ﯧ揊昝睠녽悜図⬽ꎸ㞶揄쭠諴䶱", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1132");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1133");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1134");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1135, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"銋ꆘ죳덑쭐쐒ȫ玹擷凅麊姗殄鯄劽╰௬쏘晡⫪褋ታ褿￺ꔕ䍺墴⸒튔䍲鿷ḏ圁䰝", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1136");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1137");
			Assert.AreEqual (3, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1138");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1139, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"ꃸḊ賶゗龸揱磎ቨ徸숒ﴛ뾹ߴ㹔뽳윛謱ঀ輤惬죹฻蟕歝퐘ꄤↅ뇎聺˕ὧꊼ뇅ݥ绶鍁용웸ွ⦇㷨ꟹ菁僀䤚", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1140");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1141");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1142");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1143, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"菾㚿쯏䊝槣䀌唑纺ꣁ㑚还힩귩䆗海ꁑ攸㌳㛮䁽㏙₦쐹弄輙㝥", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1144");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1145");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1146");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1147, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"時Д᪜諾ೣ鶆䦻㜅㣙熹뉔<鉎㜟㢓༨箝ﱐ궳ᗦ嵒䎁", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1148");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1149");
			Assert.AreEqual (3, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1150");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1151, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"ᘙޖ䥚ࢧ짿ᡸ꒭ᦾӚ쫅卜퓸썀쎇ⱞ셺蒙䁁䥘蹗贙삯翱믇軀뢎₣⼔甘꾑", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1152");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1153");
			Assert.AreEqual (5, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1154");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1155, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

			result = mtp.Add (@"㏯㎡⽯鍰帐椒䗓碐㉅淍믌ꚥѴῨ", out testPosition, out resultHint);
			Assert.AreEqual (false, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#1156");
			Assert.AreEqual (MaskedTextResultHint.AlphanumericCharacterExpected, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1157");
			Assert.AreEqual (0, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#1158");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 1159, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");

		}
		[Test]
		public void Add_string_int_MaskedTextResultHint_Test00896 ()
		{
			MaskedTextProvider mtp;
			int testPosition;
			MaskedTextResultHint resultHint;
			bool result;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			result = mtp.Add (@"b", out testPosition, out resultHint);
			Assert.AreEqual (true, result, "GenerateAdd_string_int_MaskedTextResultHint_Test#0");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, resultHint, "GenerateAdd_string_int_MaskedTextResultHint_Test#1");
			Assert.AreEqual (1, testPosition, "GenerateAdd_string_int_MaskedTextResultHint_Test#2");
			MaskedTextProviderTest.AssertProperties (mtp, "GenerateAdd_string_int_MaskedTextResultHint_Test", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");

		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InsertAt_string_int_TestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			mtp.InsertAt (null, 3);
		}

		[Test]
		public void InsertAt_string_int_Test ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("?z?z?z?z?");
			mtp.InsertAt ("a", 3);
			Assert.AreEqual ("_z_zaz_z_", mtp.ToString (true, true), "#01");
			mtp.InsertAt ("b", 4);
			Assert.AreEqual ("_z_zbzaz_", mtp.ToString (true, true), "#02");
		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InsertAt_string_int_int_MaskedTextResultHintTestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			int th;
			MaskedTextResultHint rh;
			mtp.InsertAt (null, 3, out th, out rh);
		}
		[Test]
		public void IsAvailablePositionTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.IsAvailablePosition (-1), "#0");
			Assert.AreEqual (false, mtp.IsAvailablePosition (0), "#1");
			Assert.AreEqual (false, mtp.IsAvailablePosition (1), "#2");
			Assert.AreEqual (false, mtp.IsAvailablePosition (2), "#3");
			Assert.AreEqual (false, mtp.IsAvailablePosition (3), "#4");
			Assert.AreEqual (false, mtp.IsAvailablePosition (4), "#5");
			MaskedTextProviderTest.AssertProperties (mtp, "IsAvailablePositionTest", 6, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		
		[Test]
		public void RemoveTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Remove (), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 1, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (), "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 3, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (), "#4");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 5, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (), "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 7, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (), "#8");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 9, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (), "#10");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveTest", 11, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Remove_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 3, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#4");
			Assert.AreEqual (0, Int32_out, "#5");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 7, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#8");
			Assert.AreEqual (0, Int32_out, "#9");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#10");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 11, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#12");
			Assert.AreEqual (0, Int32_out, "#13");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#14");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 15, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#16");
			Assert.AreEqual (0, Int32_out, "#17");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#18");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 19, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#20");
			Assert.AreEqual (0, Int32_out, "#21");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#22");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 23, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Remove_int_MaskedTextResultHintTest00004 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#72");
			Assert.AreEqual (3, Int32_out, "#73");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#74");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 75, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"aab c", @"aab c", @"aab c", @"aab_c", @"aa_", @"aab c", @"aa");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#76");
			Assert.AreEqual (1, Int32_out, "#77");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#78");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 79, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"a b c", @"a b c", @"a b c", @"a_b_c", @"a__", @"a b c", @"a");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#80");
			Assert.AreEqual (0, Int32_out, "#81");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#82");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 83, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#84");
			Assert.AreEqual (0, Int32_out, "#85");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#86");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 87, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#88");
			Assert.AreEqual (0, Int32_out, "#89");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#90");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 91, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#92");
			Assert.AreEqual (0, Int32_out, "#93");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#94");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 95, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#96");
			Assert.AreEqual (0, Int32_out, "#97");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#98");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 99, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
			Assert.AreEqual (true, mtp.Remove (out Int32_out, out MaskedTextResultHint_out), "#100");
			Assert.AreEqual (0, Int32_out, "#101");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#102");
			MaskedTextProviderTest.AssertProperties (mtp, "Remove_int_MaskedTextResultHintTest", 103, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 3, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00002 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out), "#4");
			Assert.AreEqual (0, Int32_out, "#5");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 7, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00010 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.RemoveAt (1, 0, out Int32_out, out MaskedTextResultHint_out), "#36");
			Assert.AreEqual (1, Int32_out, "#37");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#38");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 39, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00028 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out), "#108");
			Assert.AreEqual (0, Int32_out, "#109");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#110");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 111, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"aab c", @"aab c", @"aab c", @"aab_c", @"aa_", @"aab c", @"aa");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00029 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out), "#112");
			Assert.AreEqual (0, Int32_out, "#113");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#114");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 115, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"a b c", @"a b c", @"a b c", @"a_b_c", @"a__", @"a b c", @"a");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00030 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.RemoveAt (0, 0, out Int32_out, out MaskedTextResultHint_out), "#116");
			Assert.AreEqual (0, Int32_out, "#117");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#118");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 119, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00031 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out), "#120");
			Assert.AreEqual (0, Int32_out, "#121");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#122");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 123, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"a b c", @"a b c", @"a b c", @"a_b_c", @"a__", @"a b c", @"a");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00032 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.RemoveAt (0, 1, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 3, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00048 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.RemoveAt (1, 1, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (1, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 3, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"1 b c", @"1 b c", @"1 b c", @"1_b_c", @"1__", @"1 b c", @"1");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00068 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.RemoveAt (2, 3, out Int32_out, out MaskedTextResultHint_out), "#16");
			Assert.AreEqual (2, Int32_out, "#17");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#18");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 19, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @" ab c", @" ab c", @" ab c", @"_ab_c", @"_a_", @" ab c", @" a");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00111 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (false, mtp.RemoveAt (0, 2, out Int32_out, out MaskedTextResultHint_out), "#24");
			Assert.AreEqual (4, Int32_out, "#25");
			Assert.AreEqual (MaskedTextResultHint.LetterExpected, MaskedTextResultHint_out, "#26");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 27, true, false, 3, 6, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12_____3_,.:/€\", @"12_____3_", @"12     3 ,.:/€\", @"12     3");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00114 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (false, mtp.RemoveAt (0, 3, out Int32_out, out MaskedTextResultHint_out), "#28");
			Assert.AreEqual (3, Int32_out, "#29");
			Assert.AreEqual (MaskedTextResultHint.LetterExpected, MaskedTextResultHint_out, "#30");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 31, true, false, 3, 6, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12_____3_,.:/€\", @"12_____3_", @"12     3 ,.:/€\", @"12     3");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00148 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.RemoveAt (0, 15, out Int32_out, out MaskedTextResultHint_out), "#44");
			Assert.AreEqual (15, Int32_out, "#45");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#46");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 47, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest00958 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.RemoveAt (15, 0, out Int32_out, out MaskedTextResultHint_out), "#996");
			Assert.AreEqual (15, Int32_out, "#997");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#998");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 999, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void RemoveAt_int_int_int_MaskedTextResultHintTest01006 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.RemoveAt (15, 16, out Int32_out, out MaskedTextResultHint_out), "#1188");
			Assert.AreEqual (16, Int32_out, "#1189");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#1190");
			MaskedTextProviderTest.AssertProperties (mtp, "RemoveAt_int_int_int_MaskedTextResultHintTest", 1191, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		

		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace (@"", 0, 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 3, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00016 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"", 1, 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (1, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00040 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"a", 1, 1, out Int32_out, out MaskedTextResultHint_out), "#24");
			Assert.AreEqual (3, Int32_out, "#25");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#26");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 27, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00130 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"a", 1, 1, out Int32_out, out MaskedTextResultHint_out), "#160");
			Assert.AreEqual (3, Int32_out, "#161");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#162");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 163, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba", @"aa", @"aba", @"aa");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00137 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"a longer string value", 0, 2, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (3, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 3, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba", @"aa", @"aba", @"aa");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00176 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abaa");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace (@"a", 1, 3, out Int32_out, out MaskedTextResultHint_out), "#52");
			Assert.AreEqual (2, Int32_out, "#53");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#54");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 55, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 2, 4, @"abaa", true, false, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba_", @"aa_", @"aba", @"aa");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00618 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"", 15, 16, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (16, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 3, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest00636 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"", 16, 15, out Int32_out, out MaskedTextResultHint_out), "#120");
			Assert.AreEqual (15, Int32_out, "#121");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#122");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 123, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest01918 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"000-00-0000");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace (@"a", 3, 3, out Int32_out, out MaskedTextResultHint_out), "#216");
			Assert.AreEqual (4, Int32_out, "#217");
			Assert.AreEqual (MaskedTextResultHint.DigitExpected, MaskedTextResultHint_out, "#218");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 219, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 11, @"000-00-0000", false, false, '\x0', '\x5F', true, true, true, @"   -  -", @"   -  -", @"   -  -", @"___-__-____", @"_________", @"   -  -", @"");
		}
		[Test]
		public void Replace_string_int_int_int_MaskedTextResultHintTest01918bis ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"000-a0-0000");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace (@"a", 3, 3, out Int32_out, out MaskedTextResultHint_out), "#216");
			Assert.AreEqual (4, Int32_out, "#217");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#218");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_int_MaskedTextResultHintTest", 219, true, false, 1, 8, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 4, 11, @"000-a0-0000", false, false, '\x0', '\x5F', true, true, true, @"   -a -", @"   -a -", @"   -a -", @"___-a_-____", @"___a_____", @"   -a -", @"   a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 0, 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00007 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 2, out Int32_out, out MaskedTextResultHint_out), "#12");
			Assert.AreEqual (2, Int32_out, "#13");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#14");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 15, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00010 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 1, 0, out Int32_out, out MaskedTextResultHint_out), "#36");
			Assert.AreEqual (1, Int32_out, "#37");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#38");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 39, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00013 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 1, 1, out Int32_out, out MaskedTextResultHint_out), "#48");
			Assert.AreEqual (1, Int32_out, "#49");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, MaskedTextResultHint_out, "#50");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 51, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00016 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 1, 2, out Int32_out, out MaskedTextResultHint_out), "#60");
			Assert.AreEqual (3, Int32_out, "#61");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#62");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 63, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00055 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x61', 0, 0, out Int32_out, out MaskedTextResultHint_out), "#216");
			Assert.AreEqual (0, Int32_out, "#217");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#218");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 219, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00067 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x61', 1, 1, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (1, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.NonEditPosition, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00341 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 1, 2, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (2, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba", @"aa", @"aba", @"aa");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00416 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x61', 0, 2, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 1, 1, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 0, 3, @"aba", true, false, '\x0', '\x5F', true, true, true, @"ab", @"ab", @"ab", @"ab_", @"a_", @"ab", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest00417 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.Replace ('\x61', 0, 2, out Int32_out, out MaskedTextResultHint_out), "#4");
			Assert.AreEqual (0, Int32_out, "#5");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 7, true, false, 1, 1, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 0, 3, @"aba", true, false, '\x0', '\x5F', true, true, true, @"ab", @"ab", @"ab", @"ab_", @"a_", @"ab", @"a");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest02971 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 9, 10, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (15, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest03661 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 16, 15, out Int32_out, out MaskedTextResultHint_out), "#200");
			Assert.AreEqual (15, Int32_out, "#201");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#202");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 203, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest05988 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.Replace ('\x61', 3, 5, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (3, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 4, 5, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 5, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"12 a 3   ,.:/€\", @"12 a 3   ,.:/€\", @"12 a 3   ,.:/€\", @"12_a_3___,.:/€\", @"12_a_3___", @"12 a 3   ,.:/€\", @"12 a 3");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest08681 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x2F', 12, 12, out Int32_out, out MaskedTextResultHint_out), "#4");
			Assert.AreEqual (12, Int32_out, "#5");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 7, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest08686 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x2F', 12, 13, out Int32_out, out MaskedTextResultHint_out), "#24");
			Assert.AreEqual (12, Int32_out, "#25");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#26");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 27, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest60913 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"00 /00 /0000");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.Replace ('\x31', 2, 3, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (4, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 3, true, false, 5, 3, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, 9, 12, @"00 /00 /0000", false, false, '\x0', '\x5F', true, true, true, @"12 /14 / 3", @"12 /14 / 3", @"12 /14 / 3", @"12 /14 /_3__", @"1214_3__", @"12 /14 / 3", @"1214 3");
		}
		[Test]
		public void Replace_char_int_int_int_MaskedTextResultHintTest118783 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"(999)-000-0000");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.Replace ('\x20', 4, 5, out Int32_out, out MaskedTextResultHint_out), "#16");
			Assert.AreEqual (6, Int32_out, "#17");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#18");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_int_MaskedTextResultHintTest", 19, true, false, 4, 6, CultureInfo.GetCultureInfo ("es-ES"), 10, true, false, false, 8, 14, @"(999)-000-0000", false, false, '\x0', '\x5F', true, true, true, @"(12 )- 43-", @"(12 )- 43-", @"(12 )- 43-", @"(12_)-_43-____", @"12__43____", @"(12 )- 43-", @"12  43");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00004 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 1, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (1, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00037 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#96");
			Assert.AreEqual (0, Int32_out, "#97");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#98");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 99, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00038 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.Replace ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#100");
			Assert.AreEqual (0, Int32_out, "#101");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#102");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 103, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00052 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.Replace ('\x0', 2, out Int32_out, out MaskedTextResultHint_out), "#132");
			Assert.AreEqual (3, Int32_out, "#133");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, MaskedTextResultHint_out, "#134");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 135, true, false, 3, 0, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, true, '\x0', '\x5F', true, true, true, @"aabac", @"aabac", @"aabac", @"aabac", @"aaa", @"aabac", @"aaa");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00076 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x61', 0, out Int32_out, out MaskedTextResultHint_out), "#180");
			Assert.AreEqual (0, Int32_out, "#181");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#182");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 183, true, false, 3, 0, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, true, '\x0', '\x5F', true, true, true, @"aabac", @"aabac", @"aabac", @"aabac", @"aaa", @"aabac", @"aaa");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00077 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.Replace ('\x61', 0, out Int32_out, out MaskedTextResultHint_out), "#184");
			Assert.AreEqual (0, Int32_out, "#185");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#186");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 187, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"aab c", @"aab c", @"aab c", @"aab_c", @"aa_", @"aab c", @"aa");
		}
		[Test]
		public void Replace_char_int_int_MaskedTextResultHintTest00871 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace ('\x2F', 12, out Int32_out, out MaskedTextResultHint_out), "#84");
			Assert.AreEqual (12, Int32_out, "#85");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#86");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_char_int_int_MaskedTextResultHintTest", 87, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.InsertAt (@"", 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}

		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00004 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.InsertAt (@"", 1, out Int32_out, out MaskedTextResultHint_out), "#12");
			Assert.AreEqual (1, Int32_out, "#13");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#14");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 15, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00010 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"a", 0, out Int32_out, out MaskedTextResultHint_out), "#36");
			Assert.AreEqual (3, Int32_out, "#37");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#38");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 39, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00011 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.InsertAt (@"a", 0, out Int32_out, out MaskedTextResultHint_out), "#40");
			Assert.AreEqual (0, Int32_out, "#41");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#42");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 43, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00013 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"a", 1, out Int32_out, out MaskedTextResultHint_out), "#48");
			Assert.AreEqual (3, Int32_out, "#49");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#50");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 51, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00054 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (false, mtp.InsertAt (@"a", 0, out Int32_out, out MaskedTextResultHint_out), "#212");
			Assert.AreEqual (1, Int32_out, "#213");
			Assert.AreEqual (MaskedTextResultHint.LetterExpected, MaskedTextResultHint_out, "#214");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 215, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"1 b c", @"1 b c", @"1 b c", @"1_b_c", @"1__", @"1 b c", @"1");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00068 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (false, mtp.InsertAt (@"a longer string value", 0, out Int32_out, out MaskedTextResultHint_out), "#268");
			Assert.AreEqual (5, Int32_out, "#269");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#270");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 271, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @" ab c", @" ab c", @" ab c", @"_ab_c", @"_a_", @" ab c", @" a");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00069 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (false, mtp.InsertAt (@"a longer string value", 0, out Int32_out, out MaskedTextResultHint_out), "#272");
			Assert.AreEqual (5, Int32_out, "#273");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#274");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 275, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"1 b c", @"1 b c", @"1 b c", @"1_b_c", @"1__", @"1 b c", @"1");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00073 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"a longer string value", 2, out Int32_out, out MaskedTextResultHint_out), "#288");
			Assert.AreEqual (5, Int32_out, "#289");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#290");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 291, true, false, 3, 0, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, true, '\x0', '\x5F', true, true, true, @"aabac", @"aabac", @"aabac", @"aabac", @"aaa", @"aabac", @"aaa");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00142 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"", 15, out Int32_out, out MaskedTextResultHint_out), "#564");
			Assert.AreEqual (15, Int32_out, "#565");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#566");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 567, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest00145 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"", 16, out Int32_out, out MaskedTextResultHint_out), "#576");
			Assert.AreEqual (16, Int32_out, "#577");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#578");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 579, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest02650 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"00->L<LL-0000");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt (@"a longer string value", 5, out Int32_out, out MaskedTextResultHint_out), "#10596");
			Assert.AreEqual (8, Int32_out, "#10597");
			Assert.AreEqual (MaskedTextResultHint.DigitExpected, MaskedTextResultHint_out, "#10598");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 10599, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 11, @"00->L<LL-0000", false, false, '\x0', '\x5F', true, true, true, @"  -   -", @"  -   -", @"  -   -", @"__-___-____", @"_________", @"  -   -", @"");
		}
		[Test]
		public void InsertAt_string_int_int_MaskedTextResultHintTest ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			Assert.AreEqual (true, mtp.InsertAt (@" ", 0, out Int32_out, out MaskedTextResultHint_out), "#268");
			Assert.AreEqual (0, Int32_out, "#269");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#270");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_string_int_int_MaskedTextResultHintTest", 271, true, false, 0, 3, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, -1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  b c", @"  b c", @"  b c", @"__b_c", @"___", @"  b c", @"");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt ('\x0', 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.InvalidInput, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00037 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#144");
			Assert.AreEqual (3, Int32_out, "#145");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#146");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 147, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}

		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00038 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#148");
			Assert.AreEqual (0, Int32_out, "#149");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#150");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 151, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00040 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.InsertAt ('\x20', 1, out Int32_out, out MaskedTextResultHint_out), "#156");
			Assert.AreEqual (3, Int32_out, "#157");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#158");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 159, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00110 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.InsertAt ('\x20', 1, out Int32_out, out MaskedTextResultHint_out), "#436");
			Assert.AreEqual (1, Int32_out, "#437");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#438");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 439, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  bac", @"  bac", @"  bac", @"__bac", @"__a", @"  bac", @"  a");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00113 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.InsertAt ('\x20', 2, out Int32_out, out MaskedTextResultHint_out), "#448");
			Assert.AreEqual (3, Int32_out, "#449");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#450");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 451, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @" ab c", @" ab c", @" ab c", @"_ab_c", @"_a_", @" ab c", @" a");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00328 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.InsertAt ('\x2F', 12, out Int32_out, out MaskedTextResultHint_out), "#1308");
			Assert.AreEqual (12, Int32_out, "#1309");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#1310");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 1311, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest00330 ()
		{
			char chr = 'z';
			string mask = "aa" + chr.ToString () + "";
			//mask = @"09#L?&CAa.,:/$<>|\\";
			int idx = mask.IndexOf (chr);
			{	// Original
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (mask);
				mtp.Add (@"1");
				mtp.Add (@"2");
				mtp.InsertAt ('\x33', 7);
				mtp.InsertAt ('\x34', 4);
				Assert.AreEqual (false, mtp.InsertAt (chr, idx, out Int32_out, out MaskedTextResultHint_out), "#B1316");
				//Assert.AreEqual (15, Int32_out, "#B1317");
				Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#B1318");
				//MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTestB", 1, true, false, 3, 6, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12_____3_,.:/€\", @"12_____3_", @"12     3 ,.:/€\", @"12     3");

			}
			{
				// Minimal
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (mask);
				mtp.Add (@"1");
				Assert.AreEqual (false, mtp.InsertAt (chr, idx, out Int32_out, out MaskedTextResultHint_out), "#C1316");
				//Assert.AreEqual (15, Int32_out, "#C1317");
				Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#C1318");
				//MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTestC", 2, true, false, 1, 8, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 0, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"1        ,.:/€\", @"1        ,.:/€\", @"1        ,.:/€\", @"1________,.:/€\", @"1________", @"1        ,.:/€\", @"1");

			}
			{
				// No what are the actual difference between this and the minimal?
				// The added character for the minimal is BEFORE the character we're inserting here.
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (mask);
				Assert.AreEqual (true, mtp.InsertAt (chr, idx, out Int32_out, out MaskedTextResultHint_out), "#A1316");
				//Assert.AreEqual (15, Int32_out, "#A1317");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#A1318");
				//MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTestA", 3, true, false, 3, 6, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12     3 ,.:/€\", @"12_____3_,.:/€\", @"12_____3_", @"12     3 ,.:/€\", @"12     3");
			}
			
		}
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest04290 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"0000 00000");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			Assert.AreEqual (true, mtp.InsertAt ('\x20', 4, out Int32_out, out MaskedTextResultHint_out), "#17156");
			Assert.AreEqual (4, Int32_out, "#17157");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#17158");
			MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 17159, true, false, 4, 5, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 10, @"0000 00000", false, false, '\x0', '\x5F', true, true, true, @"12   4 3", @"12   4 3", @"12   4 3", @"12__ 4_3__", @"12__4_3__", @"12   4 3", @"12  4 3");
		}
		
		[Test]
		public void InsertAt_char_int_int_MaskedTextResultHintTest ()
		{

			{
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (@"aaaaaaaa");
				mtp.Add ("a");
				mtp.InsertAt ("z", 3);
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#01");
				Assert.AreEqual (0, Int32_out, "#02");
				Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#03");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 1511, true, false, 2, 6, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, 4, 8, @"aaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @" a  z", @" a  z", @" a  z", @"_a__z___", @"_a__z___", @" a  z", @" a  z");
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#04");
				Assert.AreEqual (0, Int32_out, "#05");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#1500");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 1510, true, false, 2, 6, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, 4, 8, @"aaaaaaaa", true, false, '\x0', '\x5F', true, true, true, @" a  z", @" a  z", @" a  z", @"_a__z___", @"_a__z___", @" a  z", @" a  z");
			}
			{
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (@"00000000");
				mtp.Add ("1");
				mtp.InsertAt ("9", 3);
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#01");
				Assert.AreEqual (0, Int32_out, "#02");
				Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#03");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 1511, true, false, 2, 6, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, 4, 8, @"00000000", false, false, '\x0', '\x5F', true, true, true, @" 1  9", @" 1  9", @" 1  9", @"_1__9___", @"_1__9___", @" 1  9", @" 1  9");
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#04");
				Assert.AreEqual (0, Int32_out, "#05");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#1500");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 1510, true, false, 2, 6, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, 4, 8, @"00000000", false, false, '\x0', '\x5F', true, true, true, @" 1  9", @" 1  9", @" 1  9", @"_1__9___", @"_1__9___", @" 1  9", @" 1  9");
			}
			{
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (@"a ?bc");
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 0, out Int32_out, out MaskedTextResultHint_out), "#1");
				Assert.AreEqual (0, Int32_out, "#2");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#3");
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 1, out Int32_out, out MaskedTextResultHint_out), "#4");
				Assert.AreEqual (1, Int32_out, "#5");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#6");
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 2, out Int32_out, out MaskedTextResultHint_out), "#7");
				Assert.AreEqual (2, Int32_out, "#8");
				Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#150");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 151, true, false, 0, 2, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, -1, 5, @"a ?bc", true, false, '\x0', '\x5F', true, true, true, @"   bc", @"   bc", @"   bc", @"_ _bc", @"__", @"   bc", @"");
			}
			{
				MaskedTextProvider mtp;
				int Int32_out = 0;
				MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
				mtp = new MaskedTextProvider (@"a?b?c");
				//mtp.Add (@"a");
				//mtp.Remove ();
				mtp.InsertAt ('\x61', 1);
				Assert.AreEqual (true, mtp.InsertAt ('\x20', 1, out Int32_out, out MaskedTextResultHint_out), "#436");
				Assert.AreEqual (1, Int32_out, "#437");
				Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#438");
				MaskedTextProviderTest.AssertProperties (mtp, "InsertAt_char_int_int_MaskedTextResultHintTest", 439, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"  bac", @"  bac", @"  bac", @"__bac", @"__a", @"  bac", @"  a");
			}

		}
		[Test]
		public void IsPasswordTest () 
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			Assert.IsFalse (mtp.IsPassword, "#01");
			Assert.AreEqual (char.MinValue, mtp.PasswordChar, "#02");
			mtp.IsPassword = true;
			Assert.IsTrue (mtp.IsPassword, "#03");
			Assert.AreEqual ('*', mtp.PasswordChar, "#04");
			mtp.IsPassword = false;
			Assert.IsFalse (mtp.IsPassword, "#05");
			Assert.AreEqual (char.MinValue, mtp.PasswordChar, "#06");
		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Replace_string_int_int_MaskedTextResultHintTestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			int th;
			MaskedTextResultHint rh;
			mtp.Replace (null, 1, out th, out rh);
		}
		[Test]
		public void Replace_string_int_int_MaskedTextResultHintTest00037 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace (@"", 0, out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_int_MaskedTextResultHintTest", 3, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 1, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"aab c", @"aab c", @"aab c", @"aab_c", @"aa_", @"aab c", @"aa");
		}
		[Test]
		public void Replace_string_int_Test00076 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Replace (@"a", 0), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_Test", 1, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba", @"aa", @"aba", @"aa");
		}
		[Test]
		public void Replace_string_int_Test00077 ()
		{
			MaskedTextProvider mtp;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"aba");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.Replace (@"a", 0), "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Replace_string_int_Test", 3, true, false, 2, 0, CultureInfo.GetCultureInfo ("es-ES"), 2, true, false, false, 2, 3, @"aba", true, true, '\x0', '\x5F', true, true, true, @"aba", @"aba", @"aba", @"aba", @"aa", @"aba", @"aa");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Replace_string_int_TestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			mtp.Replace (null, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Set_string_int_MaskedTextResultHintTestException ()
		{
			MaskedTextProvider mtp = new MaskedTextProvider ("a");
			int th;
			MaskedTextResultHint rh;
			mtp.Set (null, out th, out rh);
		}
		[Test]
		public void Set_string_int_MaskedTextResultHintTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Set (@"", out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (0, Int32_out, "#1");
			Assert.AreEqual (MaskedTextResultHint.Success, MaskedTextResultHint_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Set_string_int_MaskedTextResultHintTest", 3, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Set_string_int_MaskedTextResultHintTest00002 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (true, mtp.Set (@"", out Int32_out, out MaskedTextResultHint_out), "#4");
			Assert.AreEqual (0, Int32_out, "#5");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "Set_string_int_MaskedTextResultHintTest", 7, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		[Test]
		public void Set_string_int_MaskedTextResultHintTest00004 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Set (@"a", out Int32_out, out MaskedTextResultHint_out), "#8");
			Assert.AreEqual (0, Int32_out, "#9");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#10");
			MaskedTextProviderTest.AssertProperties (mtp, "Set_string_int_MaskedTextResultHintTest", 11, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void Set_string_int_MaskedTextResultHintTest00016 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.Set (@"a", out Int32_out, out MaskedTextResultHint_out), "#24");
			Assert.AreEqual (0, Int32_out, "#25");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#26");
			MaskedTextProviderTest.AssertProperties (mtp, "Set_string_int_MaskedTextResultHintTest", 27, true, false, 1, 2, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 0, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @"a b c", @"a b c", @"a b c", @"a_b_c", @"a__", @"a b c", @"a");
		}
		
		[Test]
		public void Add_char_int_MaskedTextResultHint_Test04657 ()
		{
			MaskedTextProvider mtp;
			int testPosition;
			MaskedTextResultHint resultHint;
			bool result;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			result = mtp.Add ('\x0062', out testPosition, out resultHint);/* b */
			Assert.AreEqual (true, result, "Add_char_int_MaskedTextResultHint_Test#0");
			Assert.AreEqual (MaskedTextResultHint.Success, resultHint, "Add_char_int_MaskedTextResultHint_Test#1");
			Assert.AreEqual (3, testPosition, "Add_char_int_MaskedTextResultHint_Test#2");
			MaskedTextProviderTest.AssertProperties (mtp, "Add_char_int_MaskedTextResultHint_Test", 3, true, false, 2, 1, CultureInfo.GetCultureInfo ("es-ES"), 3, true, false, false, 3, 5, @"a?b?c", true, false, '\x0', '\x5F', true, true, true, @" abbc", @" abbc", @" abbc", @"_abbc", @"_ab", @" abbc", @" ab");
		}
		[Test]
		public void Add_string_Test42871 ()
		{
			MaskedTextProvider mtp;
			//int testPosition;
			//MaskedTextResultHint resultHint;
			bool result;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"$999,999.00");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			result = mtp.Add (@"€");
			Assert.AreEqual (true, result, "Add_string_Test#0");
			MaskedTextProviderTest.AssertProperties (mtp, "Add_string_Test", 1, true, false, 0, 8, CultureInfo.GetCultureInfo ("es-ES"), 8, true, false, false, -1, 11, @"$999,999.00", false, false, '\x0', '\x5F', true, true, true, @"€   .   ,", @"€   .   ,", @"€   .   ,", @"€___.___,__", @"________", @"€   .   ,", @"");
		}
		
		[Test]
		public void FindAssignedEditPositionFromTest1 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (0, false), "#0");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (0, true), "#1");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (1, false), "#2");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionFrom (1, true), "#3");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (2, false), "#4");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionFrom (2, true), "#5");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (3, false), "#6");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionFrom (3, true), "#7");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionFrom (4, false), "#8");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionFrom (4, true), "#9");
			MaskedTextProviderTest.AssertProperties (mtp, "FindAssignedEditPositionFromTest", 10, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}

		[Test]
		public void FindAssignedEditPositionInRangeTest1 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add ("a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 0, false), "#0");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 0, true), "#1");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 1, false), "#2");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 1, true), "#3");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 2, false), "#4");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 2, true), "#5");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 3, false), "#6");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 3, true), "#7");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 4, false), "#8");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 4, true), "#9");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#10");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#11");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#12");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#13");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#14");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#15");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#16");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#17");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#18");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#19");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#20");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#21");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#22");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#23");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#24");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#25");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#26");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#27");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#28");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#29");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#30");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#31");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#32");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#33");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#34");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#35");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#36");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#37");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#38");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#39");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#40");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#41");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#42");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#43");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#44");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#45");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#46");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#47");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#48");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#49");
			AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 50, true, false, 1, 0, CultureInfo.GetCultureInfo ("en-US"), 1, true, false, false, 0, 3, "abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void FindAssignedEditPositionInRangeTest13 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"0 00 00 00 000 000 00");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, false), "#4116");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, true), "#4117");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#4118");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, true), "#4119");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#4120");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, true), "#4121");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#4122");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, true), "#4123");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, false), "#4124");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, true), "#4125");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, false), "#4126");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, true), "#4127");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, false), "#4128");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, true), "#4129");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, false), "#4130");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, true), "#4131");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, false), "#4132");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, true), "#4133");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, false), "#4134");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, true), "#4135");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, false), "#4136");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, true), "#4137");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, false), "#4138");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, true), "#4139");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, false), "#4140");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, true), "#4141");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 13, false), "#4142");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 13, true), "#4143");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 14, false), "#4144");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 14, true), "#4145");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 15, false), "#4146");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 15, true), "#4147");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 16, false), "#4148");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 16, true), "#4149");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 17, false), "#4150");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 17, true), "#4151");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 18, false), "#4152");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 18, true), "#4153");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 19, false), "#4154");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 19, true), "#4155");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 20, false), "#4156");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 20, true), "#4157");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 21, false), "#4158");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 21, true), "#4159");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 22, false), "#4160");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 22, true), "#4161");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#4162");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#4163");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#4164");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#4165");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#4166");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#4167");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#4168");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#4169");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#4170");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#4171");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, false), "#4172");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, true), "#4173");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, false), "#4174");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, true), "#4175");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, false), "#4176");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, true), "#4177");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, false), "#4178");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, true), "#4179");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, false), "#4180");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, true), "#4181");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, false), "#4182");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, true), "#4183");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, false), "#4184");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, true), "#4185");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, false), "#4186");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, true), "#4187");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 13, false), "#4188");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 13, true), "#4189");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 14, false), "#4190");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 14, true), "#4191");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 15, false), "#4192");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 15, true), "#4193");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 16, false), "#4194");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 16, true), "#4195");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 17, false), "#4196");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 17, true), "#4197");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 18, false), "#4198");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 18, true), "#4199");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 19, false), "#4200");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 19, true), "#4201");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 20, false), "#4202");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 20, true), "#4203");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 21, false), "#4204");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 21, true), "#4205");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 22, false), "#4206");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 22, true), "#4207");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#4208");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#4209");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#4210");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#4211");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#4212");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#4213");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#4214");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#4215");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#4216");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#4217");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, false), "#4218");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, true), "#4219");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, false), "#4220");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, true), "#4221");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, false), "#4222");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, true), "#4223");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, false), "#4224");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, true), "#4225");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, false), "#4226");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, true), "#4227");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, false), "#4228");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, true), "#4229");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, false), "#4230");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, true), "#4231");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, false), "#4232");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, true), "#4233");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 13, false), "#4234");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 13, true), "#4235");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 14, false), "#4236");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 14, true), "#4237");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 15, false), "#4238");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 15, true), "#4239");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 16, false), "#4240");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 16, true), "#4241");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 17, false), "#4242");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 17, true), "#4243");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 18, false), "#4244");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 18, true), "#4245");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 19, false), "#4246");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 19, true), "#4247");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 20, false), "#4248");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 20, true), "#4249");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 21, false), "#4250");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 21, true), "#4251");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 22, false), "#4252");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 22, true), "#4253");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#4254");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#4255");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#4256");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#4257");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#4258");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#4259");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#4260");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#4261");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#4262");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#4263");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, false), "#4264");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, true), "#4265");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, false), "#4266");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, true), "#4267");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, false), "#4268");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, true), "#4269");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, false), "#4270");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, true), "#4271");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, false), "#4272");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, true), "#4273");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, false), "#4274");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, true), "#4275");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, false), "#4276");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, true), "#4277");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, false), "#4278");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, true), "#4279");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 13, false), "#4280");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 13, true), "#4281");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 14, false), "#4282");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 14, true), "#4283");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 15, false), "#4284");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 15, true), "#4285");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 16, false), "#4286");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 16, true), "#4287");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 17, false), "#4288");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 17, true), "#4289");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 18, false), "#4290");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 18, true), "#4291");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 19, false), "#4292");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 19, true), "#4293");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 20, false), "#4294");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 20, true), "#4295");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 21, false), "#4296");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 21, true), "#4297");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 22, false), "#4298");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 22, true), "#4299");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#4300");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#4301");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#4302");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#4303");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#4304");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#4305");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#4306");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#4307");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#4308");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#4309");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, false), "#4310");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, true), "#4311");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, false), "#4312");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, true), "#4313");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, false), "#4314");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, true), "#4315");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, false), "#4316");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, true), "#4317");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, false), "#4318");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, true), "#4319");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, false), "#4320");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, true), "#4321");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, false), "#4322");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, true), "#4323");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, false), "#4324");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, true), "#4325");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 13, false), "#4326");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 13, true), "#4327");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 14, false), "#4328");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 14, true), "#4329");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 15, false), "#4330");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 15, true), "#4331");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 16, false), "#4332");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 16, true), "#4333");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 17, false), "#4334");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 17, true), "#4335");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 18, false), "#4336");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 18, true), "#4337");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 19, false), "#4338");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 19, true), "#4339");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 20, false), "#4340");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 20, true), "#4341");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 21, false), "#4342");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 21, true), "#4343");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 22, false), "#4344");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 22, true), "#4345");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, false), "#4346");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, true), "#4347");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, false), "#4348");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, true), "#4349");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, false), "#4350");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, true), "#4351");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, false), "#4352");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, true), "#4353");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, false), "#4354");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, true), "#4355");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, false), "#4356");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, true), "#4357");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, false), "#4358");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, true), "#4359");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, false), "#4360");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, true), "#4361");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, false), "#4362");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, true), "#4363");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, false), "#4364");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, true), "#4365");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, false), "#4366");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, true), "#4367");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, false), "#4368");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, true), "#4369");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, false), "#4370");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, true), "#4371");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 13, false), "#4372");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 13, true), "#4373");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 14, false), "#4374");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 14, true), "#4375");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 15, false), "#4376");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 15, true), "#4377");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 16, false), "#4378");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 16, true), "#4379");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 17, false), "#4380");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 17, true), "#4381");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 18, false), "#4382");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 18, true), "#4383");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 19, false), "#4384");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 19, true), "#4385");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 20, false), "#4386");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 20, true), "#4387");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 21, false), "#4388");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 21, true), "#4389");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 22, false), "#4390");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 22, true), "#4391");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, false), "#4392");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, true), "#4393");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, false), "#4394");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, true), "#4395");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, false), "#4396");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, true), "#4397");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, false), "#4398");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, true), "#4399");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, false), "#4400");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, true), "#4401");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, false), "#4402");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, true), "#4403");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, false), "#4404");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, true), "#4405");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, false), "#4406");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, true), "#4407");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, false), "#4408");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, true), "#4409");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, false), "#4410");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, true), "#4411");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, false), "#4412");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, true), "#4413");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, false), "#4414");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, true), "#4415");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, false), "#4416");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, true), "#4417");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 13, false), "#4418");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 13, true), "#4419");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 14, false), "#4420");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 14, true), "#4421");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 15, false), "#4422");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 15, true), "#4423");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 16, false), "#4424");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 16, true), "#4425");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 17, false), "#4426");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 17, true), "#4427");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 18, false), "#4428");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 18, true), "#4429");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 19, false), "#4430");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 19, true), "#4431");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 20, false), "#4432");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 20, true), "#4433");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 21, false), "#4434");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 21, true), "#4435");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 22, false), "#4436");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 22, true), "#4437");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, false), "#4438");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, true), "#4439");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, false), "#4440");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, true), "#4441");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, false), "#4442");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, true), "#4443");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, false), "#4444");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, true), "#4445");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, false), "#4446");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, true), "#4447");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, false), "#4448");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, true), "#4449");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, false), "#4450");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, true), "#4451");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, false), "#4452");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, true), "#4453");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, false), "#4454");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, true), "#4455");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, false), "#4456");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, true), "#4457");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, false), "#4458");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, true), "#4459");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, false), "#4460");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, true), "#4461");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, false), "#4462");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, true), "#4463");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 13, false), "#4464");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 13, true), "#4465");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 14, false), "#4466");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 14, true), "#4467");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 15, false), "#4468");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 15, true), "#4469");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 16, false), "#4470");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 16, true), "#4471");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 17, false), "#4472");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 17, true), "#4473");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 18, false), "#4474");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 18, true), "#4475");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 19, false), "#4476");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 19, true), "#4477");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 20, false), "#4478");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 20, true), "#4479");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 21, false), "#4480");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 21, true), "#4481");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 22, false), "#4482");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 22, true), "#4483");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, false), "#4484");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, true), "#4485");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, false), "#4486");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, true), "#4487");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, false), "#4488");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, true), "#4489");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, false), "#4490");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, true), "#4491");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, false), "#4492");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, true), "#4493");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, false), "#4494");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, true), "#4495");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, false), "#4496");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, true), "#4497");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, false), "#4498");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, true), "#4499");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, false), "#4500");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, true), "#4501");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, false), "#4502");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, true), "#4503");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, false), "#4504");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, true), "#4505");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, false), "#4506");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, true), "#4507");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, false), "#4508");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, true), "#4509");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 13, false), "#4510");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 13, true), "#4511");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 14, false), "#4512");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 14, true), "#4513");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 15, false), "#4514");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 15, true), "#4515");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 16, false), "#4516");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 16, true), "#4517");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 17, false), "#4518");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 17, true), "#4519");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 18, false), "#4520");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 18, true), "#4521");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 19, false), "#4522");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 19, true), "#4523");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 20, false), "#4524");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 20, true), "#4525");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 21, false), "#4526");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 21, true), "#4527");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 22, false), "#4528");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 22, true), "#4529");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, false), "#4530");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, true), "#4531");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, false), "#4532");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, true), "#4533");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, false), "#4534");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, true), "#4535");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, false), "#4536");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, true), "#4537");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, false), "#4538");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, true), "#4539");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, false), "#4540");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, true), "#4541");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, false), "#4542");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, true), "#4543");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, false), "#4544");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, true), "#4545");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, false), "#4546");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, true), "#4547");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, false), "#4548");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, true), "#4549");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, false), "#4550");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, true), "#4551");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, false), "#4552");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, true), "#4553");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, false), "#4554");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, true), "#4555");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 13, false), "#4556");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 13, true), "#4557");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 14, false), "#4558");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 14, true), "#4559");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 15, false), "#4560");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 15, true), "#4561");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 16, false), "#4562");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 16, true), "#4563");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 17, false), "#4564");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 17, true), "#4565");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 18, false), "#4566");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 18, true), "#4567");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 19, false), "#4568");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 19, true), "#4569");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 20, false), "#4570");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 20, true), "#4571");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 21, false), "#4572");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 21, true), "#4573");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 22, false), "#4574");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 22, true), "#4575");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, false), "#4576");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, true), "#4577");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, false), "#4578");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, true), "#4579");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, false), "#4580");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, true), "#4581");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, false), "#4582");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, true), "#4583");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, false), "#4584");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, true), "#4585");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, false), "#4586");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, true), "#4587");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, false), "#4588");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, true), "#4589");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, false), "#4590");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, true), "#4591");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, false), "#4592");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, true), "#4593");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, false), "#4594");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, true), "#4595");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, false), "#4596");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, true), "#4597");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, false), "#4598");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, true), "#4599");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, false), "#4600");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, true), "#4601");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 13, false), "#4602");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 13, true), "#4603");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 14, false), "#4604");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 14, true), "#4605");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 15, false), "#4606");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 15, true), "#4607");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 16, false), "#4608");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 16, true), "#4609");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 17, false), "#4610");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 17, true), "#4611");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 18, false), "#4612");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 18, true), "#4613");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 19, false), "#4614");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 19, true), "#4615");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 20, false), "#4616");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 20, true), "#4617");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 21, false), "#4618");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 21, true), "#4619");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 22, false), "#4620");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 22, true), "#4621");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, false), "#4622");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, true), "#4623");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, false), "#4624");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, true), "#4625");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, false), "#4626");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, true), "#4627");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, false), "#4628");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, true), "#4629");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, false), "#4630");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, true), "#4631");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, false), "#4632");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, true), "#4633");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, false), "#4634");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, true), "#4635");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, false), "#4636");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, true), "#4637");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, false), "#4638");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, true), "#4639");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, false), "#4640");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, true), "#4641");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, false), "#4642");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, true), "#4643");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, false), "#4644");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, true), "#4645");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, false), "#4646");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, true), "#4647");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 13, false), "#4648");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 13, true), "#4649");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 14, false), "#4650");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 14, true), "#4651");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 15, false), "#4652");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 15, true), "#4653");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 16, false), "#4654");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 16, true), "#4655");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 17, false), "#4656");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 17, true), "#4657");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 18, false), "#4658");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 18, true), "#4659");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 19, false), "#4660");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 19, true), "#4661");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 20, false), "#4662");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 20, true), "#4663");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 21, false), "#4664");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 21, true), "#4665");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 22, false), "#4666");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 22, true), "#4667");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, false), "#4668");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, true), "#4669");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, false), "#4670");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, true), "#4671");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, false), "#4672");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, true), "#4673");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, false), "#4674");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, true), "#4675");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, false), "#4676");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, true), "#4677");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, false), "#4678");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, true), "#4679");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, false), "#4680");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, true), "#4681");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, false), "#4682");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, true), "#4683");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, false), "#4684");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, true), "#4685");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, false), "#4686");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, true), "#4687");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, false), "#4688");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, true), "#4689");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, false), "#4690");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, true), "#4691");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, false), "#4692");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, true), "#4693");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 13, false), "#4694");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 13, true), "#4695");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 14, false), "#4696");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 14, true), "#4697");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 15, false), "#4698");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 15, true), "#4699");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 16, false), "#4700");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 16, true), "#4701");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 17, false), "#4702");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 17, true), "#4703");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 18, false), "#4704");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 18, true), "#4705");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 19, false), "#4706");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 19, true), "#4707");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 20, false), "#4708");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 20, true), "#4709");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 21, false), "#4710");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 21, true), "#4711");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 22, false), "#4712");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 22, true), "#4713");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 0, false), "#4714");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 0, true), "#4715");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 1, false), "#4716");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 1, true), "#4717");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 2, false), "#4718");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 2, true), "#4719");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 3, false), "#4720");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 3, true), "#4721");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 4, false), "#4722");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 4, true), "#4723");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 5, false), "#4724");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 5, true), "#4725");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 6, false), "#4726");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 6, true), "#4727");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 7, false), "#4728");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 7, true), "#4729");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 8, false), "#4730");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 8, true), "#4731");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 9, false), "#4732");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 9, true), "#4733");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 10, false), "#4734");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 10, true), "#4735");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 11, false), "#4736");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 11, true), "#4737");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 12, false), "#4738");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 12, true), "#4739");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 13, false), "#4740");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 13, true), "#4741");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 14, false), "#4742");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 14, true), "#4743");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 15, false), "#4744");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 15, true), "#4745");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 16, false), "#4746");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 16, true), "#4747");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 17, false), "#4748");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 17, true), "#4749");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 18, false), "#4750");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 18, true), "#4751");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 19, false), "#4752");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 19, true), "#4753");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 20, false), "#4754");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 20, true), "#4755");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 21, false), "#4756");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 21, true), "#4757");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 22, false), "#4758");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 22, true), "#4759");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 0, false), "#4760");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 0, true), "#4761");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 1, false), "#4762");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 1, true), "#4763");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 2, false), "#4764");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 2, true), "#4765");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 3, false), "#4766");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 3, true), "#4767");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 4, false), "#4768");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 4, true), "#4769");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 5, false), "#4770");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 5, true), "#4771");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 6, false), "#4772");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 6, true), "#4773");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 7, false), "#4774");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 7, true), "#4775");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 8, false), "#4776");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 8, true), "#4777");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 9, false), "#4778");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 9, true), "#4779");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 10, false), "#4780");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 10, true), "#4781");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 11, false), "#4782");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 11, true), "#4783");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 12, false), "#4784");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 12, true), "#4785");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 13, false), "#4786");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 13, true), "#4787");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 14, false), "#4788");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 14, true), "#4789");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 15, false), "#4790");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 15, true), "#4791");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 16, false), "#4792");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 16, true), "#4793");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 17, false), "#4794");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 17, true), "#4795");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 18, false), "#4796");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 18, true), "#4797");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 19, false), "#4798");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 19, true), "#4799");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 20, false), "#4800");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 20, true), "#4801");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 21, false), "#4802");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 21, true), "#4803");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 22, false), "#4804");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 22, true), "#4805");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 0, false), "#4806");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 0, true), "#4807");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 1, false), "#4808");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 1, true), "#4809");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 2, false), "#4810");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 2, true), "#4811");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 3, false), "#4812");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 3, true), "#4813");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 4, false), "#4814");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 4, true), "#4815");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 5, false), "#4816");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 5, true), "#4817");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 6, false), "#4818");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 6, true), "#4819");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 7, false), "#4820");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 7, true), "#4821");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 8, false), "#4822");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 8, true), "#4823");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 9, false), "#4824");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 9, true), "#4825");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 10, false), "#4826");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 10, true), "#4827");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 11, false), "#4828");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 11, true), "#4829");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 12, false), "#4830");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 12, true), "#4831");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 13, false), "#4832");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 13, true), "#4833");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 14, false), "#4834");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 14, true), "#4835");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 15, false), "#4836");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 15, true), "#4837");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 16, false), "#4838");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 16, true), "#4839");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 17, false), "#4840");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 17, true), "#4841");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 18, false), "#4842");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 18, true), "#4843");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 19, false), "#4844");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 19, true), "#4845");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 20, false), "#4846");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 20, true), "#4847");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 21, false), "#4848");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 21, true), "#4849");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 22, false), "#4850");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 22, true), "#4851");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 0, false), "#4852");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 0, true), "#4853");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 1, false), "#4854");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 1, true), "#4855");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 2, false), "#4856");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 2, true), "#4857");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 3, false), "#4858");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 3, true), "#4859");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 4, false), "#4860");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 4, true), "#4861");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 5, false), "#4862");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 5, true), "#4863");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 6, false), "#4864");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 6, true), "#4865");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 7, false), "#4866");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 7, true), "#4867");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 8, false), "#4868");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 8, true), "#4869");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 9, false), "#4870");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 9, true), "#4871");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 10, false), "#4872");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 10, true), "#4873");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 11, false), "#4874");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 11, true), "#4875");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 12, false), "#4876");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 12, true), "#4877");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 13, false), "#4878");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 13, true), "#4879");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 14, false), "#4880");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 14, true), "#4881");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 15, false), "#4882");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 15, true), "#4883");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 16, false), "#4884");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 16, true), "#4885");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 17, false), "#4886");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 17, true), "#4887");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 18, false), "#4888");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 18, true), "#4889");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 19, false), "#4890");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 19, true), "#4891");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 20, false), "#4892");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 20, true), "#4893");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 21, false), "#4894");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 21, true), "#4895");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 22, false), "#4896");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 22, true), "#4897");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 0, false), "#4898");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 0, true), "#4899");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 1, false), "#4900");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 1, true), "#4901");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 2, false), "#4902");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 2, true), "#4903");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 3, false), "#4904");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 3, true), "#4905");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 4, false), "#4906");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 4, true), "#4907");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 5, false), "#4908");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 5, true), "#4909");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 6, false), "#4910");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 6, true), "#4911");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 7, false), "#4912");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 7, true), "#4913");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 8, false), "#4914");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 8, true), "#4915");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 9, false), "#4916");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 9, true), "#4917");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 10, false), "#4918");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 10, true), "#4919");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 11, false), "#4920");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 11, true), "#4921");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 12, false), "#4922");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 12, true), "#4923");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 13, false), "#4924");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 13, true), "#4925");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 14, false), "#4926");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 14, true), "#4927");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 15, false), "#4928");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 15, true), "#4929");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 16, false), "#4930");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 16, true), "#4931");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 17, false), "#4932");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 17, true), "#4933");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 18, false), "#4934");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 18, true), "#4935");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 19, false), "#4936");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 19, true), "#4937");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 20, false), "#4938");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 20, true), "#4939");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 21, false), "#4940");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 21, true), "#4941");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 22, false), "#4942");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 22, true), "#4943");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 0, false), "#4944");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 0, true), "#4945");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 1, false), "#4946");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 1, true), "#4947");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 2, false), "#4948");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 2, true), "#4949");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 3, false), "#4950");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 3, true), "#4951");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 4, false), "#4952");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 4, true), "#4953");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 5, false), "#4954");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 5, true), "#4955");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 6, false), "#4956");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 6, true), "#4957");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 7, false), "#4958");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 7, true), "#4959");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 8, false), "#4960");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 8, true), "#4961");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 9, false), "#4962");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 9, true), "#4963");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 10, false), "#4964");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 10, true), "#4965");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 11, false), "#4966");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 11, true), "#4967");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 12, false), "#4968");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 12, true), "#4969");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 13, false), "#4970");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 13, true), "#4971");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 14, false), "#4972");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 14, true), "#4973");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 15, false), "#4974");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 15, true), "#4975");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 16, false), "#4976");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 16, true), "#4977");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 17, false), "#4978");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 17, true), "#4979");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 18, false), "#4980");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 18, true), "#4981");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 19, false), "#4982");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 19, true), "#4983");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 20, false), "#4984");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 20, true), "#4985");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 21, false), "#4986");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 21, true), "#4987");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 22, false), "#4988");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 22, true), "#4989");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 0, false), "#4990");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 0, true), "#4991");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 1, false), "#4992");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 1, true), "#4993");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 2, false), "#4994");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 2, true), "#4995");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 3, false), "#4996");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 3, true), "#4997");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 4, false), "#4998");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 4, true), "#4999");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 5, false), "#5000");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 5, true), "#5001");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 6, false), "#5002");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 6, true), "#5003");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 7, false), "#5004");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 7, true), "#5005");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 8, false), "#5006");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 8, true), "#5007");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 9, false), "#5008");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 9, true), "#5009");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 10, false), "#5010");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 10, true), "#5011");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 11, false), "#5012");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 11, true), "#5013");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 12, false), "#5014");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 12, true), "#5015");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 13, false), "#5016");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 13, true), "#5017");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 14, false), "#5018");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 14, true), "#5019");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 15, false), "#5020");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 15, true), "#5021");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 16, false), "#5022");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 16, true), "#5023");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 17, false), "#5024");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 17, true), "#5025");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 18, false), "#5026");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 18, true), "#5027");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 19, false), "#5028");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 19, true), "#5029");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 20, false), "#5030");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 20, true), "#5031");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 21, false), "#5032");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 21, true), "#5033");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 22, false), "#5034");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 22, true), "#5035");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 0, false), "#5036");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 0, true), "#5037");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 1, false), "#5038");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 1, true), "#5039");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 2, false), "#5040");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 2, true), "#5041");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 3, false), "#5042");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 3, true), "#5043");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 4, false), "#5044");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 4, true), "#5045");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 5, false), "#5046");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 5, true), "#5047");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 6, false), "#5048");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 6, true), "#5049");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 7, false), "#5050");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 7, true), "#5051");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 8, false), "#5052");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 8, true), "#5053");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 9, false), "#5054");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 9, true), "#5055");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 10, false), "#5056");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 10, true), "#5057");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 11, false), "#5058");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 11, true), "#5059");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 12, false), "#5060");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 12, true), "#5061");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 13, false), "#5062");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 13, true), "#5063");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 14, false), "#5064");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 14, true), "#5065");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 15, false), "#5066");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 15, true), "#5067");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 16, false), "#5068");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 16, true), "#5069");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 17, false), "#5070");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 17, true), "#5071");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 18, false), "#5072");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 18, true), "#5073");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 19, false), "#5074");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 19, true), "#5075");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 20, false), "#5076");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 20, true), "#5077");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 21, false), "#5078");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 21, true), "#5079");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 22, false), "#5080");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 22, true), "#5081");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 0, false), "#5082");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 0, true), "#5083");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 1, false), "#5084");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 1, true), "#5085");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 2, false), "#5086");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 2, true), "#5087");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 3, false), "#5088");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 3, true), "#5089");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 4, false), "#5090");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 4, true), "#5091");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 5, false), "#5092");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 5, true), "#5093");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 6, false), "#5094");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 6, true), "#5095");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 7, false), "#5096");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 7, true), "#5097");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 8, false), "#5098");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 8, true), "#5099");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 9, false), "#5100");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 9, true), "#5101");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 10, false), "#5102");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 10, true), "#5103");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 11, false), "#5104");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 11, true), "#5105");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 12, false), "#5106");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 12, true), "#5107");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 13, false), "#5108");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 13, true), "#5109");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 14, false), "#5110");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 14, true), "#5111");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 15, false), "#5112");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 15, true), "#5113");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 16, false), "#5114");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 16, true), "#5115");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 17, false), "#5116");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 17, true), "#5117");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 18, false), "#5118");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 18, true), "#5119");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 19, false), "#5120");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 19, true), "#5121");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 20, false), "#5122");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 20, true), "#5123");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 21, false), "#5124");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 21, true), "#5125");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 22, false), "#5126");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (21, 22, true), "#5127");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 0, false), "#5128");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 0, true), "#5129");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 1, false), "#5130");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 1, true), "#5131");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 2, false), "#5132");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 2, true), "#5133");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 3, false), "#5134");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 3, true), "#5135");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 4, false), "#5136");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 4, true), "#5137");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 5, false), "#5138");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 5, true), "#5139");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 6, false), "#5140");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 6, true), "#5141");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 7, false), "#5142");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 7, true), "#5143");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 8, false), "#5144");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 8, true), "#5145");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 9, false), "#5146");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 9, true), "#5147");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 10, false), "#5148");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 10, true), "#5149");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 11, false), "#5150");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 11, true), "#5151");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 12, false), "#5152");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 12, true), "#5153");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 13, false), "#5154");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 13, true), "#5155");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 14, false), "#5156");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 14, true), "#5157");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 15, false), "#5158");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 15, true), "#5159");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 16, false), "#5160");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 16, true), "#5161");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 17, false), "#5162");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 17, true), "#5163");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 18, false), "#5164");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 18, true), "#5165");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 19, false), "#5166");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 19, true), "#5167");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 20, false), "#5168");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 20, true), "#5169");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 21, false), "#5170");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 21, true), "#5171");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 22, false), "#5172");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (22, 22, true), "#5173");
			MaskedTextProviderTest.AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 5174, true, false, 0, 15, CultureInfo.GetCultureInfo ("es-ES"), 15, true, false, false, -1, 21, @"0 00 00 00 000 000 00", false, false, '\x0', '\x5F', true, true, true, @"                   ", @"                   ", @"                   ", @"_ __ __ __ ___ ___ __", @"_______________", @"                   ", @"");
		}
		[Test]
		public void FindAssignedEditPositionInRangeTest2 ()
		{
			MaskedTextProvider mtp;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add ("a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, false), "#51");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, true), "#52");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#53");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, true), "#54");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#55");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, true), "#56");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#57");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, true), "#58");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, false), "#59");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, true), "#60");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#61");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#62");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#63");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#64");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#65");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#66");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#67");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#68");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#69");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#70");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#71");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#72");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#73");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#74");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#75");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#76");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#77");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#78");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#79");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#80");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#81");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#82");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#83");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#84");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#85");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#86");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#87");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#88");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#89");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#90");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#91");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#92");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#93");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#94");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#95");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#96");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#97");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#98");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#99");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#100");
			AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 101, true, false, 0, 1, CultureInfo.GetCultureInfo ("en-US"), 1, true, false, false, -1, 3, "abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}

		[Test]
		public void FindAssignedEditPositionInRangeTest4 ()
		{
			MaskedTextProvider mtp;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"a?b?c");
			mtp.Add ("a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, false), "#201");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, true), "#202");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#203");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 1, true), "#204");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#205");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 2, true), "#206");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#207");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 3, true), "#208");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 4, false), "#209");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 4, true), "#210");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 5, false), "#211");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 5, true), "#212");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 6, false), "#213");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 6, true), "#214");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#215");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#216");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#217");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#218");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#219");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#220");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#221");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#222");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#223");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#224");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 5, false), "#225");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 5, true), "#226");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 6, false), "#227");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 6, true), "#228");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#229");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#230");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#231");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#232");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#233");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#234");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#235");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#236");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#237");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#238");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, false), "#239");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, true), "#240");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, false), "#241");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, true), "#242");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#243");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#244");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#245");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#246");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#247");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#248");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#249");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#250");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#251");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#252");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, false), "#253");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, true), "#254");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, false), "#255");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, true), "#256");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#257");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#258");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#259");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#260");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#261");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#262");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#263");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#264");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#265");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#266");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, false), "#267");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, true), "#268");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, false), "#269");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, true), "#270");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, false), "#271");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, true), "#272");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, false), "#273");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, true), "#274");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, false), "#275");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, true), "#276");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, false), "#277");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, true), "#278");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, false), "#279");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, true), "#280");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, false), "#281");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, true), "#282");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, false), "#283");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, true), "#284");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, false), "#285");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, true), "#286");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, false), "#287");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, true), "#288");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, false), "#289");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, true), "#290");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, false), "#291");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, true), "#292");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, false), "#293");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, true), "#294");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, false), "#295");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, true), "#296");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, false), "#297");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, true), "#298");
			AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 299, true, false, 1, 2, CultureInfo.GetCultureInfo ("en-US"), 3, true, false, false, 1, 5, "a?b?c", true, false, '\x0', '\x5F', true, true, true, @" ab c", @" ab c", @" ab c", @"_ab_c", @"_a_", @" ab c", @" a");
		}
		[Test]
		public void FindAssignedEditPositionInRangeTest7 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add ("a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, false), "#450");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, true), "#451");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#452");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, true), "#453");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#454");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, true), "#455");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#456");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, true), "#457");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, false), "#458");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, true), "#459");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, false), "#460");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, true), "#461");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, false), "#462");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, true), "#463");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, false), "#464");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, true), "#465");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, false), "#466");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, true), "#467");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, false), "#468");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, true), "#469");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, false), "#470");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, true), "#471");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, false), "#472");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, true), "#473");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, false), "#474");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, true), "#475");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 13, false), "#476");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 13, true), "#477");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 14, false), "#478");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 14, true), "#479");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 15, false), "#480");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 15, true), "#481");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 16, false), "#482");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 16, true), "#483");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 17, false), "#484");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 17, true), "#485");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 18, false), "#486");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 18, true), "#487");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 19, false), "#488");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 19, true), "#489");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 20, false), "#490");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 20, true), "#491");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#492");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#493");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#494");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#495");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#496");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#497");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#498");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#499");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#500");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#501");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, false), "#502");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, true), "#503");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, false), "#504");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, true), "#505");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, false), "#506");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, true), "#507");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, false), "#508");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, true), "#509");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, false), "#510");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, true), "#511");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, false), "#512");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, true), "#513");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, false), "#514");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, true), "#515");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, false), "#516");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, true), "#517");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 13, false), "#518");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 13, true), "#519");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 14, false), "#520");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 14, true), "#521");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 15, false), "#522");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 15, true), "#523");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 16, false), "#524");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 16, true), "#525");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 17, false), "#526");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 17, true), "#527");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 18, false), "#528");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 18, true), "#529");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 19, false), "#530");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 19, true), "#531");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 20, false), "#532");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 20, true), "#533");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#534");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#535");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#536");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#537");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#538");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#539");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#540");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#541");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#542");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#543");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, false), "#544");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, true), "#545");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, false), "#546");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, true), "#547");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, false), "#548");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, true), "#549");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, false), "#550");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, true), "#551");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, false), "#552");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, true), "#553");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, false), "#554");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, true), "#555");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, false), "#556");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, true), "#557");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, false), "#558");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, true), "#559");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 13, false), "#560");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 13, true), "#561");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 14, false), "#562");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 14, true), "#563");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 15, false), "#564");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 15, true), "#565");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 16, false), "#566");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 16, true), "#567");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 17, false), "#568");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 17, true), "#569");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 18, false), "#570");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 18, true), "#571");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 19, false), "#572");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 19, true), "#573");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 20, false), "#574");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 20, true), "#575");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#576");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#577");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#578");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#579");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#580");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#581");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#582");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#583");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#584");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#585");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, false), "#586");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, true), "#587");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, false), "#588");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, true), "#589");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, false), "#590");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, true), "#591");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, false), "#592");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, true), "#593");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, false), "#594");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, true), "#595");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, false), "#596");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, true), "#597");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, false), "#598");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, true), "#599");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, false), "#600");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, true), "#601");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 13, false), "#602");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 13, true), "#603");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 14, false), "#604");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 14, true), "#605");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 15, false), "#606");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 15, true), "#607");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 16, false), "#608");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 16, true), "#609");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 17, false), "#610");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 17, true), "#611");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 18, false), "#612");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 18, true), "#613");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 19, false), "#614");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 19, true), "#615");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 20, false), "#616");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 20, true), "#617");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#618");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#619");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#620");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#621");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#622");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#623");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#624");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#625");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#626");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#627");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, false), "#628");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, true), "#629");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, false), "#630");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, true), "#631");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, false), "#632");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, true), "#633");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, false), "#634");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, true), "#635");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, false), "#636");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, true), "#637");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, false), "#638");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, true), "#639");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, false), "#640");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, true), "#641");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, false), "#642");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, true), "#643");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 13, false), "#644");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 13, true), "#645");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 14, false), "#646");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 14, true), "#647");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 15, false), "#648");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 15, true), "#649");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 16, false), "#650");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 16, true), "#651");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 17, false), "#652");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 17, true), "#653");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 18, false), "#654");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 18, true), "#655");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 19, false), "#656");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 19, true), "#657");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 20, false), "#658");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 20, true), "#659");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, false), "#660");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, true), "#661");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, false), "#662");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, true), "#663");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, false), "#664");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, true), "#665");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, false), "#666");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, true), "#667");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, false), "#668");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, true), "#669");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, false), "#670");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, true), "#671");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, false), "#672");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, true), "#673");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, false), "#674");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, true), "#675");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, false), "#676");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, true), "#677");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, false), "#678");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, true), "#679");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, false), "#680");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, true), "#681");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, false), "#682");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, true), "#683");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, false), "#684");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, true), "#685");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 13, false), "#686");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 13, true), "#687");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 14, false), "#688");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 14, true), "#689");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 15, false), "#690");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 15, true), "#691");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 16, false), "#692");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 16, true), "#693");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 17, false), "#694");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 17, true), "#695");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 18, false), "#696");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 18, true), "#697");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 19, false), "#698");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 19, true), "#699");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 20, false), "#700");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 20, true), "#701");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, false), "#702");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, true), "#703");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, false), "#704");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, true), "#705");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, false), "#706");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, true), "#707");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, false), "#708");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, true), "#709");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, false), "#710");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, true), "#711");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, false), "#712");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, true), "#713");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, false), "#714");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, true), "#715");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, false), "#716");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, true), "#717");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, false), "#718");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, true), "#719");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, false), "#720");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, true), "#721");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, false), "#722");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, true), "#723");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, false), "#724");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, true), "#725");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, false), "#726");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, true), "#727");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 13, false), "#728");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 13, true), "#729");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 14, false), "#730");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 14, true), "#731");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 15, false), "#732");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 15, true), "#733");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 16, false), "#734");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 16, true), "#735");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 17, false), "#736");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 17, true), "#737");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 18, false), "#738");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 18, true), "#739");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 19, false), "#740");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 19, true), "#741");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 20, false), "#742");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 20, true), "#743");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, false), "#744");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, true), "#745");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, false), "#746");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, true), "#747");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, false), "#748");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, true), "#749");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, false), "#750");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, true), "#751");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, false), "#752");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, true), "#753");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, false), "#754");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, true), "#755");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, false), "#756");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, true), "#757");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, false), "#758");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, true), "#759");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, false), "#760");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, true), "#761");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, false), "#762");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, true), "#763");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, false), "#764");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, true), "#765");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, false), "#766");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, true), "#767");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, false), "#768");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, true), "#769");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 13, false), "#770");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 13, true), "#771");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 14, false), "#772");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 14, true), "#773");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 15, false), "#774");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 15, true), "#775");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 16, false), "#776");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 16, true), "#777");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 17, false), "#778");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 17, true), "#779");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 18, false), "#780");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 18, true), "#781");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 19, false), "#782");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 19, true), "#783");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 20, false), "#784");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 20, true), "#785");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, false), "#786");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, true), "#787");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, false), "#788");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, true), "#789");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, false), "#790");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, true), "#791");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, false), "#792");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, true), "#793");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, false), "#794");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, true), "#795");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, false), "#796");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, true), "#797");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, false), "#798");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, true), "#799");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, false), "#800");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, true), "#801");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, false), "#802");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, true), "#803");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, false), "#804");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, true), "#805");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, false), "#806");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, true), "#807");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, false), "#808");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, true), "#809");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, false), "#810");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, true), "#811");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 13, false), "#812");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 13, true), "#813");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 14, false), "#814");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 14, true), "#815");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 15, false), "#816");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 15, true), "#817");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 16, false), "#818");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 16, true), "#819");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 17, false), "#820");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 17, true), "#821");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 18, false), "#822");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 18, true), "#823");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 19, false), "#824");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 19, true), "#825");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 20, false), "#826");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 20, true), "#827");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, false), "#828");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, true), "#829");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, false), "#830");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, true), "#831");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, false), "#832");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, true), "#833");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, false), "#834");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, true), "#835");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, false), "#836");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, true), "#837");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, false), "#838");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, true), "#839");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, false), "#840");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, true), "#841");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, false), "#842");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, true), "#843");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, false), "#844");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, true), "#845");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, false), "#846");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, true), "#847");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, false), "#848");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, true), "#849");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, false), "#850");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, true), "#851");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, false), "#852");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, true), "#853");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 13, false), "#854");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 13, true), "#855");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 14, false), "#856");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 14, true), "#857");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 15, false), "#858");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 15, true), "#859");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 16, false), "#860");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 16, true), "#861");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 17, false), "#862");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 17, true), "#863");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 18, false), "#864");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 18, true), "#865");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 19, false), "#866");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 19, true), "#867");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 20, false), "#868");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 20, true), "#869");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, false), "#870");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, true), "#871");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, false), "#872");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, true), "#873");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, false), "#874");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, true), "#875");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, false), "#876");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, true), "#877");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, false), "#878");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, true), "#879");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, false), "#880");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, true), "#881");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, false), "#882");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, true), "#883");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, false), "#884");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, true), "#885");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, false), "#886");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, true), "#887");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, false), "#888");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, true), "#889");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, false), "#890");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, true), "#891");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, false), "#892");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, true), "#893");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, false), "#894");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, true), "#895");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 13, false), "#896");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 13, true), "#897");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 14, false), "#898");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 14, true), "#899");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 15, false), "#900");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 15, true), "#901");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 16, false), "#902");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 16, true), "#903");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 17, false), "#904");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 17, true), "#905");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 18, false), "#906");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 18, true), "#907");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 19, false), "#908");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 19, true), "#909");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 20, false), "#910");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 20, true), "#911");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, false), "#912");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, true), "#913");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, false), "#914");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, true), "#915");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, false), "#916");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, true), "#917");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, false), "#918");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, true), "#919");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, false), "#920");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, true), "#921");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, false), "#922");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, true), "#923");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, false), "#924");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, true), "#925");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, false), "#926");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, true), "#927");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, false), "#928");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, true), "#929");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, false), "#930");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, true), "#931");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, false), "#932");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, true), "#933");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, false), "#934");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, true), "#935");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, false), "#936");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, true), "#937");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 13, false), "#938");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 13, true), "#939");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 14, false), "#940");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 14, true), "#941");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 15, false), "#942");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 15, true), "#943");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 16, false), "#944");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 16, true), "#945");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 17, false), "#946");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 17, true), "#947");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 18, false), "#948");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 18, true), "#949");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 19, false), "#950");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 19, true), "#951");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 20, false), "#952");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 20, true), "#953");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, false), "#954");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, true), "#955");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, false), "#956");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, true), "#957");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, false), "#958");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, true), "#959");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, false), "#960");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, true), "#961");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, false), "#962");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, true), "#963");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, false), "#964");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, true), "#965");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, false), "#966");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, true), "#967");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, false), "#968");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, true), "#969");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, false), "#970");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, true), "#971");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, false), "#972");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, true), "#973");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, false), "#974");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, true), "#975");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, false), "#976");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, true), "#977");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, false), "#978");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, true), "#979");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 13, false), "#980");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 13, true), "#981");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 14, false), "#982");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 14, true), "#983");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 15, false), "#984");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 15, true), "#985");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 16, false), "#986");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 16, true), "#987");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 17, false), "#988");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 17, true), "#989");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 18, false), "#990");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 18, true), "#991");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 19, false), "#992");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 19, true), "#993");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 20, false), "#994");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 20, true), "#995");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 0, false), "#996");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 0, true), "#997");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 1, false), "#998");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 1, true), "#999");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 2, false), "#1000");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 2, true), "#1001");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 3, false), "#1002");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 3, true), "#1003");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 4, false), "#1004");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 4, true), "#1005");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 5, false), "#1006");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 5, true), "#1007");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 6, false), "#1008");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 6, true), "#1009");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 7, false), "#1010");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 7, true), "#1011");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 8, false), "#1012");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 8, true), "#1013");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 9, false), "#1014");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 9, true), "#1015");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 10, false), "#1016");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 10, true), "#1017");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 11, false), "#1018");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 11, true), "#1019");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 12, false), "#1020");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 12, true), "#1021");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 13, false), "#1022");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 13, true), "#1023");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 14, false), "#1024");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 14, true), "#1025");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 15, false), "#1026");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 15, true), "#1027");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 16, false), "#1028");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 16, true), "#1029");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 17, false), "#1030");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 17, true), "#1031");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 18, false), "#1032");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 18, true), "#1033");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 19, false), "#1034");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 19, true), "#1035");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 20, false), "#1036");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (13, 20, true), "#1037");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 0, false), "#1038");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 0, true), "#1039");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 1, false), "#1040");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 1, true), "#1041");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 2, false), "#1042");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 2, true), "#1043");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 3, false), "#1044");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 3, true), "#1045");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 4, false), "#1046");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 4, true), "#1047");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 5, false), "#1048");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 5, true), "#1049");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 6, false), "#1050");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 6, true), "#1051");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 7, false), "#1052");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 7, true), "#1053");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 8, false), "#1054");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 8, true), "#1055");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 9, false), "#1056");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 9, true), "#1057");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 10, false), "#1058");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 10, true), "#1059");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 11, false), "#1060");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 11, true), "#1061");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 12, false), "#1062");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 12, true), "#1063");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 13, false), "#1064");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 13, true), "#1065");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 14, false), "#1066");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 14, true), "#1067");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 15, false), "#1068");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 15, true), "#1069");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 16, false), "#1070");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 16, true), "#1071");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 17, false), "#1072");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 17, true), "#1073");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 18, false), "#1074");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 18, true), "#1075");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 19, false), "#1076");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 19, true), "#1077");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 20, false), "#1078");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (14, 20, true), "#1079");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 0, false), "#1080");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 0, true), "#1081");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 1, false), "#1082");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 1, true), "#1083");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 2, false), "#1084");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 2, true), "#1085");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 3, false), "#1086");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 3, true), "#1087");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 4, false), "#1088");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 4, true), "#1089");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 5, false), "#1090");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 5, true), "#1091");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 6, false), "#1092");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 6, true), "#1093");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 7, false), "#1094");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 7, true), "#1095");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 8, false), "#1096");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 8, true), "#1097");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 9, false), "#1098");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 9, true), "#1099");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 10, false), "#1100");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 10, true), "#1101");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 11, false), "#1102");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 11, true), "#1103");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 12, false), "#1104");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 12, true), "#1105");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 13, false), "#1106");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 13, true), "#1107");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 14, false), "#1108");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 14, true), "#1109");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 15, false), "#1110");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 15, true), "#1111");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 16, false), "#1112");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 16, true), "#1113");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 17, false), "#1114");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 17, true), "#1115");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 18, false), "#1116");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 18, true), "#1117");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 19, false), "#1118");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 19, true), "#1119");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 20, false), "#1120");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (15, 20, true), "#1121");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 0, false), "#1122");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 0, true), "#1123");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 1, false), "#1124");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 1, true), "#1125");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 2, false), "#1126");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 2, true), "#1127");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 3, false), "#1128");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 3, true), "#1129");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 4, false), "#1130");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 4, true), "#1131");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 5, false), "#1132");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 5, true), "#1133");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 6, false), "#1134");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 6, true), "#1135");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 7, false), "#1136");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 7, true), "#1137");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 8, false), "#1138");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 8, true), "#1139");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 9, false), "#1140");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 9, true), "#1141");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 10, false), "#1142");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 10, true), "#1143");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 11, false), "#1144");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 11, true), "#1145");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 12, false), "#1146");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 12, true), "#1147");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 13, false), "#1148");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 13, true), "#1149");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 14, false), "#1150");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 14, true), "#1151");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 15, false), "#1152");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 15, true), "#1153");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 16, false), "#1154");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 16, true), "#1155");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 17, false), "#1156");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 17, true), "#1157");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 18, false), "#1158");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 18, true), "#1159");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 19, false), "#1160");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 19, true), "#1161");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 20, false), "#1162");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (16, 20, true), "#1163");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 0, false), "#1164");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 0, true), "#1165");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 1, false), "#1166");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 1, true), "#1167");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 2, false), "#1168");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 2, true), "#1169");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 3, false), "#1170");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 3, true), "#1171");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 4, false), "#1172");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 4, true), "#1173");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 5, false), "#1174");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 5, true), "#1175");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 6, false), "#1176");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 6, true), "#1177");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 7, false), "#1178");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 7, true), "#1179");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 8, false), "#1180");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 8, true), "#1181");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 9, false), "#1182");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 9, true), "#1183");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 10, false), "#1184");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 10, true), "#1185");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 11, false), "#1186");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 11, true), "#1187");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 12, false), "#1188");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 12, true), "#1189");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 13, false), "#1190");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 13, true), "#1191");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 14, false), "#1192");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 14, true), "#1193");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 15, false), "#1194");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 15, true), "#1195");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 16, false), "#1196");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 16, true), "#1197");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 17, false), "#1198");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 17, true), "#1199");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 18, false), "#1200");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 18, true), "#1201");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 19, false), "#1202");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 19, true), "#1203");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 20, false), "#1204");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (17, 20, true), "#1205");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 0, false), "#1206");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 0, true), "#1207");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 1, false), "#1208");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 1, true), "#1209");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 2, false), "#1210");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 2, true), "#1211");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 3, false), "#1212");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 3, true), "#1213");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 4, false), "#1214");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 4, true), "#1215");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 5, false), "#1216");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 5, true), "#1217");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 6, false), "#1218");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 6, true), "#1219");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 7, false), "#1220");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 7, true), "#1221");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 8, false), "#1222");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 8, true), "#1223");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 9, false), "#1224");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 9, true), "#1225");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 10, false), "#1226");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 10, true), "#1227");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 11, false), "#1228");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 11, true), "#1229");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 12, false), "#1230");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 12, true), "#1231");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 13, false), "#1232");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 13, true), "#1233");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 14, false), "#1234");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 14, true), "#1235");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 15, false), "#1236");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 15, true), "#1237");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 16, false), "#1238");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 16, true), "#1239");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 17, false), "#1240");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 17, true), "#1241");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 18, false), "#1242");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 18, true), "#1243");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 19, false), "#1244");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 19, true), "#1245");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 20, false), "#1246");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (18, 20, true), "#1247");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 0, false), "#1248");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 0, true), "#1249");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 1, false), "#1250");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 1, true), "#1251");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 2, false), "#1252");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 2, true), "#1253");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 3, false), "#1254");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 3, true), "#1255");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 4, false), "#1256");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 4, true), "#1257");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 5, false), "#1258");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 5, true), "#1259");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 6, false), "#1260");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 6, true), "#1261");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 7, false), "#1262");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 7, true), "#1263");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 8, false), "#1264");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 8, true), "#1265");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 9, false), "#1266");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 9, true), "#1267");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 10, false), "#1268");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 10, true), "#1269");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 11, false), "#1270");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 11, true), "#1271");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 12, false), "#1272");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 12, true), "#1273");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 13, false), "#1274");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 13, true), "#1275");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 14, false), "#1276");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 14, true), "#1277");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 15, false), "#1278");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 15, true), "#1279");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 16, false), "#1280");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 16, true), "#1281");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 17, false), "#1282");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 17, true), "#1283");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 18, false), "#1284");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 18, true), "#1285");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 19, false), "#1286");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 19, true), "#1287");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 20, false), "#1288");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (19, 20, true), "#1289");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 0, false), "#1290");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 0, true), "#1291");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 1, false), "#1292");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 1, true), "#1293");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 2, false), "#1294");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 2, true), "#1295");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 3, false), "#1296");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 3, true), "#1297");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 4, false), "#1298");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 4, true), "#1299");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 5, false), "#1300");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 5, true), "#1301");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 6, false), "#1302");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 6, true), "#1303");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 7, false), "#1304");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 7, true), "#1305");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 8, false), "#1306");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 8, true), "#1307");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 9, false), "#1308");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 9, true), "#1309");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 10, false), "#1310");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 10, true), "#1311");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 11, false), "#1312");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 11, true), "#1313");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 12, false), "#1314");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 12, true), "#1315");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 13, false), "#1316");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 13, true), "#1317");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 14, false), "#1318");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 14, true), "#1319");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 15, false), "#1320");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 15, true), "#1321");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 16, false), "#1322");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 16, true), "#1323");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 17, false), "#1324");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 17, true), "#1325");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 18, false), "#1326");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 18, true), "#1327");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 19, false), "#1328");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 19, true), "#1329");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 20, false), "#1330");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (20, 20, true), "#1331");
			MaskedTextProviderTest.AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 1332, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void FindAssignedEditPositionInRangeTest10 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"000-00-0000");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, false), "#3099");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 0, true), "#3100");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#3101");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 1, true), "#3102");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#3103");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 2, true), "#3104");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#3105");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 3, true), "#3106");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, false), "#3107");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 4, true), "#3108");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, false), "#3109");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 5, true), "#3110");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, false), "#3111");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 6, true), "#3112");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, false), "#3113");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 7, true), "#3114");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, false), "#3115");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 8, true), "#3116");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, false), "#3117");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 9, true), "#3118");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, false), "#3119");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 10, true), "#3120");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, false), "#3121");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 11, true), "#3122");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, false), "#3123");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (0, 12, true), "#3124");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#3125");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#3126");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#3127");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#3128");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#3129");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#3130");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#3131");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#3132");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, false), "#3133");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#3134");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, false), "#3135");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 5, true), "#3136");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, false), "#3137");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 6, true), "#3138");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, false), "#3139");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 7, true), "#3140");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, false), "#3141");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 8, true), "#3142");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, false), "#3143");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 9, true), "#3144");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, false), "#3145");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 10, true), "#3146");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, false), "#3147");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 11, true), "#3148");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, false), "#3149");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 12, true), "#3150");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#3151");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#3152");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#3153");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#3154");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#3155");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#3156");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#3157");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#3158");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, false), "#3159");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 4, true), "#3160");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, false), "#3161");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 5, true), "#3162");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, false), "#3163");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 6, true), "#3164");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, false), "#3165");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 7, true), "#3166");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, false), "#3167");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 8, true), "#3168");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, false), "#3169");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 9, true), "#3170");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, false), "#3171");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 10, true), "#3172");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, false), "#3173");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 11, true), "#3174");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, false), "#3175");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 12, true), "#3176");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#3177");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#3178");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#3179");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#3180");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#3181");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#3182");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#3183");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#3184");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, false), "#3185");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 4, true), "#3186");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, false), "#3187");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 5, true), "#3188");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, false), "#3189");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 6, true), "#3190");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, false), "#3191");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 7, true), "#3192");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, false), "#3193");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 8, true), "#3194");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, false), "#3195");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 9, true), "#3196");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, false), "#3197");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 10, true), "#3198");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, false), "#3199");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 11, true), "#3200");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, false), "#3201");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 12, true), "#3202");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#3203");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#3204");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#3205");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#3206");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#3207");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#3208");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#3209");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#3210");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, false), "#3211");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 4, true), "#3212");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, false), "#3213");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 5, true), "#3214");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, false), "#3215");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 6, true), "#3216");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, false), "#3217");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 7, true), "#3218");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, false), "#3219");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 8, true), "#3220");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, false), "#3221");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 9, true), "#3222");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, false), "#3223");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 10, true), "#3224");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, false), "#3225");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 11, true), "#3226");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, false), "#3227");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 12, true), "#3228");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, false), "#3229");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, true), "#3230");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, false), "#3231");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, true), "#3232");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, false), "#3233");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, true), "#3234");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, false), "#3235");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, true), "#3236");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, false), "#3237");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, true), "#3238");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, false), "#3239");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, true), "#3240");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, false), "#3241");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, true), "#3242");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, false), "#3243");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 7, true), "#3244");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, false), "#3245");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 8, true), "#3246");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, false), "#3247");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 9, true), "#3248");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, false), "#3249");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 10, true), "#3250");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, false), "#3251");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 11, true), "#3252");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, false), "#3253");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 12, true), "#3254");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, false), "#3255");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, true), "#3256");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, false), "#3257");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, true), "#3258");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, false), "#3259");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, true), "#3260");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, false), "#3261");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, true), "#3262");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, false), "#3263");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, true), "#3264");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, false), "#3265");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, true), "#3266");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, false), "#3267");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, true), "#3268");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, false), "#3269");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 7, true), "#3270");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, false), "#3271");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 8, true), "#3272");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, false), "#3273");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 9, true), "#3274");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, false), "#3275");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 10, true), "#3276");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, false), "#3277");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 11, true), "#3278");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, false), "#3279");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 12, true), "#3280");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, false), "#3281");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, true), "#3282");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, false), "#3283");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, true), "#3284");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, false), "#3285");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, true), "#3286");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, false), "#3287");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, true), "#3288");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, false), "#3289");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, true), "#3290");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, false), "#3291");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, true), "#3292");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, false), "#3293");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, true), "#3294");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, false), "#3295");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 7, true), "#3296");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, false), "#3297");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 8, true), "#3298");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, false), "#3299");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 9, true), "#3300");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, false), "#3301");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 10, true), "#3302");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, false), "#3303");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 11, true), "#3304");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, false), "#3305");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 12, true), "#3306");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, false), "#3307");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, true), "#3308");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, false), "#3309");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, true), "#3310");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, false), "#3311");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, true), "#3312");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, false), "#3313");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, true), "#3314");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, false), "#3315");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, true), "#3316");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, false), "#3317");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, true), "#3318");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, false), "#3319");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, true), "#3320");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, false), "#3321");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, true), "#3322");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, false), "#3323");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, true), "#3324");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, false), "#3325");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, true), "#3326");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, false), "#3327");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, true), "#3328");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, false), "#3329");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, true), "#3330");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, false), "#3331");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, true), "#3332");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, false), "#3333");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, true), "#3334");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, false), "#3335");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, true), "#3336");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, false), "#3337");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, true), "#3338");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, false), "#3339");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, true), "#3340");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, false), "#3341");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, true), "#3342");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, false), "#3343");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, true), "#3344");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, false), "#3345");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, true), "#3346");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, false), "#3347");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, true), "#3348");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, false), "#3349");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, true), "#3350");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, false), "#3351");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, true), "#3352");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, false), "#3353");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, true), "#3354");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, false), "#3355");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, true), "#3356");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, false), "#3357");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, true), "#3358");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, false), "#3359");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, true), "#3360");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, false), "#3361");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, true), "#3362");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, false), "#3363");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, true), "#3364");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, false), "#3365");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, true), "#3366");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, false), "#3367");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, true), "#3368");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, false), "#3369");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, true), "#3370");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, false), "#3371");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, true), "#3372");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, false), "#3373");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, true), "#3374");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, false), "#3375");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, true), "#3376");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, false), "#3377");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, true), "#3378");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, false), "#3379");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, true), "#3380");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, false), "#3381");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, true), "#3382");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, false), "#3383");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, true), "#3384");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, false), "#3385");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, true), "#3386");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, false), "#3387");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, true), "#3388");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, false), "#3389");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, true), "#3390");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, false), "#3391");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, true), "#3392");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, false), "#3393");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, true), "#3394");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, false), "#3395");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, true), "#3396");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, false), "#3397");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, true), "#3398");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, false), "#3399");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, true), "#3400");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, false), "#3401");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, true), "#3402");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, false), "#3403");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, true), "#3404");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, false), "#3405");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, true), "#3406");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, false), "#3407");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, true), "#3408");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, false), "#3409");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, true), "#3410");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, false), "#3411");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, true), "#3412");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, false), "#3413");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, true), "#3414");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, false), "#3415");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, true), "#3416");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, false), "#3417");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, true), "#3418");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, false), "#3419");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, true), "#3420");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, false), "#3421");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, true), "#3422");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, false), "#3423");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, true), "#3424");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, false), "#3425");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, true), "#3426");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, false), "#3427");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, true), "#3428");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, false), "#3429");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, true), "#3430");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, false), "#3431");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, true), "#3432");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, false), "#3433");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, true), "#3434");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, false), "#3435");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, true), "#3436");
			MaskedTextProviderTest.AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 3437, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 11, @"000-00-0000", false, false, '\x0', '\x5F', true, true, true, @"   -  -", @"   -  -", @"   -  -", @"___-__-____", @"_________", @"   -  -", @"");
		}

		[Test]
		public void FindAssignedEditPositionInRangeTest12 ()
		{
			MaskedTextProvider mtp;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"000-00-0000");
			mtp.Add (@"1");
			mtp.Add (@"2");
			mtp.InsertAt ('\x33', 7);
			mtp.InsertAt ('\x34', 4);
			MaskedTextProviderTest.AssertProperties (mtp, "FindAssignedEditPositionInRangeTest", 4115, true, false, 4, 5, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, 7, 11, @"000-00-0000", false, false, '\x0', '\x5F', true, true, true, @"12 -4 -3", @"12 -4 -3", @"12 -4 -3", @"12_-4_-3___", @"12_4_3___", @"12 -4 -3", @"12 4 3");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 0, false), "#3777");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 0, true), "#3778");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 1, false), "#3779");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 1, true), "#3780");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 2, false), "#3781");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 2, true), "#3782");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (0, 3, false), "#3783");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 3, true), "#3784");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (0, 4, false), "#3785");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 4, true), "#3786");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (0, 5, false), "#3787");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 5, true), "#3788");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (0, 6, false), "#3789");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 6, true), "#3790");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 7, false), "#3791");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 7, true), "#3792");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 8, false), "#3793");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 8, true), "#3794");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 9, false), "#3795");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 9, true), "#3796");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 10, false), "#3797");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 10, true), "#3798");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 11, false), "#3799");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 11, true), "#3800");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (0, 12, false), "#3801");
			Assert.AreEqual (0, mtp.FindAssignedEditPositionInRange (0, 12, true), "#3802");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, false), "#3803");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (1, 0, true), "#3804");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 1, false), "#3805");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 1, true), "#3806");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 2, false), "#3807");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 2, true), "#3808");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 3, false), "#3809");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 3, true), "#3810");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (1, 4, false), "#3811");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 4, true), "#3812");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (1, 5, false), "#3813");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 5, true), "#3814");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (1, 6, false), "#3815");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 6, true), "#3816");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 7, false), "#3817");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 7, true), "#3818");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 8, false), "#3819");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 8, true), "#3820");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 9, false), "#3821");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 9, true), "#3822");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 10, false), "#3823");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 10, true), "#3824");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 11, false), "#3825");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 11, true), "#3826");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (1, 12, false), "#3827");
			Assert.AreEqual (1, mtp.FindAssignedEditPositionInRange (1, 12, true), "#3828");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, false), "#3829");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 0, true), "#3830");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, false), "#3831");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 1, true), "#3832");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, false), "#3833");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 2, true), "#3834");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, false), "#3835");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (2, 3, true), "#3836");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 4, false), "#3837");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 4, true), "#3838");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 5, false), "#3839");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 5, true), "#3840");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 6, false), "#3841");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 6, true), "#3842");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 7, false), "#3843");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 7, true), "#3844");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 8, false), "#3845");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 8, true), "#3846");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 9, false), "#3847");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 9, true), "#3848");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 10, false), "#3849");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 10, true), "#3850");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 11, false), "#3851");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 11, true), "#3852");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (2, 12, false), "#3853");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (2, 12, true), "#3854");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, false), "#3855");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 0, true), "#3856");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, false), "#3857");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 1, true), "#3858");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, false), "#3859");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 2, true), "#3860");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, false), "#3861");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (3, 3, true), "#3862");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 4, false), "#3863");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 4, true), "#3864");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 5, false), "#3865");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 5, true), "#3866");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 6, false), "#3867");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 6, true), "#3868");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 7, false), "#3869");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 7, true), "#3870");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 8, false), "#3871");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 8, true), "#3872");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 9, false), "#3873");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 9, true), "#3874");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 10, false), "#3875");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 10, true), "#3876");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 11, false), "#3877");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 11, true), "#3878");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (3, 12, false), "#3879");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (3, 12, true), "#3880");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, false), "#3881");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 0, true), "#3882");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, false), "#3883");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 1, true), "#3884");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, false), "#3885");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 2, true), "#3886");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, false), "#3887");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (4, 3, true), "#3888");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 4, false), "#3889");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 4, true), "#3890");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 5, false), "#3891");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 5, true), "#3892");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 6, false), "#3893");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 6, true), "#3894");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 7, false), "#3895");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 7, true), "#3896");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 8, false), "#3897");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 8, true), "#3898");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 9, false), "#3899");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 9, true), "#3900");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 10, false), "#3901");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 10, true), "#3902");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 11, false), "#3903");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 11, true), "#3904");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (4, 12, false), "#3905");
			Assert.AreEqual (4, mtp.FindAssignedEditPositionInRange (4, 12, true), "#3906");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, false), "#3907");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 0, true), "#3908");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, false), "#3909");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 1, true), "#3910");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, false), "#3911");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 2, true), "#3912");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, false), "#3913");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 3, true), "#3914");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, false), "#3915");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 4, true), "#3916");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, false), "#3917");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 5, true), "#3918");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, false), "#3919");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (5, 6, true), "#3920");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 7, false), "#3921");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 7, true), "#3922");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 8, false), "#3923");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 8, true), "#3924");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 9, false), "#3925");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 9, true), "#3926");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 10, false), "#3927");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 10, true), "#3928");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 11, false), "#3929");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 11, true), "#3930");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 12, false), "#3931");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (5, 12, true), "#3932");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, false), "#3933");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 0, true), "#3934");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, false), "#3935");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 1, true), "#3936");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, false), "#3937");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 2, true), "#3938");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, false), "#3939");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 3, true), "#3940");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, false), "#3941");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 4, true), "#3942");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, false), "#3943");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 5, true), "#3944");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, false), "#3945");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (6, 6, true), "#3946");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 7, false), "#3947");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 7, true), "#3948");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 8, false), "#3949");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 8, true), "#3950");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 9, false), "#3951");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 9, true), "#3952");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 10, false), "#3953");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 10, true), "#3954");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 11, false), "#3955");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 11, true), "#3956");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 12, false), "#3957");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (6, 12, true), "#3958");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, false), "#3959");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 0, true), "#3960");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, false), "#3961");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 1, true), "#3962");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, false), "#3963");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 2, true), "#3964");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, false), "#3965");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 3, true), "#3966");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, false), "#3967");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 4, true), "#3968");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, false), "#3969");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 5, true), "#3970");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, false), "#3971");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (7, 6, true), "#3972");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 7, false), "#3973");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 7, true), "#3974");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 8, false), "#3975");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 8, true), "#3976");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 9, false), "#3977");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 9, true), "#3978");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 10, false), "#3979");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 10, true), "#3980");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 11, false), "#3981");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 11, true), "#3982");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 12, false), "#3983");
			Assert.AreEqual (7, mtp.FindAssignedEditPositionInRange (7, 12, true), "#3984");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, false), "#3985");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 0, true), "#3986");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, false), "#3987");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 1, true), "#3988");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, false), "#3989");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 2, true), "#3990");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, false), "#3991");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 3, true), "#3992");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, false), "#3993");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 4, true), "#3994");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, false), "#3995");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 5, true), "#3996");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, false), "#3997");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 6, true), "#3998");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, false), "#3999");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 7, true), "#4000");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, false), "#4001");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 8, true), "#4002");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, false), "#4003");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 9, true), "#4004");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, false), "#4005");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 10, true), "#4006");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, false), "#4007");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 11, true), "#4008");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, false), "#4009");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (8, 12, true), "#4010");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, false), "#4011");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 0, true), "#4012");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, false), "#4013");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 1, true), "#4014");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, false), "#4015");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 2, true), "#4016");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, false), "#4017");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 3, true), "#4018");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, false), "#4019");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 4, true), "#4020");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, false), "#4021");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 5, true), "#4022");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, false), "#4023");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 6, true), "#4024");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, false), "#4025");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 7, true), "#4026");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, false), "#4027");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 8, true), "#4028");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, false), "#4029");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 9, true), "#4030");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, false), "#4031");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 10, true), "#4032");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, false), "#4033");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 11, true), "#4034");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, false), "#4035");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (9, 12, true), "#4036");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, false), "#4037");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 0, true), "#4038");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, false), "#4039");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 1, true), "#4040");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, false), "#4041");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 2, true), "#4042");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, false), "#4043");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 3, true), "#4044");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, false), "#4045");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 4, true), "#4046");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, false), "#4047");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 5, true), "#4048");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, false), "#4049");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 6, true), "#4050");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, false), "#4051");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 7, true), "#4052");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, false), "#4053");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 8, true), "#4054");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, false), "#4055");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 9, true), "#4056");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, false), "#4057");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 10, true), "#4058");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, false), "#4059");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 11, true), "#4060");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, false), "#4061");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (10, 12, true), "#4062");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, false), "#4063");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 0, true), "#4064");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, false), "#4065");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 1, true), "#4066");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, false), "#4067");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 2, true), "#4068");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, false), "#4069");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 3, true), "#4070");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, false), "#4071");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 4, true), "#4072");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, false), "#4073");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 5, true), "#4074");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, false), "#4075");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 6, true), "#4076");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, false), "#4077");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 7, true), "#4078");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, false), "#4079");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 8, true), "#4080");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, false), "#4081");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 9, true), "#4082");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, false), "#4083");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 10, true), "#4084");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, false), "#4085");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 11, true), "#4086");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, false), "#4087");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (11, 12, true), "#4088");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, false), "#4089");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 0, true), "#4090");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, false), "#4091");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 1, true), "#4092");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, false), "#4093");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 2, true), "#4094");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, false), "#4095");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 3, true), "#4096");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, false), "#4097");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 4, true), "#4098");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, false), "#4099");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 5, true), "#4100");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, false), "#4101");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 6, true), "#4102");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, false), "#4103");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 7, true), "#4104");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, false), "#4105");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 8, true), "#4106");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, false), "#4107");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 9, true), "#4108");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, false), "#4109");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 10, true), "#4110");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, false), "#4111");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 11, true), "#4112");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, false), "#4113");
			Assert.AreEqual (-1, mtp.FindAssignedEditPositionInRange (12, 12, true), "#4114");
		}
		[Test]
		public void FindEditPositionInRangeTest1 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 0, false), "#0");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 0, true), "#1");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 1, false), "#2");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 1, true), "#3");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 2, false), "#4");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 2, true), "#5");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 3, false), "#6");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 3, true), "#7");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 4, false), "#8");
			Assert.AreEqual (0, mtp.FindEditPositionInRange (0, 4, true), "#9");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 0, false), "#10");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 0, true), "#11");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 1, false), "#12");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 1, true), "#13");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 2, false), "#14");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 2, true), "#15");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 3, false), "#16");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 3, true), "#17");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 4, false), "#18");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (1, 4, true), "#19");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 0, false), "#20");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 0, true), "#21");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 1, false), "#22");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 1, true), "#23");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 2, false), "#24");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 2, true), "#25");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 3, false), "#26");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 3, true), "#27");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 4, false), "#28");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (2, 4, true), "#29");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 0, false), "#30");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 0, true), "#31");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 1, false), "#32");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 1, true), "#33");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 2, false), "#34");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 2, true), "#35");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 3, false), "#36");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 3, true), "#37");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 4, false), "#38");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (3, 4, true), "#39");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 0, false), "#40");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 0, true), "#41");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 1, false), "#42");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 1, true), "#43");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 2, false), "#44");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 2, true), "#45");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 3, false), "#46");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 3, true), "#47");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 4, false), "#48");
			Assert.AreEqual (-1, mtp.FindEditPositionInRange (4, 4, true), "#49");
			MaskedTextProviderTest.AssertProperties (mtp, "FindEditPositionInRangeTest", 50, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void VerifyCharTest01186 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"09#L?&CAa.,:/$<>|\\");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.VerifyChar ('\x2F', 12, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (MaskedTextResultHint.CharacterEscaped, MaskedTextResultHint_out, "#1");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyCharTest", 2, true, false, 0, 9, CultureInfo.GetCultureInfo ("es-ES"), 9, true, false, false, -1, 15, @"09#L?&CAa.,:/$<>|\\", false, false, '\x0', '\x5F', true, true, true, @"         ,.:/€\", @"         ,.:/€\", @"         ,.:/€\", @"_________,.:/€\", @"_________", @"         ,.:/€\", @"");
		}
		[Test]
		public void VerifyString_string_int_MaskedTextResultHintTest00007 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.VerifyString (@"a", out Int32_out, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (MaskedTextResultHint.NoEffect, MaskedTextResultHint_out, "#1");
			Assert.AreEqual (0, Int32_out, "#2");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyString_string_int_MaskedTextResultHintTest", 3, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void VerifyString_string_int_MaskedTextResultHintTest00010 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.VerifyString (@"a longer string value", out Int32_out, out MaskedTextResultHint_out), "#12");
			Assert.AreEqual (MaskedTextResultHint.UnavailableEditPosition, MaskedTextResultHint_out, "#13");
			Assert.AreEqual (3, Int32_out, "#14");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyString_string_int_MaskedTextResultHintTest", 15, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void VerifyCharTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.VerifyChar ('\x0', -1, out MaskedTextResultHint_out), "#0");
			Assert.AreEqual (MaskedTextResultHint.PositionOutOfRange, MaskedTextResultHint_out, "#1");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyCharTest", 2, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void VerifyCharTest00064 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (true, mtp.VerifyChar ('\x20', 0, out MaskedTextResultHint_out), "#138");
			Assert.AreEqual (MaskedTextResultHint.SideEffect, MaskedTextResultHint_out, "#139");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyCharTest", 140, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void VerifyEscapeCharTest00001 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.VerifyEscapeChar ('\x0', -1), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyEscapeCharTest", 1, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}

		[Test]
		public void VerifyEscapeCharTest00067 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (false, mtp.VerifyEscapeChar ('\x20', 1), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "VerifyEscapeCharTest", 1, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}

		[Test]
		public void ToDisplayString ()
		{
			MaskedTextProvider mtp;

			mtp = new MaskedTextProvider ("##-##");
			mtp.PasswordChar = '*';
			Assert.AreEqual ("__-__", mtp.ToDisplayString ());

			mtp.Add ("666");
			Assert.AreEqual ("**-*_", mtp.ToDisplayString ());
		}

		// Test the interaction between includePasswordChar and includePromptChar
		// in a password mask
		[Test]
		public void ToString_PasswordTest ()
		{
			MaskedTextProvider mtp;

			mtp = new MaskedTextProvider ("####");
			mtp.PasswordChar = '*';
			Assert.AreEqual ("____", mtp.ToString (true, true, true, 0, mtp.Length), "#A1");
			Assert.AreEqual ("____", mtp.ToString (false, true, true, 0, mtp.Length), "#A2");

			mtp.Add ("314");
			Assert.AreEqual ("314_", mtp.ToString (true, true, true, 0, mtp.Length), "#B1");
			Assert.AreEqual ("***_", mtp.ToString (false, true, true, 0, mtp.Length), "#B2");

			mtp.Clear ();

			mtp.InsertAt ("666", 1);
			Assert.AreEqual ("_666", mtp.ToString (true, true, true, 0, mtp.Length), "#C1");
			Assert.AreEqual ("_***", mtp.ToString (false, true, true, 0, mtp.Length), "#C2");
		}

		[Test]
		public void ToString_False_FalseTest ()
		{
			MaskedTextProvider mtp;

			mtp = new MaskedTextProvider ("a?a");
			Assert.AreEqual ("", mtp.ToString (false, false), "#01");
			mtp.InsertAt ('a', 1);
			Assert.AreEqual (" a", mtp.ToString (false, false), "#02");

			mtp = new MaskedTextProvider ("a?a");
			Assert.AreEqual ("", mtp.ToString (false, false), "#03");
			mtp.InsertAt ('a', 0);
			Assert.AreEqual ("a", mtp.ToString (false, false), "#04");

			mtp = new MaskedTextProvider ("a?a?a");
			Assert.AreEqual ("", mtp.ToString (false, false), "#05");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#06");

			mtp = new MaskedTextProvider ("????a");
			Assert.AreEqual ("", mtp.ToString (false, false), "#07");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#08");

			mtp = new MaskedTextProvider ("LLLLa");
			Assert.AreEqual ("", mtp.ToString (false, false), "#09");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#10");

			mtp = new MaskedTextProvider ("CCCCa");
			Assert.AreEqual ("", mtp.ToString (false, false), "#11");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#12");

			mtp = new MaskedTextProvider ("aaaaa");
			Assert.AreEqual ("", mtp.ToString (false, false), "#13");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#14");

			mtp = new MaskedTextProvider ("aaaaaaaaaaaaaaaaaaaaaa");
			Assert.AreEqual ("", mtp.ToString (false, false), "#15");
			mtp.InsertAt ('a', 3);
			Assert.AreEqual ("   a", mtp.ToString (false, false), "#16");
			mtp.InsertAt ('a', 9);
			Assert.AreEqual ("   a     a", mtp.ToString (false, false), "#17");

			mtp = new MaskedTextProvider ("aaa");
			mtp.PasswordChar = '*';
			mtp.InsertAt ('a', 2);
			Assert.AreEqual ("  a", mtp.ToString (false, false), "#18");
		}
		[Test]
		public void ToString_bool_bool_bool_int_int_Test00043 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (@"bc", mtp.ToString (true, true, true, 1, 3), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "ToString_bool_bool_bool_int_int_Test", 1, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void ToString_bool_bool_bool_int_int_Test00055 ()
		{
			MaskedTextProvider mtp;
			int Int32_out = 0;
			MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Add ('\x61');
			mtp.Add ('\x61', out Int32_out, out MaskedTextResultHint_out);
			Assert.AreEqual (@"c", mtp.ToString (true, true, true, 2, 2), "#6");
			MaskedTextProviderTest.AssertProperties (mtp, "ToString_bool_bool_bool_int_int_Test", 7, true, false, 1, 0, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, 0, 3, @"abc", true, true, '\x0', '\x5F', true, true, true, @"abc", @"abc", @"abc", @"abc", @"a", @"abc", @"a");
		}
		[Test]
		public void ToString_bool_bool_int_int_Test00008 ()
		{
			MaskedTextProvider mtp;
			//int Int32_out = 0;
			//MaskedTextResultHint MaskedTextResultHint_out = MaskedTextResultHint.Unknown;
			mtp = new MaskedTextProvider (@"abc");
			mtp.Add (@"a");
			mtp.Remove ();
			mtp.InsertAt ('\x61', 1);
			Assert.AreEqual (@"_", mtp.ToString (true, true, -1, 1), "#0");
			MaskedTextProviderTest.AssertProperties (mtp, "ToString_bool_bool_int_int_Test", 1, true, false, 0, 1, CultureInfo.GetCultureInfo ("es-ES"), 1, true, false, false, -1, 3, @"abc", true, false, '\x0', '\x5F', true, true, true, @" bc", @" bc", @" bc", @"_bc", @"_", @" bc", @"");
		}
		
		
		
		public static string join (IEnumerator e, string sep)
		{
			StringBuilder str = new StringBuilder ();
			while (e.MoveNext ()) {
				if (str.Length > 0)
					str.Append (sep);
				str.Append (e.Current.ToString ());
			}
			return str.ToString ();
		}

		/* START */
		public static void AssertProperties (MaskedTextProvider mtp, string test_name, int counter, bool allow_prompt, bool ascii_only, int assigned_edit_position_count, int available_edit_position_count,
				CultureInfo culture, int edit_position_count, bool include_literals, bool include_prompt, bool is_password, int last_assigned_position,
				int length, string mask, bool mask_completed, bool mask_full, char password_char, char prompt_char, bool reset_on_prompt, bool reset_on_space, bool skip_literals,
				string tostring, string tostring_true, string tostring_false, string tostring_true_true, string tostring_true_false, string tostring_false_true, string tostring_false_false)
		{
			// Testing all properties...
			//return;
			int i = 1;
			ArrayList asserts = new ArrayList ();
			try {
				Assert.AreEqual (allow_prompt, mtp.AllowPromptAsInput, string.Format ("{0}-#{1} (AllowPromptAsInput)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (ascii_only, mtp.AsciiOnly, string.Format ("{0}-#{1} (AsciiOnly)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (assigned_edit_position_count, mtp.AssignedEditPositionCount, string.Format ("{0}-#{1} (AssignedEditPositionCount)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (available_edit_position_count, mtp.AvailableEditPositionCount, string.Format ("{0}-#{1} (AvailableEditPositionCount)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				//Assert.AreEqual (culture, mtp.Culture, string.Format ("{0}-#{1} (Culture)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (edit_position_count, mtp.EditPositionCount, string.Format ("{0}-#{1} (EditPositionCount)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				//Assert.AreEqual ({0}, mtp.EditPositions,string.Format( "{0}-#{1} (EditPositions)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (include_literals, mtp.IncludeLiterals, string.Format ("{0}-#{1} (IncludeLiterals)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (include_prompt, mtp.IncludePrompt, string.Format ("{0}-#{1} (IncludePrompt)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (is_password, mtp.IsPassword, string.Format ("{0}-#{1} (IsPassword)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (last_assigned_position, mtp.LastAssignedPosition, string.Format ("{0}-#{1} (LastAssignedPosition)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (length, mtp.Length, string.Format ("{0}-#{1} (Length)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (mask, mtp.Mask, string.Format ("{0}-#{1} (Mask)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (mask_completed, mtp.MaskCompleted, string.Format ("{0}-#{1} (MaskCompleted)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (mask_full, mtp.MaskFull, string.Format ("{0}-#{1} (MaskFull)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (password_char, mtp.PasswordChar, string.Format ("{0}-#{1} (PasswordChar)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (prompt_char, mtp.PromptChar, string.Format ("{0}-#{1} (PromptChar)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (reset_on_prompt, mtp.ResetOnPrompt, string.Format ("{0}-#{1} (ResetOnPrompt)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (reset_on_space, mtp.ResetOnSpace, string.Format ("{0}-#{1} (ResetOnSpace)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (skip_literals, mtp.SkipLiterals, string.Format ("{0}-#{1} (SkipLiterals)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring, mtp.ToString (), string.Format ("{0}-#{1} (tostring)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_true, mtp.ToString (true), string.Format ("{0}-#{1} (tostring_true)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_false, mtp.ToString (false), string.Format ("{0}-#{1} (tostring_false)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_true_true, mtp.ToString (true, true), string.Format ("{0}-#{1} (tostring_true_true)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_true_false, mtp.ToString (true, false), string.Format ("{0}-#{1} (tostring_true_false)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_false_true, mtp.ToString (false, true), string.Format ("{0}-#{1} (tostring_false_true)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			try {
				Assert.AreEqual (tostring_false_false, mtp.ToString (false, false), string.Format ("{0}-#{1} (tostring_false_false)", test_name + counter.ToString (), (i++).ToString ()));
			} catch (AssertionException ex) {
				asserts.Add (ex);
			}
			
			if (asserts.Count > 0) {
				string msg = "";
				foreach (AssertionException ex in asserts) {
					msg += ex.Message + Environment.NewLine;
				}
				throw new AssertionException (msg);
			}

		}
		/* END */
	}
}
#endif
