// DecimalTest.cs - NUnit Test Cases for the System.Decimal struct
//
// Author: Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel, 2001
// 

using NUnit.Framework;
using System;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MonoTests.System {
    internal struct ParseTest
    {
        public ParseTest(String str, bool exceptionFlag)
        {
            this.str = str;
            this.exceptionFlag = exceptionFlag;
            this.style = NumberStyles.Number;
            this.d = 0;
        }

        public ParseTest(String str, Decimal d)
        {
            this.str = str;
            this.exceptionFlag = false;
            this.style = NumberStyles.Number;
            this.d = d;
        }

        public ParseTest(String str, Decimal d, NumberStyles style)
        {
            this.str = str;
            this.exceptionFlag = false;
            this.style = style;
            this.d = d;
        }

        public String str;
        public Decimal d;
        public NumberStyles style;
        public bool exceptionFlag;
    }

    internal struct ToStringTest
    {
        public ToStringTest(String format, Decimal d, String str)
        {
            this.format = format;
            this.d = d;
            this.str = str;
        }

        public String format;
        public Decimal d;
        public String str;
    }

    /// <summary>
    /// Tests for System.Decimal
    /// </summary>
    public class DecimalTest : TestCase
    {
	public DecimalTest() {}

        private const int negativeBitValue = unchecked ((int)0x80000000);
        private const int negativeScale4Value = unchecked ((int)0x80040000);
        private int [] parts0 = {0,0,0,0}; //Positive Zero.
        private int [] parts1 = {1,0,0,0};
        private int [] parts2 = {0,1,0,0};
        private int [] parts3 = {0,0,1,0};
        private int [] parts4 = {0,0,0,negativeBitValue}; // Negative zero.
        private int [] parts5 = {1,1,1,0};
        private int [] partsMaxValue = {-1,-1,-1,0};
        private int [] partsMinValue = {-1,-1,-1,negativeBitValue};
        private int [] parts6 = {1234, 5678, 8888, negativeScale4Value};
        private NumberFormatInfo NfiUser;

	private CultureInfo old_culture;

	protected override void SetUp() 
	{
		old_culture = Thread.CurrentThread.CurrentCulture;

		// Set culture to en-US and don't let the user override.
		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);

		NfiUser = new NumberFormatInfo();
		NfiUser.CurrencyDecimalDigits = 3;
		NfiUser.CurrencyDecimalSeparator = ",";
		NfiUser.CurrencyGroupSeparator = "_";
		NfiUser.CurrencyGroupSizes = new int[] { 2,1,0 };
		NfiUser.CurrencyNegativePattern = 10;
		NfiUser.CurrencyPositivePattern = 3;
		NfiUser.CurrencySymbol = "XYZ";
		NfiUser.NumberDecimalSeparator = "##";
		NfiUser.NumberDecimalDigits = 4;
		NfiUser.NumberGroupSeparator = "__";
		NfiUser.NumberGroupSizes = new int[] { 2,1 };
		NfiUser.PercentDecimalDigits = 1;
		NfiUser.PercentDecimalSeparator = ";";
		NfiUser.PercentGroupSeparator = "~";
		NfiUser.PercentGroupSizes = new int[] {1};
		NfiUser.PercentNegativePattern = 2;
		NfiUser.PercentPositivePattern = 2;
		NfiUser.PercentSymbol = "%%%";
        }

	protected override void TearDown()
	{
		Thread.CurrentThread.CurrentCulture = old_culture;
	}

	public void TestToString()
        {
            ToStringTest[] tab = {
                new ToStringTest("F", 12.345678m, "12.35"),
                new ToStringTest("F3", 12.345678m, "12.346"),
                new ToStringTest("F0", 12.345678m, "12"),
                new ToStringTest("F7", 12.345678m, "12.3456780"),
                new ToStringTest("g", 12.345678m, "12.345678"),
                new ToStringTest("E", 12.345678m, "1.234568E+001"),
                new ToStringTest("E3", 12.345678m, "1.235E+001"),
                new ToStringTest("E0", 12.345678m, "1E+001"),
                new ToStringTest("e8", 12.345678m, "1.23456780e+001"),
                new ToStringTest("F", 0.0012m, "0.00"),
                new ToStringTest("F3", 0.0012m, "0.001"),
                new ToStringTest("F0", 0.0012m, "0"),
                new ToStringTest("F6", 0.0012m, "0.001200"),
                new ToStringTest("e", 0.0012m, "1.200000e-003"),
                new ToStringTest("E3", 0.0012m, "1.200E-003"),
                new ToStringTest("E0", 0.0012m, "1E-003"),
                new ToStringTest("E6", 0.0012m, "1.200000E-003"),
                new ToStringTest("F4", -0.001234m, "-0.0012"),
                new ToStringTest("E3", -0.001234m, "-1.234E-003"),
                new ToStringTest("g", -0.000012m, "-1.2e-05"),
                new ToStringTest("g", -0.00012m, "-0.00012"),
                new ToStringTest("g4", -0.00012m, "-0.00012"),
                new ToStringTest("g7", -0.00012m, "-0.00012"),
                new ToStringTest("g", -0.0001234m, "-0.0001234"),
                new ToStringTest("g", -0.0012m, "-0.0012"),
                new ToStringTest("g", -0.001234m, "-0.001234"),
                new ToStringTest("g", -0.012m, "-0.012"),
                new ToStringTest("g4", -0.012m, "-0.012"),
                new ToStringTest("g", -0.12m, "-0.12"),
                new ToStringTest("g", -1.2m, "-1.2"),
                new ToStringTest("g4", -120m, "-120"),
                new ToStringTest("g", -12m, "-12"),
                new ToStringTest("g", -120m, "-120"),
                new ToStringTest("g", -1200m, "-1200"),
                new ToStringTest("g4", -1200m, "-1200"),
                new ToStringTest("g", -1234m, "-1234"),
                new ToStringTest("g", -12000m, "-12000"),
                new ToStringTest("g4", -12000m, "-1.2e+04"),
                new ToStringTest("g5", -12000m, "-12000"),
                new ToStringTest("g", -12345m, "-12345"),
                new ToStringTest("g", -120000m, "-120000"),
                new ToStringTest("g4", -120000m, "-1.2e+05"),
                new ToStringTest("g5", -120000m, "-1.2e+05"),
                new ToStringTest("g6", -120000m, "-120000"),
                new ToStringTest("g", -123456.1m, "-123456.1"),
                new ToStringTest("g5", -123456.1m, "-1.2346e+05"),
                new ToStringTest("g6", -123456.1m, "-123456"),
                new ToStringTest("g", -1200000m, "-1200000"),
                new ToStringTest("g", -123456.1m, "-123456.1"),
                new ToStringTest("g", -123456.1m, "-123456.1"),
                new ToStringTest("g", -1234567.1m, "-1234567.1"),
                new ToStringTest("g", -12000000m, "-12000000"),
                new ToStringTest("g", -12345678.1m, "-12345678.1"),
                new ToStringTest("g", -12000000000000000000m, "-12000000000000000000"),
                new ToStringTest("F", -123, "-123.00"),
                new ToStringTest("F3", -123, "-123.000"),
                new ToStringTest("F0", -123, "-123"),
                new ToStringTest("E3", -123, "-1.230E+002"),
                new ToStringTest("E0", -123, "-1E+002"),
                new ToStringTest("E", -123, "-1.230000E+002"),
                new ToStringTest("F3", Decimal.MinValue, "-79228162514264337593543950335.000"),
                new ToStringTest("F", Decimal.MinValue, "-79228162514264337593543950335.00"),
                new ToStringTest("F0", Decimal.MinValue, "-79228162514264337593543950335"),
                new ToStringTest("E", Decimal.MinValue, "-7.922816E+028"),
                new ToStringTest("E3", Decimal.MinValue, "-7.923E+028"),
                new ToStringTest("E28", Decimal.MinValue, "-7.9228162514264337593543950335E+028"),
                new ToStringTest("E30", Decimal.MinValue, "-7.922816251426433759354395033500E+028"),
                new ToStringTest("E0", Decimal.MinValue, "-8E+028"),
                new ToStringTest("N3", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.000"),
                new ToStringTest("N0", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335"),
                new ToStringTest("N", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.00"),
                new ToStringTest("n3", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.000"),
                new ToStringTest("n0", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335"),
                new ToStringTest("n", Decimal.MinValue, "-79,228,162,514,264,337,593,543,950,335.00"),
                new ToStringTest("C", 123456.7890m, NumberFormatInfo.InvariantInfo.CurrencySymbol + "123,456.79"),
                new ToStringTest("C", -123456.7890m, "(" + NumberFormatInfo.InvariantInfo.CurrencySymbol + "123,456.79)"),
                new ToStringTest("C3", 1123456.7890m, NumberFormatInfo.InvariantInfo.CurrencySymbol + "1,123,456.789"),
                new ToStringTest("P", 123456.7891m, "12,345,678.91 %"),
                new ToStringTest("P", -123456.7892m, "-12,345,678.92 %"),
                new ToStringTest("P3", 1234.56789m, "123,456.789 %"),
            };

            NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;

            for (int i = 0; i < tab.Length; i++) 
            {
                try
                {
                    string s = tab[i].d.ToString(tab[i].format, nfi);
		    AssertEquals("A01 tab[" + i + "].format = '" + tab[i].format + "')", tab[i].str, s);
                } 
                catch (OverflowException)
                {
                    Fail(tab[i].d.ToString(tab[i].format, nfi) + " (format = '" + tab[i].format + "'): unexpected exception !");
                }
		catch (NUnit.Framework.AssertionException e) {
			throw e;
		}
		catch (Exception e) {
			Fail ("Unexpected Exception when i = " + i + ". e = " + e);
		}
            }      
        }

        public void TestCurrencyPattern()
        {
            NumberFormatInfo nfi2 = (NumberFormatInfo)NfiUser.Clone();
            Decimal d = -1234567.8976m;
            string[] ergCurrencyNegativePattern = new String[16] {
                "(XYZ1234_5_67,898)", "-XYZ1234_5_67,898", "XYZ-1234_5_67,898", "XYZ1234_5_67,898-",
                "(1234_5_67,898XYZ)", "-1234_5_67,898XYZ", "1234_5_67,898-XYZ", "1234_5_67,898XYZ-",
                "-1234_5_67,898 XYZ", "-XYZ 1234_5_67,898", "1234_5_67,898 XYZ-", "XYZ 1234_5_67,898-",
                "XYZ -1234_5_67,898", "1234_5_67,898- XYZ", "(XYZ 1234_5_67,898)", "(1234_5_67,898 XYZ)",
            };

            for (int i = 0; i < ergCurrencyNegativePattern.Length; i++) 
            {
                nfi2.CurrencyNegativePattern = i;
                if (d.ToString("C", nfi2) != ergCurrencyNegativePattern[i]) 
                {
                    Fail("CurrencyNegativePattern #" + i + " failed: " +
                        d.ToString("C", nfi2) + " != " + ergCurrencyNegativePattern[i]);
                }
            }

            d = 1234567.8976m;
            string[] ergCurrencyPositivePattern = new String[4] {
                "XYZ1234_5_67,898", "1234_5_67,898XYZ", "XYZ 1234_5_67,898", "1234_5_67,898 XYZ",
            };

            for (int i = 0; i < ergCurrencyPositivePattern.Length; i++) 
            {
                nfi2.CurrencyPositivePattern = i;
                if (d.ToString("C", nfi2) != ergCurrencyPositivePattern[i]) 
                {
                    Fail("CurrencyPositivePattern #" + i + " failed: " +
                        d.ToString("C", nfi2) + " != " + ergCurrencyPositivePattern[i]);
                }
            }
        }

        public void TestNumberNegativePattern()
        {
            NumberFormatInfo nfi2 = (NumberFormatInfo)NfiUser.Clone();
            Decimal d = -1234.89765m;
            string[] ergNumberNegativePattern = new String[5] {
                "(1__2__34##8977)", "-1__2__34##8977", "- 1__2__34##8977", "1__2__34##8977-", "1__2__34##8977 -",
            };

            for (int i = 0; i < ergNumberNegativePattern.Length; i++) 
            {
                nfi2.NumberNegativePattern = i;
		AssertEquals ("NumberNegativePattern #" + i, ergNumberNegativePattern[i], d.ToString("N", nfi2));
            }
        }
        
        public void TestPercentPattern()
        {
            NumberFormatInfo nfi2 = (NumberFormatInfo)NfiUser.Clone();
            Decimal d = -1234.8976m;
            string[] ergPercentNegativePattern = new String[3] {
                "-1~2~3~4~8~9;8 %%%", "-1~2~3~4~8~9;8%%%", "-%%%1~2~3~4~8~9;8"
            };

            for (int i = 0; i < ergPercentNegativePattern.Length; i++) 
            {
                nfi2.PercentNegativePattern = i;
                if (d.ToString("P", nfi2) != ergPercentNegativePattern[i]) 
                {
                    Fail("PercentNegativePattern #" + i + " failed: " +
                        d.ToString("P", nfi2) + " != " + ergPercentNegativePattern[i]);
                }
            }

            d = 1234.8976m;
            string[] ergPercentPositivePattern = new String[3] {
                "1~2~3~4~8~9;8 %%%", "1~2~3~4~8~9;8%%%", "%%%1~2~3~4~8~9;8"
            };

            for (int i = 0; i < ergPercentPositivePattern.Length; i++) 
            {
                nfi2.PercentPositivePattern = i;
                if (d.ToString("P", nfi2) != ergPercentPositivePattern[i]) 
                {
                    Fail("PercentPositivePattern #" + i + " failed: " +
                        d.ToString("P", nfi2) + " != " + ergPercentPositivePattern[i]);
                }
            }
        }

        public void TestParse()
        {
            ParseTest[] tab = {
                new ParseTest("1.2345", 1.2345m),
                new ParseTest("-9876543210", -9876543210m),
                new ParseTest(NumberFormatInfo.InvariantInfo.CurrencySymbol 
			+ " (  79,228,162,514,264,337,593,543,950,335.000 ) ", Decimal.MinValue, NumberStyles.Currency),
                new ParseTest("1.234567890e-10", (Decimal)1.234567890e-10, NumberStyles.Float),
                new ParseTest("1.234567890e-24", 1.2346e-24m, NumberStyles.Float),
                new ParseTest("  47896396.457983645462346E10  ", 478963964579836454.62346m, NumberStyles.Float),
                new ParseTest("-7922816251426433759354395033.250000000000001", -7922816251426433759354395033.3m),
                new ParseTest("-00000000000000795033.2500000000000000", -795033.25m),
                new ParseTest("-000000000000001922816251426433759354395033.300000000000000", -1922816251426433759354395033.3m),
                new ParseTest("-7922816251426433759354395033.150000000000", -7922816251426433759354395033.2m),
                new ParseTest("-7922816251426433759354395033.2400000000000", -7922816251426433759354395033.2m),
                new ParseTest("-7922816251426433759354395033.2600000000000", -7922816251426433759354395033.3m)
            };

            Decimal d;
            for (int i = 0; i < tab.Length; i++) 
            {
                try
                {
                    d = Decimal.Parse(tab[i].str, tab[i].style, NumberFormatInfo.InvariantInfo);
                    if (tab[i].exceptionFlag)
                    {
                        Fail(tab[i].str + ": missing exception !");
                    }
                    else if (d != tab[i].d) 
                    {
                        Fail(tab[i].str + " != " + d);
                    }
                } 
                catch (OverflowException)
                {
                    if (!tab[i].exceptionFlag)
                    {
                        Fail(tab[i].str + ": unexpected exception !");
                    }
                }
            }  
    
            try 
            {
                d = Decimal.Parse(null);
                Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                //ok
            }

            try 
            {
                d = Decimal.Parse("123nx");
                Fail("Expected FormatException");
            }
            catch (FormatException)
            {
                //ok
            }

            try 
            {
                d = Decimal.Parse("79228162514264337593543950336");
                Fail("Expected OverflowException" + d);
            }
            catch (OverflowException)
            {
                //ok
            }
        }

        public void TestConstants()
        {
            AssertEquals ("Zero", 0m, Decimal.Zero);
            AssertEquals ("One", 1m, Decimal.One);
            AssertEquals ("MinusOne", -1m, Decimal.MinusOne);
            AssertEquals ("MaxValue", 79228162514264337593543950335m, Decimal.MaxValue);
            AssertEquals ("MinValue", -79228162514264337593543950335m, Decimal.MinValue);       
            Assert ("MinusOne 2", -1m == Decimal.MinusOne);
        }

        public void TestConstructInt32()
        {
            decimal[] dtab = {0m, 1m, -1m, 123456m, -1234567m};
            int[] itab = {0, 1, -1, 123456, -1234567};

            Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new Decimal(itab[i]);
                if ((decimal)d != dtab[i]) 
                {
                    Fail("Int32 -> Decimal: " + itab[i] + " != " + d);
                }
                else 
                {
                    int n = (int) d;
                    if (n != itab[i]) 
                    {
                        Fail("Decimal -> Int32: " + d + " != " + itab[i]);
                    }
                }
            }

            d = new Decimal(Int32.MaxValue);
            Assert((int)d == Int32.MaxValue);

            d = new Decimal(Int32.MinValue);
            Assert((int)d == Int32.MinValue);
        }

        public void TestConstructUInt32()
        {
            decimal[] dtab = {0m, 1m, 123456m, 123456789m};
            uint[] itab = {0, 1, 123456, 123456789};

            Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new Decimal(itab[i]);
                if ((decimal)d != dtab[i]) 
                {
                    Fail("UInt32 -> Decimal: " + itab[i] + " != " + d);
                }
                else 
                {
                    uint n = (uint) d;
                    if (n != itab[i]) 
                    {
                        Fail("Decimal -> UInt32: " + d + " != " + itab[i]);
                    }
                }
            }

            d = new Decimal(UInt32.MaxValue);
            Assert((uint)d == UInt32.MaxValue);

            d = new Decimal(UInt32.MinValue);
            Assert((uint)d == UInt32.MinValue);
        }

        public void TestConstructInt64()
        {
            decimal[] dtab = {0m, 1m, -1m, 9876543m, -9876543210m, 12345678987654321m};
            long[] itab = {0, 1, -1, 9876543, -9876543210L, 12345678987654321L};

            Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new Decimal(itab[i]);
                if ((decimal)d != dtab[i]) 
                {
                    Fail("Int64 -> Decimal: " + itab[i] + " != " + d);
                }
                else 
                {
                    long n = (long) d;
                    if (n != itab[i]) 
                    {
                        Fail("Decimal -> Int64: " + d + " != " + itab[i]);
                    }
                }
            }

            d = new Decimal(Int64.MaxValue);
            Assert((long)d == Int64.MaxValue);

            d = new Decimal(Int64.MinValue);
            Assert((long)d == Int64.MinValue);
        }

        public void TestConstructUInt64()
        {
            decimal[] dtab = {0m, 1m, 987654321m, 123456789876543210m};
            ulong[] itab = {0, 1, 987654321, 123456789876543210L};

            Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new Decimal(itab[i]);
                if ((decimal)d != dtab[i]) 
                {
                    Fail("UInt64 -> Decimal: " + itab[i] + " != " + d);
                }
                else 
                {
                    ulong n = (ulong) d;
                    if (n != itab[i]) 
                    {
                        Fail("Decimal -> UInt64: " + d + " != " + itab[i]);
                    }
                }
            }

            d = new Decimal(UInt64.MaxValue);
            Assert((ulong)d == UInt64.MaxValue);

            d = new Decimal(UInt64.MinValue);
            Assert((ulong)d == UInt64.MinValue);
        }

        public void TestConstructSingle()
        {
            Decimal d;

            d = new Decimal(-1.2345678f);
            AssertEquals("A#01", -1.234568m, (decimal)d);

            d=3;
            AssertEquals("A#02", 3.0f, (float)d);

            d = new Decimal(0.0f);
            AssertEquals("A#03", 0m, (decimal)d);
            AssertEquals("A#04", 0.0f, (float)d);

            d = new Decimal(1.0f);
            AssertEquals("A#05", 1m, (decimal)d);
            AssertEquals("A#06", 1.0f, (float)d);

            d = new Decimal(-1.2345678f);
            AssertEquals("A#07", -1.234568m, (decimal)d);
            AssertEquals("A#08", -1.234568f, (float)d);

            d = new Decimal(1.2345673f);
            AssertEquals("A#09", 1.234567m, (decimal)d);

            d = new Decimal(1.2345673e7f);
            AssertEquals("A#10", 12345670m, (decimal)d);

            d = new Decimal(1.2345673e-17f);
            AssertEquals("A#11", 0.00000000000000001234567m, (decimal)d);
            AssertEquals("A#12", 1.234567e-17f, (float)d);

            // test exceptions
            try
            {
                d = new Decimal(Single.MaxValue);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new Decimal(Single.NaN);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new Decimal(Single.PositiveInfinity);
                Fail();
            } 
            catch (OverflowException) 
            {
            }
        }

        public void TestConstructSingleRounding()
        {
            decimal d;

            d = new Decimal(1765.2356f);
            Assert(d == 1765.236m);

            d = new Decimal(1765.23554f);
            Assert("failed banker's rule rounding test 1", d == 1765.236m);

            d = new Decimal(1765.2354f);
            Assert(d == 1765.235m);

            d = new Decimal(1765.2346f);
            Assert(d == 1765.235m);

            d = new Decimal(1765.23454f);
            Assert("failed banker's rule rounding test 2", d == 1765.234m);

            d = new Decimal(1765.2344f);
            Assert(d == 1765.234m);

            d = new Decimal(0.00017652356f);
            Assert(d == 0.0001765236m);

            d = new Decimal(0.000176523554f);
            Assert("failed banker's rule rounding test 3", d == 0.0001765236m);

            d = new Decimal(0.00017652354f);
            Assert(d == 0.0001765235m);

            d = new Decimal(0.00017652346f);
            Assert(d == 0.0001765235m);

            d = new Decimal(0.000176523454f);
            Assert("failed banker's rule rounding test 4", d == 0.0001765234m);

            d = new Decimal(0.00017652344f);
            Assert(d == 0.0001765234m);

            d = new Decimal(3.7652356e10f);
            Assert(d == 37652360000m);

            d = new Decimal(3.7652356e20f);
            Assert(d == 376523600000000000000m);

            d = new Decimal(3.76523554e20f);
            Assert("failed banker's rule rounding test 5", d == 376523600000000000000m);

            d = new Decimal(3.7652352e20f);
            Assert(d == 376523500000000000000m);

            d = new Decimal(3.7652348e20f);
            Assert(d == 376523500000000000000m);

            d = new Decimal(3.76523454e20f);
            Assert("failed banker's rule rounding test 6", d == 376523400000000000000m);

            d = new Decimal(3.7652342e20f);
            Assert(d == 376523400000000000000m);
        }

        public void TestConstructDouble()
        {
            Decimal d;

            d = new Decimal(0.0);
            Assert((decimal)d == 0m);

            d = new Decimal(1.0);
            Assert((decimal)d == 1m);
            Assert(1.0 == (double)d);

            d = new Decimal(-1.2345678901234);
            Assert((decimal)d == -1.2345678901234m);
            Assert(-1.2345678901234 == (double)d);

            d = new Decimal(1.2345678901234);
            Assert((decimal)d == 1.2345678901234m);

            d = new Decimal(1.2345678901234e8);
            Assert((decimal)d == 123456789.01234m);
            Assert(1.2345678901234e8 == (double)d);

            d = new Decimal(1.2345678901234e16);
            Assert((decimal)d == 12345678901234000m);
            Assert(1.2345678901234e16 == (double)d);

            d = new Decimal(1.2345678901234e24);
            Assert((decimal)d == 1234567890123400000000000m);
            Assert(1.2345678901234e24 == (double)d);

            d = new Decimal(1.2345678901234e28);
            Assert((decimal)d == 1.2345678901234e28m);
            Assert(1.2345678901234e28 == (double)d);

            d = new Decimal(7.2345678901234e28);
            Assert((decimal)d == 7.2345678901234e28m);
            Assert(new Decimal((double)d) == d);

            d = new Decimal(1.2345678901234e-8);
            Assert((decimal)d == 1.2345678901234e-8m);

            d = new Decimal(1.2345678901234e-14);
            Assert((decimal)d == 1.2345678901234e-14m);
            Assert(1.2345678901234e-14 == (double)d);

            d = new Decimal(1.2342278901234e-25);
            Assert((decimal)d == 1.234e-25m);

            // test exceptions
            try
            {
                d = new Decimal(8e28);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new Decimal(8e48);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new Decimal(Double.NaN);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new Decimal(Double.PositiveInfinity);
                Fail();
            } 
            catch (OverflowException) 
            {
            }
        }

        public void TestConstructDoubleRound()
        {
            decimal d;
	    int TestNum = 1;
            
	    try {
			d = new Decimal(1765.231234567857);
			AssertEquals("A01", 1765.23123456786m, d);

			TestNum++;
			d = new Decimal(1765.2312345678554);
			AssertEquals("A02, failed banker's rule rounding test 1", 1765.23123456786m, d);
			AssertEquals("A03", 1765.23123456786, (double)d);

			TestNum++;
			d = new Decimal(1765.231234567853);
			Assert(d == 1765.23123456785m);

			TestNum++;
			d = new Decimal(1765.231234567847);
			Assert(d == 1765.23123456785m);

			TestNum++;
			d = new Decimal(1765.231234567843);
			Assert(d == 1765.23123456784m);

			TestNum++;
			d = new Decimal(1.765231234567857e-9);
			Assert(d == 1.76523123456786e-9m);

			TestNum++;
			d = new Decimal(1.7652312345678554e-9);
			Assert("failed banker's rule rounding test 3", d == 1.76523123456786e-9m);

			TestNum++;
			d = new Decimal(1.765231234567853e-9);
			Assert(d == 1.76523123456785e-9m);

			TestNum++;
			d = new Decimal(1.765231234567857e+24);
			Assert(d == 1.76523123456786e+24m);

			TestNum++;
			d = new Decimal(1.7652312345678554e+24);
			Assert("failed banker's rule rounding test 4", d == 1.76523123456786e+24m);

			TestNum++;
			d = new Decimal(1.765231234567853e+24);
			Assert(d == 1.76523123456785e+24m);

			TestNum++;
			d = new Decimal(1765.2312345678454);
			Assert(d == 1765.23123456785m);
		}
		catch (Exception e) {
			Fail("At TestNum = " + TestNum + " unexpected exception. e = " + e);
		}
        }

        public void TestNegate()
        {
            decimal d;

            d = new Decimal(12345678);
            Assert((decimal)Decimal.Negate(d) == -12345678m);
        }

        public void TestPartConstruct()
        {
            decimal d;
            
            d = new Decimal(parts0);
            Assert(d == 0);

            d = new Decimal(parts1);
            Assert(d == 1);

            d = new Decimal(parts2);
            Assert(d == 4294967296m);

            d = new Decimal(parts3);
            Assert(d == 18446744073709551616m);

            d = new Decimal(parts4);
            Assert(d == 0m);

            d = new Decimal(parts5);
            Assert(d == 18446744078004518913m);
            
            d = new Decimal(partsMaxValue);
            Assert(d == Decimal.MaxValue);
            
            d = new Decimal(partsMinValue);
            Assert(d == Decimal.MinValue);

            d = new Decimal(parts6);
            int[] erg = Decimal.GetBits(d);
            for (int i = 0; i < 4; i++) 
            {
                Assert(erg[i] == parts6[i]); 
            }
        }

        public void TestFloorTruncate()
        {
            decimal[,] dtab = {
                {0m, 0m, 0m}, {1m, 1m, 1m}, {-1m, -1m, -1m}, {1.1m, 1m, 1m}, 
                {-1.000000000001m, -2m, -1m}, {12345.67890m,12345m,12345m},
                {-123456789012345.67890m, -123456789012346m, -123456789012345m},
                {Decimal.MaxValue, Decimal.MaxValue, Decimal.MaxValue},
                {Decimal.MinValue, Decimal.MinValue, Decimal.MinValue},
                {6.999999999m, 6m, 6m}, {-6.999999999m, -7m, -6m}, 
                {0.00001m, 0m, 0m}, {-0.00001m, -1m, 0m}
            };

            decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = Decimal.Floor(dtab[i,0]);
                if (d != dtab[i,1]) 
                {
                    Fail("Floor: Floor(" + dtab[i,0] + ") != " + d);
                }
                d = Decimal.Truncate(dtab[i,0]);
                if (d != dtab[i,2]) 
                {
                    Fail("Truncate: Truncate(" + dtab[i,0] + ") != " + d);
                }
            }
        }

        public void TestRound()
        {
            decimal[,] dtab = { 
                {1m, 0, 1m}, {1.234567890m, 1, 1.2m}, 
                {1.234567890m, 2, 1.23m}, {1.23450000001m, 3, 1.235m}, 
                {1.2345m, 3, 1.234m}, {1.2355m, 3, 1.236m}, 
                {1.234567890m, 4, 1.2346m}, {1.23567890m, 2, 1.24m}, 
                {47893764694.4578563236436621m, 7, 47893764694.4578563m},
                {-47893764694.4578563236436621m, 9, -47893764694.457856324m},
                {-47893764694.4578m, 5, -47893764694.4578m}
            };

            decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = Decimal.Round(dtab[i,0], (int)dtab[i,1]);
                if (d != dtab[i,2]) 
                {
                    Fail("Round: Round(" + dtab[i,0] + "," + (int)dtab[i,1] + ") != " + d);
                }
            }
        }
    }

}
