// Int64Test.cs - NUnit Test Cases for the System.Int64 struct
//
// Author: Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel, 2001
// 
// tests ToString and Parse function with the culture independent 
// NumberFormatInfo.InvariantInfo

using NUnit.Framework;
using System;
using System.Globalization;

    /// <summary>
    /// Tests for System.Int64
    /// </summary>
public class Int64Test : TestCase
{
    private static long[] vals
        = { 0, Int64.MaxValue, Int64.MinValue,
              1L, 12L, 123L, 1234L, -123L, 
              1234567890123456L, 6543210987654321L };
    private const long val1 = -1234567L;
    private const long val2 = 1234567L;
    private const string sval1Test1 = "  -1,234,567   ";
    private const string sval1Test2 = "  -1234567   ";
    //private const string sval1Test3 = "  -12345,,,,67   "; // interesting: this case works on SDK Beta2, but the specification says nothing about this case
    private const string sval1Test4 = "  -12345 67   ";
    private const string sval1Test5 = "  -$1,234567.00 ";
    private const string sval1Test6 = "($1,234,567.00)";
    private const string sval1Test7 = "(1,234,567.00)";
    private const string sval1UserCur1 = "1234_5_67,000 XYZ-";
    private const string sval2UserCur1 = "1234_5_67,000 XYZ";
    private const string sval1UserPercent1 = "-%%%1~2~3~4~5~6~7~0~0;0";
    private const string sval2UserPercent1 = "%%%1~2~3~4~5~6~7~0~0;0";
    private const NumberStyles style1 =  NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign
        | NumberStyles.AllowTrailingWhite | NumberStyles.AllowThousands;
    private NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;
    private NumberFormatInfo nfiUser;

    public Int64Test(string name) : base(name) {}

    public static ITest Suite 
    {
        get { return new TestSuite(typeof(Int64Test)); }
    }

    protected override void SetUp() 
    {
        nfiUser = new NumberFormatInfo();
        nfiUser.CurrencyDecimalDigits = 3;
        nfiUser.CurrencyDecimalSeparator = ",";
        nfiUser.CurrencyGroupSeparator = "_";
        nfiUser.CurrencyGroupSizes = new int[] { 2,1,0 };
        nfiUser.CurrencyNegativePattern = 10;
        nfiUser.CurrencyPositivePattern = 3;
        nfiUser.CurrencySymbol = "XYZ";
        nfiUser.PercentDecimalDigits = 1;
        nfiUser.PercentDecimalSeparator = ";";
        nfiUser.PercentGroupSeparator = "~";
        nfiUser.PercentGroupSizes = new int[] {1};
        nfiUser.PercentNegativePattern = 2;
        nfiUser.PercentPositivePattern = 2;
        nfiUser.PercentSymbol = "%%%";
    }

    public void TestRoundTripGeneral() 
    {
        foreach(long lv in vals) 
        {
            string s = lv.ToString(nfi);
            long lv2 = Int64.Parse(s);
            Assert(lv == lv2);
            long lv3 = Int64.Parse(s, NumberStyles.Integer, nfi);
            Assert(lv == lv3);
        }
    }

    public void TestRoundTripHex() 
    {
        foreach(long lv in vals) 
        {
            string s = lv.ToString("x", nfi);
            long lv2 = Int64.Parse(s, NumberStyles.HexNumber, nfi);
            Assert(lv == lv2);
        }
    }

    public void TestParseNull()
    {
        try 
        {
            Int64.Parse(null);
            Fail("Should raise System.ArgumentNullException"); 
        } 
        catch (ArgumentNullException) 
        {
            // ok
        }
    }

    public void TestParse()
    {
        long lv;

        lv = Int64.Parse(sval1Test1, style1, nfi);
        Assert(lv == val1);

        try
        {
            lv = Int64.Parse(sval1Test1, nfi);
            Fail("Should raise System.FormatException 1");
        }
        catch (System.FormatException)
        {
            // ok
        }

        lv = Int64.Parse(sval1Test2, style1, nfi);
        Assert(lv == val1);
        lv = Int64.Parse(sval1Test2, nfi);
        Assert(lv == val1);

        try
        {
            lv = Int64.Parse(sval1Test4, style1, nfi);
            Fail("Should raise System.FormatException 3");
        }
        catch (System.FormatException)
        {
            // ok
        }

        lv = Int64.Parse(sval1Test5, NumberStyles.Currency, nfi);
        Assert(lv == val1);
    }

    public void TestToString() 
    {
        string s;

        s = val1.ToString("c", nfi);
        Assert(s.Equals(sval1Test6));

        s = val1.ToString("n", nfi);
        Assert(s.Equals(sval1Test7));
    }

    public void TestUserCurrency()
    {
        string s;
        long v;

        s = val1.ToString("c", nfiUser);
        Assert(s.Equals(sval1UserCur1));
        v = Int64.Parse(s, NumberStyles.Currency, nfiUser);
        Assert(v == val1);
   
        s = val2.ToString("c", nfiUser);
        Assert(s.Equals(sval2UserCur1));
        v = Int64.Parse(s, NumberStyles.Currency, nfiUser);
        Assert(v == val2);
    }

    public void TestUserPercent()
    {
        string s;

        s = val1.ToString("p", nfiUser);
        Assert(s.Equals(sval1UserPercent1));

        s = val2.ToString("p", nfiUser);
        Assert(s.Equals(sval2UserPercent1));
    }
}

