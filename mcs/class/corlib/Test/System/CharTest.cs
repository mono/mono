// CharTest.cs - NUnit Test Cases for the System.Char struct
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{
[TestFixture]
public class CharTest
{
	[Test]
	public void TestCompareTo()
	{
		Char c1 = 'a';
		Char c2 = 'b';
		Char c3 = 'b';
		Assert.IsTrue(c1.CompareTo(c2) == -1, "Less than");
		Assert.IsTrue(c2.CompareTo(c1) == 1, "Greater than");
		Assert.IsTrue(c2.CompareTo(c3) == 0, "Equal 1");
		Assert.IsTrue(c1.CompareTo(c1) == 0, "Equal 2");
	}

	[Test]	
	public void TestEquals()
	{
		Char c1 = 'a';
		Char c2 = 'b';
		Char c3 = 'b';
		Assert.IsTrue(c1.Equals(c1), "Same");
		Assert.IsTrue(c2.Equals(c3), "Same value");
		Assert.IsTrue(!c1.Equals(c2), "Not same");
	}

	[Test]
	public void TestGetHashValue()
	{
		Char c1 = ' ';
		Assert.AreEqual(c1.GetHashCode(), c1.GetHashCode(), "deterministic hash code ");
		// TODO - the spec doesn't say what algorithm is used to get hash codes.  So far, just a weak test for determinism and mostly-uniqueness.
	}

	[Test]
	public void TestGetNumericValue()
	{
		Char c1 = ' ';
		Char c2 = '3';
		Assert.AreEqual(-1.0, Char.GetNumericValue(c1), 0.1, "code 1");
		Assert.AreEqual(3.0, Char.GetNumericValue(c2), 0.1, "code 2");

		string s1 = " 3 ";
		Assert.AreEqual(-1.0, Char.GetNumericValue(s1, 0), 0.1, "space not number");
		Assert.AreEqual(3.0, Char.GetNumericValue(s1, 1), 0.1, "space not number");
		Assert.AreEqual(-1.0, Char.GetNumericValue(s1, 2), 0.1, "space not number");
	}

	[Test]
	public void TestGetUnicodeCategory()
	{
		{
			char Pe1 = ']';
			char Pe2 = '}';
			char Pe3 = ')';
			Assert.AreEqual(UnicodeCategory.ClosePunctuation, 
							Char.GetUnicodeCategory(Pe1),
							"Close Punctuation");
			Assert.AreEqual(UnicodeCategory.ClosePunctuation, 
							Char.GetUnicodeCategory(Pe2),
							"Close Punctuation");
			Assert.AreEqual(UnicodeCategory.ClosePunctuation, 
							Char.GetUnicodeCategory(Pe3),
							"Close Punctuation");
		}
		// TODO - ConnectorPunctuation
		{
			char c1 = (char)0; // 0000-001F, 007F-009F
			char c2 = (char)0x001F;
			char c3 = (char)0x007F;
			char c4 = (char)0x00F;
			Assert.AreEqual(UnicodeCategory.Control, 
							Char.GetUnicodeCategory(c1),
							"Control");
			Assert.AreEqual(UnicodeCategory.Control, 
							Char.GetUnicodeCategory(c2),
							"Control");
			Assert.AreEqual(UnicodeCategory.Control,
							Char.GetUnicodeCategory(c3),
							"Control");
			Assert.AreEqual(UnicodeCategory.Control,
							Char.GetUnicodeCategory(c4),
							"Control");
		}
		{
			// TODO - more currencies?
			char c1 = '$';
			Assert.AreEqual(
				     UnicodeCategory.CurrencySymbol,
				     Char.GetUnicodeCategory(c1),
					 "Currency");
		}
		{
			char c1 = '-';
			Assert.AreEqual(UnicodeCategory.DashPunctuation,
							Char.GetUnicodeCategory(c1),
							"Dash Punctuation");
		}
		{
			char c1 = '2';
			char c2 = '7';
			Assert.AreEqual(UnicodeCategory.DecimalDigitNumber,
							Char.GetUnicodeCategory(c1),
							"Decimal Digit");
			Assert.AreEqual(UnicodeCategory.DecimalDigitNumber,
							Char.GetUnicodeCategory(c2),
							"Decimal Digit");
		}
		// TODO - EnclosingMark
		// TODO - FinalQuotePunctuation
		// TODO - Format
		// TODO - InitialQuotePunctuation
		// TODO - LetterNumber
		// TODO - LineSeparator (not '\n', that's a control char)
		{
			char c1 = 'a';
			char c2 = 'z';
			Assert.AreEqual(UnicodeCategory.LowercaseLetter,
							Char.GetUnicodeCategory(c1),
							"LowercaseLetter");
			Assert.AreEqual(UnicodeCategory.LowercaseLetter, 
							Char.GetUnicodeCategory(c2),
							"LowercaseLetter");
		}
		{
			char c1 = '+';
			char c2 = '=';
			Assert.AreEqual(UnicodeCategory.MathSymbol,
							Char.GetUnicodeCategory(c1),
							"MathSymbol");
			Assert.AreEqual(UnicodeCategory.MathSymbol,
							Char.GetUnicodeCategory(c2),
							"MathSymbol");
		}
		// TODO - ModifierSymbol
		// TODO - NonSpacingMark
		// TODO - OpenPunctuation
		{
			char c1 = '[';
			char c2 = '{';
			char c3 = '(';
			Assert.AreEqual(UnicodeCategory.OpenPunctuation,
							Char.GetUnicodeCategory(c1),
							"OpenPunctuation");
			Assert.AreEqual(UnicodeCategory.OpenPunctuation, 
							Char.GetUnicodeCategory(c2),
							"OpenPunctuation");
			Assert.AreEqual(UnicodeCategory.OpenPunctuation,
							Char.GetUnicodeCategory(c3),
							"OpenPunctuation");
		}
		// TODO - OtherLetter
		// TODO - OtherNotAssigned
		// TODO - OtherNumber
		{
			char c1 = '/';
			Assert.AreEqual(UnicodeCategory.OtherPunctuation,
							Char.GetUnicodeCategory(c1),
							"OtherPunctuation");
		}
		// TODO - OtherSymbol
		// TODO - ParagraphSeparator
		// TODO - PrivateUse
		{
			char c1 = ' ';
			Assert.AreEqual(UnicodeCategory.SpaceSeparator,
							Char.GetUnicodeCategory(c1),
							"SpaceSeparator");
		}
		// TODO - SpacingCombiningMark
		{
			char c1 = (char)0xD800; // D800-DBFF
			char c2 = (char)0xDBFF; // D800-DBFF
			char c3 = (char)0xDC01; // DC00-DEFF
			char c4 = (char)0xDEFF; // DC00-DEFF
			Assert.AreEqual(UnicodeCategory.Surrogate,
							Char.GetUnicodeCategory(c1),
							"High Surrogate");
			Assert.AreEqual(UnicodeCategory.Surrogate,
							Char.GetUnicodeCategory(c2),
							"High Surrogate");
			Assert.AreEqual(UnicodeCategory.Surrogate,
							Char.GetUnicodeCategory(c3),
							"Low Surrogate");
			Assert.AreEqual(UnicodeCategory.Surrogate,
							Char.GetUnicodeCategory(c4),
							"Low Surrogate");
		}
		// TODO - TitlecaseLetter
		// TODO - UppercaseLetter
		{
			char c1 = 'A';
			char c2 = 'Z';
			Assert.AreEqual(UnicodeCategory.UppercaseLetter,
							Char.GetUnicodeCategory(c1),
							"UppercaseLetter");
			Assert.AreEqual(UnicodeCategory.UppercaseLetter,
							Char.GetUnicodeCategory(c2),
							"UppercaseLetter");
		}
	}

	[Test]
	public void TestGetUnicodeCategoryStringIndex ()
	{
		Assert.AreEqual (Char.GetUnicodeCategory ("\uD800\uDF80", 0), UnicodeCategory.OtherLetter, "#C01");
		Assert.AreEqual (Char.GetUnicodeCategory ("\uD800\uDF80", 1), UnicodeCategory.Surrogate, "#C02");
		Assert.AreEqual (Char.GetUnicodeCategory ("\uD800", 0), UnicodeCategory.Surrogate, "#C03");
		Assert.AreEqual (Char.GetUnicodeCategory ("\uD800!", 0), UnicodeCategory.Surrogate, "#C04");
		Assert.AreEqual (Char.GetUnicodeCategory ("!", 0), UnicodeCategory.OtherPunctuation, "#C05");
		Assert.AreEqual (Char.GetUnicodeCategory ("!\uD800", 1), UnicodeCategory.Surrogate, "#C06");
		Assert.AreEqual (Char.GetUnicodeCategory ("!\uD800\uDF80", 1), UnicodeCategory.OtherLetter, "#C07");
	}

	[Test]
	public void TestGetUnicodeCategoryAstralPlanes ()
	{
		const int up_to = 0x10ffff;
		// const int increment = 1;
		const int increment = 0x1000 + 17;

		for (int codepoint = 0x10000; codepoint < up_to; codepoint += increment) {
			string combined = Char.ConvertFromUtf32 (codepoint);

			Assert.AreEqual (combined.Length, 2, "#D01");
			Assert.AreEqual (Char.GetUnicodeCategory (combined [0]), UnicodeCategory.Surrogate, "#D02");
			Assert.AreEqual (Char.GetUnicodeCategory (combined [1]), UnicodeCategory.Surrogate, "#D03");
			Assert.AreNotEqual (Char.GetUnicodeCategory (combined, 0), UnicodeCategory.Surrogate, "#D04");
		}
	}

	[Test]
	public void TestIsControl()
	{
		// control is 0000-001F, 007F-009F
		char c1 = (char)0;
		char c2 = (char)0x001F;
		char c3 = (char)0x007F;
		char c4 = (char)0x009F;
		Assert.IsTrue(!Char.IsControl(' '), "Not control");
		Assert.IsTrue(Char.IsControl(c1), "control");
		Assert.IsTrue(Char.IsControl(c2), "control");
		Assert.IsTrue(Char.IsControl(c3), "control");
		Assert.IsTrue(Char.IsControl(c4), "control");

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert.IsTrue(!Char.IsControl(s1, 0), "Not control");
		Assert.IsTrue(Char.IsControl(s1, 1), "control");
		Assert.IsTrue(Char.IsControl(s1, 2), "control");
		Assert.IsTrue(Char.IsControl(s1, 3), "control");
		Assert.IsTrue(Char.IsControl(s1, 4), "control");
	}

	[Test]
	public void TestIsDigit()
	{
		char c1 = '0';
		char c2 = '9';
		Assert.IsTrue(!Char.IsDigit(' '), "Not digit");
		Assert.IsTrue(Char.IsDigit(c1), "digit");
		Assert.IsTrue(Char.IsDigit(c2), "digit");

		string s1 = " " + c1 + c2;
		Assert.IsTrue(!Char.IsDigit(s1, 0), "Not digit");
		Assert.IsTrue(Char.IsDigit(s1, 1), "digit");
		Assert.IsTrue(Char.IsDigit(s1, 2), "digit");
	}

	[Test]
	public void TestIsLetter()
	{
		char c1 = 'a';
		char c2 = 'z';
		char c3 = 'A';
		char c4 = 'Z';
		Assert.IsTrue(!Char.IsLetter(' '), "Not letter");
		Assert.IsTrue(Char.IsLetter(c1), "letter");
		Assert.IsTrue(Char.IsLetter(c2), "letter");
		Assert.IsTrue(Char.IsLetter(c3), "letter");
		Assert.IsTrue(Char.IsLetter(c4), "letter");

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert.IsTrue(!Char.IsLetter(s1, 0), "Not letter");
		Assert.IsTrue(Char.IsLetter(s1, 1), "letter");
		Assert.IsTrue(Char.IsLetter(s1, 2), "letter");
		Assert.IsTrue(Char.IsLetter(s1, 3), "letter");
		Assert.IsTrue(Char.IsLetter(s1, 4), "letter");
	}

	[Test]
	public void TestIsLetterOrDigit()
	{
		char c1 = 'a';
		char c2 = 'z';
		char c3 = 'A';
		char c4 = 'Z';
		char c5 = '0';
		char c6 = '9';
		Assert.IsTrue(!Char.IsLetterOrDigit(' '), "Not letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c1), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c2), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c3), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c4), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c5), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(c6), "letterordigit");

		string s1 = " " + c1 + c2 + c3 + c4 + c5 + c6;
		Assert.IsTrue(!Char.IsLetterOrDigit(s1, 0), "Not letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 1), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 2), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 3), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 4), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 5), "letterordigit");
		Assert.IsTrue(Char.IsLetterOrDigit(s1, 6), "letterordigit");
	}

	[Test]
	public void TestIsLower()
	{
		char c1 = 'a';
		char c2 = 'z';
		Assert.IsTrue(!Char.IsLower(' '), "Not lower");
		Assert.IsTrue(Char.IsLower(c1), "lower");
		Assert.IsTrue(Char.IsLower(c2), "lower");

		string s1 = " " + c1 + c2;
		Assert.IsTrue(!Char.IsLower(s1, 0), "Not lower");
		Assert.IsTrue(Char.IsLower(s1, 1), "lower");
		Assert.IsTrue(Char.IsLower(s1, 2), "lower");
	}

	[Test]
	public void TestIsNumber()
	{
		char c1 = '0';
		char c2 = '9';
		// TODO - IsNumber of less obvious characters

		Assert.IsTrue(!Char.IsNumber(' '), "Not number");
		Assert.IsTrue(Char.IsNumber(c1), "number");
		Assert.IsTrue(Char.IsNumber(c2), "number");

		string s1 = " " + c1 + c2;
		Assert.IsTrue(!Char.IsNumber(s1, 0), "Not number");
		Assert.IsTrue(Char.IsNumber(s1, 1), "number");
		Assert.IsTrue(Char.IsNumber(s1, 2), "number");
	}

	[Test]
	public void TestIsPunctuation()
	{
		char c1 = '.';
		char c2 = '?';
		Assert.IsTrue(!Char.IsPunctuation(' '), "Not punctuation");
		Assert.IsTrue(Char.IsPunctuation(c1), "punctuation");
		Assert.IsTrue(Char.IsPunctuation(c2), "punctuation");

		string s1 = " " + c1 + c2;
		Assert.IsTrue(!Char.IsPunctuation(s1, 0), "Not punctuation");
		Assert.IsTrue(Char.IsPunctuation(s1, 1), "punctuation");
		Assert.IsTrue(Char.IsPunctuation(s1, 2), "punctuation");
	}

	[Test]
	public void TestIsSeparator()
	{
		char c1 = ' ';

		Assert.IsTrue(!Char.IsSeparator('.'), "Not separator");
		Assert.IsTrue(Char.IsSeparator(c1), "separator1");

		string s1 = "." + c1;
		Assert.IsTrue(!Char.IsSeparator(s1, 0), "Not separator");
		Assert.IsTrue(Char.IsSeparator(s1, 1), "separator1-2");
	}

	[Test]
	public void TestIsSurrogate()
	{
		// high surrogate - D800-DBFF
		// low surrogate - DC00-DEFF
		char c1 = (char)0xD800;
		char c2 = (char)0xDBFF;
		char c3 = (char)0xDC00;
		char c4 = (char)0xDEFF;
		Assert.IsTrue(!Char.IsSurrogate(' '), "Not surrogate");
		Assert.IsTrue(Char.IsSurrogate(c1), "surrogate1");
		Assert.IsTrue(Char.IsSurrogate(c2), "surrogate2");
		Assert.IsTrue(Char.IsSurrogate(c3), "surrogate3");
		Assert.IsTrue(Char.IsSurrogate(c4), "surrogate4");

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert.IsTrue(!Char.IsSurrogate(s1, 0), "Not surrogate");
		Assert.IsTrue(Char.IsSurrogate(s1, 1), "surrogate1-2");
		Assert.IsTrue(Char.IsSurrogate(s1, 2), "surrogate2-2");
		Assert.IsTrue(Char.IsSurrogate(s1, 3), "surrogate3-2");
		Assert.IsTrue(Char.IsSurrogate(s1, 4), "surrogate4-2");
	}

	[Test]
	public void TestIsSymbol()
	{
		char c1 = '+';
		char c2 = '=';
		Assert.IsTrue(!Char.IsSymbol(' '), "Not symbol");
		Assert.IsTrue(Char.IsSymbol(c1), "symbol");
		Assert.IsTrue(Char.IsSymbol(c2), "symbol");

		string s1 = " " + c1 + c2;
		Assert.IsTrue(!Char.IsSymbol(s1, 0), "Not symbol");
		Assert.IsTrue(Char.IsSymbol(s1, 1), "symbol");
		Assert.IsTrue(Char.IsSymbol(s1, 2), "symbol");
	}

	[Test]
	public void TestIsUpper()
	{
		char c1 = 'A';
		char c2 = 'Z';
		Assert.IsTrue(!Char.IsUpper('a'), "Not upper");
		Assert.IsTrue(Char.IsUpper(c1), "upper");
		Assert.IsTrue(Char.IsUpper(c2), "upper");

		string s1 = "a" + c1 + c2;
		Assert.IsTrue(!Char.IsUpper(s1, 0), "Not upper");
		Assert.IsTrue(Char.IsUpper(s1, 1), "upper");
		Assert.IsTrue(Char.IsUpper(s1, 2), "upper");
	}

	[Test]
	public void TestIsWhiteSpace()
	{
		char c1 = ' ';
		char c2 = '\n';
		char c3 = '\t';

		Assert.IsTrue(!Char.IsWhiteSpace('.'), "Not whitespace");
		Assert.IsTrue(Char.IsWhiteSpace(c1), "whitespace1");
		Assert.IsTrue(Char.IsWhiteSpace(c2), "whitespace2");
		Assert.IsTrue(Char.IsWhiteSpace(c3), "whitespace3");

		string s1 = "." + c1 + c2 + c3;
		Assert.IsTrue(!Char.IsWhiteSpace(s1, 0), "Not whitespace");
		Assert.IsTrue(Char.IsWhiteSpace(s1, 1), "whitespace1-2");
		Assert.IsTrue(Char.IsWhiteSpace(s1, 2), "whitespace2-2");
		Assert.IsTrue(Char.IsWhiteSpace(s1, 3), "whitespace3-2");

		for (int i = 0; i < ushort.MaxValue; ++i ) {
			switch (i) {
			case '\x9':
			case '\xa':
			case '\xb':
			case '\xc':
			case '\xd':
			case '\x20':
			case '\x85':
			case '\xa0':
			case '\x1680':
			case '\x180e':
			case '\x2000':
			case '\x2001':
			case '\x2002':
			case '\x2003':
			case '\x2004':
			case '\x2005':
			case '\x2006':
			case '\x2007':
			case '\x2008':
			case '\x2009':
			case '\x200a':
			case '\x2028':
			case '\x2029':
			case '\x202f':
			case '\x205f':
			case '\x3000':
				Assert.IsTrue (char.IsWhiteSpace ((char)i), i.ToString ());
				break;
			default:
				Assert.IsFalse (char.IsWhiteSpace ((char)i), i.ToString ());
				break;
			}
		}
	}

	[Test]
	public void TestParse()
	{
		char c1 = 'a';
		string s1 = "a";
		Assert.IsTrue(c1.Equals(Char.Parse(s1)));
	}	

	[Test]
	public void TestTryParseValid ()
	{
		char c1 = 'a';
		string s1 = "a";
		char c2;

		Assert.AreEqual (true, Char.TryParse (s1, out c2), "TryParse1");
		Assert.AreEqual (c2, c1, "TryParse2");
	}

	[Test]
	public void TestTryParseInvalid ()
	{
		string s = "abc";
		char c;
		Assert.AreEqual (false, Char.TryParse (s, out c), "TryParse3");
		Assert.AreEqual ('\0', c, "TryParse4");
	}

	[Test]	
	public void TestToLower()
	{
		char a1 = 'a';
		char a2 = 'A';
		char a3 = 'z';
		char a4 = 'Z';
		char a5 = ' ';
		char a6 = '+';
		char b1 = 'a';
		char b2 = 'a';
		char b3 = 'z';
		char b4 = 'z';
		char b5 = ' ';
		char b6 = '+';
		Assert.AreEqual(b1, Char.ToLower(a1), "char lowered");
		Assert.AreEqual(b2, Char.ToLower(a2), "char lowered");
		Assert.AreEqual(b3, Char.ToLower(a3), "char lowered");
		Assert.AreEqual(b4, Char.ToLower(a4), "char lowered");
		Assert.AreEqual(b5, Char.ToLower(a5), "char lowered");
		Assert.AreEqual(b6, Char.ToLower(a6), "char lowered");
	}

	[Test]
	public void TestToUpper()
	{
		char a1 = 'a';
		char a2 = 'A';
		char a3 = 'z';
		char a4 = 'Z';
		char a5 = ' ';
		char a6 = '+';
		char b1 = 'A';
		char b2 = 'A';
		char b3 = 'Z';
		char b4 = 'Z';
		char b5 = ' ';
		char b6 = '+';
		Assert.AreEqual(b1, Char.ToUpper(a1), "char uppered");
		Assert.AreEqual(b2, Char.ToUpper(a2), "char uppered");
		Assert.AreEqual(b3, Char.ToUpper(a3), "char uppered");
		Assert.AreEqual(b4, Char.ToUpper(a4), "char uppered");
		Assert.AreEqual(b5, Char.ToUpper(a5), "char uppered");
		Assert.AreEqual(b6, Char.ToUpper(a6), "char uppered");
	}

	[Test]
	public void TestToString()
	{
		char c1 = 'a';
		string s1 = "a";
		Assert.IsTrue(s1.Equals(c1.ToString()));
	}

	[Test]
	public void TestGetTypeCode()
	{
		char c1 = 'a';
		Assert.IsTrue(c1.GetTypeCode().Equals(TypeCode.Char));
	}

	[Test]
	public void TestConvertFromUtf32 ()
	{
		Assert.AreEqual ("A", Char.ConvertFromUtf32 (0x41), "#1");
		Assert.AreEqual ("\uD800\uDC00", Char.ConvertFromUtf32 (0x10000), "#2");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TestConvertFromUtf32Fail1 ()
	{
		Char.ConvertFromUtf32 (-1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TestConvertFromUtf32Fail2 ()
	{
		Char.ConvertFromUtf32 (0x110001);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TestConvertFromUtf32Fail3 ()
	{
		Char.ConvertFromUtf32 (0xD800);
	}

	[Test]
	public void TestConvertToUtf32 ()
	{
		Assert.AreEqual (0x10000, Char.ConvertToUtf32 ('\uD800', '\uDC00'), "#1");
		Assert.AreEqual (0x10FFFF, Char.ConvertToUtf32 ('\uDBFF', '\uDFFF'), "#2");
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TestConvertToUtf32Fail1 ()
	{
		Char.ConvertToUtf32 ('A', '\uDC00');
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void TestConvertUtf32Fail2 ()
	{
		Char.ConvertToUtf32 ('\uD800', '\uD800');
	}
}

}
