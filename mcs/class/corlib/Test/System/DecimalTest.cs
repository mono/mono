// DecimalTest.cs - NUnit Test Cases for the System.Decimal struct
//
// Author: Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel, 2001
// 

using NUnit.Framework;
using System;
using S = System; // for implementation switching only

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Test {
    /// <summary>
    /// Tests for System.Decimal
    /// </summary>
    public class DecimalTest : TestCase
    {
        public DecimalTest(string name) : base(name) {}

        public static ITest Suite 
        {
            get { return new TestSuite(typeof(DecimalTest)); }
        }

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

        public void TestToString()
        {
            NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo;
            S.Decimal d;

            d = 12.345678m;
            Assert(d.ToString("F", nfi) == "12.35");
            Assert(d.ToString("F3", nfi) == "12.346");
            Assert(d.ToString("F0", nfi) == "12");
            Assert(d.ToString("F7", nfi) == "12.3456780");
            Assert(d.ToString("E", nfi) == "1.234568E+01");
            Assert(d.ToString("E3", nfi) == "1.235E+01");
            Assert(d.ToString("E0", nfi) == "1E+01");
            Assert(d.ToString("e8", nfi) == "1.23456780e+01");

            d = 0.0012m;
            Assert(d.ToString("F", nfi) == "0.00");
            Assert(d.ToString("F3", nfi) == "0.001");
            Assert(d.ToString("F0", nfi) == "0");
            Assert(d.ToString("F6", nfi) == "0.001200");
            Assert(d.ToString("e", nfi) == "1.200000e-03");
            Assert(d.ToString("E3", nfi) == "1.200E-03");
            Assert(d.ToString("E0", nfi) == "1E-03");
            Assert(d.ToString("E6", nfi) == "1.200000E-03");

            d = -0.0012m;
            Assert(d.ToString("F3", nfi) == "-0.001");
            Assert(d.ToString("F2", nfi) == "-0.00");
            Assert(d.ToString("F0", nfi) == "-0");
            Assert(d.ToString("F6", nfi) == "-0.001200");
            Assert(d.ToString("e3", nfi) == "-1.200e-03");
            Assert(d.ToString("e", nfi) == "-1.200000e-03");

            d = -0.000012m;
            Assert(d.ToString("g", nfi) == "-1.2e-05");

            d = -123;
            Assert(d.ToString("F", nfi) == "-123.00");
            Assert(d.ToString("F3", nfi) == "-123.000");
            Assert(d.ToString("F0", nfi) == "-123");
            Assert(d.ToString("E3", nfi) == "-1.230E+02");
            Assert(d.ToString("E0", nfi) == "-1E+02");
            Assert(d.ToString("E", nfi) == "-1.230000E+02");

            d = S.Decimal.MinValue;
            Assert(d.ToString("F3", nfi) == "-79228162514264337593543950335.000");
            Assert(d.ToString("F", nfi) == "-79228162514264337593543950335.00");
            Assert(d.ToString("F0", nfi) == "-79228162514264337593543950335");
            Assert(d.ToString("E", nfi) == "-7.922816E+28");
            Assert(d.ToString("E3", nfi) == "-7.923E+28");
            Assert(d.ToString("E28", nfi) == "-7.9228162514264337593543950335E+28");
            Assert(d.ToString("E30", nfi) == "-7.922816251426433759354395033500E+28");
            Assert(d.ToString("E0", nfi) == "-8E+28");
            Assert(d.ToString("N3", nfi) == "(79,228,162,514,264,337,593,543,950,335.000)");
            Assert(d.ToString("N0", nfi) == "(79,228,162,514,264,337,593,543,950,335)");
            Assert(d.ToString("N", nfi) == "(79,228,162,514,264,337,593,543,950,335.00)");
        }

        public void TestParse()
        {
            const int size = 6;
            string[] stab = new String[size] {
                "1.2345", "-9876543210", "$ (  79,228,162,514,264,337,593,543,950,335.000 ) ",
                "1.234567890e-10", "1.234567890e-24", "  47896396.457983645462346E10  "
            };
            NumberStyles[] styleTab = new NumberStyles[size] {
                NumberStyles.Number, NumberStyles.Number, NumberStyles.Currency,
                NumberStyles.Float, NumberStyles.Float, NumberStyles.Float
            };

            S.Decimal[] dtab = new S.Decimal[size] {
                1.2345m, -9876543210m, S.Decimal.MinValue,
                (S.Decimal)1.234567890e-10, 0.0000000000000000000000012346m, 478963964579836454.62346m
            };

            for (int i = 0; i < size; i++) 
            {
                S.Decimal d;
                d = S.Decimal.Parse(stab[i], styleTab[i], NumberFormatInfo.InvariantInfo);
                if (d != dtab[i]) 
                {
                    Fail(stab[i] + " != " + d);
                }
            }      
        }

        public void TestConstants()
        {
            Assert(0m == (decimal)S.Decimal.Zero);
            Assert(1m == (decimal)S.Decimal.One);
            Assert(-1m == (decimal)S.Decimal.MinusOne);
            Assert(0m == (decimal)S.Decimal.Zero);
            Assert(79228162514264337593543950335m == (decimal)S.Decimal.MaxValue);
            Assert(-79228162514264337593543950335m == (decimal)S.Decimal.MinValue);       
        }

        public void TestConstructInt32()
        {
            decimal[] dtab = {0m, 1m, -1m, 123456m, -1234567m};
            int[] itab = {0, 1, -1, 123456, -1234567};

            S.Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new S.Decimal(itab[i]);
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

            d = new S.Decimal(Int32.MaxValue);
            Assert((int)d == Int32.MaxValue);

            d = new S.Decimal(Int32.MinValue);
            Assert((int)d == Int32.MinValue);
        }

        public void TestConstructUInt32()
        {
            decimal[] dtab = {0m, 1m, 123456m, 123456789m};
            uint[] itab = {0, 1, 123456, 123456789};

            S.Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new S.Decimal(itab[i]);
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

            d = new S.Decimal(UInt32.MaxValue);
            Assert((uint)d == UInt32.MaxValue);

            d = new Decimal(UInt32.MinValue);
            Assert((uint)d == UInt32.MinValue);
        }

        public void TestConstructInt64()
        {
            decimal[] dtab = {0m, 1m, -1m, 9876543m, -9876543210m, 12345678987654321m};
            long[] itab = {0, 1, -1, 9876543, -9876543210L, 12345678987654321L};

            S.Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new S.Decimal(itab[i]);
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

            d = new S.Decimal(Int64.MaxValue);
            Assert((long)d == Int64.MaxValue);

            d = new Decimal(Int64.MinValue);
            Assert((long)d == Int64.MinValue);
        }

        public void TestConstructUInt64()
        {
            decimal[] dtab = {0m, 1m, 987654321m, 123456789876543210m};
            ulong[] itab = {0, 1, 987654321, 123456789876543210L};

            S.Decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = new S.Decimal(itab[i]);
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

            d = new S.Decimal(UInt64.MaxValue);
            Assert((ulong)d == UInt64.MaxValue);

            d = new Decimal(UInt64.MinValue);
            Assert((ulong)d == UInt64.MinValue);
        }

        public void TestConstructSingle()
        {
            S.Decimal d;

            d = new S.Decimal(-1.2345678f);
            Assert((decimal)d == -1.234568m);

            d=3;
            Assert(3.0f == (float)d);

            d = new S.Decimal(0.0f);
            Assert((decimal)d == 0m);
            Assert(0.0f == (float)d);

            d = new S.Decimal(1.0f);
            Assert((decimal)d == 1m);
            Assert(1.0f == (float)d);

            d = new S.Decimal(-1.2345678f);
            Assert((decimal)d == -1.234568m);
            Assert(-1.234568f == (float)d);

            d = new S.Decimal(1.2345673f);
            Assert((decimal)d == 1.234567m);

            d = new S.Decimal(1.2345673e7f);
            Assert((decimal)d == 12345670m);

            d = new S.Decimal(1.2345673e-17f);
            Assert((decimal)d == 0.00000000000000001234567m);
            Assert(1.234567e-17f == (float)d);

            // test exceptions
            try
            {
                d = new S.Decimal(Single.MaxValue);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new S.Decimal(Single.NaN);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new S.Decimal(Single.PositiveInfinity);
                Fail();
            } 
            catch (OverflowException) 
            {
            }
        }

        public void TestConstructSingleRounding()
        {
            decimal d;

            d = new S.Decimal(1765.2356f);
            Assert(d == 1765.236m);

            d = new S.Decimal(1765.23554f);
            Assert("failed banker's rule rounding test 1", d == 1765.236m);

            d = new S.Decimal(1765.2354f);
            Assert(d == 1765.235m);

            d = new S.Decimal(1765.2346f);
            Assert(d == 1765.235m);

            d = new S.Decimal(1765.23454f);
            Assert("failed banker's rule rounding test 2", d == 1765.234m);

            d = new S.Decimal(1765.2344f);
            Assert(d == 1765.234m);

            d = new S.Decimal(0.00017652356f);
            Assert(d == 0.0001765236m);

            d = new S.Decimal(0.000176523554f);
            Assert("failed banker's rule rounding test 3", d == 0.0001765236m);

            d = new S.Decimal(0.00017652354f);
            Assert(d == 0.0001765235m);

            d = new S.Decimal(0.00017652346f);
            Assert(d == 0.0001765235m);

            d = new S.Decimal(0.000176523454f);
            Assert("failed banker's rule rounding test 4", d == 0.0001765234m);

            d = new S.Decimal(0.00017652344f);
            Assert(d == 0.0001765234m);

            d = new S.Decimal(3.7652356e10f);
            Assert(d == 37652360000m);

            d = new S.Decimal(3.7652356e20f);
            Assert(d == 376523600000000000000m);

            d = new S.Decimal(3.76523554e20f);
            Assert("failed banker's rule rounding test 5", d == 376523600000000000000m);

            d = new S.Decimal(3.7652352e20f);
            Assert(d == 376523500000000000000m);

            d = new S.Decimal(3.7652348e20f);
            Assert(d == 376523500000000000000m);

            d = new S.Decimal(3.76523454e20f);
            Assert("failed banker's rule rounding test 6", d == 376523400000000000000m);

            d = new S.Decimal(3.7652342e20f);
            Assert(d == 376523400000000000000m);
        }

        public void TestConstructDouble()
        {
            S.Decimal d;

            d = new S.Decimal(0.0);
            Assert((decimal)d == 0m);

            d = new S.Decimal(1.0);
            Assert((decimal)d == 1m);
            Assert(1.0 == (double)d);

            d = new S.Decimal(-1.2345678901234);
            Assert((decimal)d == -1.2345678901234m);
            Assert(-1.2345678901234 == (double)d);

            d = new S.Decimal(1.2345678901234);
            Assert((decimal)d == 1.2345678901234m);

            d = new S.Decimal(1.2345678901234e8);
            Assert((decimal)d == 123456789.01234m);
            Assert(1.2345678901234e8 == (double)d);

            d = new S.Decimal(1.2345678901234e16);
            Assert((decimal)d == 12345678901234000m);
            Assert(1.2345678901234e16 == (double)d);

            d = new S.Decimal(1.2345678901234e24);
            Assert((decimal)d == 1234567890123400000000000m);
            Assert(1.2345678901234e24 == (double)d);

            d = new S.Decimal(1.2345678901234e28);
            Assert((decimal)d == 1.2345678901234e28m);
            Assert(1.2345678901234e28 == (double)d);

            d = new S.Decimal(7.2345678901234e28);
            Assert((decimal)d == 7.2345678901234e28m);
            Assert(new S.Decimal((double)d) == d);

            d = new S.Decimal(1.2345678901234e-8);
            Assert((decimal)d == 1.2345678901234e-8m);

            d = new S.Decimal(1.2345678901234e-14);
            Assert((decimal)d == 1.2345678901234e-14m);
            Assert(1.2345678901234e-14 == (double)d);

            d = new S.Decimal(1.2342278901234e-25);
            Assert((decimal)d == 1.234e-25m);

            // test exceptions
            try
            {
                d = new S.Decimal(8e28);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new S.Decimal(8e48);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new S.Decimal(Double.NaN);
                Fail();
            } 
            catch (OverflowException) 
            {
            }

            try
            {
                d = new S.Decimal(Double.PositiveInfinity);
                Fail();
            } 
            catch (OverflowException) 
            {
            }
        }

        public void TestConstructDoubleRound()
        {
            decimal d;
            
            d = new S.Decimal(1765.231234567857);
            Assert(d == 1765.23123456786m);

            d = new S.Decimal(1765.2312345678554);
            Assert("failed banker's rule rounding test 1", d == 1765.23123456786m);
            Assert(1765.23123456786 == (double)d);

            d = new S.Decimal(1765.231234567853);
            Assert(d == 1765.23123456785m);

            d = new S.Decimal(1765.231234567847);
            Assert(d == 1765.23123456785m);

            d = new S.Decimal(1765.231234567843);
            Assert(d == 1765.23123456784m);

            d = new S.Decimal(1.765231234567857e-9);
            Assert(d == 1.76523123456786e-9m);

            d = new S.Decimal(1.7652312345678554e-9);
            Assert("failed banker's rule rounding test 3", d == 1.76523123456786e-9m);

            d = new S.Decimal(1.765231234567853e-9);
            Assert(d == 1.76523123456785e-9m);

            d = new S.Decimal(1.765231234567857e+24);
            Assert(d == 1.76523123456786e+24m);

            d = new S.Decimal(1.7652312345678554e+24);
            Assert("failed banker's rule rounding test 4", d == 1.76523123456786e+24m);

            d = new S.Decimal(1.765231234567853e+24);
            Assert(d == 1.76523123456785e+24m);
        }

        public void TestConstructDoubleRoundHard()
        {
            S.Decimal d;

            d = new S.Decimal(1765.2312345678454);
            // this case fails in Microsoft implementation
            // but it conforms to specification (rounding 15 digits according to banker's rule)
            Assert("failed banker's rule rounding test 2", d == 1765.23123456784m);
        }

        public void TestNegate()
        {
            decimal d;

            d = new S.Decimal(12345678);
            Assert((decimal)S.Decimal.Negate(d) == -12345678m);
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
                {S.Decimal.MaxValue, S.Decimal.MaxValue, S.Decimal.MaxValue},
                {S.Decimal.MinValue, S.Decimal.MinValue, S.Decimal.MinValue},
                {6.999999999m, 6m, 6m}, {-6.999999999m, -7m, -6m}, 
                {0.00001m, 0m, 0m}, {-0.00001m, -1m, 0m}
            };

            decimal d;
            
            for (int i = 0; i < dtab.GetLength(0); i++)
            {
                d = S.Decimal.Floor(dtab[i,0]);
                if (d != dtab[i,1]) 
                {
                    Fail("Floor: Floor(" + dtab[i,0] + ") != " + d);
                }
                d = S.Decimal.Truncate(dtab[i,0]);
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
                d = S.Decimal.Round(dtab[i,0], (int)dtab[i,1]);
                if (d != dtab[i,2]) 
                {
                    Fail("Round: Round(" + dtab[i,0] + "," + (int)dtab[i,1] + ") != " + d);
                }
            }
        }
    }

}