// FinancialTest.cs - 	NUnit Test Cases for vb module Financial
//						(class Microsoft.VisualBasic.Financial)
//
// Rob Tillie (Rob@flep-tech.nl)
//
// (C) 2004 Rob Tillie
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;

namespace MonoTests.Microsoft.VisualBasic
{

	[TestFixture]
	public class FinancialTest : Assertion {
	
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}
		
		
		// -- DDB Tests
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDDBArg1()
		{
			Financial.DDB (-1, 1, 1, 1, 1);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDDBArg2()
		{
			Financial.DDB (1, -1, 1, 1, 1);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDDBArg3()
		{
			Financial.DDB (1, 1, 0, 1, 1);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDDBArg4()
		{
			Financial.DDB (1, 1, 1, 1, 0);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestDDBArg5()
		{
			// Period has to be > Life
			Financial.DDB (1, 1, 1, 2, 1);
		}
		
		[Test]
		public void TestDDB()
		{
			double ddb = Financial.DDB (1000, 50, 10, 5, 3);
			AssertEquals ("#DDB01", 1425, ddb, 0);
			
			// TODO: How should we test an optional parameter in C#?
			ddb = Financial.DDB (1000, 50, 10, 5, 2);
			AssertEquals ("#DDB02", 950, ddb, 0);
		}
		
		[Test]
		public void TestFV()
		{
			double d = Financial.FV (10, 5, 3, 7, DueDate.BegOfPeriod);
			AssertEquals ("#FV01", -1658822, d);
			
			d = Financial.FV (10, 5, 3, 7, DueDate.EndOfPeriod);
			AssertEquals ("#FV02", -1175672, d);
			
			d = Financial.FV (0, 5, 3, 7, DueDate.BegOfPeriod);
			AssertEquals ("#FV03", -22, d);
			
			d = Financial.FV(0, 1, 1, 1, DueDate.BegOfPeriod);
			AssertEquals ("#FV04", -2, d);
			
			d = Financial.FV (0, 0, 0, 0, DueDate.BegOfPeriod);
			AssertEquals ("#FV05", 0, d);
			
			d = Financial.FV (-3, -5, -6, -4, DueDate.BegOfPeriod);
			AssertEquals ("#FV06", -4.25, d);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestIPmtArgs1()
		{
			Financial.IPmt (3, 6, 4, 2, 2, DueDate.BegOfPeriod);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestIPmtArgs2()
		{
			Financial.IPmt (3, 0, 4, 2, 2, DueDate.BegOfPeriod);
		}
		
		[Test]
		public void TestIPmt()
		{
			double d = Financial.IPmt (10, 2, 3, 7, 9, DueDate.BegOfPeriod);
        		AssertEquals ("#IPmt01", -6.25427204374573, d);
			
        		d = Financial.IPmt (10, 4, 4, 7, 4, DueDate.EndOfPeriod);
        		AssertEquals ("#IPmt02", -60.0068306011053, d);
			
        		d = Financial.IPmt (0, 5, 7, 7, 2, DueDate.BegOfPeriod);
			AssertEquals ("#IPmt03", 0, d);
			
			d = Financial.IPmt (-5, 5, 7, -7, -2, DueDate.BegOfPeriod);
			AssertEquals ("#IPmt04", 8.92508391821792, d);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestPmtArgs()
		{
			Financial.Pmt (1, 0, 1, 1, DueDate.BegOfPeriod);
		}
		
		[Test]
		public void TestPmt()
		{
			double d = Financial.Pmt (2, 5, 2, 3, DueDate.BegOfPeriod);
			AssertEquals ("#Pmt01", -1.34710743801653, d);
			
			d = Financial.Pmt (2, 5, 2, 3, DueDate.EndOfPeriod);
			AssertEquals ("#Pmt02", -4.04132231404959, d);
			
        		d = Financial.Pmt (-3, -5, -3, -4, DueDate.BegOfPeriod);
			AssertEquals ("#Pmt03", -5.68181818181818, d);
			
        		d = Financial.Pmt (-3, -5, -3, -4, DueDate.EndOfPeriod);
			AssertEquals ("#Pmt04", 11.3636363636364, d);
			
        		d = Financial.Pmt (0, 1, 0, 0, DueDate.BegOfPeriod);
			AssertEquals ("#Pmt05", 0, d);
			
        		d = Financial.Pmt (0, 1, 0, 0, DueDate.EndOfPeriod);
			AssertEquals ("#Pmt06", 0, d);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void TestSLNArgs()
		{
			Financial.SLN (0, 0, 0);
		}		
		
		[Test]
		public void TestSLN()
		{
		        double d = Financial.SLN (0, 0, 1);
			AssertEquals ("#SLN01", 0, d);
        
        		d = Financial.SLN (45, 32, 345);
        		AssertEquals ("#SLN02", 0.0376811594202899, d, 0.0001);
        
        		d = Financial.SLN (-54, -4, -76);
			AssertEquals ("#SLN03", 0.657894736842105, d, 0.001);
        	}
        	
        	[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestSYDArgs1()
		{
			Financial.SYD (1, 1, 1, -1);
		}	
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestSYDArgs2()
		{
			Financial.SYD (1, -1, 1, 1);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestSYDArgs3()
		{
			Financial.SYD (1, 1, 1, 2);
		}
		
		[Test]
		public void TestSYD()
		{
			double d = Financial.SYD (23, 34, 26, 21);
        		AssertEquals ("#SYD01", -0.188034188034188, d);

        		d = Financial.SYD (0, 1, 1, 1);
        		AssertEquals ("#SYD02", -1, d);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestIRRArgs1()
		{
			double [] arr = new double [0];
			Financial.IRR (ref arr, 0.1);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestIRRArgs2()
		{
			double [] arr = new double [] {134};
			Financial.IRR (ref arr, 0.1);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestIRRArgs3()
		{
			// -0.99 as Guess throws an exception on MS.NET, -0.98 doesn't
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.IRR (ref arr, -0.99);
		}
		
		[Test]
		public void TestIRR()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.IRR (ref arr, 0.1);
			AssertEquals ("#IRR01", 0.177435884422527, d);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestNPVArgs1()
		{
			double [] arr = null;
			double d = Financial.NPV (0.0625, ref arr);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestNPVArgs2()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.NPV (-1, ref arr);
		}
			
		[Test]
		public void TestNPV()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.NPV (0.0625, ref arr);	
			AssertEquals ("#NPV01", 19312.5702095352, d);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestNPerArgs1()
		{
			double d = Financial.NPer (-1, 2, 2, 2, DueDate.BegOfPeriod);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestNPerArgs2()
		{
			double d = Financial.NPer (0, 0, 2, 2, DueDate.BegOfPeriod);
		}
		
		[Test]
		public void TestNPer()
		{
			double d = Financial.NPer (3, 4, 6, 2, DueDate.BegOfPeriod);
			AssertEquals ("#NPer01", -0.882767373181489, d, 0.001);
			
			d = Financial.NPer (1, -4, -6, -2, DueDate.EndOfPeriod);
			AssertEquals ("#NPer02", -2.32192809488736, d, 0.001);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestMIRRArgs1()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.MIRR(ref arr, -1, 1);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestMIRRArgs2()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.MIRR(ref arr, 1, -1);
		}
		
		[Test]
		public void TestMIRR()
		{
			double [] arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			double d = Financial.MIRR (ref arr, 1, 1);
			AssertEquals ("#MIRR01", 0.509044845533018, d);
			
			arr = new double [] {-70000, 22000, 25000, 28000, 31000};
			d = Financial.MIRR (ref arr, 5, 5);
			AssertEquals ("#MIRR02", 2.02366041666348, d);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestPPmtArgs1()
		{
			double d = Financial.PPmt (2, -1, 1, 1, 1, DueDate.EndOfPeriod);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestPPmtArgs2()
		{
			double d = Financial.PPmt (1, 2, 1, 1, 1, DueDate.BegOfPeriod);
		}
		
		[Test]
		public void TestPPmt()
		{
			double d = Financial.PPmt (10, 2, 3, 7, 9, DueDate.BegOfPeriod);
        		AssertEquals("#PPmt01", -0.120300751879702, d);
			
        		d = Financial.PPmt (10, 4, 4, 7, 4, DueDate.EndOfPeriod);
        		AssertEquals("#PPmt02", -10.0006830600969, d);
			
        		d = Financial.PPmt (0, 5, 7, 7, 2, DueDate.BegOfPeriod);
			AssertEquals("#PPmt03", -1.28571428571429, d);
			
			d = Financial.PPmt (-5, 5, 7, -7, -2, DueDate.BegOfPeriod);
			AssertEquals("#PPmt04", -0.175770521818777, d);
		}
		
		[Test]
		public void TestPV()
		{
			double d = Financial.PV (1, 1, 1, 1, DueDate.BegOfPeriod);
			AssertEquals ("#PV01", -1.5, d);
			
			d = Financial.PV (1, 1, 1, 1, DueDate.EndOfPeriod);
			AssertEquals ("#PV02", -1, d);
		}
		
		[Test]
        	[ExpectedException(typeof(ArgumentException))]
		public void TestRateArgs1()
		{
			double d = Financial.Rate (-1, 1, 1, 1, DueDate.BegOfPeriod, 1);
		}
		
		[Test]
		public void TestRate()
		{
			double d = Financial.Rate (1, 1, 1, 1, DueDate.BegOfPeriod, 0.1);
			AssertEquals("#Rate01", -1.5, d, 0.01);
			
			d = Financial.Rate (1, -1, -1, -1, DueDate.BegOfPeriod, 0.1);
			AssertEquals("#Rate02", -1.50000000000001, d, 0.01);
			
			d = Financial.Rate (1, 2, 12, 10, DueDate.BegOfPeriod, 0.5);
			AssertEquals("#Rate03", -1.71428571428571, d);
		}
	}
}
