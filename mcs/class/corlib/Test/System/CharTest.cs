// CharTest.cs - NUnit Test Cases for the System.Char struct
//
// David Brandt (bucky@keystreams.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class CharTest : TestCase
{
	public CharTest() {}

	protected override void SetUp() 
	{
	}

	protected override void TearDown() 
	{
	}

	public void TestCompareTo()
	{
		Char c1 = 'a';
		Char c2 = 'b';
		Char c3 = 'b';
		Assert("Less than", c1.CompareTo(c2) == -1);
		Assert("Greater than", c2.CompareTo(c1) == 1);
		Assert("Equal 1", c2.CompareTo(c3) == 0);
		Assert("Equal 2", c1.CompareTo(c1) == 0);
	}
	
	public void TestEquals()
	{
		Char c1 = 'a';
		Char c2 = 'b';
		Char c3 = 'b';
		Assert("Same", c1.Equals(c1));
		Assert("Same value", c2.Equals(c3));
		Assert("Not same", !c1.Equals(c2));
	}

	public void TestGetHashValue()
	{
		Char c1 = ' ';
		AssertEquals("deterministic hash code ", c1.GetHashCode(), c1.GetHashCode());
		// TODO - the spec doesn't say what algorithm is used to get hash codes.  So far, just a weak test for determinism and mostly-uniqueness.
	}

	public void TestGetNumericValue()
	{
		Char c1 = ' ';
		Char c2 = '3';
		AssertEquals("code 1", -1.0, Char.GetNumericValue(c1), 0.1);
		AssertEquals("code 2", 3.0, Char.GetNumericValue(c2), 0.1);

		string s1 = " 3 ";
		AssertEquals("space not number", -1.0, Char.GetNumericValue(s1, 0), 0.1);
		AssertEquals("space not number", 3.0, Char.GetNumericValue(s1, 1), 0.1);
		AssertEquals("space not number", -1.0, Char.GetNumericValue(s1, 2), 0.1);
	}

	public void TestGetUnicodeCategory()
	{
		{
			char Pe1 = ']';
			char Pe2 = '}';
			char Pe3 = ')';
			AssertEquals("Close Punctuation", 
				     UnicodeCategory.ClosePunctuation, 
				     Char.GetUnicodeCategory(Pe1));
			AssertEquals("Close Punctuation", 
				     UnicodeCategory.ClosePunctuation, 
				     Char.GetUnicodeCategory(Pe2));
			AssertEquals("Close Punctuation", 
				     UnicodeCategory.ClosePunctuation, 
				     Char.GetUnicodeCategory(Pe3));
		}
		// TODO - ConnectorPunctuation
		{
			char c1 = (char)0; // 0000-001F, 007F-009F
			char c2 = (char)0x001F;
			char c3 = (char)0x007F;
			char c4 = (char)0x00F;
			AssertEquals("Control", 
				     UnicodeCategory.Control, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("Control", 
				     UnicodeCategory.Control, 
				     Char.GetUnicodeCategory(c2));
			AssertEquals("Control", 
				     UnicodeCategory.Control, 
				     Char.GetUnicodeCategory(c3));
			AssertEquals("Control", 
				     UnicodeCategory.Control, 
				     Char.GetUnicodeCategory(c4));
		}
		{
			// TODO - more currencies?
			char c1 = '$';
			AssertEquals("Currency",
				     UnicodeCategory.CurrencySymbol,
				     Char.GetUnicodeCategory(c1));
		}
		{
			char c1 = '-';
			AssertEquals("Dash Punctuation", 
				     UnicodeCategory.DashPunctuation, 
				     Char.GetUnicodeCategory(c1));
		}
		{
			char c1 = '2';
			char c2 = '7';
			AssertEquals("Decimal Digit", 
				     UnicodeCategory.DecimalDigitNumber, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("Decimal Digit", 
				     UnicodeCategory.DecimalDigitNumber, 
				     Char.GetUnicodeCategory(c2));
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
			AssertEquals("LowercaseLetter", 
				     UnicodeCategory.LowercaseLetter, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("LowercaseLetter", 
				     UnicodeCategory.LowercaseLetter, 
				     Char.GetUnicodeCategory(c2));
		}
		{
			char c1 = '+';
			char c2 = '=';
			AssertEquals("MathSymbol", 
				     UnicodeCategory.MathSymbol, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("MathSymbol", 
				     UnicodeCategory.MathSymbol, 
				     Char.GetUnicodeCategory(c2));
		}
		// TODO - ModifierSymbol
		// TODO - NonSpacingMark
		// TODO - OpenPunctuation
		{
			char c1 = '[';
			char c2 = '{';
			char c3 = '(';
			AssertEquals("OpenPunctuation", 
				     UnicodeCategory.OpenPunctuation, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("OpenPunctuation", 
				     UnicodeCategory.OpenPunctuation, 
				     Char.GetUnicodeCategory(c2));
			AssertEquals("OpenPunctuation", 
				     UnicodeCategory.OpenPunctuation, 
				     Char.GetUnicodeCategory(c3));
		}
		// TODO - OtherLetter
		// TODO - OtherNotAssigned
		// TODO - OtherNumber
		{
			char c1 = '/';
			AssertEquals("OtherPunctuation", 
				     UnicodeCategory.OtherPunctuation, 
				     Char.GetUnicodeCategory(c1));
		}
		// TODO - OtherSymbol
		// TODO - ParagraphSeparator
		// TODO - PrivateUse
		{
			char c1 = ' ';
			AssertEquals("SpaceSeparator", 
				     UnicodeCategory.SpaceSeparator, 
				     Char.GetUnicodeCategory(c1));
		}
		// TODO - SpacingCombiningMark
		{
			char c1 = (char)0xD800; // D800-DBFF
			char c2 = (char)0xDBFF; // D800-DBFF
			char c3 = (char)0xDC01; // DC00-DEFF
			char c4 = (char)0xDEFF; // DC00-DEFF
			AssertEquals("High Surrogate", 
				     UnicodeCategory.Surrogate, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("High Surrogate", 
				     UnicodeCategory.Surrogate, 
				     Char.GetUnicodeCategory(c2));
			AssertEquals("Low Surrogate", 
				     UnicodeCategory.Surrogate, 
				     Char.GetUnicodeCategory(c3));
			AssertEquals("Low Surrogate", 
				     UnicodeCategory.Surrogate, 
				     Char.GetUnicodeCategory(c4));
		}
		// TODO - TitlecaseLetter
		// TODO - UppercaseLetter
		{
			char c1 = 'A';
			char c2 = 'Z';
			AssertEquals("UppercaseLetter", 
				     UnicodeCategory.UppercaseLetter, 
				     Char.GetUnicodeCategory(c1));
			AssertEquals("UppercaseLetter", 
				     UnicodeCategory.UppercaseLetter, 
				     Char.GetUnicodeCategory(c2));
		}
	}

	public void TestIsControl()
	{
		// control is 0000-001F, 007F-009F
		char c1 = (char)0;
		char c2 = (char)0x001F;
		char c3 = (char)0x007F;
		char c4 = (char)0x009F;
		Assert("Not control", !Char.IsControl(' '));
		Assert("control", Char.IsControl(c1));
		Assert("control", Char.IsControl(c2));
		Assert("control", Char.IsControl(c3));
		Assert("control", Char.IsControl(c4));

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert("Not control", !Char.IsControl(s1, 0));
		Assert("control", Char.IsControl(s1, 1));
		Assert("control", Char.IsControl(s1, 2));
		Assert("control", Char.IsControl(s1, 3));
		Assert("control", Char.IsControl(s1, 4));
	}

	public void TestIsDigit()
	{
		char c1 = '0';
		char c2 = '9';
		Assert("Not digit", !Char.IsDigit(' '));
		Assert("digit", Char.IsDigit(c1));
		Assert("digit", Char.IsDigit(c2));

		string s1 = " " + c1 + c2;
		Assert("Not digit", !Char.IsDigit(s1, 0));
		Assert("digit", Char.IsDigit(s1, 1));
		Assert("digit", Char.IsDigit(s1, 2));
	}

	public void TestIsLetter()
	{
		char c1 = 'a';
		char c2 = 'z';
		char c3 = 'A';
		char c4 = 'Z';
		Assert("Not letter", !Char.IsLetter(' '));
		Assert("letter", Char.IsLetter(c1));
		Assert("letter", Char.IsLetter(c2));
		Assert("letter", Char.IsLetter(c3));
		Assert("letter", Char.IsLetter(c4));

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert("Not letter", !Char.IsLetter(s1, 0));
		Assert("letter", Char.IsLetter(s1, 1));
		Assert("letter", Char.IsLetter(s1, 2));
		Assert("letter", Char.IsLetter(s1, 3));
		Assert("letter", Char.IsLetter(s1, 4));
	}

	public void TestIsLetterOrDigit()
	{
		char c1 = 'a';
		char c2 = 'z';
		char c3 = 'A';
		char c4 = 'Z';
		char c5 = '0';
		char c6 = '9';
		Assert("Not letterordigit", !Char.IsLetterOrDigit(' '));
		Assert("letterordigit", Char.IsLetterOrDigit(c1));
		Assert("letterordigit", Char.IsLetterOrDigit(c2));
		Assert("letterordigit", Char.IsLetterOrDigit(c3));
		Assert("letterordigit", Char.IsLetterOrDigit(c4));
		Assert("letterordigit", Char.IsLetterOrDigit(c5));
		Assert("letterordigit", Char.IsLetterOrDigit(c6));

		string s1 = " " + c1 + c2 + c3 + c4 + c5 + c6;
		Assert("Not letterordigit", !Char.IsLetterOrDigit(s1, 0));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 1));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 2));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 3));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 4));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 5));
		Assert("letterordigit", Char.IsLetterOrDigit(s1, 6));
	}

	public void TestIsLower()
	{
		char c1 = 'a';
		char c2 = 'z';
		Assert("Not lower", !Char.IsLower(' '));
		Assert("lower", Char.IsLower(c1));
		Assert("lower", Char.IsLower(c2));

		string s1 = " " + c1 + c2;
		Assert("Not lower", !Char.IsLower(s1, 0));
		Assert("lower", Char.IsLower(s1, 1));
		Assert("lower", Char.IsLower(s1, 2));
	}

	public void TestIsNumber()
	{
		char c1 = '0';
		char c2 = '9';
		// TODO - IsNumber of less obvious characters

		Assert("Not number", !Char.IsNumber(' '));
		Assert("number", Char.IsNumber(c1));
		Assert("number", Char.IsNumber(c2));

		string s1 = " " + c1 + c2;
		Assert("Not number", !Char.IsNumber(s1, 0));
		Assert("number", Char.IsNumber(s1, 1));
		Assert("number", Char.IsNumber(s1, 2));
	}

	public void TestIsPunctuation()
	{
		char c1 = '.';
		char c2 = '?';
		Assert("Not punctuation", !Char.IsPunctuation(' '));
		Assert("punctuation", Char.IsPunctuation(c1));
		Assert("punctuation", Char.IsPunctuation(c2));

		string s1 = " " + c1 + c2;
		Assert("Not punctuation", !Char.IsPunctuation(s1, 0));
		Assert("punctuation", Char.IsPunctuation(s1, 1));
		Assert("punctuation", Char.IsPunctuation(s1, 2));
	}

	public void TestIsSeparator()
	{
		char c1 = ' ';

		Assert("Not separator", !Char.IsSeparator('.'));
		Assert("separator1", Char.IsSeparator(c1));

		string s1 = "." + c1;
		Assert("Not separator", !Char.IsSeparator(s1, 0));
		Assert("separator1-2", Char.IsSeparator(s1, 1));
	}

	public void TestIsSurrogate()
	{
		// high surrogate - D800-DBFF
		// low surrogate - DC00-DEFF
		char c1 = (char)0xD800;
		char c2 = (char)0xDBFF;
		char c3 = (char)0xDC00;
		char c4 = (char)0xDEFF;
		Assert("Not surrogate", !Char.IsSurrogate(' '));
		Assert("surrogate1", Char.IsSurrogate(c1));
		Assert("surrogate2", Char.IsSurrogate(c2));
		Assert("surrogate3", Char.IsSurrogate(c3));
		Assert("surrogate4", Char.IsSurrogate(c4));

		string s1 = " " + c1 + c2 + c3 + c4;
		Assert("Not surrogate", !Char.IsSurrogate(s1, 0));
		Assert("surrogate1-2", Char.IsSurrogate(s1, 1));
		Assert("surrogate2-2", Char.IsSurrogate(s1, 2));
		Assert("surrogate3-2", Char.IsSurrogate(s1, 3));
		Assert("surrogate4-2", Char.IsSurrogate(s1, 4));
	}

	public void TestIsSymbol()
	{
		char c1 = '+';
		char c2 = '=';
		Assert("Not symbol", !Char.IsSymbol(' '));
		Assert("symbol", Char.IsSymbol(c1));
		Assert("symbol", Char.IsSymbol(c2));

		string s1 = " " + c1 + c2;
		Assert("Not symbol", !Char.IsSymbol(s1, 0));
		Assert("symbol", Char.IsSymbol(s1, 1));
		Assert("symbol", Char.IsSymbol(s1, 2));
	}

	public void TestIsUpper()
	{
		char c1 = 'A';
		char c2 = 'Z';
		Assert("Not upper", !Char.IsUpper('a'));
		Assert("upper", Char.IsUpper(c1));
		Assert("upper", Char.IsUpper(c2));

		string s1 = "a" + c1 + c2;
		Assert("Not upper", !Char.IsUpper(s1, 0));
		Assert("upper", Char.IsUpper(s1, 1));
		Assert("upper", Char.IsUpper(s1, 2));
	}

	public void TestIsWhiteSpace()
	{
		char c1 = ' ';
		char c2 = '\n';
		char c3 = '\t';

		Assert("Not whitespace", !Char.IsWhiteSpace('.'));
		Assert("whitespace1", Char.IsWhiteSpace(c1));
		Assert("whitespace2", Char.IsWhiteSpace(c2));
		Assert("whitespace3", Char.IsWhiteSpace(c3));

		string s1 = "." + c1 + c2 + c3;
		Assert("Not whitespace", !Char.IsWhiteSpace(s1, 0));
		Assert("whitespace1-2", Char.IsWhiteSpace(s1, 1));
		Assert("whitespace2-2", Char.IsWhiteSpace(s1, 2));
		Assert("whitespace3-2", Char.IsWhiteSpace(s1, 3));
	}


	public void TestParse()
	{
		char c1 = 'a';
		string s1 = "a";
		Assert(c1.Equals(Char.Parse(s1)));
	}	

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
		AssertEquals("char lowered", b1, Char.ToLower(a1));
		AssertEquals("char lowered", b2, Char.ToLower(a2));
		AssertEquals("char lowered", b3, Char.ToLower(a3));
		AssertEquals("char lowered", b4, Char.ToLower(a4));
		AssertEquals("char lowered", b5, Char.ToLower(a5));
		AssertEquals("char lowered", b6, Char.ToLower(a6));
	}

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
		AssertEquals("char uppered", b1, Char.ToUpper(a1));
		AssertEquals("char uppered", b2, Char.ToUpper(a2));
		AssertEquals("char uppered", b3, Char.ToUpper(a3));
		AssertEquals("char uppered", b4, Char.ToUpper(a4));
		AssertEquals("char uppered", b5, Char.ToUpper(a5));
		AssertEquals("char uppered", b6, Char.ToUpper(a6));
	}


	public void TestToString()
	{
		char c1 = 'a';
		string s1 = "a";
		Assert(s1.Equals(c1.ToString()));
	}

	public void TestGetTypeCode()
	{
		char c1 = 'a';
		Assert(c1.GetTypeCode().Equals(TypeCode.Char));
	}

}

}
