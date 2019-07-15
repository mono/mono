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

namespace MonoTests.System
{

    public enum TestResultInfo
    {
        Ok = 0,
        Overflow = 1,
        ReverseRound = 2,
        DivideByZero = 3,
        ReverseOverflow = 4
    }

    public struct TestResult
    {
        public TestResult(int i, decimal v)
        {
            info = (TestResultInfo) i;
            val = v;
        }

        public TestResultInfo info;
        public decimal val;
    }


    /// <summary>
    /// Tests for System.Decimal
    /// </summary>
    [TestFixture]
    public class DecimalTest2
    {
        private void ReportOpError(string msg, int i, int j, decimal d1, decimal d2, decimal d3, decimal d3b)
        {
		decimal delta = 0;
		try {
			delta = d3 - d3b;
		} catch (Exception e) {
			Assert.Fail ("ReportOpError: Unexpected exception on " + d3 + " - " + d3b + ". e:" + e);
		}
		Assert.Fail ("*** " + msg + " for d1=" + d1 + " i=" + i + " d2=" + d2 + " j=" + j + " d3=" + d3 + " d3b=" + d3b + "\n"
			+ "is:" + d3 +  "  must be:" + d3b + "  delta=" + (delta) + " == " + (d3 == d3b));
        }

	[Test]
	     
        public void TestCompare()
        {
            const int size = 14;
            decimal[] data = new decimal[size] {
                0m,	1m, -1m, 2m, 10m, 0.1m, 0.11m,
                79228162514264337593543950335m,
                -79228162514264337593543950335m,
                27703302467091960609331879.532m,
                -3203854.9559968181492513385018m,
                -3203854.9559968181492513385017m,
                -48466870444188873796420.0286m,
                -48466870444188873796420.02860m
            };

            short[,] cmpTable = new short[size,size] {
                {0,-1,1,-1,-1,-1,-1,-1,1,-1,1,1,1,1},
                {1,0,1,-1,-1,1,1,-1,1,-1,1,1,1,1},
                {-1,-1,0,-1,-1,-1,-1,-1,1,-1,1,1,1,1},
                {1,1,1,0,-1,1,1,-1,1,-1,1,1,1,1},
                {1,1,1,1,0,1,1,-1,1,-1,1,1,1,1},
                {1,-1,1,-1,-1,0,-1,-1,1,-1,1,1,1,1},
                {1,-1,1,-1,-1,1,0,-1,1,-1,1,1,1,1},
                {1,1,1,1,1,1,1,0,1,1,1,1,1,1},
                {-1,-1,-1,-1,-1,-1,-1,-1,0,-1,-1,-1,-1,-1},
                {1,1,1,1,1,1,1,-1,1,0,1,1,1,1},
                {-1,-1,-1,-1,-1,-1,-1,-1,1,-1,0,-1,1,1},
                {-1,-1,-1,-1,-1,-1,-1,-1,1,-1,1,0,1,1},
                {-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,0,0},
                {-1,-1,-1,-1,-1,-1,-1,-1,1,-1,-1,-1,0,0}
            };

            for (int i = 0; i < size; i++) 
            {
                Decimal d1 = data[i];
                for (int j = 0; j < size; j++) 
                {
                    Assert.IsTrue (cmpTable[i,j] == -cmpTable[j,i]);
                    int x = cmpTable[i,j];
                    Decimal d2 = data[j];

                    int y = Decimal.Compare(d1, d2);
                    if (y < 0) y = -1;
                    else if (y > 0) y = 1;
                    Assert.IsTrue (x == y);

                    y = d1.CompareTo(d2);
                    if (y < 0) y = -1;
                    else if (y > 0) y = 1;
                    Assert.IsTrue (x == y);

                    bool b = d1 < d2;
                    if (x != -1) b = !b;
                    Assert.IsTrue (b);

                    b = d1 <= d2;
                    if (x == 1) b = !b;
                    Assert.IsTrue (b);

                    b = d1 >= d2;
                    if (x == -1) b = !b;
                    Assert.IsTrue (b);

                    b = d1 > d2;
                    if (x != 1) b = !b;
                    Assert.IsTrue (b);

                    b = d1 == d2;
                    if (x != 0) b = !b;
                    Assert.IsTrue (b);

                    b = d1.Equals(d2);
                    if (x != 0) b = !b;
                    Assert.IsTrue (b);

                    b = Decimal.Equals(d1, d2);
                    if (x != 0) b = !b;
                    Assert.IsTrue (b);
                }
            }
        }

        private bool AreNotEqual(Decimal v1, Decimal v2)
        {
            return v1 != v2;
        }

	[Test]
	     
        public void TestRemainder()
        {
            Assert.IsTrue ((decimal)Decimal.Remainder(3.6m, 1.3m) == 1.0m);
            decimal res = 24420760848422211464106753.012m;
            decimal remainder = Decimal.Remainder(79228162514264337593543950335m, 27703302467091960609331879.53200m);
            if (AreNotEqual (res, remainder))
                Assert.AreEqual (res, remainder, "A02");

            Assert.IsTrue ((decimal)Decimal.Remainder(45937986975432m, 43987453m)
                == 42334506m);
            Assert.IsTrue ((decimal)Decimal.Remainder(45937986975000m, 5000m)
                == 0m);
            Assert.IsTrue ((decimal)Decimal.Remainder(-54789548973.6234m, 1.3356m) 
                == -0.1074m);
        }

	[Test]
	     
        public void TestAdd()
        {
            decimal[] args = auto_build2;
            TestResult[] trs = trAuto_Add_build2;
            int errOverflow = 0;
            int errOp = 0;
            int count = args.GetLength(0);
            int n = 0;
            for (int i = 0; i < count; i++) 
            {
                decimal d1 = args[i];
                for (int j = 0; j < count; j++, n++) 
                {
                    decimal d2 = args[j];
                    decimal d3 = 0;
                    decimal d4 = 0;
                    TestResult tr = trs[n];
                    try
                    {
                        d3 = Decimal.Add(d1, d2);
                        if (AreNotEqual (d3, tr.val))
                        {
                            if (tr.info == TestResultInfo.Overflow)
                            {
                                ReportOpError("Add: expected overflow", i, j, d1, d2, d3, tr.val);
                                errOverflow++;
                            }
                            else
                            {
                                ReportOpError("Add: result mismatch", i, j, d1, d2, d3, tr.val);
                                errOp++;
                            }
                        }
                        else if (tr.info == TestResultInfo.Ok)
                        {
                            d4 = Decimal.Subtract(d3, d2);
                            if (AreNotEqual (d4, d1))
                            {
                                ReportOpError("Subtract: result mismatch", i, j, d3, d2, d4, d1);
                                errOp++;
                            }
                        }
                    }
                    catch (OverflowException)
                    {
                        if (tr.info != TestResultInfo.Overflow) 
                        {
                            ReportOpError("Add: unexpected overflow", i, j, d1, d2, d3, 0);
                            errOverflow++;
                        }
                    }
                }
            }

            if (errOverflow + errOp > 0) 
            {
                Assert.Fail ("" + errOp + " wrong additions, " + errOverflow + " wrong overflows");
            }
        }

	[Test]
	     
        public void TestMult()
        {
            decimal[] args = auto_build2;
            TestResult[] trs = trAuto_Mult_build2;
            int errOverflow = 0;
            int errOp = 0;
            int count = args.GetLength(0);
            int n = 0;
            for (int i = 0; i < count; i++) 
            {
                decimal d1 = args[i];
                for (int j = 0; j < count; j++, n++) 
                {
                    decimal d2 = args[j];
                    decimal d3 = 0;
                    decimal d4 = 0;
                    TestResult tr = trs[n];
                    try
                    {
                        d3 = Decimal.Multiply(d1, d2);
                        if (AreNotEqual (d3, tr.val)) 
                        {
                            if (tr.info == TestResultInfo.Overflow)
                            {
                                ReportOpError("Mult: expected overflow", i, j, d1, d2, d3, tr.val);
                                errOverflow++;
                            }
                            else 
                            {
                                ReportOpError("Mult: result mismatch", i, j, d1, d2, d3, tr.val);
                                errOp++;
                            }
                        } 
                    }
                    catch (OverflowException)
                    {
                        if (tr.info != TestResultInfo.Overflow) 
                        {
                            ReportOpError("Mult: unexpected overflow", i, j, d1, d2, d3, 0);
                            errOverflow++;
                        }
                    }

                    if (d2 != 0 && tr.info != TestResultInfo.Overflow)
                    {
                        try 
                        {
                            d4 = Decimal.Divide(d3, d2);
                            if (AreNotEqual (d4, d1) && tr.info != TestResultInfo.ReverseRound)
                            {
                                ReportOpError("MultDiv: result mismatch", i, j, d3, d2, d4, d1);
                                errOp++;
                            }
                        }
                        catch (OverflowException)
                        {
                            if (tr.info != TestResultInfo.ReverseOverflow) 
                            {
                                ReportOpError("MultDiv: unexpected overflow", i, j, d3, d2, d4, d1);
                                errOverflow++;
                            }
                        }
                    }

                }
            }

            if (errOverflow + errOp > 0) 
            {
                Assert.Fail ("" + errOp + " wrong multiplications, " + errOverflow + " wrong overflows");
            }
        }

	// MS 1.x is being less precise than Mono (2 cases). MS 2.0 is correct.
	// Mono doesn't produce the same result for (i==21/j==3)
	[Test]
	     
        public void TestDiv()
        {
            decimal[] args = auto_build2;
            TestResult[] trs = trAuto_Div_build2;
            int errOverflow = 0;
            int errDivideByZero = 0;
            int errOp = 0;
            int count = args.GetLength(0);
            int n = 0;
            for (int i = 0; i < count; i++) 
            {
                decimal d1 = args[i];
                for (int j = 0; j < count; j++, n++) 
                {
                    decimal d2 = args[j];
                    decimal d3 = 0;
                    decimal d4 = 0;
                    TestResult tr = trs[n];
                    try
                    {
                        d3 = Decimal.Divide(d1, d2);
                        if (AreNotEqual (d3, tr.val)) 
                        {
                            if (tr.info == TestResultInfo.Overflow)
                            {
                                ReportOpError("Div: expected overflow", i, j, d1, d2, d3, tr.val);
                                errOverflow++;
                            }
                            else if (tr.info == TestResultInfo.DivideByZero)
                            {
                                ReportOpError("Div: expected divide by zero", i, j, d1, d2, d3, tr.val);
                                errDivideByZero++;
                            }
                            else 
                            {
				    // very small difference 0.00000000000000001 between Mono and MS
				    if ((i == 21) && (j == 3))
					    continue;
				    ReportOpError ("Div: result mismatch", i, j, d1, d2, d3, tr.val);
				    errOp++;
			    }
                        }
                    }
                    catch (OverflowException)
                    {
                        if (tr.info != TestResultInfo.Overflow) 
                        {
                            ReportOpError("Div: unexpected overflow", i, j, d1, d2, d3, 0);
                            errOverflow++;
                        }
                    }
                    catch (DivideByZeroException)
                    {
                        if (tr.info != TestResultInfo.DivideByZero) 
                        {
                            ReportOpError("Div: unexpected divide by zero", i, j, d1, d2, d3, 0);
                            errDivideByZero++;
                        }
                    }

                    if (d3 != 0)
                    {
                        try
                        {
                            d4 = Decimal.Multiply(d3, d2);
                            if (AreNotEqual(d4, d1) && tr.info != TestResultInfo.ReverseRound)
                            {
                                ReportOpError("DivMult: result mismatch", i, j, d3, d2, d4, d1);
                                errOp++;
                            }
                        }
                        catch (OverflowException)
                        {
                            if (tr.info != TestResultInfo.ReverseOverflow) 
                            {
                                ReportOpError("DivMult: unexpected overflow", i, j, d3, d2, d4, d1);
                                errOverflow++;
                            }
                        }
                    }
                }
            }

            if (errOverflow + errOp > 0) 
            {
                Assert.Fail ("" + errOp + " wrong division, " + errOverflow + " wrong overflows, " + errDivideByZero + " wrong divide by zero, ");
            }
        }

        #region Data


        // generated argument list build2
        decimal[] auto_build2 = new decimal[] {
	    0m, // 0
	    1m, // 1
	    -1m, // 2
	    2m, // 3
	    10m, // 4
	    0.1m, // 5
	    79228162514264337593543950335m, // 6
	    -79228162514264337593543950335m, // 7
	    27703302467091960609331879.532m, // 8
	    -3203854.9559968181492513385018m, // 9
	    -48466870444188873796420.028868m, // 10
	    -545193693242804794.30331374676m, // 11
	    0.7629234053338741809892531431m, // 12
	    -400453059665371395972.33474452m, // 13
	    222851627785191714190050.61676m, // 14
	    14246043379204153213661335.584m, // 15
	    -421123.30446308691436596648186m, // 16
	    24463288738299545.200508898642m, // 17
	    -5323259153836385912697776.001m, // 18
	    102801066199805834724673169.19m, // 19
	    7081320760.3793287174700927968m, // 20
	    415752273939.77704245656837041m, // 21
	    -6389392489892.6362673670820462m, // 22
	    442346282742915.0596416330681m, // 23
	    -512833780867323.89020837443764m, // 24
	    608940580690915704.1450897514m, // 25
	    -42535053313319986966115.037787m, // 26
	    -7808274522591953107485.8812311m, // 27
	    1037807626804273037330059471.7m, // 28
	    -4997122966.448652425771563042m, // 29
        };


        // generated result list build2
        TestResult[] trAuto_Add_build2 = new TestResult[] {
	    new TestResult(0, 0m), // 0 + 0
	    new TestResult(0, 1m), // 0 + 1
	    new TestResult(0, -1m), // 0 + 2
	    new TestResult(0, 2m), // 0 + 3
	    new TestResult(0, 10m), // 0 + 4
	    new TestResult(0, 0.1m), // 0 + 5
	    new TestResult(0, 79228162514264337593543950335m), // 0 + 6
	    new TestResult(0, -79228162514264337593543950335m), // 0 + 7
	    new TestResult(0, 27703302467091960609331879.532m), // 0 + 8
	    new TestResult(0, -3203854.9559968181492513385018m), // 0 + 9
	    new TestResult(0, -48466870444188873796420.028868m), // 0 + 10
	    new TestResult(0, -545193693242804794.30331374676m), // 0 + 11
	    new TestResult(0, 0.7629234053338741809892531431m), // 0 + 12
	    new TestResult(0, -400453059665371395972.33474452m), // 0 + 13
	    new TestResult(0, 222851627785191714190050.61676m), // 0 + 14
	    new TestResult(0, 14246043379204153213661335.584m), // 0 + 15
	    new TestResult(0, -421123.30446308691436596648186m), // 0 + 16
	    new TestResult(0, 24463288738299545.200508898642m), // 0 + 17
	    new TestResult(0, -5323259153836385912697776.001m), // 0 + 18
	    new TestResult(0, 102801066199805834724673169.19m), // 0 + 19
	    new TestResult(0, 7081320760.3793287174700927968m), // 0 + 20
	    new TestResult(0, 415752273939.77704245656837041m), // 0 + 21
	    new TestResult(0, -6389392489892.6362673670820462m), // 0 + 22
	    new TestResult(0, 442346282742915.0596416330681m), // 0 + 23
	    new TestResult(0, -512833780867323.89020837443764m), // 0 + 24
	    new TestResult(0, 608940580690915704.1450897514m), // 0 + 25
	    new TestResult(0, -42535053313319986966115.037787m), // 0 + 26
	    new TestResult(0, -7808274522591953107485.8812311m), // 0 + 27
	    new TestResult(0, 1037807626804273037330059471.7m), // 0 + 28
	    new TestResult(0, -4997122966.448652425771563042m), // 0 + 29
	    new TestResult(0, 1m), // 1 + 0
	    new TestResult(0, 2m), // 1 + 1
	    new TestResult(0, 0m), // 1 + 2
	    new TestResult(0, 3m), // 1 + 3
	    new TestResult(0, 11m), // 1 + 4
	    new TestResult(0, 1.1m), // 1 + 5
	    new TestResult(1, 0m), // 1 + 6
	    new TestResult(0, -79228162514264337593543950334m), // 1 + 7
	    new TestResult(0, 27703302467091960609331880.532m), // 1 + 8
	    new TestResult(0, -3203853.9559968181492513385018m), // 1 + 9
	    new TestResult(0, -48466870444188873796419.028868m), // 1 + 10
	    new TestResult(0, -545193693242804793.30331374676m), // 1 + 11
	    new TestResult(0, 1.7629234053338741809892531431m), // 1 + 12
	    new TestResult(0, -400453059665371395971.33474452m), // 1 + 13
	    new TestResult(0, 222851627785191714190051.61676m), // 1 + 14
	    new TestResult(0, 14246043379204153213661336.584m), // 1 + 15
	    new TestResult(0, -421122.30446308691436596648186m), // 1 + 16
	    new TestResult(0, 24463288738299546.200508898642m), // 1 + 17
	    new TestResult(0, -5323259153836385912697775.001m), // 1 + 18
	    new TestResult(0, 102801066199805834724673170.19m), // 1 + 19
	    new TestResult(0, 7081320761.3793287174700927968m), // 1 + 20
	    new TestResult(0, 415752273940.77704245656837041m), // 1 + 21
	    new TestResult(0, -6389392489891.6362673670820462m), // 1 + 22
	    new TestResult(0, 442346282742916.0596416330681m), // 1 + 23
	    new TestResult(0, -512833780867322.89020837443764m), // 1 + 24
	    new TestResult(0, 608940580690915705.1450897514m), // 1 + 25
	    new TestResult(0, -42535053313319986966114.037787m), // 1 + 26
	    new TestResult(0, -7808274522591953107484.8812311m), // 1 + 27
	    new TestResult(0, 1037807626804273037330059472.7m), // 1 + 28
	    new TestResult(0, -4997122965.448652425771563042m), // 1 + 29
	    new TestResult(0, -1m), // 2 + 0
	    new TestResult(0, 0m), // 2 + 1
	    new TestResult(0, -2m), // 2 + 2
	    new TestResult(0, 1m), // 2 + 3
	    new TestResult(0, 9m), // 2 + 4
	    new TestResult(0, -0.9m), // 2 + 5
	    new TestResult(0, 79228162514264337593543950334m), // 2 + 6
	    new TestResult(1, 0m), // 2 + 7
	    new TestResult(0, 27703302467091960609331878.532m), // 2 + 8
	    new TestResult(0, -3203855.9559968181492513385018m), // 2 + 9
	    new TestResult(0, -48466870444188873796421.028868m), // 2 + 10
	    new TestResult(0, -545193693242804795.30331374676m), // 2 + 11
	    new TestResult(0, -0.2370765946661258190107468569m), // 2 + 12
	    new TestResult(0, -400453059665371395973.33474452m), // 2 + 13
	    new TestResult(0, 222851627785191714190049.61676m), // 2 + 14
	    new TestResult(0, 14246043379204153213661334.584m), // 2 + 15
	    new TestResult(0, -421124.30446308691436596648186m), // 2 + 16
	    new TestResult(0, 24463288738299544.200508898642m), // 2 + 17
	    new TestResult(0, -5323259153836385912697777.001m), // 2 + 18
	    new TestResult(0, 102801066199805834724673168.19m), // 2 + 19
	    new TestResult(0, 7081320759.3793287174700927968m), // 2 + 20
	    new TestResult(0, 415752273938.77704245656837041m), // 2 + 21
	    new TestResult(0, -6389392489893.6362673670820462m), // 2 + 22
	    new TestResult(0, 442346282742914.0596416330681m), // 2 + 23
	    new TestResult(0, -512833780867324.89020837443764m), // 2 + 24
	    new TestResult(0, 608940580690915703.1450897514m), // 2 + 25
	    new TestResult(0, -42535053313319986966116.037787m), // 2 + 26
	    new TestResult(0, -7808274522591953107486.8812311m), // 2 + 27
	    new TestResult(0, 1037807626804273037330059470.7m), // 2 + 28
	    new TestResult(0, -4997122967.448652425771563042m), // 2 + 29
	    new TestResult(0, 2m), // 3 + 0
	    new TestResult(0, 3m), // 3 + 1
	    new TestResult(0, 1m), // 3 + 2
	    new TestResult(0, 4m), // 3 + 3
	    new TestResult(0, 12m), // 3 + 4
	    new TestResult(0, 2.1m), // 3 + 5
	    new TestResult(1, 0m), // 3 + 6
	    new TestResult(0, -79228162514264337593543950333m), // 3 + 7
	    new TestResult(0, 27703302467091960609331881.532m), // 3 + 8
	    new TestResult(0, -3203852.9559968181492513385018m), // 3 + 9
	    new TestResult(0, -48466870444188873796418.028868m), // 3 + 10
	    new TestResult(0, -545193693242804792.30331374676m), // 3 + 11
	    new TestResult(0, 2.7629234053338741809892531431m), // 3 + 12
	    new TestResult(0, -400453059665371395970.33474452m), // 3 + 13
	    new TestResult(0, 222851627785191714190052.61676m), // 3 + 14
	    new TestResult(0, 14246043379204153213661337.584m), // 3 + 15
	    new TestResult(0, -421121.30446308691436596648186m), // 3 + 16
	    new TestResult(0, 24463288738299547.200508898642m), // 3 + 17
	    new TestResult(0, -5323259153836385912697774.001m), // 3 + 18
	    new TestResult(0, 102801066199805834724673171.19m), // 3 + 19
	    new TestResult(0, 7081320762.3793287174700927968m), // 3 + 20
	    new TestResult(0, 415752273941.77704245656837041m), // 3 + 21
	    new TestResult(0, -6389392489890.6362673670820462m), // 3 + 22
	    new TestResult(0, 442346282742917.0596416330681m), // 3 + 23
	    new TestResult(0, -512833780867321.89020837443764m), // 3 + 24
	    new TestResult(0, 608940580690915706.1450897514m), // 3 + 25
	    new TestResult(0, -42535053313319986966113.037787m), // 3 + 26
	    new TestResult(0, -7808274522591953107483.8812311m), // 3 + 27
	    new TestResult(0, 1037807626804273037330059473.7m), // 3 + 28
	    new TestResult(0, -4997122964.448652425771563042m), // 3 + 29
	    new TestResult(0, 10m), // 4 + 0
	    new TestResult(0, 11m), // 4 + 1
	    new TestResult(0, 9m), // 4 + 2
	    new TestResult(0, 12m), // 4 + 3
	    new TestResult(0, 20m), // 4 + 4
	    new TestResult(0, 10.1m), // 4 + 5
	    new TestResult(1, 0m), // 4 + 6
	    new TestResult(0, -79228162514264337593543950325m), // 4 + 7
	    new TestResult(0, 27703302467091960609331889.532m), // 4 + 8
	    new TestResult(0, -3203844.9559968181492513385018m), // 4 + 9
	    new TestResult(0, -48466870444188873796410.028868m), // 4 + 10
	    new TestResult(0, -545193693242804784.30331374676m), // 4 + 11
	    new TestResult(0, 10.762923405333874180989253143m), // 4 + 12
	    new TestResult(0, -400453059665371395962.33474452m), // 4 + 13
	    new TestResult(0, 222851627785191714190060.61676m), // 4 + 14
	    new TestResult(0, 14246043379204153213661345.584m), // 4 + 15
	    new TestResult(0, -421113.30446308691436596648186m), // 4 + 16
	    new TestResult(0, 24463288738299555.200508898642m), // 4 + 17
	    new TestResult(0, -5323259153836385912697766.001m), // 4 + 18
	    new TestResult(0, 102801066199805834724673179.19m), // 4 + 19
	    new TestResult(0, 7081320770.3793287174700927968m), // 4 + 20
	    new TestResult(0, 415752273949.77704245656837041m), // 4 + 21
	    new TestResult(0, -6389392489882.6362673670820462m), // 4 + 22
	    new TestResult(0, 442346282742925.0596416330681m), // 4 + 23
	    new TestResult(0, -512833780867313.89020837443764m), // 4 + 24
	    new TestResult(0, 608940580690915714.1450897514m), // 4 + 25
	    new TestResult(0, -42535053313319986966105.037787m), // 4 + 26
	    new TestResult(0, -7808274522591953107475.8812311m), // 4 + 27
	    new TestResult(0, 1037807626804273037330059481.7m), // 4 + 28
	    new TestResult(0, -4997122956.448652425771563042m), // 4 + 29
	    new TestResult(0, 0.1m), // 5 + 0
	    new TestResult(0, 1.1m), // 5 + 1
	    new TestResult(0, -0.9m), // 5 + 2
	    new TestResult(0, 2.1m), // 5 + 3
	    new TestResult(0, 10.1m), // 5 + 4
	    new TestResult(0, 0.2m), // 5 + 5
	    new TestResult(2, 79228162514264337593543950335m), // 5 + 6
	    new TestResult(2, -79228162514264337593543950335m), // 5 + 7
	    new TestResult(0, 27703302467091960609331879.632m), // 5 + 8
	    new TestResult(0, -3203854.8559968181492513385018m), // 5 + 9
	    new TestResult(0, -48466870444188873796419.928868m), // 5 + 10
	    new TestResult(0, -545193693242804794.20331374676m), // 5 + 11
	    new TestResult(0, 0.8629234053338741809892531431m), // 5 + 12
	    new TestResult(0, -400453059665371395972.23474452m), // 5 + 13
	    new TestResult(0, 222851627785191714190050.71676m), // 5 + 14
	    new TestResult(0, 14246043379204153213661335.684m), // 5 + 15
	    new TestResult(0, -421123.20446308691436596648186m), // 5 + 16
	    new TestResult(0, 24463288738299545.300508898642m), // 5 + 17
	    new TestResult(0, -5323259153836385912697775.901m), // 5 + 18
	    new TestResult(0, 102801066199805834724673169.29m), // 5 + 19
	    new TestResult(0, 7081320760.4793287174700927968m), // 5 + 20
	    new TestResult(0, 415752273939.87704245656837041m), // 5 + 21
	    new TestResult(0, -6389392489892.5362673670820462m), // 5 + 22
	    new TestResult(0, 442346282742915.1596416330681m), // 5 + 23
	    new TestResult(0, -512833780867323.79020837443764m), // 5 + 24
	    new TestResult(0, 608940580690915704.2450897514m), // 5 + 25
	    new TestResult(0, -42535053313319986966114.937787m), // 5 + 26
	    new TestResult(0, -7808274522591953107485.7812311m), // 5 + 27
	    new TestResult(0, 1037807626804273037330059471.8m), // 5 + 28
	    new TestResult(0, -4997122966.348652425771563042m), // 5 + 29
	    new TestResult(0, 79228162514264337593543950335m), // 6 + 0
	    new TestResult(1, 0m), // 6 + 1
	    new TestResult(0, 79228162514264337593543950334m), // 6 + 2
	    new TestResult(1, 0m), // 6 + 3
	    new TestResult(1, 0m), // 6 + 4
	    new TestResult(0, 79228162514264337593543950335m), // 6 + 5
	    new TestResult(1, 0m), // 6 + 6
	    new TestResult(0, 0m), // 6 + 7
	    new TestResult(1, 0m), // 6 + 8
	    new TestResult(0, 79228162514264337593540746480m), // 6 + 9
	    new TestResult(0, 79228114047393893404670153915m), // 6 + 10
	    new TestResult(0, 79228162513719143900301145541m), // 6 + 11
	    new TestResult(1, 0m), // 6 + 12
	    new TestResult(0, 79228162113811277928172554363m), // 6 + 13
	    new TestResult(1, 0m), // 6 + 14
	    new TestResult(1, 0m), // 6 + 15
	    new TestResult(0, 79228162514264337593543529212m), // 6 + 16
	    new TestResult(1, 0m), // 6 + 17
	    new TestResult(0, 79222839255110501207631252559m), // 6 + 18
	    new TestResult(1, 0m), // 6 + 19
	    new TestResult(1, 0m), // 6 + 20
	    new TestResult(1, 0m), // 6 + 21
	    new TestResult(0, 79228162514264331204151460442m), // 6 + 22
	    new TestResult(1, 0m), // 6 + 23
	    new TestResult(0, 79228162514263824759763083011m), // 6 + 24
	    new TestResult(1, 0m), // 6 + 25
	    new TestResult(0, 79228119979211024273556984220m), // 6 + 26
	    new TestResult(0, 79228154705989815001590842849m), // 6 + 27
	    new TestResult(1, 0m), // 6 + 28
	    new TestResult(0, 79228162514264337588546827369m), // 6 + 29
	    new TestResult(0, -79228162514264337593543950335m), // 7 + 0
	    new TestResult(0, -79228162514264337593543950334m), // 7 + 1
	    new TestResult(1, 0m), // 7 + 2
	    new TestResult(0, -79228162514264337593543950333m), // 7 + 3
	    new TestResult(0, -79228162514264337593543950325m), // 7 + 4
	    new TestResult(0, -79228162514264337593543950335m), // 7 + 5
	    new TestResult(0, 0m), // 7 + 6
	    new TestResult(1, 0m), // 7 + 7
	    new TestResult(0, -79200459211797245632934618455m), // 7 + 8
	    new TestResult(1, 0m), // 7 + 9
	    new TestResult(1, 0m), // 7 + 10
	    new TestResult(1, 0m), // 7 + 11
	    new TestResult(0, -79228162514264337593543950334m), // 7 + 12
	    new TestResult(1, 0m), // 7 + 13
	    new TestResult(0, -79227939662636552401829760284m), // 7 + 14
	    new TestResult(0, -79213916470885133440330288999m), // 7 + 15
	    new TestResult(1, 0m), // 7 + 16
	    new TestResult(0, -79228162514239874304805650790m), // 7 + 17
	    new TestResult(1, 0m), // 7 + 18
	    new TestResult(0, -79125361448064531758819277166m), // 7 + 19
	    new TestResult(0, -79228162514264337586462629575m), // 7 + 20
	    new TestResult(0, -79228162514264337177791676395m), // 7 + 21
	    new TestResult(1, 0m), // 7 + 22
	    new TestResult(0, -79228162514263895247261207420m), // 7 + 23
	    new TestResult(1, 0m), // 7 + 24
	    new TestResult(0, -79228162513655397012853034631m), // 7 + 25
	    new TestResult(1, 0m), // 7 + 26
	    new TestResult(1, 0m), // 7 + 27
	    new TestResult(0, -78190354887460064556213890863m), // 7 + 28
	    new TestResult(1, 0m), // 7 + 29
	    new TestResult(0, 27703302467091960609331879.532m), // 8 + 0
	    new TestResult(0, 27703302467091960609331880.532m), // 8 + 1
	    new TestResult(0, 27703302467091960609331878.532m), // 8 + 2
	    new TestResult(0, 27703302467091960609331881.532m), // 8 + 3
	    new TestResult(0, 27703302467091960609331889.532m), // 8 + 4
	    new TestResult(0, 27703302467091960609331879.632m), // 8 + 5
	    new TestResult(1, 0m), // 8 + 6
	    new TestResult(2, -79200459211797245632934618455m), // 8 + 7
	    new TestResult(0, 55406604934183921218663759.064m), // 8 + 8
	    new TestResult(0, 27703302467091960606128024.576m), // 8 + 9
	    new TestResult(0, 27654835596647771735535459.503m), // 8 + 10
	    new TestResult(0, 27703301921898267366527085.229m), // 8 + 11
	    new TestResult(0, 27703302467091960609331880.295m), // 8 + 12
	    new TestResult(0, 27702902014032295237935907.197m), // 8 + 13
	    new TestResult(0, 27926154094877152323521930.149m), // 8 + 14
	    new TestResult(0, 41949345846296113822993215.116m), // 8 + 15
	    new TestResult(0, 27703302467091960608910756.228m), // 8 + 16
	    new TestResult(0, 27703302491555249347631424.733m), // 8 + 17
	    new TestResult(0, 22380043313255574696634103.531m), // 8 + 18
	    new TestResult(2, 130504368666897795334005048.72m), // 8 + 19
	    new TestResult(0, 27703302467091967690652639.911m), // 8 + 20
	    new TestResult(0, 27703302467092376361605819.309m), // 8 + 21
	    new TestResult(0, 27703302467085571216841986.896m), // 8 + 22
	    new TestResult(0, 27703302467534306892074794.592m), // 8 + 23
	    new TestResult(0, 27703302466579126828464555.642m), // 8 + 24
	    new TestResult(0, 27703303076032541300247583.677m), // 8 + 25
	    new TestResult(0, 27660767413778640622365764.494m), // 8 + 26
	    new TestResult(0, 27695494192569368656224393.651m), // 8 + 27
	    new TestResult(2, 1065510929271364997939391351.2m), // 8 + 28
	    new TestResult(0, 27703302467091955612208913.083m), // 8 + 29
	    new TestResult(0, -3203854.9559968181492513385018m), // 9 + 0
	    new TestResult(0, -3203853.9559968181492513385018m), // 9 + 1
	    new TestResult(0, -3203855.9559968181492513385018m), // 9 + 2
	    new TestResult(0, -3203852.9559968181492513385018m), // 9 + 3
	    new TestResult(0, -3203844.9559968181492513385018m), // 9 + 4
	    new TestResult(0, -3203854.8559968181492513385018m), // 9 + 5
	    new TestResult(2, 79228162514264337593540746480m), // 9 + 6
	    new TestResult(1, 0m), // 9 + 7
	    new TestResult(2, 27703302467091960606128024.576m), // 9 + 8
	    new TestResult(0, -6407709.9119936362985026770036m), // 9 + 9
	    new TestResult(2, -48466870444188877000274.984865m), // 9 + 10
	    new TestResult(2, -545193693246008649.25931056491m), // 9 + 11
	    new TestResult(0, -3203854.1930734128153771575125m), // 9 + 12
	    new TestResult(2, -400453059665374599827.29074134m), // 9 + 13
	    new TestResult(2, 222851627785191710986195.66076m), // 9 + 14
	    new TestResult(2, 14246043379204153210457480.628m), // 9 + 15
	    new TestResult(0, -3624978.2604599050636173049837m), // 9 + 16
	    new TestResult(2, 24463288735095690.244512080493m), // 9 + 17
	    new TestResult(2, -5323259153836385915901630.957m), // 9 + 18
	    new TestResult(2, 102801066199805834721469314.23m), // 9 + 19
	    new TestResult(2, 7078116905.4233318993208414583m), // 9 + 20
	    new TestResult(2, 415749070084.82104563841911907m), // 9 + 21
	    new TestResult(2, -6389395693747.5922641852312975m), // 9 + 22
	    new TestResult(2, 442346279539060.10364481491885m), // 9 + 23
	    new TestResult(2, -512833784071178.84620519258689m), // 9 + 24
	    new TestResult(2, 608940580687711849.18909293325m), // 9 + 25
	    new TestResult(2, -42535053313319990169969.993784m), // 9 + 26
	    new TestResult(2, -7808274522591956311340.8372279m), // 9 + 27
	    new TestResult(2, 1037807626804273037326855616.7m), // 9 + 28
	    new TestResult(2, -5000326821.4046492439208143805m), // 9 + 29
	    new TestResult(0, -48466870444188873796420.028868m), // 10 + 0
	    new TestResult(0, -48466870444188873796419.028868m), // 10 + 1
	    new TestResult(0, -48466870444188873796421.028868m), // 10 + 2
	    new TestResult(0, -48466870444188873796418.028868m), // 10 + 3
	    new TestResult(0, -48466870444188873796410.028868m), // 10 + 4
	    new TestResult(0, -48466870444188873796419.928868m), // 10 + 5
	    new TestResult(2, 79228114047393893404670153915m), // 10 + 6
	    new TestResult(1, 0m), // 10 + 7
	    new TestResult(2, 27654835596647771735535459.503m), // 10 + 8
	    new TestResult(0, -48466870444188877000274.984865m), // 10 + 9
	    new TestResult(2, -96933740888377747592840.05774m), // 10 + 10
	    new TestResult(0, -48467415637882116601214.332182m), // 10 + 11
	    new TestResult(0, -48466870444188873796419.265945m), // 10 + 12
	    new TestResult(0, -48867323503854245192392.363613m), // 10 + 13
	    new TestResult(2, 174384757341002840393630.58789m), // 10 + 14
	    new TestResult(2, 14197576508759964339864915.555m), // 10 + 15
	    new TestResult(0, -48466870444188874217543.333331m), // 10 + 16
	    new TestResult(0, -48466845980900135496874.828359m), // 10 + 17
	    new TestResult(2, -5371726024280574786494196.0299m), // 10 + 18
	    new TestResult(2, 102752599329361645850876749.16m), // 10 + 19
	    new TestResult(0, -48466870444181792475659.649539m), // 10 + 20
	    new TestResult(0, -48466870443773121522480.251826m), // 10 + 21
	    new TestResult(0, -48466870450578266286312.665135m), // 10 + 22
	    new TestResult(0, -48466870001842591053504.969226m), // 10 + 23
	    new TestResult(0, -48466870957022654663743.919076m), // 10 + 24
	    new TestResult(0, -48466261503608182880715.883778m), // 10 + 25
	    new TestResult(2, -91001923757508860762535.06666m), // 10 + 26
	    new TestResult(0, -56275144966780826903905.910099m), // 10 + 27
	    new TestResult(2, 1037759159933828848456263051.7m), // 10 + 28
	    new TestResult(0, -48466870444193870919386.47752m), // 10 + 29
	    new TestResult(0, -545193693242804794.30331374676m), // 11 + 0
	    new TestResult(0, -545193693242804793.30331374676m), // 11 + 1
	    new TestResult(0, -545193693242804795.30331374676m), // 11 + 2
	    new TestResult(0, -545193693242804792.30331374676m), // 11 + 3
	    new TestResult(0, -545193693242804784.30331374676m), // 11 + 4
	    new TestResult(0, -545193693242804794.20331374676m), // 11 + 5
	    new TestResult(2, 79228162513719143900301145541m), // 11 + 6
	    new TestResult(1, 0m), // 11 + 7
	    new TestResult(2, 27703301921898267366527085.229m), // 11 + 8
	    new TestResult(0, -545193693246008649.25931056491m), // 11 + 9
	    new TestResult(2, -48467415637882116601214.332182m), // 11 + 10
	    new TestResult(2, -1090387386485609588.6066274935m), // 11 + 11
	    new TestResult(0, -545193693242804793.54039034143m), // 11 + 12
	    new TestResult(2, -400998253358614200766.63805827m), // 11 + 13
	    new TestResult(2, 222851082591498471385256.31345m), // 11 + 14
	    new TestResult(2, 14246042834010459970856541.281m), // 11 + 15
	    new TestResult(0, -545193693243225917.60777683367m), // 11 + 16
	    new TestResult(0, -520730404504505249.10280484812m), // 11 + 17
	    new TestResult(2, -5323259699030079155502570.3043m), // 11 + 18
	    new TestResult(2, 102801065654612141481868374.89m), // 11 + 19
	    new TestResult(0, -545193686161484033.92398502929m), // 11 + 20
	    new TestResult(0, -545193277490530854.52627129019m), // 11 + 21
	    new TestResult(0, -545200082635294686.93958111384m), // 11 + 22
	    new TestResult(0, -544751346960061879.24367211369m), // 11 + 23
	    new TestResult(0, -545706527023672118.1935221212m), // 11 + 24
	    new TestResult(0, 63746887448110909.84177600464m), // 11 + 25
	    new TestResult(2, -42535598507013229770909.341101m), // 11 + 26
	    new TestResult(2, -7808819716285195912280.1845448m), // 11 + 27
	    new TestResult(2, 1037807626259079344087254677.4m), // 11 + 28
	    new TestResult(0, -545193698239927760.75196617253m), // 11 + 29
	    new TestResult(0, 0.7629234053338741809892531431m), // 12 + 0
	    new TestResult(0, 1.7629234053338741809892531431m), // 12 + 1
	    new TestResult(0, -0.2370765946661258190107468569m), // 12 + 2
	    new TestResult(0, 2.7629234053338741809892531431m), // 12 + 3
	    new TestResult(2, 10.762923405333874180989253143m), // 12 + 4
	    new TestResult(0, 0.8629234053338741809892531431m), // 12 + 5
	    new TestResult(1, 0m), // 12 + 6
	    new TestResult(2, -79228162514264337593543950334m), // 12 + 7
	    new TestResult(2, 27703302467091960609331880.295m), // 12 + 8
	    new TestResult(2, -3203854.1930734128153771575125m), // 12 + 9
	    new TestResult(2, -48466870444188873796419.265945m), // 12 + 10
	    new TestResult(2, -545193693242804793.54039034143m), // 12 + 11
	    new TestResult(0, 1.5258468106677483619785062862m), // 12 + 12
	    new TestResult(2, -400453059665371395971.57182111m), // 12 + 13
	    new TestResult(2, 222851627785191714190051.37968m), // 12 + 14
	    new TestResult(2, 14246043379204153213661336.347m), // 12 + 15
	    new TestResult(2, -421122.54153968158049178549261m), // 12 + 16
	    new TestResult(2, 24463288738299545.963432303976m), // 12 + 17
	    new TestResult(2, -5323259153836385912697775.2381m), // 12 + 18
	    new TestResult(2, 102801066199805834724673169.95m), // 12 + 19
	    new TestResult(2, 7081320761.1422521228039669778m), // 12 + 20
	    new TestResult(2, 415752273940.53996586190224459m), // 12 + 21
	    new TestResult(2, -6389392489891.873343961748172m), // 12 + 22
	    new TestResult(2, 442346282742915.82256503840197m), // 12 + 23
	    new TestResult(2, -512833780867323.12728496910377m), // 12 + 24
	    new TestResult(2, 608940580690915704.90801315673m), // 12 + 25
	    new TestResult(2, -42535053313319986966114.274864m), // 12 + 26
	    new TestResult(2, -7808274522591953107485.1183077m), // 12 + 27
	    new TestResult(2, 1037807626804273037330059472.5m), // 12 + 28
	    new TestResult(2, -4997122965.685729020437688861m), // 12 + 29
	    new TestResult(0, -400453059665371395972.33474452m), // 13 + 0
	    new TestResult(0, -400453059665371395971.33474452m), // 13 + 1
	    new TestResult(0, -400453059665371395973.33474452m), // 13 + 2
	    new TestResult(0, -400453059665371395970.33474452m), // 13 + 3
	    new TestResult(0, -400453059665371395962.33474452m), // 13 + 4
	    new TestResult(0, -400453059665371395972.23474452m), // 13 + 5
	    new TestResult(2, 79228162113811277928172554363m), // 13 + 6
	    new TestResult(1, 0m), // 13 + 7
	    new TestResult(2, 27702902014032295237935907.197m), // 13 + 8
	    new TestResult(0, -400453059665374599827.29074134m), // 13 + 9
	    new TestResult(2, -48867323503854245192392.363613m), // 13 + 10
	    new TestResult(0, -400998253358614200766.63805827m), // 13 + 11
	    new TestResult(0, -400453059665371395971.57182111m), // 13 + 12
	    new TestResult(2, -800906119330742791944.669489m), // 13 + 13
	    new TestResult(2, 222451174725526342794078.28202m), // 13 + 14
	    new TestResult(2, 14245642926144487842265363.249m), // 13 + 15
	    new TestResult(0, -400453059665371817095.63920761m), // 13 + 16
	    new TestResult(0, -400428596376633096427.13423562m), // 13 + 17
	    new TestResult(2, -5323659606896051284093748.3357m), // 13 + 18
	    new TestResult(2, 102800665746746169353277196.86m), // 13 + 19
	    new TestResult(0, -400453059658290075211.9554158m), // 13 + 20
	    new TestResult(0, -400453059249619122032.55770206m), // 13 + 21
	    new TestResult(0, -400453066054763885864.97101189m), // 13 + 22
	    new TestResult(0, -400452617319088653057.27510289m), // 13 + 23
	    new TestResult(0, -400453572499152263296.22495289m), // 13 + 24
	    new TestResult(0, -399844119084680480268.18965477m), // 13 + 25
	    new TestResult(2, -42935506372985358362087.372532m), // 13 + 26
	    new TestResult(2, -8208727582257324503458.215976m), // 13 + 27
	    new TestResult(2, 1037807226351213371958663499.4m), // 13 + 28
	    new TestResult(0, -400453059670368518938.78339695m), // 13 + 29
	    new TestResult(0, 222851627785191714190050.61676m), // 14 + 0
	    new TestResult(0, 222851627785191714190051.61676m), // 14 + 1
	    new TestResult(0, 222851627785191714190049.61676m), // 14 + 2
	    new TestResult(0, 222851627785191714190052.61676m), // 14 + 3
	    new TestResult(0, 222851627785191714190060.61676m), // 14 + 4
	    new TestResult(0, 222851627785191714190050.71676m), // 14 + 5
	    new TestResult(1, 0m), // 14 + 6
	    new TestResult(2, -79227939662636552401829760284m), // 14 + 7
	    new TestResult(2, 27926154094877152323521930.149m), // 14 + 8
	    new TestResult(0, 222851627785191710986195.66076m), // 14 + 9
	    new TestResult(0, 174384757341002840393630.58789m), // 14 + 10
	    new TestResult(0, 222851082591498471385256.31345m), // 14 + 11
	    new TestResult(0, 222851627785191714190051.37968m), // 14 + 12
	    new TestResult(0, 222451174725526342794078.28202m), // 14 + 13
	    new TestResult(0, 445703255570383428380101.23352m), // 14 + 14
	    new TestResult(2, 14468895006989344927851386.201m), // 14 + 15
	    new TestResult(0, 222851627785191713768927.3123m), // 14 + 16
	    new TestResult(0, 222851652248480452489595.81727m), // 14 + 17
	    new TestResult(2, -5100407526051194198507725.3842m), // 14 + 18
	    new TestResult(2, 103023917827591026438863219.81m), // 14 + 19
	    new TestResult(0, 222851627785198795510810.99609m), // 14 + 20
	    new TestResult(0, 222851627785607466463990.3938m), // 14 + 21
	    new TestResult(0, 222851627778802321700157.98049m), // 14 + 22
	    new TestResult(0, 222851628227537996932965.6764m), // 14 + 23
	    new TestResult(0, 222851627272357933322726.72655m), // 14 + 24
	    new TestResult(0, 222852236725772405105754.76185m), // 14 + 25
	    new TestResult(0, 180316574471871727223935.57897m), // 14 + 26
	    new TestResult(0, 215043353262599761082564.73553m), // 14 + 27
	    new TestResult(2, 1038030478432058229044249522.3m), // 14 + 28
	    new TestResult(0, 222851627785186717067084.16811m), // 14 + 29
	    new TestResult(0, 14246043379204153213661335.584m), // 15 + 0
	    new TestResult(0, 14246043379204153213661336.584m), // 15 + 1
	    new TestResult(0, 14246043379204153213661334.584m), // 15 + 2
	    new TestResult(0, 14246043379204153213661337.584m), // 15 + 3
	    new TestResult(0, 14246043379204153213661345.584m), // 15 + 4
	    new TestResult(0, 14246043379204153213661335.684m), // 15 + 5
	    new TestResult(1, 0m), // 15 + 6
	    new TestResult(2, -79213916470885133440330288999m), // 15 + 7
	    new TestResult(0, 41949345846296113822993215.116m), // 15 + 8
	    new TestResult(0, 14246043379204153210457480.628m), // 15 + 9
	    new TestResult(0, 14197576508759964339864915.555m), // 15 + 10
	    new TestResult(0, 14246042834010459970856541.281m), // 15 + 11
	    new TestResult(0, 14246043379204153213661336.347m), // 15 + 12
	    new TestResult(0, 14245642926144487842265363.249m), // 15 + 13
	    new TestResult(0, 14468895006989344927851386.201m), // 15 + 14
	    new TestResult(0, 28492086758408306427322671.168m), // 15 + 15
	    new TestResult(0, 14246043379204153213240212.28m), // 15 + 16
	    new TestResult(0, 14246043403667441951960880.785m), // 15 + 17
	    new TestResult(0, 8922784225367767300963559.583m), // 15 + 18
	    new TestResult(2, 117047109579009987938334504.77m), // 15 + 19
	    new TestResult(0, 14246043379204160294982095.963m), // 15 + 20
	    new TestResult(0, 14246043379204568965935275.361m), // 15 + 21
	    new TestResult(0, 14246043379197763821171442.948m), // 15 + 22
	    new TestResult(0, 14246043379646499496404250.644m), // 15 + 23
	    new TestResult(0, 14246043378691319432794011.694m), // 15 + 24
	    new TestResult(0, 14246043988144733904577039.729m), // 15 + 25
	    new TestResult(0, 14203508325890833226695220.546m), // 15 + 26
	    new TestResult(0, 14238235104681561260553849.703m), // 15 + 27
	    new TestResult(2, 1052053670183477190543720807.3m), // 15 + 28
	    new TestResult(0, 14246043379204148216538369.135m), // 15 + 29
	    new TestResult(0, -421123.30446308691436596648186m), // 16 + 0
	    new TestResult(0, -421122.30446308691436596648186m), // 16 + 1
	    new TestResult(0, -421124.30446308691436596648186m), // 16 + 2
	    new TestResult(0, -421121.30446308691436596648186m), // 16 + 3
	    new TestResult(0, -421113.30446308691436596648186m), // 16 + 4
	    new TestResult(0, -421123.20446308691436596648186m), // 16 + 5
	    new TestResult(2, 79228162514264337593543529212m), // 16 + 6
	    new TestResult(1, 0m), // 16 + 7
	    new TestResult(2, 27703302467091960608910756.228m), // 16 + 8
	    new TestResult(2, -3624978.2604599050636173049837m), // 16 + 9
	    new TestResult(2, -48466870444188874217543.333331m), // 16 + 10
	    new TestResult(2, -545193693243225917.60777683367m), // 16 + 11
	    new TestResult(0, -421122.54153968158049178549261m), // 16 + 12
	    new TestResult(2, -400453059665371817095.63920761m), // 16 + 13
	    new TestResult(2, 222851627785191713768927.3123m), // 16 + 14
	    new TestResult(2, 14246043379204153213240212.28m), // 16 + 15
	    new TestResult(2, -842246.6089261738287319329637m), // 16 + 16
	    new TestResult(2, 24463288737878421.896045811728m), // 16 + 17
	    new TestResult(2, -5323259153836385913118899.3055m), // 16 + 18
	    new TestResult(2, 102801066199805834724252045.89m), // 16 + 19
	    new TestResult(2, 7080899637.0748656305557268303m), // 16 + 20
	    new TestResult(2, 415751852816.47257936965400444m), // 16 + 21
	    new TestResult(2, -6389392911015.9407304539964122m), // 16 + 22
	    new TestResult(2, 442346282321791.75517854615373m), // 16 + 23
	    new TestResult(2, -512833781288447.19467146135201m), // 16 + 24
	    new TestResult(2, 608940580690494580.84062666449m), // 16 + 25
	    new TestResult(2, -42535053313319987387238.34225m), // 16 + 26
	    new TestResult(2, -7808274522591953528609.1856942m), // 16 + 27
	    new TestResult(2, 1037807626804273037329638348.4m), // 16 + 28
	    new TestResult(2, -4997544089.7531155126859290085m), // 16 + 29
	    new TestResult(0, 24463288738299545.200508898642m), // 17 + 0
	    new TestResult(0, 24463288738299546.200508898642m), // 17 + 1
	    new TestResult(0, 24463288738299544.200508898642m), // 17 + 2
	    new TestResult(0, 24463288738299547.200508898642m), // 17 + 3
	    new TestResult(0, 24463288738299555.200508898642m), // 17 + 4
	    new TestResult(0, 24463288738299545.300508898642m), // 17 + 5
	    new TestResult(1, 0m), // 17 + 6
	    new TestResult(2, -79228162514239874304805650790m), // 17 + 7
	    new TestResult(2, 27703302491555249347631424.733m), // 17 + 8
	    new TestResult(0, 24463288735095690.244512080493m), // 17 + 9
	    new TestResult(2, -48466845980900135496874.828359m), // 17 + 10
	    new TestResult(2, -520730404504505249.10280484812m), // 17 + 11
	    new TestResult(0, 24463288738299545.963432303976m), // 17 + 12
	    new TestResult(2, -400428596376633096427.13423562m), // 17 + 13
	    new TestResult(2, 222851652248480452489595.81727m), // 17 + 14
	    new TestResult(2, 14246043403667441951960880.785m), // 17 + 15
	    new TestResult(0, 24463288737878421.896045811728m), // 17 + 16
	    new TestResult(0, 48926577476599090.401017797284m), // 17 + 17
	    new TestResult(2, -5323259129373097174398230.8005m), // 17 + 18
	    new TestResult(2, 102801066224269123462972714.39m), // 17 + 19
	    new TestResult(0, 24463295819620305.579837616112m), // 17 + 20
	    new TestResult(0, 24463704490573484.97755135521m), // 17 + 21
	    new TestResult(0, 24456899345809652.56424153156m), // 17 + 22
	    new TestResult(0, 24905635021042460.26015053171m), // 17 + 23
	    new TestResult(0, 23950454957432221.310300524204m), // 17 + 24
	    new TestResult(2, 633403869429215249.34559865004m), // 17 + 25
	    new TestResult(2, -42535028850031248666569.837278m), // 17 + 26
	    new TestResult(2, -7808250059303214807940.6807222m), // 17 + 27
	    new TestResult(2, 1037807626828736326068359016.9m), // 17 + 28
	    new TestResult(0, 24463283741176578.75185647287m), // 17 + 29
	    new TestResult(0, -5323259153836385912697776.001m), // 18 + 0
	    new TestResult(0, -5323259153836385912697775.001m), // 18 + 1
	    new TestResult(0, -5323259153836385912697777.001m), // 18 + 2
	    new TestResult(0, -5323259153836385912697774.001m), // 18 + 3
	    new TestResult(0, -5323259153836385912697766.001m), // 18 + 4
	    new TestResult(0, -5323259153836385912697775.901m), // 18 + 5
	    new TestResult(2, 79222839255110501207631252559m), // 18 + 6
	    new TestResult(1, 0m), // 18 + 7
	    new TestResult(0, 22380043313255574696634103.531m), // 18 + 8
	    new TestResult(0, -5323259153836385915901630.957m), // 18 + 9
	    new TestResult(0, -5371726024280574786494196.0299m), // 18 + 10
	    new TestResult(0, -5323259699030079155502570.3043m), // 18 + 11
	    new TestResult(0, -5323259153836385912697775.2381m), // 18 + 12
	    new TestResult(0, -5323659606896051284093748.3357m), // 18 + 13
	    new TestResult(0, -5100407526051194198507725.3842m), // 18 + 14
	    new TestResult(0, 8922784225367767300963559.583m), // 18 + 15
	    new TestResult(0, -5323259153836385913118899.3055m), // 18 + 16
	    new TestResult(0, -5323259129373097174398230.8005m), // 18 + 17
	    new TestResult(0, -10646518307672771825395552.002m), // 18 + 18
	    new TestResult(2, 97477807045969448811975393.19m), // 18 + 19
	    new TestResult(0, -5323259153836378831377015.6217m), // 18 + 20
	    new TestResult(0, -5323259153835970160423836.224m), // 18 + 21
	    new TestResult(0, -5323259153842775305187668.6373m), // 18 + 22
	    new TestResult(0, -5323259153394039629954860.9414m), // 18 + 23
	    new TestResult(0, -5323259154349219693565099.8912m), // 18 + 24
	    new TestResult(0, -5323258544895805221782071.8559m), // 18 + 25
	    new TestResult(0, -5365794207149705899663891.0388m), // 18 + 26
	    new TestResult(0, -5331067428358977865805261.8822m), // 18 + 27
	    new TestResult(2, 1032484367650436651417361695.7m), // 18 + 28
	    new TestResult(0, -5323259153836390909820742.4497m), // 18 + 29
	    new TestResult(0, 102801066199805834724673169.19m), // 19 + 0
	    new TestResult(0, 102801066199805834724673170.19m), // 19 + 1
	    new TestResult(0, 102801066199805834724673168.19m), // 19 + 2
	    new TestResult(0, 102801066199805834724673171.19m), // 19 + 3
	    new TestResult(0, 102801066199805834724673179.19m), // 19 + 4
	    new TestResult(0, 102801066199805834724673169.29m), // 19 + 5
	    new TestResult(1, 0m), // 19 + 6
	    new TestResult(2, -79125361448064531758819277166m), // 19 + 7
	    new TestResult(0, 130504368666897795334005048.72m), // 19 + 8
	    new TestResult(0, 102801066199805834721469314.23m), // 19 + 9
	    new TestResult(0, 102752599329361645850876749.16m), // 19 + 10
	    new TestResult(0, 102801065654612141481868374.89m), // 19 + 11
	    new TestResult(0, 102801066199805834724673169.95m), // 19 + 12
	    new TestResult(0, 102800665746746169353277196.86m), // 19 + 13
	    new TestResult(0, 103023917827591026438863219.81m), // 19 + 14
	    new TestResult(0, 117047109579009987938334504.77m), // 19 + 15
	    new TestResult(0, 102801066199805834724252045.89m), // 19 + 16
	    new TestResult(0, 102801066224269123462972714.39m), // 19 + 17
	    new TestResult(0, 97477807045969448811975393.19m), // 19 + 18
	    new TestResult(0, 205602132399611669449346338.38m), // 19 + 19
	    new TestResult(0, 102801066199805841805993929.57m), // 19 + 20
	    new TestResult(0, 102801066199806250476947108.97m), // 19 + 21
	    new TestResult(0, 102801066199799445332183276.55m), // 19 + 22
	    new TestResult(0, 102801066200248181007416084.25m), // 19 + 23
	    new TestResult(0, 102801066199293000943805845.3m), // 19 + 24
	    new TestResult(0, 102801066808746415415588873.34m), // 19 + 25
	    new TestResult(0, 102758531146492514737707054.15m), // 19 + 26
	    new TestResult(0, 102793257925283242771565683.31m), // 19 + 27
	    new TestResult(2, 1140608693004078872054732640.9m), // 19 + 28
	    new TestResult(0, 102801066199805829727550202.74m), // 19 + 29
	    new TestResult(0, 7081320760.3793287174700927968m), // 20 + 0
	    new TestResult(0, 7081320761.3793287174700927968m), // 20 + 1
	    new TestResult(0, 7081320759.3793287174700927968m), // 20 + 2
	    new TestResult(0, 7081320762.3793287174700927968m), // 20 + 3
	    new TestResult(0, 7081320770.3793287174700927968m), // 20 + 4
	    new TestResult(0, 7081320760.4793287174700927968m), // 20 + 5
	    new TestResult(1, 0m), // 20 + 6
	    new TestResult(2, -79228162514264337586462629575m), // 20 + 7
	    new TestResult(2, 27703302467091967690652639.911m), // 20 + 8
	    new TestResult(0, 7078116905.4233318993208414583m), // 20 + 9
	    new TestResult(2, -48466870444181792475659.649539m), // 20 + 10
	    new TestResult(2, -545193686161484033.92398502929m), // 20 + 11
	    new TestResult(0, 7081320761.1422521228039669778m), // 20 + 12
	    new TestResult(2, -400453059658290075211.9554158m), // 20 + 13
	    new TestResult(2, 222851627785198795510810.99609m), // 20 + 14
	    new TestResult(2, 14246043379204160294982095.963m), // 20 + 15
	    new TestResult(0, 7080899637.0748656305557268303m), // 20 + 16
	    new TestResult(2, 24463295819620305.579837616112m), // 20 + 17
	    new TestResult(2, -5323259153836378831377015.6217m), // 20 + 18
	    new TestResult(2, 102801066199805841805993929.57m), // 20 + 19
	    new TestResult(2, 14162641520.758657434940185594m), // 20 + 20
	    new TestResult(2, 422833594700.15637117403846321m), // 20 + 21
	    new TestResult(2, -6382311169132.2569386496119534m), // 20 + 22
	    new TestResult(2, 442353364063675.43897035053819m), // 20 + 23
	    new TestResult(2, -512826699546563.51087965696755m), // 20 + 24
	    new TestResult(2, 608940587772236464.52441846887m), // 20 + 25
	    new TestResult(2, -42535053313312905645354.658458m), // 20 + 26
	    new TestResult(2, -7808274522584871786725.5019024m), // 20 + 27
	    new TestResult(2, 1037807626804273044411380232.1m), // 20 + 28
	    new TestResult(0, 2084197793.9306762916985297548m), // 20 + 29
	    new TestResult(0, 415752273939.77704245656837041m), // 21 + 0
	    new TestResult(0, 415752273940.77704245656837041m), // 21 + 1
	    new TestResult(0, 415752273938.77704245656837041m), // 21 + 2
	    new TestResult(0, 415752273941.77704245656837041m), // 21 + 3
	    new TestResult(0, 415752273949.77704245656837041m), // 21 + 4
	    new TestResult(0, 415752273939.87704245656837041m), // 21 + 5
	    new TestResult(1, 0m), // 21 + 6
	    new TestResult(2, -79228162514264337177791676395m), // 21 + 7
	    new TestResult(2, 27703302467092376361605819.309m), // 21 + 8
	    new TestResult(0, 415749070084.82104563841911907m), // 21 + 9
	    new TestResult(2, -48466870443773121522480.251826m), // 21 + 10
	    new TestResult(2, -545193277490530854.52627129019m), // 21 + 11
	    new TestResult(0, 415752273940.53996586190224459m), // 21 + 12
	    new TestResult(2, -400453059249619122032.55770206m), // 21 + 13
	    new TestResult(2, 222851627785607466463990.3938m), // 21 + 14
	    new TestResult(2, 14246043379204568965935275.361m), // 21 + 15
	    new TestResult(0, 415751852816.47257936965400444m), // 21 + 16
	    new TestResult(2, 24463704490573484.97755135521m), // 21 + 17
	    new TestResult(2, -5323259153835970160423836.224m), // 21 + 18
	    new TestResult(2, 102801066199806250476947108.97m), // 21 + 19
	    new TestResult(0, 422833594700.15637117403846321m), // 21 + 20
	    new TestResult(2, 831504547879.5540849131367408m), // 21 + 21
	    new TestResult(2, -5973640215952.8592249105136758m), // 21 + 22
	    new TestResult(2, 442762035016854.83668408963647m), // 21 + 23
	    new TestResult(2, -512418028593384.11316591786927m), // 21 + 24
	    new TestResult(2, 608940996443189643.92213220797m), // 21 + 25
	    new TestResult(2, -42535053312904234692175.260745m), // 21 + 26
	    new TestResult(2, -7808274522176200833546.1041886m), // 21 + 27
	    new TestResult(2, 1037807626804273453082333411.5m), // 21 + 28
	    new TestResult(0, 410755150973.32839003079680737m), // 21 + 29
	    new TestResult(0, -6389392489892.6362673670820462m), // 22 + 0
	    new TestResult(0, -6389392489891.6362673670820462m), // 22 + 1
	    new TestResult(0, -6389392489893.6362673670820462m), // 22 + 2
	    new TestResult(0, -6389392489890.6362673670820462m), // 22 + 3
	    new TestResult(0, -6389392489882.6362673670820462m), // 22 + 4
	    new TestResult(0, -6389392489892.5362673670820462m), // 22 + 5
	    new TestResult(2, 79228162514264331204151460442m), // 22 + 6
	    new TestResult(1, 0m), // 22 + 7
	    new TestResult(2, 27703302467085571216841986.896m), // 22 + 8
	    new TestResult(0, -6389395693747.5922641852312975m), // 22 + 9
	    new TestResult(2, -48466870450578266286312.665135m), // 22 + 10
	    new TestResult(2, -545200082635294686.93958111384m), // 22 + 11
	    new TestResult(0, -6389392489891.873343961748172m), // 22 + 12
	    new TestResult(2, -400453066054763885864.97101189m), // 22 + 13
	    new TestResult(2, 222851627778802321700157.98049m), // 22 + 14
	    new TestResult(2, 14246043379197763821171442.948m), // 22 + 15
	    new TestResult(0, -6389392911015.9407304539964122m), // 22 + 16
	    new TestResult(2, 24456899345809652.56424153156m), // 22 + 17
	    new TestResult(2, -5323259153842775305187668.6373m), // 22 + 18
	    new TestResult(2, 102801066199799445332183276.55m), // 22 + 19
	    new TestResult(0, -6382311169132.2569386496119534m), // 22 + 20
	    new TestResult(0, -5973640215952.8592249105136758m), // 22 + 21
	    new TestResult(2, -12778784979785.272534734164092m), // 22 + 22
	    new TestResult(2, 435956890253022.42337426598605m), // 22 + 23
	    new TestResult(2, -519223173357216.52647574151969m), // 22 + 24
	    new TestResult(2, 608934191298425811.50882238432m), // 22 + 25
	    new TestResult(2, -42535053319709379456007.674054m), // 22 + 26
	    new TestResult(2, -7808274528981345597378.5174985m), // 22 + 27
	    new TestResult(2, 1037807626804266647937569579.1m), // 22 + 28
	    new TestResult(0, -6394389612859.0849197928536092m), // 22 + 29
	    new TestResult(0, 442346282742915.0596416330681m), // 23 + 0
	    new TestResult(0, 442346282742916.0596416330681m), // 23 + 1
	    new TestResult(0, 442346282742914.0596416330681m), // 23 + 2
	    new TestResult(0, 442346282742917.0596416330681m), // 23 + 3
	    new TestResult(0, 442346282742925.0596416330681m), // 23 + 4
	    new TestResult(0, 442346282742915.1596416330681m), // 23 + 5
	    new TestResult(1, 0m), // 23 + 6
	    new TestResult(2, -79228162514263895247261207420m), // 23 + 7
	    new TestResult(2, 27703302467534306892074794.592m), // 23 + 8
	    new TestResult(0, 442346279539060.10364481491885m), // 23 + 9
	    new TestResult(2, -48466870001842591053504.969226m), // 23 + 10
	    new TestResult(2, -544751346960061879.24367211369m), // 23 + 11
	    new TestResult(0, 442346282742915.82256503840197m), // 23 + 12
	    new TestResult(2, -400452617319088653057.27510289m), // 23 + 13
	    new TestResult(2, 222851628227537996932965.6764m), // 23 + 14
	    new TestResult(2, 14246043379646499496404250.644m), // 23 + 15
	    new TestResult(0, 442346282321791.75517854615373m), // 23 + 16
	    new TestResult(2, 24905635021042460.26015053171m), // 23 + 17
	    new TestResult(2, -5323259153394039629954860.9414m), // 23 + 18
	    new TestResult(2, 102801066200248181007416084.25m), // 23 + 19
	    new TestResult(0, 442353364063675.43897035053819m), // 23 + 20
	    new TestResult(0, 442762035016854.83668408963647m), // 23 + 21
	    new TestResult(0, 435956890253022.42337426598605m), // 23 + 22
	    new TestResult(0, 884692565485830.1192832661362m), // 23 + 23
	    new TestResult(0, -70487498124408.83056674136954m), // 23 + 24
	    new TestResult(2, 609382926973658619.20473138447m), // 23 + 25
	    new TestResult(2, -42535052870973704223199.978145m), // 23 + 26
	    new TestResult(2, -7808274080245670364570.8215895m), // 23 + 27
	    new TestResult(2, 1037807626804715383612802386.8m), // 23 + 28
	    new TestResult(0, 442341285619948.61098920729654m), // 23 + 29
	    new TestResult(0, -512833780867323.89020837443764m), // 24 + 0
	    new TestResult(0, -512833780867322.89020837443764m), // 24 + 1
	    new TestResult(0, -512833780867324.89020837443764m), // 24 + 2
	    new TestResult(0, -512833780867321.89020837443764m), // 24 + 3
	    new TestResult(0, -512833780867313.89020837443764m), // 24 + 4
	    new TestResult(0, -512833780867323.79020837443764m), // 24 + 5
	    new TestResult(2, 79228162514263824759763083011m), // 24 + 6
	    new TestResult(1, 0m), // 24 + 7
	    new TestResult(2, 27703302466579126828464555.642m), // 24 + 8
	    new TestResult(0, -512833784071178.84620519258689m), // 24 + 9
	    new TestResult(2, -48466870957022654663743.919076m), // 24 + 10
	    new TestResult(2, -545706527023672118.1935221212m), // 24 + 11
	    new TestResult(0, -512833780867323.12728496910377m), // 24 + 12
	    new TestResult(2, -400453572499152263296.22495289m), // 24 + 13
	    new TestResult(2, 222851627272357933322726.72655m), // 24 + 14
	    new TestResult(2, 14246043378691319432794011.694m), // 24 + 15
	    new TestResult(0, -512833781288447.19467146135201m), // 24 + 16
	    new TestResult(2, 23950454957432221.310300524204m), // 24 + 17
	    new TestResult(2, -5323259154349219693565099.8912m), // 24 + 18
	    new TestResult(2, 102801066199293000943805845.3m), // 24 + 19
	    new TestResult(0, -512826699546563.51087965696755m), // 24 + 20
	    new TestResult(0, -512418028593384.11316591786927m), // 24 + 21
	    new TestResult(0, -519223173357216.52647574151969m), // 24 + 22
	    new TestResult(0, -70487498124408.83056674136954m), // 24 + 23
	    new TestResult(2, -1025667561734647.7804167488753m), // 24 + 24
	    new TestResult(2, 608427746910048380.25488137696m), // 24 + 25
	    new TestResult(2, -42535053826153767833438.927995m), // 24 + 26
	    new TestResult(2, -7808275035425733974809.7714395m), // 24 + 27
	    new TestResult(2, 1037807626803760203549192147.8m), // 24 + 28
	    new TestResult(0, -512838777990290.3388608002092m), // 24 + 29
	    new TestResult(0, 608940580690915704.1450897514m), // 25 + 0
	    new TestResult(0, 608940580690915705.1450897514m), // 25 + 1
	    new TestResult(0, 608940580690915703.1450897514m), // 25 + 2
	    new TestResult(0, 608940580690915706.1450897514m), // 25 + 3
	    new TestResult(0, 608940580690915714.1450897514m), // 25 + 4
	    new TestResult(0, 608940580690915704.2450897514m), // 25 + 5
	    new TestResult(1, 0m), // 25 + 6
	    new TestResult(2, -79228162513655397012853034631m), // 25 + 7
	    new TestResult(2, 27703303076032541300247583.677m), // 25 + 8
	    new TestResult(0, 608940580687711849.18909293325m), // 25 + 9
	    new TestResult(2, -48466261503608182880715.883778m), // 25 + 10
	    new TestResult(0, 63746887448110909.84177600464m), // 25 + 11
	    new TestResult(0, 608940580690915704.90801315673m), // 25 + 12
	    new TestResult(2, -399844119084680480268.18965477m), // 25 + 13
	    new TestResult(2, 222852236725772405105754.76185m), // 25 + 14
	    new TestResult(2, 14246043988144733904577039.729m), // 25 + 15
	    new TestResult(0, 608940580690494580.84062666449m), // 25 + 16
	    new TestResult(0, 633403869429215249.34559865004m), // 25 + 17
	    new TestResult(2, -5323258544895805221782071.8559m), // 25 + 18
	    new TestResult(2, 102801066808746415415588873.34m), // 25 + 19
	    new TestResult(0, 608940587772236464.52441846887m), // 25 + 20
	    new TestResult(0, 608940996443189643.92213220797m), // 25 + 21
	    new TestResult(0, 608934191298425811.50882238432m), // 25 + 22
	    new TestResult(0, 609382926973658619.20473138447m), // 25 + 23
	    new TestResult(0, 608427746910048380.25488137696m), // 25 + 24
	    new TestResult(0, 1217881161381831408.2901795028m), // 25 + 25
	    new TestResult(2, -42534444372739296050410.892697m), // 25 + 26
	    new TestResult(2, -7807665582011262191781.7361413m), // 25 + 27
	    new TestResult(2, 1037807627413213618020975175.8m), // 25 + 28
	    new TestResult(0, 608940575693792737.69643732563m), // 25 + 29
	    new TestResult(0, -42535053313319986966115.037787m), // 26 + 0
	    new TestResult(0, -42535053313319986966114.037787m), // 26 + 1
	    new TestResult(0, -42535053313319986966116.037787m), // 26 + 2
	    new TestResult(0, -42535053313319986966113.037787m), // 26 + 3
	    new TestResult(0, -42535053313319986966105.037787m), // 26 + 4
	    new TestResult(0, -42535053313319986966114.937787m), // 26 + 5
	    new TestResult(2, 79228119979211024273556984220m), // 26 + 6
	    new TestResult(1, 0m), // 26 + 7
	    new TestResult(2, 27660767413778640622365764.494m), // 26 + 8
	    new TestResult(0, -42535053313319990169969.993784m), // 26 + 9
	    new TestResult(2, -91001923757508860762535.06666m), // 26 + 10
	    new TestResult(0, -42535598507013229770909.341101m), // 26 + 11
	    new TestResult(0, -42535053313319986966114.274864m), // 26 + 12
	    new TestResult(0, -42935506372985358362087.372532m), // 26 + 13
	    new TestResult(2, 180316574471871727223935.57897m), // 26 + 14
	    new TestResult(2, 14203508325890833226695220.546m), // 26 + 15
	    new TestResult(0, -42535053313319987387238.34225m), // 26 + 16
	    new TestResult(0, -42535028850031248666569.837278m), // 26 + 17
	    new TestResult(2, -5365794207149705899663891.0388m), // 26 + 18
	    new TestResult(2, 102758531146492514737707054.15m), // 26 + 19
	    new TestResult(0, -42535053313312905645354.658458m), // 26 + 20
	    new TestResult(0, -42535053312904234692175.260745m), // 26 + 21
	    new TestResult(0, -42535053319709379456007.674054m), // 26 + 22
	    new TestResult(0, -42535052870973704223199.978145m), // 26 + 23
	    new TestResult(0, -42535053826153767833438.927995m), // 26 + 24
	    new TestResult(0, -42534444372739296050410.892697m), // 26 + 25
	    new TestResult(2, -85070106626639973932230.07557m), // 26 + 26
	    new TestResult(0, -50343327835911940073600.919018m), // 26 + 27
	    new TestResult(2, 1037765091750959717343093356.7m), // 26 + 28
	    new TestResult(0, -42535053313324984089081.486439m), // 26 + 29
	    new TestResult(0, -7808274522591953107485.8812311m), // 27 + 0
	    new TestResult(0, -7808274522591953107484.8812311m), // 27 + 1
	    new TestResult(0, -7808274522591953107486.8812311m), // 27 + 2
	    new TestResult(0, -7808274522591953107483.8812311m), // 27 + 3
	    new TestResult(0, -7808274522591953107475.8812311m), // 27 + 4
	    new TestResult(0, -7808274522591953107485.7812311m), // 27 + 5
	    new TestResult(2, 79228154705989815001590842849m), // 27 + 6
	    new TestResult(1, 0m), // 27 + 7
	    new TestResult(2, 27695494192569368656224393.651m), // 27 + 8
	    new TestResult(0, -7808274522591956311340.8372279m), // 27 + 9
	    new TestResult(2, -56275144966780826903905.910099m), // 27 + 10
	    new TestResult(0, -7808819716285195912280.1845448m), // 27 + 11
	    new TestResult(0, -7808274522591953107485.1183077m), // 27 + 12
	    new TestResult(2, -8208727582257324503458.215976m), // 27 + 13
	    new TestResult(2, 215043353262599761082564.73553m), // 27 + 14
	    new TestResult(2, 14238235104681561260553849.703m), // 27 + 15
	    new TestResult(0, -7808274522591953528609.1856942m), // 27 + 16
	    new TestResult(0, -7808250059303214807940.6807222m), // 27 + 17
	    new TestResult(2, -5331067428358977865805261.8822m), // 27 + 18
	    new TestResult(2, 102793257925283242771565683.31m), // 27 + 19
	    new TestResult(0, -7808274522584871786725.5019024m), // 27 + 20
	    new TestResult(0, -7808274522176200833546.1041886m), // 27 + 21
	    new TestResult(0, -7808274528981345597378.5174985m), // 27 + 22
	    new TestResult(0, -7808274080245670364570.8215895m), // 27 + 23
	    new TestResult(0, -7808275035425733974809.7714395m), // 27 + 24
	    new TestResult(0, -7807665582011262191781.7361413m), // 27 + 25
	    new TestResult(2, -50343327835911940073600.919018m), // 27 + 26
	    new TestResult(2, -15616549045183906214971.762462m), // 27 + 27
	    new TestResult(2, 1037799818529750445376951985.8m), // 27 + 28
	    new TestResult(0, -7808274522596950230452.3298835m), // 27 + 29
	    new TestResult(0, 1037807626804273037330059471.7m), // 28 + 0
	    new TestResult(0, 1037807626804273037330059472.7m), // 28 + 1
	    new TestResult(0, 1037807626804273037330059470.7m), // 28 + 2
	    new TestResult(0, 1037807626804273037330059473.7m), // 28 + 3
	    new TestResult(0, 1037807626804273037330059481.7m), // 28 + 4
	    new TestResult(0, 1037807626804273037330059471.8m), // 28 + 5
	    new TestResult(1, 0m), // 28 + 6
	    new TestResult(2, -78190354887460064556213890863m), // 28 + 7
	    new TestResult(0, 1065510929271364997939391351.2m), // 28 + 8
	    new TestResult(0, 1037807626804273037326855616.7m), // 28 + 9
	    new TestResult(0, 1037759159933828848456263051.7m), // 28 + 10
	    new TestResult(0, 1037807626259079344087254677.4m), // 28 + 11
	    new TestResult(0, 1037807626804273037330059472.5m), // 28 + 12
	    new TestResult(0, 1037807226351213371958663499.4m), // 28 + 13
	    new TestResult(0, 1038030478432058229044249522.3m), // 28 + 14
	    new TestResult(0, 1052053670183477190543720807.3m), // 28 + 15
	    new TestResult(0, 1037807626804273037329638348.4m), // 28 + 16
	    new TestResult(0, 1037807626828736326068359016.9m), // 28 + 17
	    new TestResult(0, 1032484367650436651417361695.7m), // 28 + 18
	    new TestResult(0, 1140608693004078872054732640.9m), // 28 + 19
	    new TestResult(0, 1037807626804273044411380232.1m), // 28 + 20
	    new TestResult(0, 1037807626804273453082333411.5m), // 28 + 21
	    new TestResult(0, 1037807626804266647937569579.1m), // 28 + 22
	    new TestResult(0, 1037807626804715383612802386.8m), // 28 + 23
	    new TestResult(0, 1037807626803760203549192147.8m), // 28 + 24
	    new TestResult(0, 1037807627413213618020975175.8m), // 28 + 25
	    new TestResult(0, 1037765091750959717343093356.7m), // 28 + 26
	    new TestResult(0, 1037799818529750445376951985.8m), // 28 + 27
	    new TestResult(0, 2075615253608546074660118943.4m), // 28 + 28
	    new TestResult(0, 1037807626804273032332936505.3m), // 28 + 29
	    new TestResult(0, -4997122966.448652425771563042m), // 29 + 0
	    new TestResult(0, -4997122965.448652425771563042m), // 29 + 1
	    new TestResult(0, -4997122967.448652425771563042m), // 29 + 2
	    new TestResult(0, -4997122964.448652425771563042m), // 29 + 3
	    new TestResult(0, -4997122956.448652425771563042m), // 29 + 4
	    new TestResult(0, -4997122966.348652425771563042m), // 29 + 5
	    new TestResult(2, 79228162514264337588546827369m), // 29 + 6
	    new TestResult(1, 0m), // 29 + 7
	    new TestResult(2, 27703302467091955612208913.083m), // 29 + 8
	    new TestResult(0, -5000326821.4046492439208143805m), // 29 + 9
	    new TestResult(2, -48466870444193870919386.47752m), // 29 + 10
	    new TestResult(2, -545193698239927760.75196617253m), // 29 + 11
	    new TestResult(0, -4997122965.685729020437688861m), // 29 + 12
	    new TestResult(2, -400453059670368518938.78339695m), // 29 + 13
	    new TestResult(2, 222851627785186717067084.16811m), // 29 + 14
	    new TestResult(2, 14246043379204148216538369.135m), // 29 + 15
	    new TestResult(0, -4997544089.7531155126859290085m), // 29 + 16
	    new TestResult(2, 24463283741176578.75185647287m), // 29 + 17
	    new TestResult(2, -5323259153836390909820742.4497m), // 29 + 18
	    new TestResult(2, 102801066199805829727550202.74m), // 29 + 19
	    new TestResult(0, 2084197793.9306762916985297548m), // 29 + 20
	    new TestResult(2, 410755150973.32839003079680737m), // 29 + 21
	    new TestResult(2, -6394389612859.0849197928536092m), // 29 + 22
	    new TestResult(2, 442341285619948.61098920729654m), // 29 + 23
	    new TestResult(2, -512838777990290.3388608002092m), // 29 + 24
	    new TestResult(2, 608940575693792737.69643732563m), // 29 + 25
	    new TestResult(2, -42535053313324984089081.486439m), // 29 + 26
	    new TestResult(2, -7808274522596950230452.3298835m), // 29 + 27
	    new TestResult(2, 1037807626804273032332936505.3m), // 29 + 28
	    new TestResult(0, -9994245932.897304851543126084m), // 29 + 29
        };


        // generated result list build2
        TestResult[] trAuto_Mult_build2 = new TestResult[] {
	    new TestResult(0, 0m), // 0 * 0
	    new TestResult(0, 0m), // 0 * 1
	    new TestResult(0, 0m), // 0 * 2
	    new TestResult(0, 0m), // 0 * 3
	    new TestResult(0, 0m), // 0 * 4
	    new TestResult(0, 0m), // 0 * 5
	    new TestResult(0, 0m), // 0 * 6
	    new TestResult(0, 0m), // 0 * 7
	    new TestResult(0, 0m), // 0 * 8
	    new TestResult(0, 0m), // 0 * 9
	    new TestResult(0, 0m), // 0 * 10
	    new TestResult(0, 0m), // 0 * 11
	    new TestResult(0, 0m), // 0 * 12
	    new TestResult(0, 0m), // 0 * 13
	    new TestResult(0, 0m), // 0 * 14
	    new TestResult(0, 0m), // 0 * 15
	    new TestResult(0, 0m), // 0 * 16
	    new TestResult(0, 0m), // 0 * 17
	    new TestResult(0, 0m), // 0 * 18
	    new TestResult(0, 0m), // 0 * 19
	    new TestResult(0, 0m), // 0 * 20
	    new TestResult(0, 0m), // 0 * 21
	    new TestResult(0, 0m), // 0 * 22
	    new TestResult(0, 0m), // 0 * 23
	    new TestResult(0, 0m), // 0 * 24
	    new TestResult(0, 0m), // 0 * 25
	    new TestResult(0, 0m), // 0 * 26
	    new TestResult(0, 0m), // 0 * 27
	    new TestResult(0, 0m), // 0 * 28
	    new TestResult(0, 0m), // 0 * 29
	    new TestResult(0, 0m), // 1 * 0
	    new TestResult(0, 1m), // 1 * 1
	    new TestResult(0, -1m), // 1 * 2
	    new TestResult(0, 2m), // 1 * 3
	    new TestResult(0, 10m), // 1 * 4
	    new TestResult(0, 0.1m), // 1 * 5
	    new TestResult(0, 79228162514264337593543950335m), // 1 * 6
	    new TestResult(0, -79228162514264337593543950335m), // 1 * 7
	    new TestResult(0, 27703302467091960609331879.532m), // 1 * 8
	    new TestResult(0, -3203854.9559968181492513385018m), // 1 * 9
	    new TestResult(0, -48466870444188873796420.028868m), // 1 * 10
	    new TestResult(0, -545193693242804794.30331374676m), // 1 * 11
	    new TestResult(0, 0.7629234053338741809892531431m), // 1 * 12
	    new TestResult(0, -400453059665371395972.33474452m), // 1 * 13
	    new TestResult(0, 222851627785191714190050.61676m), // 1 * 14
	    new TestResult(0, 14246043379204153213661335.584m), // 1 * 15
	    new TestResult(0, -421123.30446308691436596648186m), // 1 * 16
	    new TestResult(0, 24463288738299545.200508898642m), // 1 * 17
	    new TestResult(0, -5323259153836385912697776.001m), // 1 * 18
	    new TestResult(0, 102801066199805834724673169.19m), // 1 * 19
	    new TestResult(0, 7081320760.3793287174700927968m), // 1 * 20
	    new TestResult(0, 415752273939.77704245656837041m), // 1 * 21
	    new TestResult(0, -6389392489892.6362673670820462m), // 1 * 22
	    new TestResult(0, 442346282742915.0596416330681m), // 1 * 23
	    new TestResult(0, -512833780867323.89020837443764m), // 1 * 24
	    new TestResult(0, 608940580690915704.1450897514m), // 1 * 25
	    new TestResult(0, -42535053313319986966115.037787m), // 1 * 26
	    new TestResult(0, -7808274522591953107485.8812311m), // 1 * 27
	    new TestResult(0, 1037807626804273037330059471.7m), // 1 * 28
	    new TestResult(0, -4997122966.448652425771563042m), // 1 * 29
	    new TestResult(0, 0m), // 2 * 0
	    new TestResult(0, -1m), // 2 * 1
	    new TestResult(0, 1m), // 2 * 2
	    new TestResult(0, -2m), // 2 * 3
	    new TestResult(0, -10m), // 2 * 4
	    new TestResult(0, -0.1m), // 2 * 5
	    new TestResult(0, -79228162514264337593543950335m), // 2 * 6
	    new TestResult(0, 79228162514264337593543950335m), // 2 * 7
	    new TestResult(0, -27703302467091960609331879.532m), // 2 * 8
	    new TestResult(0, 3203854.9559968181492513385018m), // 2 * 9
	    new TestResult(0, 48466870444188873796420.028868m), // 2 * 10
	    new TestResult(0, 545193693242804794.30331374676m), // 2 * 11
	    new TestResult(0, -0.7629234053338741809892531431m), // 2 * 12
	    new TestResult(0, 400453059665371395972.33474452m), // 2 * 13
	    new TestResult(0, -222851627785191714190050.61676m), // 2 * 14
	    new TestResult(0, -14246043379204153213661335.584m), // 2 * 15
	    new TestResult(0, 421123.30446308691436596648186m), // 2 * 16
	    new TestResult(0, -24463288738299545.200508898642m), // 2 * 17
	    new TestResult(0, 5323259153836385912697776.001m), // 2 * 18
	    new TestResult(0, -102801066199805834724673169.19m), // 2 * 19
	    new TestResult(0, -7081320760.3793287174700927968m), // 2 * 20
	    new TestResult(0, -415752273939.77704245656837041m), // 2 * 21
	    new TestResult(0, 6389392489892.6362673670820462m), // 2 * 22
	    new TestResult(0, -442346282742915.0596416330681m), // 2 * 23
	    new TestResult(0, 512833780867323.89020837443764m), // 2 * 24
	    new TestResult(0, -608940580690915704.1450897514m), // 2 * 25
	    new TestResult(0, 42535053313319986966115.037787m), // 2 * 26
	    new TestResult(0, 7808274522591953107485.8812311m), // 2 * 27
	    new TestResult(0, -1037807626804273037330059471.7m), // 2 * 28
	    new TestResult(0, 4997122966.448652425771563042m), // 2 * 29
	    new TestResult(0, 0m), // 3 * 0
	    new TestResult(0, 2m), // 3 * 1
	    new TestResult(0, -2m), // 3 * 2
	    new TestResult(0, 4m), // 3 * 3
	    new TestResult(0, 20m), // 3 * 4
	    new TestResult(0, 0.2m), // 3 * 5
	    new TestResult(1, 0m), // 3 * 6
	    new TestResult(1, 0m), // 3 * 7
	    new TestResult(0, 55406604934183921218663759.064m), // 3 * 8
	    new TestResult(0, -6407709.9119936362985026770036m), // 3 * 9
	    new TestResult(2, -96933740888377747592840.05774m), // 3 * 10
	    new TestResult(0, -1090387386485609588.6066274935m), // 3 * 11
	    new TestResult(0, 1.5258468106677483619785062862m), // 3 * 12
	    new TestResult(2, -800906119330742791944.669489m), // 3 * 13
	    new TestResult(0, 445703255570383428380101.23352m), // 3 * 14
	    new TestResult(0, 28492086758408306427322671.168m), // 3 * 15
	    new TestResult(0, -842246.6089261738287319329637m), // 3 * 16
	    new TestResult(0, 48926577476599090.401017797284m), // 3 * 17
	    new TestResult(0, -10646518307672771825395552.002m), // 3 * 18
	    new TestResult(0, 205602132399611669449346338.38m), // 3 * 19
	    new TestResult(2, 14162641520.758657434940185594m), // 3 * 20
	    new TestResult(0, 831504547879.5540849131367408m), // 3 * 21
	    new TestResult(2, -12778784979785.272534734164092m), // 3 * 22
	    new TestResult(0, 884692565485830.1192832661362m), // 3 * 23
	    new TestResult(0, -1025667561734647.7804167488753m), // 3 * 24
	    new TestResult(0, 1217881161381831408.2901795028m), // 3 * 25
	    new TestResult(2, -85070106626639973932230.07557m), // 3 * 26
	    new TestResult(0, -15616549045183906214971.762462m), // 3 * 27
	    new TestResult(0, 2075615253608546074660118943.4m), // 3 * 28
	    new TestResult(0, -9994245932.897304851543126084m), // 3 * 29
	    new TestResult(0, 0m), // 4 * 0
	    new TestResult(0, 10m), // 4 * 1
	    new TestResult(0, -10m), // 4 * 2
	    new TestResult(0, 20m), // 4 * 3
	    new TestResult(0, 100m), // 4 * 4
	    new TestResult(0, 1m), // 4 * 5
	    new TestResult(1, 0m), // 4 * 6
	    new TestResult(1, 0m), // 4 * 7
	    new TestResult(0, 277033024670919606093318795.32m), // 4 * 8
	    new TestResult(0, -32038549.559968181492513385018m), // 4 * 9
	    new TestResult(0, -484668704441888737964200.28868m), // 4 * 10
	    new TestResult(0, -5451936932428047943.0331374676m), // 4 * 11
	    new TestResult(0, 7.629234053338741809892531431m), // 4 * 12
	    new TestResult(0, -4004530596653713959723.3474452m), // 4 * 13
	    new TestResult(0, 2228516277851917141900506.1676m), // 4 * 14
	    new TestResult(0, 142460433792041532136613355.84m), // 4 * 15
	    new TestResult(0, -4211233.0446308691436596648186m), // 4 * 16
	    new TestResult(0, 244632887382995452.00508898642m), // 4 * 17
	    new TestResult(0, -53232591538363859126977760.01m), // 4 * 18
	    new TestResult(0, 1028010661998058347246731691.9m), // 4 * 19
	    new TestResult(0, 70813207603.793287174700927968m), // 4 * 20
	    new TestResult(0, 4157522739397.7704245656837041m), // 4 * 21
	    new TestResult(0, -63893924898926.362673670820462m), // 4 * 22
	    new TestResult(0, 4423462827429150.596416330681m), // 4 * 23
	    new TestResult(0, -5128337808673238.9020837443764m), // 4 * 24
	    new TestResult(0, 6089405806909157041.450897514m), // 4 * 25
	    new TestResult(0, -425350533133199869661150.37787m), // 4 * 26
	    new TestResult(0, -78082745225919531074858.812311m), // 4 * 27
	    new TestResult(0, 10378076268042730373300594717m), // 4 * 28
	    new TestResult(0, -49971229664.48652425771563042m), // 4 * 29
	    new TestResult(0, 0m), // 5 * 0
	    new TestResult(0, 0.1m), // 5 * 1
	    new TestResult(0, -0.1m), // 5 * 2
	    new TestResult(0, 0.2m), // 5 * 3
	    new TestResult(0, 1m), // 5 * 4
	    new TestResult(0, 0.01m), // 5 * 5
	    new TestResult(0, 7922816251426433759354395033.5m), // 5 * 6
	    new TestResult(0, -7922816251426433759354395033.5m), // 5 * 7
	    new TestResult(0, 2770330246709196060933187.9532m), // 5 * 8
	    new TestResult(0, -320385.49559968181492513385018m), // 5 * 9
	    new TestResult(0, -4846687044418887379642.0028868m), // 5 * 10
	    new TestResult(0, -54519369324280479.430331374676m), // 5 * 11
	    new TestResult(0, 0.0762923405333874180989253143m), // 5 * 12
	    new TestResult(0, -40045305966537139597.233474452m), // 5 * 13
	    new TestResult(0, 22285162778519171419005.061676m), // 5 * 14
	    new TestResult(0, 1424604337920415321366133.5584m), // 5 * 15
	    new TestResult(0, -42112.330446308691436596648186m), // 5 * 16
	    new TestResult(0, 2446328873829954.5200508898642m), // 5 * 17
	    new TestResult(0, -532325915383638591269777.6001m), // 5 * 18
	    new TestResult(0, 10280106619980583472467316.919m), // 5 * 19
	    new TestResult(0, 708132076.03793287174700927968m), // 5 * 20
	    new TestResult(0, 41575227393.977704245656837041m), // 5 * 21
	    new TestResult(0, -638939248989.26362673670820462m), // 5 * 22
	    new TestResult(0, 44234628274291.50596416330681m), // 5 * 23
	    new TestResult(0, -51283378086732.389020837443764m), // 5 * 24
	    new TestResult(0, 60894058069091570.41450897514m), // 5 * 25
	    new TestResult(0, -4253505331331998696611.5037787m), // 5 * 26
	    new TestResult(0, -780827452259195310748.58812311m), // 5 * 27
	    new TestResult(0, 103780762680427303733005947.17m), // 5 * 28
	    new TestResult(0, -499712296.6448652425771563042m), // 5 * 29
	    new TestResult(0, 0m), // 6 * 0
	    new TestResult(0, 79228162514264337593543950335m), // 6 * 1
	    new TestResult(0, -79228162514264337593543950335m), // 6 * 2
	    new TestResult(1, 0m), // 6 * 3
	    new TestResult(1, 0m), // 6 * 4
	    new TestResult(0, 7922816251426433759354395033.5m), // 6 * 5
	    new TestResult(1, 0m), // 6 * 6
	    new TestResult(1, 0m), // 6 * 7
	    new TestResult(1, 0m), // 6 * 8
	    new TestResult(1, 0m), // 6 * 9
	    new TestResult(1, 0m), // 6 * 10
	    new TestResult(1, 0m), // 6 * 11
	    new TestResult(4, 60445019543728147377669509413m), // 6 * 12
	    new TestResult(1, 0m), // 6 * 13
	    new TestResult(1, 0m), // 6 * 14
	    new TestResult(1, 0m), // 6 * 15
	    new TestResult(1, 0m), // 6 * 16
	    new TestResult(1, 0m), // 6 * 17
	    new TestResult(1, 0m), // 6 * 18
	    new TestResult(1, 0m), // 6 * 19
	    new TestResult(1, 0m), // 6 * 20
	    new TestResult(1, 0m), // 6 * 21
	    new TestResult(1, 0m), // 6 * 22
	    new TestResult(1, 0m), // 6 * 23
	    new TestResult(1, 0m), // 6 * 24
	    new TestResult(1, 0m), // 6 * 25
	    new TestResult(1, 0m), // 6 * 26
	    new TestResult(1, 0m), // 6 * 27
	    new TestResult(1, 0m), // 6 * 28
	    new TestResult(1, 0m), // 6 * 29
	    new TestResult(0, 0m), // 7 * 0
	    new TestResult(0, -79228162514264337593543950335m), // 7 * 1
	    new TestResult(0, 79228162514264337593543950335m), // 7 * 2
	    new TestResult(1, 0m), // 7 * 3
	    new TestResult(1, 0m), // 7 * 4
	    new TestResult(0, -7922816251426433759354395033.5m), // 7 * 5
	    new TestResult(1, 0m), // 7 * 6
	    new TestResult(1, 0m), // 7 * 7
	    new TestResult(1, 0m), // 7 * 8
	    new TestResult(1, 0m), // 7 * 9
	    new TestResult(1, 0m), // 7 * 10
	    new TestResult(1, 0m), // 7 * 11
	    new TestResult(4, -60445019543728147377669509413m), // 7 * 12
	    new TestResult(1, 0m), // 7 * 13
	    new TestResult(1, 0m), // 7 * 14
	    new TestResult(1, 0m), // 7 * 15
	    new TestResult(1, 0m), // 7 * 16
	    new TestResult(1, 0m), // 7 * 17
	    new TestResult(1, 0m), // 7 * 18
	    new TestResult(1, 0m), // 7 * 19
	    new TestResult(1, 0m), // 7 * 20
	    new TestResult(1, 0m), // 7 * 21
	    new TestResult(1, 0m), // 7 * 22
	    new TestResult(1, 0m), // 7 * 23
	    new TestResult(1, 0m), // 7 * 24
	    new TestResult(1, 0m), // 7 * 25
	    new TestResult(1, 0m), // 7 * 26
	    new TestResult(1, 0m), // 7 * 27
	    new TestResult(1, 0m), // 7 * 28
	    new TestResult(1, 0m), // 7 * 29
	    new TestResult(0, 0m), // 8 * 0
	    new TestResult(0, 27703302467091960609331879.532m), // 8 * 1
	    new TestResult(0, -27703302467091960609331879.532m), // 8 * 2
	    new TestResult(0, 55406604934183921218663759.064m), // 8 * 3
	    new TestResult(0, 277033024670919606093318795.32m), // 8 * 4
	    new TestResult(0, 2770330246709196060933187.9532m), // 8 * 5
	    new TestResult(1, 0m), // 8 * 6
	    new TestResult(1, 0m), // 8 * 7
	    new TestResult(1, 0m), // 8 * 8
	    new TestResult(1, 0m), // 8 * 9
	    new TestResult(1, 0m), // 8 * 10
	    new TestResult(1, 0m), // 8 * 11
	    new TestResult(2, 21135497857188116458095236.68m), // 8 * 12
	    new TestResult(1, 0m), // 8 * 13
	    new TestResult(1, 0m), // 8 * 14
	    new TestResult(1, 0m), // 8 * 15
	    new TestResult(1, 0m), // 8 * 16
	    new TestResult(1, 0m), // 8 * 17
	    new TestResult(1, 0m), // 8 * 18
	    new TestResult(1, 0m), // 8 * 19
	    new TestResult(1, 0m), // 8 * 20
	    new TestResult(1, 0m), // 8 * 21
	    new TestResult(1, 0m), // 8 * 22
	    new TestResult(1, 0m), // 8 * 23
	    new TestResult(1, 0m), // 8 * 24
	    new TestResult(1, 0m), // 8 * 25
	    new TestResult(1, 0m), // 8 * 26
	    new TestResult(1, 0m), // 8 * 27
	    new TestResult(1, 0m), // 8 * 28
	    new TestResult(1, 0m), // 8 * 29
	    new TestResult(0, 0m), // 9 * 0
	    new TestResult(0, -3203854.9559968181492513385018m), // 9 * 1
	    new TestResult(0, 3203854.9559968181492513385018m), // 9 * 2
	    new TestResult(0, -6407709.9119936362985026770036m), // 9 * 3
	    new TestResult(0, -32038549.559968181492513385018m), // 9 * 4
	    new TestResult(0, -320385.49559968181492513385018m), // 9 * 5
	    new TestResult(1, 0m), // 9 * 6
	    new TestResult(1, 0m), // 9 * 7
	    new TestResult(1, 0m), // 9 * 8
	    new TestResult(2, 10264686579065.373559419307221m), // 9 * 9
	    new TestResult(1, 0m), // 9 * 10
	    new TestResult(2, 1746721516074169126608222.1692m), // 9 * 11
	    new TestResult(2, -2444295.933224902121034426698m), // 9 * 12
	    new TestResult(2, 1282993519852989666698903060.5m), // 9 * 13
	    new TestResult(1, 0m), // 9 * 14
	    new TestResult(1, 0m), // 9 * 15
	    new TestResult(0, 1349217986089.8179781485646335m), // 9 * 16
	    new TestResult(0, -78376828864182146369609.767831m), // 9 * 17
	    new TestResult(1, 0m), // 9 * 18
	    new TestResult(1, 0m), // 9 * 19
	    new TestResult(0, -22687524613144469.045656755412m), // 9 * 20
	    new TestResult(0, -1332009983328901461.3254059884m), // 9 * 21
	    new TestResult(0, 20470686794551372519.831909846m), // 9 * 22
	    new TestResult(0, -1417213330232658207868.9685141m), // 9 * 23
	    new TestResult(0, 1643045050434361863551.7087135m), // 9 * 24
	    new TestResult(0, -1950957297354170624860913.7855m), // 9 * 25
	    new TestResult(1, 0m), // 9 * 26
	    new TestResult(0, 25016579026989918165002777574m), // 9 * 27
	    new TestResult(1, 0m), // 9 * 28
	    new TestResult(2, 16010057181782036.694377696165m), // 9 * 29
	    new TestResult(0, 0m), // 10 * 0
	    new TestResult(0, -48466870444188873796420.028868m), // 10 * 1
	    new TestResult(0, 48466870444188873796420.028868m), // 10 * 2
	    new TestResult(2, -96933740888377747592840.05774m), // 10 * 3
	    new TestResult(0, -484668704441888737964200.28868m), // 10 * 4
	    new TestResult(0, -4846687044418887379642.0028868m), // 10 * 5
	    new TestResult(1, 0m), // 10 * 6
	    new TestResult(1, 0m), // 10 * 7
	    new TestResult(1, 0m), // 10 * 8
	    new TestResult(1, 0m), // 10 * 9
	    new TestResult(1, 0m), // 10 * 10
	    new TestResult(1, 0m), // 10 * 11
	    new TestResult(0, -36976509845156274734545.845161m), // 10 * 12
	    new TestResult(1, 0m), // 10 * 13
	    new TestResult(1, 0m), // 10 * 14
	    new TestResult(1, 0m), // 10 * 15
	    new TestResult(2, 20410528638441139616161910791m), // 10 * 16
	    new TestResult(1, 0m), // 10 * 17
	    new TestResult(1, 0m), // 10 * 18
	    new TestResult(1, 0m), // 10 * 19
	    new TestResult(1, 0m), // 10 * 20
	    new TestResult(1, 0m), // 10 * 21
	    new TestResult(1, 0m), // 10 * 22
	    new TestResult(1, 0m), // 10 * 23
	    new TestResult(1, 0m), // 10 * 24
	    new TestResult(1, 0m), // 10 * 25
	    new TestResult(1, 0m), // 10 * 26
	    new TestResult(1, 0m), // 10 * 27
	    new TestResult(1, 0m), // 10 * 28
	    new TestResult(1, 0m), // 10 * 29
	    new TestResult(0, 0m), // 11 * 0
	    new TestResult(0, -545193693242804794.30331374676m), // 11 * 1
	    new TestResult(0, 545193693242804794.30331374676m), // 11 * 2
	    new TestResult(2, -1090387386485609588.6066274935m), // 11 * 3
	    new TestResult(0, -5451936932428047943.0331374676m), // 11 * 4
	    new TestResult(0, -54519369324280479.430331374676m), // 11 * 5
	    new TestResult(1, 0m), // 11 * 6
	    new TestResult(1, 0m), // 11 * 7
	    new TestResult(1, 0m), // 11 * 8
	    new TestResult(2, 1746721516074169126608222.1692m), // 11 * 9
	    new TestResult(1, 0m), // 11 * 10
	    new TestResult(1, 0m), // 11 * 11
	    new TestResult(0, -415941029015352223.2321562927m), // 11 * 12
	    new TestResult(1, 0m), // 11 * 13
	    new TestResult(1, 0m), // 11 * 14
	    new TestResult(1, 0m), // 11 * 15
	    new TestResult(0, 229593769670844494339647.60593m), // 11 * 16
	    new TestResult(1, 0m), // 11 * 17
	    new TestResult(1, 0m), // 11 * 18
	    new TestResult(1, 0m), // 11 * 19
	    new TestResult(0, -3860691418388152934958161711.9m), // 11 * 20
	    new TestResult(1, 0m), // 11 * 21
	    new TestResult(1, 0m), // 11 * 22
	    new TestResult(1, 0m), // 11 * 23
	    new TestResult(1, 0m), // 11 * 24
	    new TestResult(1, 0m), // 11 * 25
	    new TestResult(1, 0m), // 11 * 26
	    new TestResult(1, 0m), // 11 * 27
	    new TestResult(1, 0m), // 11 * 28
	    new TestResult(2, 2724399925666581324856736883m), // 11 * 29
	    new TestResult(0, 0m), // 12 * 0
	    new TestResult(0, 0.7629234053338741809892531431m), // 12 * 1
	    new TestResult(0, -0.7629234053338741809892531431m), // 12 * 2
	    new TestResult(0, 1.5258468106677483619785062862m), // 12 * 3
	    new TestResult(0, 7.629234053338741809892531431m), // 12 * 4
	    new TestResult(2, 0.0762923405333874180989253143m), // 12 * 5
	    new TestResult(0, 60445019543728147377669509413m), // 12 * 6
	    new TestResult(0, -60445019543728147377669509413m), // 12 * 7
	    new TestResult(0, 21135497857188116458095236.68m), // 12 * 8
	    new TestResult(0, -2444295.933224902121034426698m), // 12 * 9
	    new TestResult(0, -36976509845156274734545.845161m), // 12 * 10
	    new TestResult(0, -415941029015352223.2321562927m), // 12 * 11
	    new TestResult(0, 0.5820521224062348791152865214m), // 12 * 12
	    new TestResult(0, -305515011956274243325.23330625m), // 12 * 13
	    new TestResult(0, 170018722754075475876563.00661m), // 12 * 14
	    new TestResult(0, 10868639927396524825477357.557m), // 12 * 15
	    new TestResult(0, -321284.82550643216389212760083m), // 12 * 16
	    new TestResult(0, 18663615549889303.426127037208m), // 12 * 17
	    new TestResult(0, -4061239001119573143590088.0528m), // 12 * 18
	    new TestResult(0, 78429339497108899549297058.831m), // 12 * 19
	    new TestResult(0, 5402505348.7700567259404098662m), // 12 * 20
	    new TestResult(0, 317187140609.43641612785737895m), // 12 * 21
	    new TestResult(0, -4874617076403.5713301079734445m), // 12 * 22
	    new TestResult(0, 337476332367005.49979200414696m), // 12 * 23
	    new TestResult(0, -391252894469544.55412631906773m), // 12 * 24
	    new TestResult(0, 464575021466700199.22364418475m), // 12 * 25
	    new TestResult(0, -32450987719855972399063.033158m), // 12 * 26
	    new TestResult(0, -5957115388557583551533.0994303m), // 12 * 27
	    new TestResult(0, 791767728722982425613218218.59m), // 12 * 28
	    new TestResult(0, -3812422070.43511700405678157m), // 12 * 29
	    new TestResult(0, 0m), // 13 * 0
	    new TestResult(0, -400453059665371395972.33474452m), // 13 * 1
	    new TestResult(0, 400453059665371395972.33474452m), // 13 * 2
	    new TestResult(2, -800906119330742791944.669489m), // 13 * 3
	    new TestResult(0, -4004530596653713959723.3474452m), // 13 * 4
	    new TestResult(0, -40045305966537139597.233474452m), // 13 * 5
	    new TestResult(1, 0m), // 13 * 6
	    new TestResult(1, 0m), // 13 * 7
	    new TestResult(1, 0m), // 13 * 8
	    new TestResult(2, 1282993519852989666698903060.5m), // 13 * 9
	    new TestResult(1, 0m), // 13 * 10
	    new TestResult(1, 0m), // 13 * 11
	    new TestResult(0, -305515011956274243325.23330625m), // 13 * 12
	    new TestResult(1, 0m), // 13 * 13
	    new TestResult(1, 0m), // 13 * 14
	    new TestResult(1, 0m), // 13 * 15
	    new TestResult(2, 168640115768634908407809010.03m), // 13 * 16
	    new TestResult(1, 0m), // 13 * 17
	    new TestResult(1, 0m), // 13 * 18
	    new TestResult(1, 0m), // 13 * 19
	    new TestResult(1, 0m), // 13 * 20
	    new TestResult(1, 0m), // 13 * 21
	    new TestResult(1, 0m), // 13 * 22
	    new TestResult(1, 0m), // 13 * 23
	    new TestResult(1, 0m), // 13 * 24
	    new TestResult(1, 0m), // 13 * 25
	    new TestResult(1, 0m), // 13 * 26
	    new TestResult(1, 0m), // 13 * 27
	    new TestResult(1, 0m), // 13 * 28
	    new TestResult(1, 0m), // 13 * 29
	    new TestResult(0, 0m), // 14 * 0
	    new TestResult(0, 222851627785191714190050.61676m), // 14 * 1
	    new TestResult(0, -222851627785191714190050.61676m), // 14 * 2
	    new TestResult(0, 445703255570383428380101.23352m), // 14 * 3
	    new TestResult(0, 2228516277851917141900506.1676m), // 14 * 4
	    new TestResult(0, 22285162778519171419005.061676m), // 14 * 5
	    new TestResult(1, 0m), // 14 * 6
	    new TestResult(1, 0m), // 14 * 7
	    new TestResult(1, 0m), // 14 * 8
	    new TestResult(1, 0m), // 14 * 9
	    new TestResult(1, 0m), // 14 * 10
	    new TestResult(1, 0m), // 14 * 11
	    new TestResult(0, 170018722754075475876563.00661m), // 14 * 12
	    new TestResult(1, 0m), // 14 * 13
	    new TestResult(1, 0m), // 14 * 14
	    new TestResult(1, 0m), // 14 * 15
	    new TestResult(1, 0m), // 14 * 16
	    new TestResult(1, 0m), // 14 * 17
	    new TestResult(1, 0m), // 14 * 18
	    new TestResult(1, 0m), // 14 * 19
	    new TestResult(1, 0m), // 14 * 20
	    new TestResult(1, 0m), // 14 * 21
	    new TestResult(1, 0m), // 14 * 22
	    new TestResult(1, 0m), // 14 * 23
	    new TestResult(1, 0m), // 14 * 24
	    new TestResult(1, 0m), // 14 * 25
	    new TestResult(1, 0m), // 14 * 26
	    new TestResult(1, 0m), // 14 * 27
	    new TestResult(1, 0m), // 14 * 28
	    new TestResult(1, 0m), // 14 * 29
	    new TestResult(0, 0m), // 15 * 0
	    new TestResult(0, 14246043379204153213661335.584m), // 15 * 1
	    new TestResult(0, -14246043379204153213661335.584m), // 15 * 2
	    new TestResult(0, 28492086758408306427322671.168m), // 15 * 3
	    new TestResult(0, 142460433792041532136613355.84m), // 15 * 4
	    new TestResult(0, 1424604337920415321366133.5584m), // 15 * 5
	    new TestResult(1, 0m), // 15 * 6
	    new TestResult(1, 0m), // 15 * 7
	    new TestResult(1, 0m), // 15 * 8
	    new TestResult(1, 0m), // 15 * 9
	    new TestResult(1, 0m), // 15 * 10
	    new TestResult(1, 0m), // 15 * 11
	    new TestResult(0, 10868639927396524825477357.557m), // 15 * 12
	    new TestResult(1, 0m), // 15 * 13
	    new TestResult(1, 0m), // 15 * 14
	    new TestResult(1, 0m), // 15 * 15
	    new TestResult(1, 0m), // 15 * 16
	    new TestResult(1, 0m), // 15 * 17
	    new TestResult(1, 0m), // 15 * 18
	    new TestResult(1, 0m), // 15 * 19
	    new TestResult(1, 0m), // 15 * 20
	    new TestResult(1, 0m), // 15 * 21
	    new TestResult(1, 0m), // 15 * 22
	    new TestResult(1, 0m), // 15 * 23
	    new TestResult(1, 0m), // 15 * 24
	    new TestResult(1, 0m), // 15 * 25
	    new TestResult(1, 0m), // 15 * 26
	    new TestResult(1, 0m), // 15 * 27
	    new TestResult(1, 0m), // 15 * 28
	    new TestResult(1, 0m), // 15 * 29
	    new TestResult(0, 0m), // 16 * 0
	    new TestResult(0, -421123.30446308691436596648186m), // 16 * 1
	    new TestResult(0, 421123.30446308691436596648186m), // 16 * 2
	    new TestResult(2, -842246.6089261738287319329637m), // 16 * 3
	    new TestResult(0, -4211233.0446308691436596648186m), // 16 * 4
	    new TestResult(0, -42112.330446308691436596648186m), // 16 * 5
	    new TestResult(1, 0m), // 16 * 6
	    new TestResult(1, 0m), // 16 * 7
	    new TestResult(1, 0m), // 16 * 8
	    new TestResult(0, 1349217986089.8179781485646335m), // 16 * 9
	    new TestResult(2, 20410528638441139616161910791m), // 16 * 10
	    new TestResult(0, 229593769670844494339647.60593m), // 16 * 11
	    new TestResult(0, -321284.82550643216389212760083m), // 16 * 12
	    new TestResult(2, 168640115768634908407809010.03m), // 16 * 13
	    new TestResult(1, 0m), // 16 * 14
	    new TestResult(1, 0m), // 16 * 15
	    new TestResult(2, 177344837561.90979904837123025m), // 16 * 16
	    new TestResult(2, -10302060991507324713598.483586m), // 16 * 17
	    new TestResult(1, 0m), // 16 * 18
	    new TestResult(1, 0m), // 16 * 19
	    new TestResult(2, -2982109198574002.1833628108505m), // 16 * 20
	    new TestResult(2, -175082971439561442.82206371811m), // 16 * 21
	    new TestResult(2, 2690722078855217643.0013135833m), // 16 * 22
	    new TestResult(2, -186282328305659347664.31347679m), // 16 * 23
	    new TestResult(2, 215966256439146035447.16512997m), // 16 * 24
	    new TestResult(0, -256439069562229438659442.67473m), // 16 * 25
	    new TestResult(2, 17912502206818886711664532432m), // 16 * 26
	    new TestResult(0, 3288246369108855691627019039.6m), // 16 * 27
	    new TestResult(1, 0m), // 16 * 28
	    new TestResult(2, 2104404936439239.9111285468803m), // 16 * 29
	    new TestResult(0, 0m), // 17 * 0
	    new TestResult(0, 24463288738299545.200508898642m), // 17 * 1
	    new TestResult(0, -24463288738299545.200508898642m), // 17 * 2
	    new TestResult(0, 48926577476599090.401017797284m), // 17 * 3
	    new TestResult(0, 244632887382995452.00508898642m), // 17 * 4
	    new TestResult(0, 2446328873829954.5200508898642m), // 17 * 5
	    new TestResult(1, 0m), // 17 * 6
	    new TestResult(1, 0m), // 17 * 7
	    new TestResult(1, 0m), // 17 * 8
	    new TestResult(0, -78376828864182146369609.767831m), // 17 * 9
	    new TestResult(1, 0m), // 17 * 10
	    new TestResult(1, 0m), // 17 * 11
	    new TestResult(0, 18663615549889303.426127037208m), // 17 * 12
	    new TestResult(1, 0m), // 17 * 13
	    new TestResult(1, 0m), // 17 * 14
	    new TestResult(1, 0m), // 17 * 15
	    new TestResult(2, -10302060991507324713598.483586m), // 17 * 16
	    new TestResult(1, 0m), // 17 * 17
	    new TestResult(1, 0m), // 17 * 18
	    new TestResult(1, 0m), // 17 * 19
	    new TestResult(0, 173232394409674404469121757.58m), // 17 * 20
	    new TestResult(2, 10170667920993375211218037940m), // 17 * 21
	    new TestResult(1, 0m), // 17 * 22
	    new TestResult(1, 0m), // 17 * 23
	    new TestResult(1, 0m), // 17 * 24
	    new TestResult(1, 0m), // 17 * 25
	    new TestResult(1, 0m), // 17 * 26
	    new TestResult(1, 0m), // 17 * 27
	    new TestResult(1, 0m), // 17 * 28
	    new TestResult(0, -122246061989021334943606343.1m), // 17 * 29
	    new TestResult(0, 0m), // 18 * 0
	    new TestResult(0, -5323259153836385912697776.001m), // 18 * 1
	    new TestResult(0, 5323259153836385912697776.001m), // 18 * 2
	    new TestResult(0, -10646518307672771825395552.002m), // 18 * 3
	    new TestResult(0, -53232591538363859126977760.01m), // 18 * 4
	    new TestResult(0, -532325915383638591269777.6001m), // 18 * 5
	    new TestResult(1, 0m), // 18 * 6
	    new TestResult(1, 0m), // 18 * 7
	    new TestResult(1, 0m), // 18 * 8
	    new TestResult(1, 0m), // 18 * 9
	    new TestResult(1, 0m), // 18 * 10
	    new TestResult(1, 0m), // 18 * 11
	    new TestResult(2, -4061239001119573143590088.0528m), // 18 * 12
	    new TestResult(1, 0m), // 18 * 13
	    new TestResult(1, 0m), // 18 * 14
	    new TestResult(1, 0m), // 18 * 15
	    new TestResult(1, 0m), // 18 * 16
	    new TestResult(1, 0m), // 18 * 17
	    new TestResult(1, 0m), // 18 * 18
	    new TestResult(1, 0m), // 18 * 19
	    new TestResult(1, 0m), // 18 * 20
	    new TestResult(1, 0m), // 18 * 21
	    new TestResult(1, 0m), // 18 * 22
	    new TestResult(1, 0m), // 18 * 23
	    new TestResult(1, 0m), // 18 * 24
	    new TestResult(1, 0m), // 18 * 25
	    new TestResult(1, 0m), // 18 * 26
	    new TestResult(1, 0m), // 18 * 27
	    new TestResult(1, 0m), // 18 * 28
	    new TestResult(1, 0m), // 18 * 29
	    new TestResult(0, 0m), // 19 * 0
	    new TestResult(0, 102801066199805834724673169.19m), // 19 * 1
	    new TestResult(0, -102801066199805834724673169.19m), // 19 * 2
	    new TestResult(0, 205602132399611669449346338.38m), // 19 * 3
	    new TestResult(0, 1028010661998058347246731691.9m), // 19 * 4
	    new TestResult(0, 10280106619980583472467316.919m), // 19 * 5
	    new TestResult(1, 0m), // 19 * 6
	    new TestResult(1, 0m), // 19 * 7
	    new TestResult(1, 0m), // 19 * 8
	    new TestResult(1, 0m), // 19 * 9
	    new TestResult(1, 0m), // 19 * 10
	    new TestResult(1, 0m), // 19 * 11
	    new TestResult(0, 78429339497108899549297058.831m), // 19 * 12
	    new TestResult(1, 0m), // 19 * 13
	    new TestResult(1, 0m), // 19 * 14
	    new TestResult(1, 0m), // 19 * 15
	    new TestResult(1, 0m), // 19 * 16
	    new TestResult(1, 0m), // 19 * 17
	    new TestResult(1, 0m), // 19 * 18
	    new TestResult(1, 0m), // 19 * 19
	    new TestResult(1, 0m), // 19 * 20
	    new TestResult(1, 0m), // 19 * 21
	    new TestResult(1, 0m), // 19 * 22
	    new TestResult(1, 0m), // 19 * 23
	    new TestResult(1, 0m), // 19 * 24
	    new TestResult(1, 0m), // 19 * 25
	    new TestResult(1, 0m), // 19 * 26
	    new TestResult(1, 0m), // 19 * 27
	    new TestResult(1, 0m), // 19 * 28
	    new TestResult(1, 0m), // 19 * 29
	    new TestResult(0, 0m), // 20 * 0
	    new TestResult(0, 7081320760.3793287174700927968m), // 20 * 1
	    new TestResult(0, -7081320760.3793287174700927968m), // 20 * 2
	    new TestResult(2, 14162641520.758657434940185594m), // 20 * 3
	    new TestResult(0, 70813207603.793287174700927968m), // 20 * 4
	    new TestResult(0, 708132076.03793287174700927968m), // 20 * 5
	    new TestResult(1, 0m), // 20 * 6
	    new TestResult(1, 0m), // 20 * 7
	    new TestResult(1, 0m), // 20 * 8
	    new TestResult(0, -22687524613144469.045656755412m), // 20 * 9
	    new TestResult(1, 0m), // 20 * 10
	    new TestResult(0, -3860691418388152934958161711.9m), // 20 * 11
	    new TestResult(0, 5402505348.7700567259404098662m), // 20 * 12
	    new TestResult(1, 0m), // 20 * 13
	    new TestResult(1, 0m), // 20 * 14
	    new TestResult(1, 0m), // 20 * 15
	    new TestResult(2, -2982109198574002.1833628108505m), // 20 * 16
	    new TestResult(2, 173232394409674404469121757.58m), // 20 * 17
	    new TestResult(1, 0m), // 20 * 18
	    new TestResult(1, 0m), // 20 * 19
	    new TestResult(0, 50145103711379274243.914175878m), // 20 * 20
	    new TestResult(2, 2944075208624656937377.9493098m), // 20 * 21
	    new TestResult(0, -45245337684888495429839.445203m), // 20 * 22
	    new TestResult(2, 3132395915264028802867467.4171m), // 20 * 23
	    new TestResult(2, -3631540499079604049748263.216m), // 20 * 24
	    new TestResult(0, 4312103575884025168661620716.9m), // 20 * 25
	    new TestResult(1, 0m), // 20 * 26
	    new TestResult(1, 0m), // 20 * 27
	    new TestResult(1, 0m), // 20 * 28
	    new TestResult(2, -35386230604481178142.543841269m), // 20 * 29
	    new TestResult(0, 0m), // 21 * 0
	    new TestResult(0, 415752273939.77704245656837041m), // 21 * 1
	    new TestResult(0, -415752273939.77704245656837041m), // 21 * 2
	    new TestResult(2, 831504547879.5540849131367408m), // 21 * 3
	    new TestResult(0, 4157522739397.7704245656837041m), // 21 * 4
	    new TestResult(0, 41575227393.977704245656837041m), // 21 * 5
	    new TestResult(1, 0m), // 21 * 6
	    new TestResult(1, 0m), // 21 * 7
	    new TestResult(1, 0m), // 21 * 8
	    new TestResult(2, -1332009983328901461.3254059884m), // 21 * 9
	    new TestResult(1, 0m), // 21 * 10
	    new TestResult(1, 0m), // 21 * 11
	    new TestResult(0, 317187140609.43641612785737895m), // 21 * 12
	    new TestResult(1, 0m), // 21 * 13
	    new TestResult(1, 0m), // 21 * 14
	    new TestResult(1, 0m), // 21 * 15
	    new TestResult(2, -175082971439561442.82206371811m), // 21 * 16
	    new TestResult(2, 10170667920993375211218037940m), // 21 * 17
	    new TestResult(1, 0m), // 21 * 18
	    new TestResult(1, 0m), // 21 * 19
	    new TestResult(2, 2944075208624656937377.9493098m), // 21 * 20
	    new TestResult(2, 172849953286095412912252.49708m), // 21 * 21
	    new TestResult(2, -2656404456766597431365611.212m), // 21 * 22
	    new TestResult(0, 183906472919174492029733196.17m), // 21 * 23
	    new TestResult(0, -213211810548723232636647639.7m), // 21 * 24
	    new TestResult(1, 0m), // 21 * 25
	    new TestResult(1, 0m), // 21 * 26
	    new TestResult(1, 0m), // 21 * 27
	    new TestResult(1, 0m), // 21 * 28
	    new TestResult(0, -2077565236457711426002.3992246m), // 21 * 29
	    new TestResult(0, 0m), // 22 * 0
	    new TestResult(0, -6389392489892.6362673670820462m), // 22 * 1
	    new TestResult(0, 6389392489892.6362673670820462m), // 22 * 2
	    new TestResult(2, -12778784979785.272534734164092m), // 22 * 3
	    new TestResult(0, -63893924898926.362673670820462m), // 22 * 4
	    new TestResult(0, -638939248989.26362673670820462m), // 22 * 5
	    new TestResult(1, 0m), // 22 * 6
	    new TestResult(1, 0m), // 22 * 7
	    new TestResult(1, 0m), // 22 * 8
	    new TestResult(2, 20470686794551372519.831909846m), // 22 * 9
	    new TestResult(1, 0m), // 22 * 10
	    new TestResult(1, 0m), // 22 * 11
	    new TestResult(0, -4874617076403.5713301079734445m), // 22 * 12
	    new TestResult(1, 0m), // 22 * 13
	    new TestResult(1, 0m), // 22 * 14
	    new TestResult(1, 0m), // 22 * 15
	    new TestResult(2, 2690722078855217643.0013135833m), // 22 * 16
	    new TestResult(1, 0m), // 22 * 17
	    new TestResult(1, 0m), // 22 * 18
	    new TestResult(1, 0m), // 22 * 19
	    new TestResult(0, -45245337684888495429839.445203m), // 22 * 20
	    new TestResult(2, -2656404456766597431365611.212m), // 22 * 21
	    new TestResult(2, 40824336389896422046045259.169m), // 22 * 22
	    new TestResult(2, -2826324016889506134750576955.1m), // 22 * 23
	    new TestResult(2, 3276696308036925201560804370m), // 22 * 24
	    new TestResult(1, 0m), // 22 * 25
	    new TestResult(1, 0m), // 22 * 26
	    new TestResult(1, 0m), // 22 * 27
	    new TestResult(1, 0m), // 22 * 28
	    new TestResult(2, 31928579952897032005741.500403m), // 22 * 29
	    new TestResult(0, 0m), // 23 * 0
	    new TestResult(0, 442346282742915.0596416330681m), // 23 * 1
	    new TestResult(0, -442346282742915.0596416330681m), // 23 * 2
	    new TestResult(0, 884692565485830.1192832661362m), // 23 * 3
	    new TestResult(0, 4423462827429150.596416330681m), // 23 * 4
	    new TestResult(0, 44234628274291.50596416330681m), // 23 * 5
	    new TestResult(1, 0m), // 23 * 6
	    new TestResult(1, 0m), // 23 * 7
	    new TestResult(1, 0m), // 23 * 8
	    new TestResult(2, -1417213330232658207868.9685141m), // 23 * 9
	    new TestResult(1, 0m), // 23 * 10
	    new TestResult(1, 0m), // 23 * 11
	    new TestResult(2, 337476332367005.49979200414696m), // 23 * 12
	    new TestResult(1, 0m), // 23 * 13
	    new TestResult(1, 0m), // 23 * 14
	    new TestResult(1, 0m), // 23 * 15
	    new TestResult(2, -186282328305659347664.31347679m), // 23 * 16
	    new TestResult(1, 0m), // 23 * 17
	    new TestResult(1, 0m), // 23 * 18
	    new TestResult(1, 0m), // 23 * 19
	    new TestResult(2, 3132395915264028802867467.4171m), // 23 * 20
	    new TestResult(0, 183906472919174492029733196.17m), // 23 * 21
	    new TestResult(2, -2826324016889506134750576955.1m), // 23 * 22
	    new TestResult(1, 0m), // 23 * 23
	    new TestResult(1, 0m), // 23 * 24
	    new TestResult(1, 0m), // 23 * 25
	    new TestResult(1, 0m), // 23 * 26
	    new TestResult(1, 0m), // 23 * 27
	    new TestResult(1, 0m), // 23 * 28
	    new TestResult(2, -2210458768617810051106106.871m), // 23 * 29
	    new TestResult(0, 0m), // 24 * 0
	    new TestResult(0, -512833780867323.89020837443764m), // 24 * 1
	    new TestResult(0, 512833780867323.89020837443764m), // 24 * 2
	    new TestResult(2, -1025667561734647.7804167488753m), // 24 * 3
	    new TestResult(0, -5128337808673238.9020837443764m), // 24 * 4
	    new TestResult(0, -51283378086732.389020837443764m), // 24 * 5
	    new TestResult(1, 0m), // 24 * 6
	    new TestResult(1, 0m), // 24 * 7
	    new TestResult(1, 0m), // 24 * 8
	    new TestResult(0, 1643045050434361863551.7087135m), // 24 * 9
	    new TestResult(1, 0m), // 24 * 10
	    new TestResult(1, 0m), // 24 * 11
	    new TestResult(0, -391252894469544.55412631906773m), // 24 * 12
	    new TestResult(1, 0m), // 24 * 13
	    new TestResult(1, 0m), // 24 * 14
	    new TestResult(1, 0m), // 24 * 15
	    new TestResult(2, 215966256439146035447.16512997m), // 24 * 16
	    new TestResult(1, 0m), // 24 * 17
	    new TestResult(1, 0m), // 24 * 18
	    new TestResult(1, 0m), // 24 * 19
	    new TestResult(2, -3631540499079604049748263.216m), // 24 * 20
	    new TestResult(0, -213211810548723232636647639.7m), // 24 * 21
	    new TestResult(2, 3276696308036925201560804370m), // 24 * 22
	    new TestResult(1, 0m), // 24 * 23
	    new TestResult(1, 0m), // 24 * 24
	    new TestResult(1, 0m), // 24 * 25
	    new TestResult(1, 0m), // 24 * 26
	    new TestResult(1, 0m), // 24 * 27
	    new TestResult(1, 0m), // 24 * 28
	    new TestResult(2, 2562693464342799730524457.4865m), // 24 * 29
	    new TestResult(0, 0m), // 25 * 0
	    new TestResult(0, 608940580690915704.1450897514m), // 25 * 1
	    new TestResult(0, -608940580690915704.1450897514m), // 25 * 2
	    new TestResult(0, 1217881161381831408.2901795028m), // 25 * 3
	    new TestResult(0, 6089405806909157041.450897514m), // 25 * 4
	    new TestResult(0, 60894058069091570.41450897514m), // 25 * 5
	    new TestResult(1, 0m), // 25 * 6
	    new TestResult(1, 0m), // 25 * 7
	    new TestResult(1, 0m), // 25 * 8
	    new TestResult(0, -1950957297354170624860913.7855m), // 25 * 9
	    new TestResult(1, 0m), // 25 * 10
	    new TestResult(1, 0m), // 25 * 11
	    new TestResult(0, 464575021466700199.22364418475m), // 25 * 12
	    new TestResult(1, 0m), // 25 * 13
	    new TestResult(1, 0m), // 25 * 14
	    new TestResult(1, 0m), // 25 * 15
	    new TestResult(0, -256439069562229438659442.67473m), // 25 * 16
	    new TestResult(1, 0m), // 25 * 17
	    new TestResult(1, 0m), // 25 * 18
	    new TestResult(1, 0m), // 25 * 19
	    new TestResult(0, 4312103575884025168661620716.9m), // 25 * 20
	    new TestResult(1, 0m), // 25 * 21
	    new TestResult(1, 0m), // 25 * 22
	    new TestResult(1, 0m), // 25 * 23
	    new TestResult(1, 0m), // 25 * 24
	    new TestResult(1, 0m), // 25 * 25
	    new TestResult(1, 0m), // 25 * 26
	    new TestResult(1, 0m), // 25 * 27
	    new TestResult(1, 0m), // 25 * 28
	    new TestResult(0, -3042950960973153681431212945.8m), // 25 * 29
	    new TestResult(0, 0m), // 26 * 0
	    new TestResult(0, -42535053313319986966115.037787m), // 26 * 1
	    new TestResult(0, 42535053313319986966115.037787m), // 26 * 2
	    new TestResult(2, -85070106626639973932230.07557m), // 26 * 3
	    new TestResult(0, -425350533133199869661150.37787m), // 26 * 4
	    new TestResult(0, -4253505331331998696611.5037787m), // 26 * 5
	    new TestResult(1, 0m), // 26 * 6
	    new TestResult(1, 0m), // 26 * 7
	    new TestResult(1, 0m), // 26 * 8
	    new TestResult(1, 0m), // 26 * 9
	    new TestResult(1, 0m), // 26 * 10
	    new TestResult(1, 0m), // 26 * 11
	    new TestResult(0, -32450987719855972399063.033158m), // 26 * 12
	    new TestResult(1, 0m), // 26 * 13
	    new TestResult(1, 0m), // 26 * 14
	    new TestResult(1, 0m), // 26 * 15
	    new TestResult(2, 17912502206818886711664532432m), // 26 * 16
	    new TestResult(1, 0m), // 26 * 17
	    new TestResult(1, 0m), // 26 * 18
	    new TestResult(1, 0m), // 26 * 19
	    new TestResult(1, 0m), // 26 * 20
	    new TestResult(1, 0m), // 26 * 21
	    new TestResult(1, 0m), // 26 * 22
	    new TestResult(1, 0m), // 26 * 23
	    new TestResult(1, 0m), // 26 * 24
	    new TestResult(1, 0m), // 26 * 25
	    new TestResult(1, 0m), // 26 * 26
	    new TestResult(1, 0m), // 26 * 27
	    new TestResult(1, 0m), // 26 * 28
	    new TestResult(1, 0m), // 26 * 29
	    new TestResult(0, 0m), // 27 * 0
	    new TestResult(0, -7808274522591953107485.8812311m), // 27 * 1
	    new TestResult(0, 7808274522591953107485.8812311m), // 27 * 2
	    new TestResult(2, -15616549045183906214971.762462m), // 27 * 3
	    new TestResult(0, -78082745225919531074858.812311m), // 27 * 4
	    new TestResult(0, -780827452259195310748.58812311m), // 27 * 5
	    new TestResult(1, 0m), // 27 * 6
	    new TestResult(1, 0m), // 27 * 7
	    new TestResult(1, 0m), // 27 * 8
	    new TestResult(0, 25016579026989918165002777574m), // 27 * 9
	    new TestResult(1, 0m), // 27 * 10
	    new TestResult(1, 0m), // 27 * 11
	    new TestResult(0, -5957115388557583551533.0994303m), // 27 * 12
	    new TestResult(1, 0m), // 27 * 13
	    new TestResult(1, 0m), // 27 * 14
	    new TestResult(1, 0m), // 27 * 15
	    new TestResult(0, 3288246369108855691627019039.6m), // 27 * 16
	    new TestResult(1, 0m), // 27 * 17
	    new TestResult(1, 0m), // 27 * 18
	    new TestResult(1, 0m), // 27 * 19
	    new TestResult(1, 0m), // 27 * 20
	    new TestResult(1, 0m), // 27 * 21
	    new TestResult(1, 0m), // 27 * 22
	    new TestResult(1, 0m), // 27 * 23
	    new TestResult(1, 0m), // 27 * 24
	    new TestResult(1, 0m), // 27 * 25
	    new TestResult(1, 0m), // 27 * 26
	    new TestResult(1, 0m), // 27 * 27
	    new TestResult(1, 0m), // 27 * 28
	    new TestResult(1, 0m), // 27 * 29
	    new TestResult(0, 0m), // 28 * 0
	    new TestResult(0, 1037807626804273037330059471.7m), // 28 * 1
	    new TestResult(0, -1037807626804273037330059471.7m), // 28 * 2
	    new TestResult(0, 2075615253608546074660118943.4m), // 28 * 3
	    new TestResult(0, 10378076268042730373300594717m), // 28 * 4
	    new TestResult(0, 103780762680427303733005947.17m), // 28 * 5
	    new TestResult(1, 0m), // 28 * 6
	    new TestResult(1, 0m), // 28 * 7
	    new TestResult(1, 0m), // 28 * 8
	    new TestResult(1, 0m), // 28 * 9
	    new TestResult(1, 0m), // 28 * 10
	    new TestResult(1, 0m), // 28 * 11
	    new TestResult(0, 791767728722982425613218218.59m), // 28 * 12
	    new TestResult(1, 0m), // 28 * 13
	    new TestResult(1, 0m), // 28 * 14
	    new TestResult(1, 0m), // 28 * 15
	    new TestResult(1, 0m), // 28 * 16
	    new TestResult(1, 0m), // 28 * 17
	    new TestResult(1, 0m), // 28 * 18
	    new TestResult(1, 0m), // 28 * 19
	    new TestResult(1, 0m), // 28 * 20
	    new TestResult(1, 0m), // 28 * 21
	    new TestResult(1, 0m), // 28 * 22
	    new TestResult(1, 0m), // 28 * 23
	    new TestResult(1, 0m), // 28 * 24
	    new TestResult(1, 0m), // 28 * 25
	    new TestResult(1, 0m), // 28 * 26
	    new TestResult(1, 0m), // 28 * 27
	    new TestResult(1, 0m), // 28 * 28
	    new TestResult(1, 0m), // 28 * 29
	    new TestResult(0, 0m), // 29 * 0
	    new TestResult(0, -4997122966.448652425771563042m), // 29 * 1
	    new TestResult(0, 4997122966.448652425771563042m), // 29 * 2
	    new TestResult(0, -9994245932.897304851543126084m), // 29 * 3
	    new TestResult(0, -49971229664.48652425771563042m), // 29 * 4
	    new TestResult(0, -499712296.6448652425771563042m), // 29 * 5
	    new TestResult(1, 0m), // 29 * 6
	    new TestResult(1, 0m), // 29 * 7
	    new TestResult(1, 0m), // 29 * 8
	    new TestResult(2, 16010057181782036.694377696165m), // 29 * 9
	    new TestResult(1, 0m), // 29 * 10
	    new TestResult(2, 2724399925666581324856736883m), // 29 * 11
	    new TestResult(2, -3812422070.43511700405678157m), // 29 * 12
	    new TestResult(1, 0m), // 29 * 13
	    new TestResult(1, 0m), // 29 * 14
	    new TestResult(1, 0m), // 29 * 15
	    new TestResult(2, 2104404936439239.9111285468803m), // 29 * 16
	    new TestResult(2, -122246061989021334943606343.1m), // 29 * 17
	    new TestResult(1, 0m), // 29 * 18
	    new TestResult(1, 0m), // 29 * 19
	    new TestResult(0, -35386230604481178142.543841269m), // 29 * 20
	    new TestResult(0, -2077565236457711426002.3992246m), // 29 * 21
	    new TestResult(2, 31928579952897032005741.500403m), // 29 * 22
	    new TestResult(2, -2210458768617810051106106.871m), // 29 * 23
	    new TestResult(2, 2562693464342799730524457.4865m), // 29 * 24
	    new TestResult(0, -3042950960973153681431212945.8m), // 29 * 25
	    new TestResult(1, 0m), // 29 * 26
	    new TestResult(1, 0m), // 29 * 27
	    new TestResult(1, 0m), // 29 * 28
	    new TestResult(0, 24971237941808579837.350664893m), // 29 * 29
        };


        // generated result list build2
        TestResult[] trAuto_Div_build2 = new TestResult[] {
	    new TestResult(3, 0m), // 0 / 0
	    new TestResult(0, 0m), // 0 / 1
	    new TestResult(0, 0m), // 0 / 2
	    new TestResult(0, 0m), // 0 / 3
	    new TestResult(0, 0m), // 0 / 4
	    new TestResult(0, 0m), // 0 / 5
	    new TestResult(0, 0m), // 0 / 6
	    new TestResult(0, 0m), // 0 / 7
	    new TestResult(0, 0m), // 0 / 8
	    new TestResult(0, 0m), // 0 / 9
	    new TestResult(0, 0m), // 0 / 10
	    new TestResult(0, 0m), // 0 / 11
	    new TestResult(0, 0m), // 0 / 12
	    new TestResult(0, 0m), // 0 / 13
	    new TestResult(0, 0m), // 0 / 14
	    new TestResult(0, 0m), // 0 / 15
	    new TestResult(0, 0m), // 0 / 16
	    new TestResult(0, 0m), // 0 / 17
	    new TestResult(0, 0m), // 0 / 18
	    new TestResult(0, 0m), // 0 / 19
	    new TestResult(0, 0m), // 0 / 20
	    new TestResult(0, 0m), // 0 / 21
	    new TestResult(0, 0m), // 0 / 22
	    new TestResult(0, 0m), // 0 / 23
	    new TestResult(0, 0m), // 0 / 24
	    new TestResult(0, 0m), // 0 / 25
	    new TestResult(0, 0m), // 0 / 26
	    new TestResult(0, 0m), // 0 / 27
	    new TestResult(0, 0m), // 0 / 28
	    new TestResult(0, 0m), // 0 / 29
	    new TestResult(3, 0m), // 1 / 0
	    new TestResult(0, 1m), // 1 / 1
	    new TestResult(0, -1m), // 1 / 2
	    new TestResult(0, 0.5m), // 1 / 3
	    new TestResult(0, 0.1m), // 1 / 4
	    new TestResult(0, 10m), // 1 / 5
	    new TestResult(0, 0m), // 1 / 6
	    new TestResult(0, 0m), // 1 / 7
	    new TestResult(2, 3.61E-26m), // 1 / 8
	    new TestResult(2, -3.121239924198969049581E-07m), // 1 / 9
	    new TestResult(2, -2.06327E-23m), // 1 / 10
	    new TestResult(2, -1.8342105061E-18m), // 1 / 11
	    new TestResult(0, 1.3107475704751451797758879069m), // 1 / 12
	    new TestResult(2, -2.4971716E-21m), // 1 / 13
	    new TestResult(2, 4.4873E-24m), // 1 / 14
	    new TestResult(2, 7.02E-26m), // 1 / 15
	    new TestResult(2, -2.3746014276625098667414E-06m), // 1 / 16
	    new TestResult(2, 4.08775782642E-17m), // 1 / 17
	    new TestResult(2, -1.879E-25m), // 1 / 18
	    new TestResult(2, 9.7E-27m), // 1 / 19
	    new TestResult(2, 1.412165941691409118E-10m), // 1 / 20
	    new TestResult(2, 2.4052784859689137E-12m), // 1 / 21
	    new TestResult(2, -1.565094023542766E-13m), // 1 / 22
	    new TestResult(2, 2.2606723262128E-15m), // 1 / 23
	    new TestResult(2, -1.9499495495573E-15m), // 1 / 24
	    new TestResult(2, 1.6421963517E-18m), // 1 / 25
	    new TestResult(2, -2.351E-23m), // 1 / 26
	    new TestResult(2, -1.280693E-22m), // 1 / 27
	    new TestResult(2, 1E-27m), // 1 / 28
	    new TestResult(2, -2.00115147598754898E-10m), // 1 / 29
	    new TestResult(3, 0m), // 2 / 0
	    new TestResult(0, -1m), // 2 / 1
	    new TestResult(0, 1m), // 2 / 2
	    new TestResult(0, -0.5m), // 2 / 3
	    new TestResult(0, -0.1m), // 2 / 4
	    new TestResult(0, -10m), // 2 / 5
	    new TestResult(0, 0m), // 2 / 6
	    new TestResult(0, 0m), // 2 / 7
	    new TestResult(2, -3.61E-26m), // 2 / 8
	    new TestResult(2, 3.121239924198969049581E-07m), // 2 / 9
	    new TestResult(2, 2.06327E-23m), // 2 / 10
	    new TestResult(2, 1.8342105061E-18m), // 2 / 11
	    new TestResult(0, -1.3107475704751451797758879069m), // 2 / 12
	    new TestResult(2, 2.4971716E-21m), // 2 / 13
	    new TestResult(2, -4.4873E-24m), // 2 / 14
	    new TestResult(2, -7.02E-26m), // 2 / 15
	    new TestResult(2, 2.3746014276625098667414E-06m), // 2 / 16
	    new TestResult(2, -4.08775782642E-17m), // 2 / 17
	    new TestResult(2, 1.879E-25m), // 2 / 18
	    new TestResult(2, -9.7E-27m), // 2 / 19
	    new TestResult(2, -1.412165941691409118E-10m), // 2 / 20
	    new TestResult(2, -2.4052784859689137E-12m), // 2 / 21
	    new TestResult(2, 1.565094023542766E-13m), // 2 / 22
	    new TestResult(2, -2.2606723262128E-15m), // 2 / 23
	    new TestResult(2, 1.9499495495573E-15m), // 2 / 24
	    new TestResult(2, -1.6421963517E-18m), // 2 / 25
	    new TestResult(2, 2.351E-23m), // 2 / 26
	    new TestResult(2, 1.280693E-22m), // 2 / 27
	    new TestResult(2, -1E-27m), // 2 / 28
	    new TestResult(2, 2.00115147598754898E-10m), // 2 / 29
	    new TestResult(3, 0m), // 3 / 0
	    new TestResult(0, 2m), // 3 / 1
	    new TestResult(0, -2m), // 3 / 2
	    new TestResult(0, 1m), // 3 / 3
	    new TestResult(0, 0.2m), // 3 / 4
	    new TestResult(0, 20m), // 3 / 5
	    new TestResult(0, 0m), // 3 / 6
	    new TestResult(0, 0m), // 3 / 7
	    new TestResult(2, 7.22E-26m), // 3 / 8
	    new TestResult(2, -6.242479848397938099161E-07m), // 3 / 9
	    new TestResult(2, -4.12653E-23m), // 3 / 10
	    new TestResult(2, -3.6684210122E-18m), // 3 / 11
	    new TestResult(0, 2.6214951409502903595517758138m), // 3 / 12
	    new TestResult(2, -4.9943432E-21m), // 3 / 13
	    new TestResult(2, 8.9746E-24m), // 3 / 14
	    new TestResult(2, 1.404E-25m), // 3 / 15
	    new TestResult(2, -4.7492028553250197334829E-06m), // 3 / 16
	    new TestResult(2, 8.17551565284E-17m), // 3 / 17
	    new TestResult(2, -3.757E-25m), // 3 / 18
	    new TestResult(2, 1.95E-26m), // 3 / 19
	    new TestResult(2, 2.824331883382818237E-10m), // 3 / 20
	    new TestResult(2, 4.8105569719378275E-12m), // 3 / 21
	    new TestResult(2, -3.130188047085533E-13m), // 3 / 22
	    new TestResult(2, 4.5213446524256E-15m), // 3 / 23
	    new TestResult(2, -3.8998990991146E-15m), // 3 / 24
	    new TestResult(2, 3.2843927034E-18m), // 3 / 25
	    new TestResult(2, -4.702E-23m), // 3 / 26
	    new TestResult(2, -2.561385E-22m), // 3 / 27
	    new TestResult(2, 1.9E-27m), // 3 / 28
	    new TestResult(2, -4.002302951975097959E-10m), // 3 / 29
	    new TestResult(3, 0m), // 4 / 0
	    new TestResult(0, 10m), // 4 / 1
	    new TestResult(0, -10m), // 4 / 2
	    new TestResult(0, 5m), // 4 / 3
	    new TestResult(0, 1m), // 4 / 4
	    new TestResult(0, 100m), // 4 / 5
	    new TestResult(2, 1E-28m), // 4 / 6
	    new TestResult(2, -1E-28m), // 4 / 7
	    new TestResult(2, 3.61E-25m), // 4 / 8
	    new TestResult(2, -3.1212399241989690495806E-06m), // 4 / 9
	    new TestResult(2, -2.063265E-22m), // 4 / 10
	    new TestResult(2, -1.83421050609E-17m), // 4 / 11
	    new TestResult(0, 13.107475704751451797758879069m), // 4 / 12
	    new TestResult(2, -2.49717158E-20m), // 4 / 13
	    new TestResult(2, 4.48729E-23m), // 4 / 14
	    new TestResult(2, 7.019E-25m), // 4 / 15
	    new TestResult(2, -2.37460142766250986674143E-05m), // 4 / 16
	    new TestResult(2, 4.087757826422E-16m), // 4 / 17
	    new TestResult(2, -1.8785E-24m), // 4 / 18
	    new TestResult(2, 9.73E-26m), // 4 / 19
	    new TestResult(2, 1.4121659416914091185E-09m), // 4 / 20
	    new TestResult(2, 2.40527848596891375E-11m), // 4 / 21
	    new TestResult(2, -1.5650940235427663E-12m), // 4 / 22
	    new TestResult(2, 2.26067232621278E-14m), // 4 / 23
	    new TestResult(2, -1.94994954955729E-14m), // 4 / 24
	    new TestResult(2, 1.64219635168E-17m), // 4 / 25
	    new TestResult(2, -2.351002E-22m), // 4 / 26
	    new TestResult(2, -1.2806927E-21m), // 4 / 27
	    new TestResult(2, 9.6E-27m), // 4 / 28
	    new TestResult(2, -2.0011514759875489796E-09m), // 4 / 29
	    new TestResult(3, 0m), // 5 / 0
	    new TestResult(0, 0.1m), // 5 / 1
	    new TestResult(0, -0.1m), // 5 / 2
	    new TestResult(0, 0.05m), // 5 / 3
	    new TestResult(0, 0.01m), // 5 / 4
	    new TestResult(0, 1m), // 5 / 5
	    new TestResult(0, 0m), // 5 / 6
	    new TestResult(0, 0m), // 5 / 7
	    new TestResult(2, 3.6E-27m), // 5 / 8
	    new TestResult(2, -3.12123992419896904958E-08m), // 5 / 9
	    new TestResult(2, -2.0633E-24m), // 5 / 10
	    new TestResult(2, -1.834210506E-19m), // 5 / 11
	    new TestResult(0, 0.1310747570475145179775887907m), // 5 / 12
	    new TestResult(2, -2.497172E-22m), // 5 / 13
	    new TestResult(2, 4.487E-25m), // 5 / 14
	    new TestResult(2, 7E-27m), // 5 / 15
	    new TestResult(2, -2.374601427662509866741E-07m), // 5 / 16
	    new TestResult(2, 4.0877578264E-18m), // 5 / 17
	    new TestResult(2, -1.88E-26m), // 5 / 18
	    new TestResult(2, 1E-27m), // 5 / 19
	    new TestResult(2, 1.41216594169140912E-11m), // 5 / 20
	    new TestResult(2, 2.405278485968914E-13m), // 5 / 21
	    new TestResult(2, -1.56509402354277E-14m), // 5 / 22
	    new TestResult(2, 2.260672326213E-16m), // 5 / 23
	    new TestResult(2, -1.949949549557E-16m), // 5 / 24
	    new TestResult(2, 1.642196352E-19m), // 5 / 25
	    new TestResult(2, -2.351E-24m), // 5 / 26
	    new TestResult(2, -1.28069E-23m), // 5 / 27
	    new TestResult(2, 1E-28m), // 5 / 28
	    new TestResult(2, -2.00115147598754898E-11m), // 5 / 29
	    new TestResult(3, 0m), // 6 / 0
	    new TestResult(0, 79228162514264337593543950335m), // 6 / 1
	    new TestResult(0, -79228162514264337593543950335m), // 6 / 2
	    new TestResult(4, 39614081257132168796771975168m), // 6 / 3
	    new TestResult(4, 7922816251426433759354395033.5m), // 6 / 4
	    new TestResult(1, 0m), // 6 / 5
	    new TestResult(0, 1m), // 6 / 6
	    new TestResult(0, -1m), // 6 / 7
	    new TestResult(2, 2859.8815108190596050496514036m), // 6 / 8
	    new TestResult(2, -24729010396044602218186.765763m), // 6 / 9
	    new TestResult(0, -1634686.9890330150250120997619m), // 6 / 10
	    new TestResult(2, -145321128061.87519668639091926m), // 6 / 11
	    new TestResult(1, 0m), // 6 / 12
	    new TestResult(0, -197846315.8215581473367403789m), // 6 / 13
	    new TestResult(0, 355519.78373087286985534915017m), // 6 / 14
	    new TestResult(0, 5561.4152228343398478498516935m), // 6 / 15
	    new TestResult(0, -188135307817449443294332.61564m), // 6 / 16
	    new TestResult(4, 3238655413907.0152158135371692m), // 6 / 17
	    new TestResult(0, -14883.393842880239095927799143m), // 6 / 18
	    new TestResult(0, 770.69397665852204557888702506m), // 6 / 19
	    new TestResult(4, 11188331272543609822.484506627m), // 6 / 20
	    new TestResult(0, 190565794778408772.72439011791m), // 6 / 21
	    new TestResult(4, -12399952364735014.543439368391m), // 6 / 22
	    new TestResult(2, 179108914452685.79816067884477m), // 6 / 23
	    new TestResult(2, -154490919806941.48506503891762m), // 6 / 24
	    new TestResult(2, 130108199431.16705963085822836m), // 6 / 25
	    new TestResult(2, -1862655.7707746844883926874832m), // 6 / 26
	    new TestResult(2, -10146692.753313261555369747673m), // 6 / 27
	    new TestResult(0, 76.341858036091015923443233609m), // 6 / 28
	    new TestResult(2, -15854755435520147876.101506668m), // 6 / 29
	    new TestResult(3, 0m), // 7 / 0
	    new TestResult(0, -79228162514264337593543950335m), // 7 / 1
	    new TestResult(0, 79228162514264337593543950335m), // 7 / 2
	    new TestResult(4, -39614081257132168796771975168m), // 7 / 3
	    new TestResult(4, -7922816251426433759354395033.5m), // 7 / 4
	    new TestResult(1, 0m), // 7 / 5
	    new TestResult(0, -1m), // 7 / 6
	    new TestResult(0, 1m), // 7 / 7
	    new TestResult(2, -2859.8815108190596050496514036m), // 7 / 8
	    new TestResult(2, 24729010396044602218186.765763m), // 7 / 9
	    new TestResult(0, 1634686.9890330150250120997619m), // 7 / 10
	    new TestResult(2, 145321128061.87519668639091926m), // 7 / 11
	    new TestResult(1, 0m), // 7 / 12
	    new TestResult(0, 197846315.8215581473367403789m), // 7 / 13
	    new TestResult(0, -355519.78373087286985534915017m), // 7 / 14
	    new TestResult(0, -5561.4152228343398478498516935m), // 7 / 15
	    new TestResult(0, 188135307817449443294332.61564m), // 7 / 16
	    new TestResult(4, -3238655413907.0152158135371692m), // 7 / 17
	    new TestResult(0, 14883.393842880239095927799143m), // 7 / 18
	    new TestResult(0, -770.69397665852204557888702506m), // 7 / 19
	    new TestResult(4, -11188331272543609822.484506627m), // 7 / 20
	    new TestResult(0, -190565794778408772.72439011791m), // 7 / 21
	    new TestResult(4, 12399952364735014.543439368391m), // 7 / 22
	    new TestResult(2, -179108914452685.79816067884477m), // 7 / 23
	    new TestResult(2, 154490919806941.48506503891762m), // 7 / 24
	    new TestResult(2, -130108199431.16705963085822836m), // 7 / 25
	    new TestResult(2, 1862655.7707746844883926874832m), // 7 / 26
	    new TestResult(2, 10146692.753313261555369747673m), // 7 / 27
	    new TestResult(0, -76.341858036091015923443233609m), // 7 / 28
	    new TestResult(2, 15854755435520147876.101506668m), // 7 / 29
	    new TestResult(3, 0m), // 8 / 0
	    new TestResult(0, 27703302467091960609331879.532m), // 8 / 1
	    new TestResult(0, -27703302467091960609331879.532m), // 8 / 2
	    new TestResult(0, 13851651233545980304665939.766m), // 8 / 3
	    new TestResult(0, 2770330246709196060933187.9532m), // 8 / 4
	    new TestResult(0, 277033024670919606093318795.32m), // 8 / 5
	    new TestResult(2, 0.0003496648361888264585379374m), // 8 / 6
	    new TestResult(2, -0.0003496648361888264585379374m), // 8 / 7
	    new TestResult(0, 1m), // 8 / 8
	    new TestResult(2, -8646865369244722339.540450168m), // 8 / 9
	    new TestResult(0, -571.59255824023515226354006889m), // 8 / 10
	    new TestResult(0, -50813688.438531062464476589278m), // 8 / 11
	    new TestResult(0, 36312036402878882966329689.018m), // 8 / 12
	    new TestResult(0, -69179.899612307954004328275068m), // 8 / 13
	    new TestResult(2, 124.31276694014267187689413445m), // 8 / 14
	    new TestResult(0, 1.9446313428704152390534655913m), // 8 / 15
	    new TestResult(0, -65784301589322901428.917817252m), // 8 / 16
	    new TestResult(2, 1132443914.7758524269197245123m), // 8 / 17
	    new TestResult(0, -5.2041994700045071214948001057m), // 8 / 18
	    new TestResult(2, 0.2694845831000173532986745484m), // 8 / 19
	    new TestResult(0, 3912166021640285.6025706982227m), // 8 / 20
	    new TestResult(0, 66634157414385.824002109431498m), // 8 / 21
	    new TestResult(0, -4335827312364.3201348613764117m), // 8 / 22
	    new TestResult(0, 62628089232.056911382057917869m), // 8 / 23
	    new TestResult(0, -54020042166.955319300276527479m), // 8 / 24
	    new TestResult(0, 45494262.240922193719761217507m), // 8 / 25
	    new TestResult(0, -651.30522496410233725620872491m), // 8 / 26
	    new TestResult(0, -3547.9416594456341168641639951m), // 8 / 27
	    new TestResult(2, 0.0266940632845404098551052086m), // 8 / 28
	    new TestResult(0, -5543850462175058.4016671641533m), // 8 / 29
	    new TestResult(3, 0m), // 9 / 0
	    new TestResult(0, -3203854.9559968181492513385018m), // 9 / 1
	    new TestResult(0, 3203854.9559968181492513385018m), // 9 / 2
	    new TestResult(0, -1601927.4779984090746256692509m), // 9 / 3
	    new TestResult(0, -320385.49559968181492513385018m), // 9 / 4
	    new TestResult(0, -32038549.559968181492513385018m), // 9 / 5
	    new TestResult(2, -4.04383E-23m), // 9 / 6
	    new TestResult(2, 4.04383E-23m), // 9 / 7
	    new TestResult(2, -1.156488458E-19m), // 9 / 8
	    new TestResult(0, 1m), // 9 / 9
	    new TestResult(2, 6.61040196455E-17m), // 9 / 10
	    new TestResult(2, 5.8765444202781065E-12m), // 9 / 11
	    new TestResult(0, -4199445.0997275825559060668592m), // 9 / 12
	    new TestResult(2, 8.0005755447943E-15m), // 9 / 13
	    new TestResult(2, -1.43766280186E-17m), // 9 / 14
	    new TestResult(2, -2.248943704E-19m), // 9 / 15
	    new TestResult(0, 7.6078785525336521046280405305m), // 9 / 16
	    new TestResult(2, -1.309658317109623269E-10m), // 9 / 17
	    new TestResult(2, 6.018596622E-19m), // 9 / 18
	    new TestResult(2, -3.1165581E-20m), // 9 / 19
	    new TestResult(2, -0.0004524374850977934825669157m), // 9 / 20
	    new TestResult(2, -7.7061633978240275370839E-06m), // 9 / 21
	    new TestResult(2, 5.014334243928492665444E-07m), // 9 / 22
	    new TestResult(2, -7.242866236221657103E-09m), // 9 / 23
	    new TestResult(2, 6.2473555282928855602E-09m), // 9 / 24
	    new TestResult(2, -5.261358920047113E-12m), // 9 / 25
	    new TestResult(2, 7.53226975501E-17m), // 9 / 26
	    new TestResult(2, 4.103153579868E-16m), // 9 / 27
	    new TestResult(2, -3.0871376E-21m), // 9 / 28
	    new TestResult(2, 0.0006411399074043056427231896m), // 9 / 29
	    new TestResult(3, 0m), // 10 / 0
	    new TestResult(0, -48466870444188873796420.028868m), // 10 / 1
	    new TestResult(0, 48466870444188873796420.028868m), // 10 / 2
	    new TestResult(0, -24233435222094436898210.014434m), // 10 / 3
	    new TestResult(0, -4846687044418887379642.0028868m), // 10 / 4
	    new TestResult(0, -484668704441888737964200.28868m), // 10 / 5
	    new TestResult(2, -6.11737908669317400216E-07m), // 10 / 6
	    new TestResult(2, 6.11737908669317400216E-07m), // 10 / 7
	    new TestResult(2, -0.001749497934470499347067655m), // 10 / 8
	    new TestResult(2, 15127673103138133.38384122385m), // 10 / 9
	    new TestResult(0, 1m), // 10 / 10
	    new TestResult(2, 88898.44296603758700885485801m), // 10 / 11
	    new TestResult(0, -63527832683254186820073.01815m), // 10 / 12
	    new TestResult(2, 121.03009147860926410013615872m), // 10 / 13
	    new TestResult(2, -0.217484928990092181791376891m), // 10 / 14
	    new TestResult(2, -0.0034021285176583848675539818m), // 10 / 15
	    new TestResult(2, 115089499751104803.45385549602m), // 10 / 16
	    new TestResult(2, -1981208.2898040400164777217752m), // 10 / 17
	    new TestResult(2, 0.0091047362233453526323903968m), // 10 / 18
	    new TestResult(2, -0.0004714627215051239953535366m), // 10 / 19
	    new TestResult(0, -6844326374165.3505116694016252m), // 10 / 20
	    new TestResult(2, -116576320761.65010852644993727m), // 10 / 21
	    new TestResult(0, 7585520927.2021546507200976307m), // 10 / 22
	    new TestResult(2, -109567712.75131768812510902617m), // 10 / 23
	    new TestResult(2, 94507952.1910976087542637813m), // 10 / 24
	    new TestResult(0, -79592.11782075260884714858998m), // 10 / 25
	    new TestResult(0, 1.1394571457845409462816707224m), // 10 / 26
	    new TestResult(0, 6.2071166048219727069770728387m), // 10 / 27
	    new TestResult(2, -4.67012085789282405269787E-05m), // 10 / 28
	    new TestResult(2, 9698954932588.587842740128447m), // 10 / 29
	    new TestResult(3, 0m), // 11 / 0
	    new TestResult(0, -545193693242804794.30331374676m), // 11 / 1
	    new TestResult(0, 545193693242804794.30331374676m), // 11 / 2
	    new TestResult(0, -272596846621402397.15165687338m), // 11 / 3
	    new TestResult(0, -54519369324280479.430331374676m), // 11 / 4
	    new TestResult(0, -5451936932428047943.0331374676m), // 11 / 5
	    new TestResult(2, -6.8813118459568899E-12m), // 11 / 6
	    new TestResult(2, 6.8813118459568899E-12m), // 11 / 7
	    new TestResult(2, -1.96797365184322821747E-08m), // 11 / 8
	    new TestResult(2, 170168032177.09280206999941461m), // 11 / 9
	    new TestResult(2, 1.12487909420644868170014E-05m), // 11 / 10
	    new TestResult(0, 1m), // 11 / 11
	    new TestResult(0, -714611308856377959.50595110446m), // 11 / 12
	    new TestResult(2, 0.0013614421967418161221277362m), // 11 / 13
	    new TestResult(2, -2.4464424992592870530962E-06m), // 11 / 14
	    new TestResult(2, -3.82698324531749191876E-08m), // 11 / 15
	    new TestResult(2, 1294617722326.9607230936474447m), // 11 / 16
	    new TestResult(0, -22.286197864690758108786295635m), // 11 / 17
	    new TestResult(2, 1.024172743590536270877E-07m), // 11 / 18
	    new TestResult(2, -5.3033855911879105358E-09m), // 11 / 19
	    new TestResult(0, -76990396.522244266468310204941m), // 11 / 20
	    new TestResult(2, -1311342.6610428539176292952583m), // 11 / 21
	    new TestResult(2, 85327.93909675220473779587097m), // 11 / 22
	    new TestResult(2, -1232.5042947397459817875590485m), // 11 / 23
	    new TestResult(2, 1063.1001965602823514109428061m), // 11 / 24
	    new TestResult(2, -0.8953150940018113683593315845m), // 11 / 25
	    new TestResult(2, 1.28175152203717976446025E-05m), // 11 / 26
	    new TestResult(2, 6.98225570406594773014162E-05m), // 11 / 27
	    new TestResult(2, -5.253321320461122964E-10m), // 11 / 28
	    new TestResult(2, 109101516.393194182269828744m), // 11 / 29
	    new TestResult(3, 0m), // 12 / 0
	    new TestResult(0, 0.7629234053338741809892531431m), // 12 / 1
	    new TestResult(0, -0.7629234053338741809892531431m), // 12 / 2
	    new TestResult(2, 0.3814617026669370904946265716m), // 12 / 3
	    new TestResult(2, 0.0762923405333874180989253143m), // 12 / 4
	    new TestResult(0, 7.629234053338741809892531431m), // 12 / 5
	    new TestResult(0, 0m), // 12 / 6
	    new TestResult(0, 0m), // 12 / 7
	    new TestResult(2, 2.75E-26m), // 12 / 8
	    new TestResult(2, -2.381266991833920788159E-07m), // 12 / 9
	    new TestResult(2, -1.57411E-23m), // 12 / 10
	    new TestResult(2, -1.3993621254E-18m), // 12 / 11
	    new TestResult(0, 1m), // 12 / 12
	    new TestResult(2, -1.9051506E-21m), // 12 / 13
	    new TestResult(2, 3.4235E-24m), // 12 / 14
	    new TestResult(2, 5.36E-26m), // 12 / 15
	    new TestResult(2, -1.8116390075029613252172E-06m), // 12 / 16
	    new TestResult(2, 3.11864612111E-17m), // 12 / 17
	    new TestResult(2, -1.433E-25m), // 12 / 18
	    new TestResult(2, 7.4E-27m), // 12 / 19
	    new TestResult(2, 1.077374449131727051E-10m), // 12 / 20
	    new TestResult(2, 1.8350432532917088E-12m), // 12 / 21
	    new TestResult(2, -1.194046862108942E-13m), // 12 / 22
	    new TestResult(2, 1.7247198294583E-15m), // 12 / 23
	    new TestResult(2, -1.4876621505775E-15m), // 12 / 24
	    new TestResult(2, 1.2528700328E-18m), // 12 / 25
	    new TestResult(2, -1.79363E-23m), // 12 / 26
	    new TestResult(2, -9.7707E-23m), // 12 / 27
	    new TestResult(2, 7E-28m), // 12 / 28
	    new TestResult(2, -1.526725298649329415E-10m), // 12 / 29
	    new TestResult(3, 0m), // 13 / 0
	    new TestResult(0, -400453059665371395972.33474452m), // 13 / 1
	    new TestResult(0, 400453059665371395972.33474452m), // 13 / 2
	    new TestResult(0, -200226529832685697986.16737226m), // 13 / 3
	    new TestResult(0, -40045305966537139597.233474452m), // 13 / 4
	    new TestResult(0, -4004530596653713959723.3474452m), // 13 / 5
	    new TestResult(2, -5.0544282103383796846E-09m), // 13 / 6
	    new TestResult(2, 5.0544282103383796846E-09m), // 13 / 7
	    new TestResult(2, -1.44550657865090008770424E-05m), // 13 / 8
	    new TestResult(0, 124991007759518.90460797449561m), // 13 / 9
	    new TestResult(2, 0.0082624080324415766315344098m), // 13 / 10
	    new TestResult(0, 734.51520923413833710945795487m), // 13 / 11
	    new TestResult(0, -524892875045723911444.75281192m), // 13 / 12
	    new TestResult(0, 1m), // 13 / 13
	    new TestResult(2, -0.0017969492242227235536064218m), // 13 / 14
	    new TestResult(2, -2.81097739916991934127E-05m), // 13 / 15
	    new TestResult(0, 950916407193211.1625559901416m), // 13 / 16
	    new TestResult(2, -16369.551287616739221174657438m), // 13 / 17
	    new TestResult(2, 7.52270457050304262522489E-05m), // 13 / 18
	    new TestResult(2, -3.8954173771607025488213E-06m), // 13 / 19
	    new TestResult(0, -56550617210.555523949138629466m), // 13 / 20
	    new TestResult(2, -963201129.053543593301215979m), // 13 / 21
	    new TestResult(0, 62674669.039168758653283589393m), // 13 / 22
	    new TestResult(2, -905293.1499327386264702284996m), // 13 / 23
	    new TestResult(0, 780863.26331332938464898250347m), // 13 / 24
	    new TestResult(0, -657.62255360122271090761257385m), // 13 / 25
	    new TestResult(2, 0.0094146598739531437042039072m), // 13 / 26
	    new TestResult(2, 0.0512857300939825548661107378m), // 13 / 27
	    new TestResult(2, -3.858644408872661628546E-07m), // 13 / 28
	    new TestResult(2, 80136723141.3087985919301071m), // 13 / 29
	    new TestResult(3, 0m), // 14 / 0
	    new TestResult(0, 222851627785191714190050.61676m), // 14 / 1
	    new TestResult(0, -222851627785191714190050.61676m), // 14 / 2
	    new TestResult(0, 111425813892595857095025.30838m), // 14 / 3
	    new TestResult(0, 22285162778519171419005.061676m), // 14 / 4
	    new TestResult(0, 2228516277851917141900506.1676m), // 14 / 5
	    new TestResult(2, 2.8127829891936371496034E-06m), // 14 / 6
	    new TestResult(2, -2.8127829891936371496034E-06m), // 14 / 7
	    new TestResult(2, 0.0080442260647412496180191824m), // 14 / 8
	    new TestResult(0, -69557339781586865.090190294226m), // 14 / 9
	    new TestResult(0, -4.5980197554082303507855592074m), // 14 / 10
	    new TestResult(0, -408756.79698287266169556154972m), // 14 / 11
	    new TestResult(0, 292102229695871398106087.53771m), // 14 / 12
	    new TestResult(0, -556.49875161751071295835933612m), // 14 / 13
	    new TestResult(0, 1m), // 14 / 14
	    new TestResult(2, 0.0156430541346309720805875037m), // 14 / 15
	    new TestResult(0, -529183793495630496.21134784463m), // 14 / 16
	    new TestResult(0, 9109634.856097530429701051341m), // 14 / 17
	    new TestResult(2, -0.0418637570227228532534385582m), // 14 / 18
	    new TestResult(2, 0.0021677949074190888565424407m), // 14 / 19
	    new TestResult(0, 31470347880873.8650457716423m), // 14 / 20
	    new TestResult(0, 536020225874.87383772032904409m), // 14 / 21
	    new TestResult(0, -34878375078.338063831579069802m), // 14 / 22
	    new TestResult(0, 503794507.78545299809142202221m), // 14 / 23
	    new TestResult(0, -434549431.21784335466036244264m), // 14 / 24
	    new TestResult(0, 365966.13011459996262470838791m), // 14 / 25
	    new TestResult(0, -5.2392464667583952349382510034m), // 14 / 26
	    new TestResult(0, -28.540444773093892153704750451m), // 14 / 27
	    new TestResult(2, 0.0002147330796473523774307252m), // 14 / 28
	    new TestResult(0, -44595986386856.427962855912902m), // 14 / 29
	    new TestResult(3, 0m), // 15 / 0
	    new TestResult(0, 14246043379204153213661335.584m), // 15 / 1
	    new TestResult(0, -14246043379204153213661335.584m), // 15 / 2
	    new TestResult(0, 7123021689602076606830667.792m), // 15 / 3
	    new TestResult(0, 1424604337920415321366133.5584m), // 15 / 4
	    new TestResult(0, 142460433792041532136613355.84m), // 15 / 5
	    new TestResult(2, 0.000179810346814988644017588m), // 15 / 6
	    new TestResult(2, -0.000179810346814988644017588m), // 15 / 7
	    new TestResult(2, 0.5142362863101488055319975668m), // 15 / 8
	    new TestResult(0, -4446531935704239606.853757764m), // 15 / 9
	    new TestResult(0, -293.9336344319759696558350238m), // 15 / 10
	    new TestResult(0, -26130242.436351157631251931791m), // 15 / 11
	    new TestResult(0, 18672966748175371201221341.455m), // 15 / 12
	    new TestResult(0, -35574.814663942145387025413585m), // 15 / 13
	    new TestResult(0, 63.926135612238008074953534656m), // 15 / 14
	    new TestResult(0, 1m), // 15 / 15
	    new TestResult(0, -33828674946800228646.235978357m), // 15 / 16
	    new TestResult(0, 582343753.18886100200299543357m), // 15 / 17
	    new TestResult(0, -2.676188208672362394790189458m), // 15 / 18
	    new TestResult(2, 0.1385787512311916118536189686m), // 15 / 19
	    new TestResult(0, 2011777726397049.7148998294303m), // 15 / 20
	    new TestResult(0, 34265701650179.633435597272951m), // 15 / 21
	    new TestResult(0, -2229639735192.3415269889385254m), // 15 / 22
	    new TestResult(0, 32205636025.393565317814400562m), // 15 / 23
	    new TestResult(0, -27779065870.252746877787654023m), // 15 / 24
	    new TestResult(0, 23394800.463191857206004010592m), // 15 / 25
	    new TestResult(0, -334.92478013993600679391750784m), // 15 / 26
	    new TestResult(0, -1824.480342998389574945394373m), // 15 / 27
	    new TestResult(2, 0.0137270559699701534233028604m), // 15 / 28
	    new TestResult(0, -2850849073527704.1131980257228m), // 15 / 29
	    new TestResult(3, 0m), // 16 / 0
	    new TestResult(0, -421123.30446308691436596648186m), // 16 / 1
	    new TestResult(0, 421123.30446308691436596648186m), // 16 / 2
	    new TestResult(0, -210561.65223154345718298324093m), // 16 / 3
	    new TestResult(0, -42112.330446308691436596648186m), // 16 / 4
	    new TestResult(0, -4211233.0446308691436596648186m), // 16 / 5
	    new TestResult(2, -5.3153E-24m), // 16 / 6
	    new TestResult(2, 5.3153E-24m), // 16 / 7
	    new TestResult(2, -1.52011951E-20m), // 16 / 8
	    new TestResult(2, 0.1314426870900784765046305445m), // 16 / 9
	    new TestResult(2, 8.6888899697E-18m), // 16 / 10
	    new TestResult(2, 7.724287894055618E-13m), // 16 / 11
	    new TestResult(0, -551986.34819545603591091806686m), // 16 / 12
	    new TestResult(2, 1.0516171478749E-15m), // 16 / 13
	    new TestResult(2, -1.8897026181E-18m), // 16 / 14
	    new TestResult(2, -2.95607204E-20m), // 16 / 15
	    new TestResult(0, 1m), // 16 / 16
	    new TestResult(2, -1.72145008370758982E-11m), // 16 / 17
	    new TestResult(2, 7.91100512E-20m), // 16 / 18
	    new TestResult(2, -4.0964877E-21m), // 16 / 19
	    new TestResult(2, -5.94695987815313124908788E-05m), // 16 / 20
	    new TestResult(2, -1.012918824165199591208E-06m), // 16 / 21
	    new TestResult(2, 6.59097566989758101184E-08m), // 16 / 22
	    new TestResult(2, -9.520218003229776933E-10m), // 16 / 23
	    new TestResult(2, 8.211691978458736692E-10m), // 16 / 24
	    new TestResult(2, -6.915671541963459E-13m), // 16 / 25
	    new TestResult(2, 9.9006177649E-18m), // 16 / 26
	    new TestResult(2, 5.39329532081E-17m), // 16 / 27
	    new TestResult(2, -4.057817E-22m), // 16 / 28
	    new TestResult(2, 8.42731522299060351340978E-05m), // 16 / 29
	    new TestResult(3, 0m), // 17 / 0
	    new TestResult(0, 24463288738299545.200508898642m), // 17 / 1
	    new TestResult(0, -24463288738299545.200508898642m), // 17 / 2
	    new TestResult(0, 12231644369149772.600254449321m), // 17 / 3
	    new TestResult(0, 2446328873829954.5200508898642m), // 17 / 4
	    new TestResult(0, 244632887382995452.00508898642m), // 17 / 5
	    new TestResult(2, 3.087701135804474E-13m), // 17 / 6
	    new TestResult(2, -3.087701135804474E-13m), // 17 / 7
	    new TestResult(2, 8.830459389222225669E-10m), // 17 / 8
	    new TestResult(0, -7635579348.7187565660747357791m), // 17 / 9
	    new TestResult(2, -5.047424872722036356353E-07m), // 17 / 10
	    new TestResult(2, -0.0448708212173039475800037318m), // 17 / 11
	    new TestResult(0, 32065196279558108.529140993113m), // 17 / 12
	    new TestResult(2, -6.10890294076955781848965E-05m), // 17 / 13
	    new TestResult(2, 1.097738840026777145636E-07m), // 17 / 14
	    new TestResult(2, 1.7171988100225883459E-09m), // 17 / 15
	    new TestResult(0, -58090560363.286299748647320613m), // 17 / 16
	    new TestResult(0, 1m), // 17 / 17
	    new TestResult(2, -4.5955472073286630491E-09m), // 17 / 18
	    new TestResult(2, 2.379672667086185355E-10m), // 17 / 19
	    new TestResult(0, 3454622.3177989620890093190782m), // 17 / 20
	    new TestResult(0, 58841.022098277508341771781449m), // 17 / 21
	    new TestResult(0, -3828.7347000513678647353825371m), // 17 / 22
	    new TestResult(0, 55.303479858826432190514052161m), // 17 / 23
	    new TestResult(0, -47.702178855937114485246771806m), // 17 / 24
	    new TestResult(2, 0.0401735235161089558399723624m), // 17 / 25
	    new TestResult(2, -5.751324339033751411128E-07m), // 17 / 26
	    new TestResult(2, -3.1329954739064384030107E-06m), // 17 / 27
	    new TestResult(2, 2.35720841767462147E-11m), // 17 / 28
	    new TestResult(0, -4895474.6366157719700764456737m), // 17 / 29
	    new TestResult(3, 0m), // 18 / 0
	    new TestResult(0, -5323259153836385912697776.001m), // 18 / 1
	    new TestResult(0, 5323259153836385912697776.001m), // 18 / 2
	    new TestResult(0, -2661629576918192956348888.0005m), // 18 / 3
	    new TestResult(0, -532325915383638591269777.6001m), // 18 / 4
	    new TestResult(0, -53232591538363859126977760.01m), // 18 / 5
	    new TestResult(2, -6.71889765571425396685425E-05m), // 18 / 6
	    new TestResult(2, 6.71889765571425396685425E-05m), // 18 / 7
	    new TestResult(2, -0.1921525117866271842350897941m), // 18 / 8
	    new TestResult(2, 1661516899781174928.9093780261m), // 18 / 9
	    new TestResult(2, 109.83294578440517035615430815m), // 18 / 10
	    new TestResult(0, 9763977.866606841461047851247m), // 18 / 11
	    new TestResult(0, -6977449002900619940294072.5616m), // 18 / 12
	    new TestResult(2, 13293.091475651689504900168879m), // 18 / 13
	    new TestResult(2, -23.887010414694002921991680138m), // 18 / 14
	    new TestResult(2, -0.3736657970315521134142448751m), // 18 / 15
	    new TestResult(2, 12640618786517406225.453062932m), // 18 / 16
	    new TestResult(2, -217601942.68166121398235985303m), // 18 / 17
	    new TestResult(0, 1m), // 18 / 18
	    new TestResult(2, -0.0517821395304403973786481636m), // 18 / 19
	    new TestResult(0, -751732527584477.35915419894761m), // 18 / 20
	    new TestResult(2, -12803920717959.743225565448731m), // 18 / 21
	    new TestResult(2, 833140108743.8650907762143944m), // 18 / 22
	    new TestResult(2, -12034144654.336754703603784598m), // 18 / 23
	    new TestResult(2, 10380086789.199979490407779016m), // 18 / 24
	    new TestResult(2, -8741836.761472709884309939817m), // 18 / 25
	    new TestResult(2, 125.14993491660654415586289072m), // 18 / 26
	    new TestResult(0, 681.74590153489282094280759828m), // 18 / 27
	    new TestResult(2, -0.0051293313099156230719389272m), // 18 / 28
	    new TestResult(2, 1065264791276391.4723331595476m), // 18 / 29
	    new TestResult(3, 0m), // 19 / 0
	    new TestResult(0, 102801066199805834724673169.19m), // 19 / 1
	    new TestResult(0, -102801066199805834724673169.19m), // 19 / 2
	    new TestResult(0, 51400533099902917362336584.595m), // 19 / 3
	    new TestResult(0, 10280106619980583472467316.919m), // 19 / 4
	    new TestResult(0, 1028010661998058347246731691.9m), // 19 / 5
	    new TestResult(2, 0.0012975318742410238535080931m), // 19 / 6
	    new TestResult(2, -0.0012975318742410238535080931m), // 19 / 7
	    new TestResult(0, 3.7107874168403053465719309061m), // 19 / 8
	    new TestResult(0, -32086679207305516282.844795417m), // 19 / 9
	    new TestResult(0, -2121.0584726774239906503858037m), // 19 / 10
	    new TestResult(0, -188558795.66094477019222763757m), // 19 / 11
	    new TestResult(0, 134746247763650063417846672.74m), // 19 / 12
	    new TestResult(0, -256711.90097962787395549999647m), // 19 / 13
	    new TestResult(0, 461.29825131408293475234811789m), // 19 / 14
	    new TestResult(0, 7.2161135175168023022995955438m), // 19 / 15
	    new TestResult(0, -244111558563287122929.04506056m), // 19 / 16
	    new TestResult(0, 4202258629.2276083227657702227m), // 19 / 17
	    new TestResult(0, -19.311677908019711128996883838m), // 19 / 18
	    new TestResult(0, 1m), // 19 / 19
	    new TestResult(0, 14517216445692969.517326696553m), // 19 / 20
	    new TestResult(0, 247265192865059.0518154425151m), // 19 / 21
	    new TestResult(0, -16089333432314.039236231362245m), // 19 / 22
	    new TestResult(0, 232399525463.06860877964908684m), // 19 / 23
	    new TestResult(0, -200456892730.3205001486617745m), // 19 / 24
	    new TestResult(0, 168819535.86204710848979659043m), // 19 / 25
	    new TestResult(0, -2416.8552333191352676140967438m), // 19 / 26
	    new TestResult(0, -13165.657265554370962757268367m), // 19 / 27
	    new TestResult(2, 0.0990559941406113443335355768m), // 19 / 28
	    new TestResult(0, -20572050535883517.889932557941m), // 19 / 29
	    new TestResult(3, 0m), // 20 / 0
	    new TestResult(0, 7081320760.3793287174700927968m), // 20 / 1
	    new TestResult(0, -7081320760.3793287174700927968m), // 20 / 2
	    new TestResult(0, 3540660380.1896643587350463984m), // 20 / 3
	    new TestResult(0, 708132076.03793287174700927968m), // 20 / 4
	    new TestResult(0, 70813207603.793287174700927968m), // 20 / 5
	    new TestResult(2, 8.93788337E-20m), // 20 / 6
	    new TestResult(2, -8.93788337E-20m), // 20 / 7
	    new TestResult(2, 2.556128739088E-16m), // 20 / 8
	    new TestResult(0, -2210.250107335496183875574735m), // 20 / 9
	    new TestResult(2, -1.461064165166945E-13m), // 20 / 10
	    new TestResult(2, -1.2988632935681496171E-08m), // 20 / 11
	    new TestResult(2, 9281823982.422412820396304075m), // 20 / 12
	    new TestResult(2, -1.76832729566273202E-11m), // 20 / 13
	    new TestResult(2, 3.17759436211301E-14m), // 20 / 14
	    new TestResult(2, 4.970728062443E-16m), // 20 / 15
	    new TestResult(2, -16815.314387332923907118773285m), // 20 / 16
	    new TestResult(2, 2.894672435964370128732E-07m), // 20 / 17
	    new TestResult(2, -1.3302603829227E-15m), // 20 / 18
	    new TestResult(2, 6.88837287603E-17m), // 20 / 19
	    new TestResult(0, 1m), // 20 / 20
	    new TestResult(2, 0.0170325484771854288433021822m), // 20 / 21
	    new TestResult(2, -0.0011082932800859005017661851m), // 20 / 22
	    new TestResult(2, 1.60085458760255588702037E-05m), // 20 / 23
	    new TestResult(2, -1.38082182269723557757959E-05m), // 20 / 24
	    new TestResult(2, 1.16289191177647018971E-08m), // 20 / 25
	    new TestResult(2, -1.6648200034492E-13m), // 20 / 26
	    new TestResult(2, -9.068995640318096E-13m), // 20 / 27
	    new TestResult(2, 6.8233462325E-18m), // 20 / 28
	    new TestResult(2, -1.4170795491574366313541174399m), // 20 / 29
	    new TestResult(3, 0m), // 21 / 0
	    new TestResult(0, 415752273939.77704245656837041m), // 21 / 1
	    new TestResult(0, -415752273939.77704245656837041m), // 21 / 2
	    new TestResult(2, 207876136969.8885212282841852m), // 21 / 3
	    new TestResult(0, 41575227393.977704245656837041m), // 21 / 4
	    new TestResult(0, 4157522739397.7704245656837041m), // 21 / 5
	    new TestResult(2, 5.2475314427E-18m), // 21 / 6
	    new TestResult(2, -5.2475314427E-18m), // 21 / 7
	    new TestResult(2, 1.50073181503771E-14m), // 21 / 8
	    new TestResult(2, -129766.25959973387113808743405m), // 21 / 9
	    new TestResult(2, -8.5780713738991847E-12m), // 21 / 10
	    new TestResult(2, -7.625771887911763590261E-07m), // 21 / 11
	    new TestResult(0, 544946282986.07977368508338172m), // 21 / 12
	    new TestResult(2, -1.0382047630930577807E-09m), // 21 / 13
	    new TestResult(2, 1.8656012436244067E-12m), // 21 / 14
	    new TestResult(2, 2.91837012476515E-14m), // 21 / 15
	    new TestResult(2, -987245.9432513294606844938685m), // 21 / 16
	    new TestResult(2, 1.69949461164997956913075E-05m), // 21 / 17
	    new TestResult(2, -7.81010771643817E-14m), // 21 / 18
	    new TestResult(2, 4.0442408752037E-15m), // 21 / 19
	    new TestResult(0, 58.711120143850993772970798576m), // 21 / 20
	    new TestResult(0, 1m), // 21 / 21
	    new TestResult(2, -0.0650691399217460043995169619m), // 21 / 22
	    new TestResult(2, 0.0009398796602556869430220663m), // 21 / 23
	    new TestResult(2, -0.0008106959592962871308194687m), // 21 / 24
	    new TestResult(2, 6.827468674662091164433E-07m), // 21 / 25
	    new TestResult(2, -9.7743447240392408E-12m), // 21 / 26
	    new TestResult(2, -5.32450892622776623E-11m), // 21 / 27
	    new TestResult(2, 4.006063004374E-16m), // 21 / 28
	    new TestResult(2, -83.19832766397646235747803502m), // 21 / 29
	    new TestResult(3, 0m), // 22 / 0
	    new TestResult(0, -6389392489892.6362673670820462m), // 22 / 1
	    new TestResult(0, 6389392489892.6362673670820462m), // 22 / 2
	    new TestResult(0, -3194696244946.3181336835410231m), // 22 / 3
	    new TestResult(0, -638939248989.26362673670820462m), // 22 / 4
	    new TestResult(0, -63893924898926.362673670820462m), // 22 / 5
	    new TestResult(2, -8.06454710942E-17m), // 22 / 6
	    new TestResult(2, 8.06454710942E-17m), // 22 / 7
	    new TestResult(2, -2.306364917136659E-13m), // 22 / 8
	    new TestResult(2, 1994282.6930829954142428134453m), // 22 / 9
	    new TestResult(2, 1.31830102322167113E-10m), // 22 / 10
	    new TestResult(2, 1.17194908324940723524544E-05m), // 22 / 11
	    new TestResult(2, -8374880682938.911591614017805m), // 22 / 12
	    new TestResult(2, 1.59554093436863052632E-08m), // 22 / 13
	    new TestResult(2, -2.8671060442293101E-11m), // 22 / 14
	    new TestResult(2, -4.485029505960676E-13m), // 22 / 15
	    new TestResult(2, 15172260.52839517272431927284m), // 22 / 16
	    new TestResult(2, -0.0002611828915663923103773216m), // 22 / 17
	    new TestResult(2, 1.2002783079399592E-12m), // 22 / 18
	    new TestResult(2, -6.21529788171079E-14m), // 22 / 19
	    new TestResult(2, -902.2882462325251912660653119m), // 22 / 20
	    new TestResult(2, -15.368268294350108200187006165m), // 22 / 21
	    new TestResult(0, 1m), // 22 / 22
	    new TestResult(2, -0.0144443227832120251558802997m), // 22 / 23
	    new TestResult(2, 0.0124589930076108753677011728m), // 22 / 24
	    new TestResult(2, -1.04926370363477969575619E-05m), // 22 / 25
	    new TestResult(2, 1.50214752120500522E-10m), // 22 / 26
	    new TestResult(2, 8.182848171393031905E-10m), // 22 / 27
	    new TestResult(2, -6.156625105529E-15m), // 22 / 28
	    new TestResult(2, 1278.6142211812409691677177837m), // 22 / 29
	    new TestResult(3, 0m), // 23 / 0
	    new TestResult(0, 442346282742915.0596416330681m), // 23 / 1
	    new TestResult(0, -442346282742915.0596416330681m), // 23 / 2
	    new TestResult(0, 221173141371457.52982081653405m), // 23 / 3
	    new TestResult(0, 44234628274291.50596416330681m), // 23 / 4
	    new TestResult(0, 4423462827429150.596416330681m), // 23 / 5
	    new TestResult(2, 5.5831950244116E-15m), // 23 / 6
	    new TestResult(2, -5.5831950244116E-15m), // 23 / 7
	    new TestResult(2, 1.59672762216117307E-11m), // 23 / 8
	    new TestResult(0, -138066887.80181919317571960944m), // 23 / 9
	    new TestResult(2, -9.1267762636395250963E-09m), // 23 / 10
	    new TestResult(2, -0.0008113561991369439495762269m), // 23 / 11
	    new TestResult(0, 579804315413987.55317875373327m), // 23 / 12
	    new TestResult(2, -1.104614566093091384748E-06m), // 23 / 13
	    new TestResult(2, 1.9849362876061009571E-09m), // 23 / 14
	    new TestResult(2, 3.10504658008156699E-11m), // 23 / 15
	    new TestResult(2, -1050396114.5225303515983863327m), // 23 / 16
	    new TestResult(2, 0.018082044792709369766201304m), // 23 / 17
	    new TestResult(2, -8.30968904499273382E-11m), // 23 / 18
	    new TestResult(2, 4.3029347758238576E-12m), // 23 / 19
	    new TestResult(0, 62466.635492334295970467255163m), // 23 / 20
	    new TestResult(2, 1063.9659972298557739709718754m), // 23 / 21
	    new TestResult(0, -69.231352345729507070036842909m), // 23 / 22
	    new TestResult(0, 1m), // 23 / 23
	    new TestResult(2, -0.8625529347828887027197930493m), // 23 / 24
	    new TestResult(2, 0.000726419451699245356034835m), // 23 / 25
	    new TestResult(2, -1.03995704315807902618E-08m), // 23 / 26
	    new TestResult(2, -5.66509644945319386873E-08m), // 23 / 27
	    new TestResult(2, 4.262314819414408E-13m), // 23 / 28
	    new TestResult(0, -88520.19166085901375500091819m), // 23 / 29
	    new TestResult(3, 0m), // 24 / 0
	    new TestResult(0, -512833780867323.89020837443764m), // 24 / 1
	    new TestResult(0, 512833780867323.89020837443764m), // 24 / 2
	    new TestResult(0, -256416890433661.94510418721882m), // 24 / 3
	    new TestResult(0, -51283378086732.389020837443764m), // 24 / 4
	    new TestResult(0, -5128337808673238.9020837443764m), // 24 / 5
	    new TestResult(2, -6.4728723296466E-15m), // 24 / 6
	    new TestResult(2, 6.4728723296466E-15m), // 24 / 7
	    new TestResult(2, -1.85116478974485417E-11m), // 24 / 8
	    new TestResult(0, 160067727.13209967231293920454m), // 24 / 9
	    new TestResult(2, 1.05811201789450821647E-08m), // 24 / 10
	    new TestResult(2, 0.0009406451087447388135269385m), // 24 / 11
	    new TestResult(0, -672195632329427.78049892650167m), // 24 / 12
	    new TestResult(2, 1.280633943203881720602E-06m), // 24 / 13
	    new TestResult(2, -2.3012341707535026743E-09m), // 24 / 14
	    new TestResult(2, -3.59983307095596564E-11m), // 24 / 15
	    new TestResult(2, 1217775828.2011100471829103026m), // 24 / 16
	    new TestResult(2, -0.0209634030139388041036175456m), // 24 / 17
	    new TestResult(2, 9.63383081768117492E-11m), // 24 / 18
	    new TestResult(2, -4.988603716138233E-12m), // 24 / 19
	    new TestResult(0, -72420.639908967018993409156115m), // 24 / 20
	    new TestResult(2, -1233.5080599982704934889448056m), // 24 / 21
	    new TestResult(2, 80.26330855062892819843621147m), // 24 / 22
	    new TestResult(0, -1.1593491363538259770624590073m), // 24 / 23
	    new TestResult(0, 1m), // 24 / 24
	    new TestResult(2, -0.0008421737639581399076256596m), // 24 / 25
	    new TestResult(2, 1.20567329983039744731E-08m), // 24 / 26
	    new TestResult(2, 6.56782467603468627053E-08m), // 24 / 27
	    new TestResult(2, -4.941511004756208E-13m), // 24 / 28
	    new TestResult(2, 102625.80775189204591883200668m), // 24 / 29
	    new TestResult(3, 0m), // 25 / 0
	    new TestResult(0, 608940580690915704.1450897514m), // 25 / 1
	    new TestResult(0, -608940580690915704.1450897514m), // 25 / 2
	    new TestResult(0, 304470290345457852.0725448757m), // 25 / 3
	    new TestResult(0, 60894058069091570.41450897514m), // 25 / 4
	    new TestResult(0, 6089405806909157041.450897514m), // 25 / 5
	    new TestResult(2, 7.6859106833543095E-12m), // 25 / 6
	    new TestResult(2, -7.6859106833543095E-12m), // 25 / 7
	    new TestResult(2, 2.19807938571316735336E-08m), // 25 / 8
	    new TestResult(2, -190064965191.73899284873850192m), // 25 / 9
	    new TestResult(2, -1.2564058192949139177057E-05m), // 25 / 10
	    new TestResult(2, -1.1169252106878663178369471734m), // 25 / 11
	    new TestResult(2, 798167386704341861.9682340376m), // 25 / 12
	    new TestResult(2, -0.0015206291124352045188095451m), // 25 / 13
	    new TestResult(2, 2.7324933039209294299951E-06m), // 25 / 14
	    new TestResult(2, 4.27445406757517405142E-08m), // 25 / 15
	    new TestResult(0, -1445991172270.2862199666258075m), // 25 / 16
	    new TestResult(2, 24.892016245451201449485802529m), // 25 / 17
	    new TestResult(2, -1.143924357415629812325E-07m), // 25 / 18
	    new TestResult(2, 5.9234850687965514433E-09m), // 25 / 19
	    new TestResult(2, 85992514.8565500475609760009m), // 25 / 20
	    new TestResult(2, 1464671.6779692768786214751455m), // 25 / 21
	    new TestResult(2, -95304.9263532013818698238713m), // 25 / 22
	    new TestResult(2, 1376.6151190758908669955780933m), // 25 / 23
	    new TestResult(2, -1187.4034110254054607086172841m), // 25 / 24
	    new TestResult(0, 1m), // 25 / 25
	    new TestResult(2, -1.43162058880087033607313E-05m), // 25 / 26
	    new TestResult(2, -7.79865742334041504368931E-05m), // 25 / 27
	    new TestResult(2, 5.867567022667099857E-10m), // 25 / 28
	    new TestResult(2, -121858234.18383411295432055833m), // 25 / 29
	    new TestResult(3, 0m), // 26 / 0
	    new TestResult(0, -42535053313319986966115.037787m), // 26 / 1
	    new TestResult(0, 42535053313319986966115.037787m), // 26 / 2
	    new TestResult(2, -21267526656659993483057.518894m), // 26 / 3
	    new TestResult(0, -4253505331331998696611.5037787m), // 26 / 4
	    new TestResult(0, -425350533133199869661150.37787m), // 26 / 5
	    new TestResult(2, -5.368678505659136383798E-07m), // 26 / 6
	    new TestResult(2, 5.368678505659136383798E-07m), // 26 / 7
	    new TestResult(2, -0.0015353784395866262203076433m), // 26 / 8
	    new TestResult(2, 13276210657946598.343741130625m), // 26 / 9
	    new TestResult(2, 0.8776108901502200170717671109m), // 26 / 10
	    new TestResult(0, 78018.241664392812128305230978m), // 26 / 11
	    new TestResult(0, -55752717790464947101601.359896m), // 26 / 12
	    new TestResult(2, 106.21732631750483463727976216m), // 26 / 13
	    new TestResult(2, -0.1908671421252521905897157731m), // 26 / 14
	    new TestResult(2, -0.0029857450367876236635795704m), // 26 / 15
	    new TestResult(2, 101003798323510611.65980529603m), // 26 / 16
	    new TestResult(0, -1738729.9707879186275737174853m), // 26 / 17
	    new TestResult(2, 0.0079904156615530673319808542m), // 26 / 18
	    new TestResult(2, -0.0004137608186927571471957746m), // 26 / 19
	    new TestResult(0, -6006655361709.8810944800037905m), // 26 / 20
	    new TestResult(2, -102308648634.06932650747319597m), // 26 / 21
	    new TestResult(0, 6657135773.1750052361186583781m), // 26 / 22
	    new TestResult(2, -96157817.91940752862823678187m), // 26 / 23
	    new TestResult(2, 82941208.04870360868726566113m), // 26 / 24
	    new TestResult(0, -69850.909369611886817979504214m), // 26 / 25
	    new TestResult(0, 1m), // 26 / 26
	    new TestResult(0, 5.4474331288240229212686639513m), // 26 / 27
	    new TestResult(2, -4.09854892320443047650146E-05m), // 26 / 28
	    new TestResult(0, 8511908471915.937756062101659m), // 26 / 29
	    new TestResult(3, 0m), // 27 / 0
	    new TestResult(0, -7808274522591953107485.8812311m), // 27 / 1
	    new TestResult(0, 7808274522591953107485.8812311m), // 27 / 2
	    new TestResult(2, -3904137261295976553742.9406156m), // 27 / 3
	    new TestResult(0, -780827452259195310748.58812311m), // 27 / 4
	    new TestResult(0, -78082745225919531074858.812311m), // 27 / 5
	    new TestResult(2, -9.85542801296968307756E-08m), // 27 / 6
	    new TestResult(2, 9.85542801296968307756E-08m), // 27 / 7
	    new TestResult(2, -0.000281853563555002197999826m), // 27 / 8
	    new TestResult(2, 2437149817901964.8960845694514m), // 27 / 9
	    new TestResult(2, 0.1611053994415304138070003376m), // 27 / 10
	    new TestResult(2, 14322.019163773595213651372685m), // 27 / 11
	    new TestResult(2, -10234676860090376638967.506512m), // 27 / 12
	    new TestResult(2, 19.498601232106311831598265533m), // 27 / 13
	    new TestResult(2, -0.0350379963574616786880617938m), // 27 / 14
	    new TestResult(2, -0.0005481012737887758520091497m), // 27 / 15
	    new TestResult(2, 18541539828927654.501195934005m), // 27 / 16
	    new TestResult(2, -319183.3529057512147016471775m), // 27 / 17
	    new TestResult(2, 0.0014668221660718241029342142m), // 27 / 18
	    new TestResult(2, -7.59551900698740123366376E-05m), // 27 / 19
	    new TestResult(2, -1102657934418.1103423727465413m), // 27 / 20
	    new TestResult(2, -18781074721.729615778535996388m), // 27 / 21
	    new TestResult(2, 1222068378.9489912725931951927m), // 27 / 22
	    new TestResult(2, -17651950.128695901472129097893m), // 27 / 23
	    new TestResult(0, 15225741.38814783974528909088m), // 27 / 24
	    new TestResult(2, -12822.719933909700245783228909m), // 27 / 25
	    new TestResult(2, 0.1835726986181246222761380025m), // 27 / 26
	    new TestResult(0, 1m), // 27 / 27
	    new TestResult(2, -7.5238168625104611316081E-06m), // 27 / 28
	    new TestResult(2, 1562554008580.0861321324839011m), // 27 / 29
	    new TestResult(3, 0m), // 28 / 0
	    new TestResult(0, 1037807626804273037330059471.7m), // 28 / 1
	    new TestResult(0, -1037807626804273037330059471.7m), // 28 / 2
	    new TestResult(0, 518903813402136518665029735.85m), // 28 / 3
	    new TestResult(0, 103780762680427303733005947.17m), // 28 / 4
	    new TestResult(0, 10378076268042730373300594717m), // 28 / 5
	    new TestResult(2, 0.013098973822817421173845813m), // 28 / 6
	    new TestResult(2, -0.013098973822817421173845813m), // 28 / 7
	    new TestResult(0, 37.461513046578399246836695461m), // 28 / 8
	    new TestResult(0, -323924659841968113506.41166762m), // 28 / 9
	    new TestResult(0, -21412.722077843692663812014719m), // 28 / 10
	    new TestResult(0, -1903557652.3848013647110715272m), // 28 / 11
	    new TestResult(0, 1360303825454277040707598638.1m), // 28 / 12
	    new TestResult(0, -2591583.7118874583629347087379m), // 28 / 13
	    new TestResult(0, 4656.9443405844146143879311m), // 28 / 14
	    new TestResult(0, 72.848832421725332869329889807m), // 28 / 15
	    new TestResult(0, -2464379472248467996678.919247m), // 28 / 16
	    new TestResult(0, 42423062487.893912564215173398m), // 28 / 17
	    new TestResult(0, -194.95718634257023426451589779m), // 28 / 18
	    new TestResult(0, 10.095300225653040883323838635m), // 28 / 19
	    new TestResult(0, 146555658460058271.30028503504m), // 28 / 20
	    new TestResult(0, 2496216357326773.3203945609668m), // 28 / 21
	    new TestResult(0, -162426651429846.93508891542812m), // 28 / 22
	    new TestResult(0, 2346142981848.9761467960716552m), // 28 / 23
	    new TestResult(0, -2023672514414.1119557698582125m), // 28 / 24
	    new TestResult(0, 1704283898.4827658021193146276m), // 28 / 25
	    new TestResult(0, -24398.879182297399040382596287m), // 28 / 26
	    new TestResult(0, -132911.26276382163838029345835m), // 28 / 27
	    new TestResult(0, 1m), // 28 / 28
	    new TestResult(0, -207681026417050638.7817636979m), // 28 / 29
	    new TestResult(3, 0m), // 29 / 0
	    new TestResult(0, -4997122966.448652425771563042m), // 29 / 1
	    new TestResult(0, 4997122966.448652425771563042m), // 29 / 2
	    new TestResult(0, -2498561483.224326212885781521m), // 29 / 3
	    new TestResult(0, -499712296.6448652425771563042m), // 29 / 4
	    new TestResult(0, -49971229664.48652425771563042m), // 29 / 5
	    new TestResult(2, -6.30725591E-20m), // 29 / 6
	    new TestResult(2, 6.30725591E-20m), // 29 / 7
	    new TestResult(2, -1.803800457503E-16m), // 29 / 8
	    new TestResult(0, 1559.7219709011119254589559305m), // 29 / 9
	    new TestResult(2, 1.031038918059089E-13m), // 29 / 10
	    new TestResult(2, 9.1657754452841005337E-09m), // 29 / 11
	    new TestResult(0, -6549966787.6381215871101448624m), // 29 / 12
	    new TestResult(2, 1.24786734570697837E-11m), // 29 / 13
	    new TestResult(2, -2.24235425880102E-14m), // 29 / 14
	    new TestResult(2, -3.507726905944E-16m), // 29 / 15
	    new TestResult(2, 11866.175330334086443056587349m), // 29 / 16
	    new TestResult(2, -2.042702851569255038994E-07m), // 29 / 17
	    new TestResult(2, 9.387337384931E-16m), // 29 / 18
	    new TestResult(2, -4.86096414286E-17m), // 29 / 19
	    new TestResult(2, -0.705676685966272906628710787m), // 29 / 20
	    new TestResult(2, -0.0120194723629401016807892544m), // 29 / 21
	    new TestResult(2, 0.0007820967289697085504718691m), // 29 / 22
	    new TestResult(2, -1.1296857600932761513324E-05m), // 29 / 23
	    new TestResult(2, 9.7441376775089367720444E-06m), // 29 / 24
	    new TestResult(2, -8.2062571043940289217E-09m), // 29 / 25
	    new TestResult(2, 1.17482466276439E-13m), // 29 / 26
	    new TestResult(2, 6.399778788502251E-13m), // 29 / 27
	    new TestResult(2, -4.8150763565E-18m), // 29 / 28
	    new TestResult(0, 1m), // 29 / 29
        };

        #endregion
    }
}
