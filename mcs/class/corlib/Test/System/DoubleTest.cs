// BooleanTest.cs - NUnit Test Cases for the System.Double class
//
// Bob Doan <bdoan@sicompos.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System
{

public class DoubleTest : TestCase
{
	private const Double d_zero = 0.0;
	private const Double d_neg = -1234.5678;
	private const Double d_pos = 1234.9999;
	private const Double d_pos2 = 1234.9999;
	private const Double d_nan = Double.NaN;
	private const Double d_pinf = Double.PositiveInfinity;
	private const Double d_ninf = Double.NegativeInfinity;
	private const String s = "What Ever";
	
	public DoubleTest (string name) : base (name) {}
	

	protected override void SetUp ()
	{
	}

	public static ITest Suite {
		get {
			return new TestSuite (typeof (DoubleTest));
		}
	}

	public void TestPublicFields ()
	{												  
		AssertEquals("Epsilon Field has wrong value", 3.9406564584124654e-324, Double.Epsilon);
		AssertEquals("MaxValue Field has wrong value", 1.7976931348623157e+308, Double.MaxValue);
		AssertEquals("MinValue Field has wrong value", -1.7976931348623157e+308, Double.MinValue);
		AssertEquals("NegativeInfinity Field has wrong value",  (double)-1.0 / (double)(0.0), Double.NegativeInfinity);		
		AssertEquals("PositiveInfinity Field has wrong value",  (double)1.0 / (double)(0.0), Double.PositiveInfinity);		
	}

	public void TestCompareTo () {
		//If you do int foo =  d_ninf.CompareTo(d_pinf); Assert(".." foo < 0, true) this works.... WHY???
		AssertEquals("CompareTo Infinity failed", d_ninf.CompareTo(d_pinf) < 0, true);		

		AssertEquals("CompareTo Failed", d_neg.CompareTo(d_pos) < 0, true);
		AssertEquals("CompareTo NaN Failed", d_nan.CompareTo(d_neg) < 0, true);				

		AssertEquals("CompareTo Failed", 0, d_pos.CompareTo(d_pos2));		
		AssertEquals("CompareTo Failed", 0, d_pinf.CompareTo(d_pinf));		
		AssertEquals("CompareTo Failed", 0, d_ninf.CompareTo(d_ninf));		
		AssertEquals("CompareTo Failed", 0, d_nan.CompareTo(d_nan));		

		AssertEquals("CompareTo Failed", d_pos.CompareTo(d_neg) > 0, true);		
		AssertEquals("CompareTo Failed", d_pos.CompareTo(d_nan) > 0, true);		
		AssertEquals("CompareTo Failed", d_pos.CompareTo(null) > 0, true);		
		
		try {
			d_pos.CompareTo(s);
			Fail("CompareTo should raise a System.ArgumentException");
		}
		catch (Exception e) {
			Assert("CompareTo should be a System.ArgumentException", typeof(ArgumentException) == e.GetType());
		}		
		
	}

	public void TestEquals () {
		AssertEquals("Equals Failed", true, d_pos.Equals(d_pos2));
		AssertEquals("Equals Failed", false, d_pos.Equals(d_neg));
		AssertEquals("Equals Failed", false, d_pos.Equals(s));
		
	}

	public void TestGetHasCode () {
		//I have no idea why this is failing....
		AssertEquals("GetHashCode 1 Failed", 1234, d_pos.GetHashCode());		
		AssertEquals("GetHashCode 2 Failed", -1234, d_neg.GetHashCode());
	}

	public void TestTypeCode () {
		AssertEquals("GetTypeCode Failed", TypeCode.Double, d_pos.GetTypeCode());		
	}

	public void TestIsInfinity() {
		AssertEquals("IsInfinity Failed", true, Double.IsInfinity(Double.PositiveInfinity));
		AssertEquals("IsInfinity Failed", true, Double.IsInfinity(Double.NegativeInfinity));
		AssertEquals("IsInfinity Failed", false, Double.IsInfinity(12));		
	}

	public void TestIsNan() {
		AssertEquals("IsNan Failed", true, Double.IsNaN(Double.NaN));
		AssertEquals("IsNan Failed", false, Double.IsNaN(12));
		AssertEquals("IsNan Failed", false, Double.IsNaN(Double.PositiveInfinity));
	}

	public void TestIsNegativeInfinity() {
		AssertEquals("IsNegativeInfinity Failed", true, Double.IsNegativeInfinity(Double.NegativeInfinity));
		AssertEquals("IsNegativeInfinity Failed", false, Double.IsNegativeInfinity(12));		
	}

	public void TestIsPositiveInfinity() {
		AssertEquals("IsPositiveInfinity Failed", true, Double.IsPositiveInfinity(Double.PositiveInfinity));
		AssertEquals("IsPositiveInfinity Failed", false, Double.IsPositiveInfinity(12));		
	}

	public void TestParse() {
		//I get a System.Security.SecuriytException with this... why???
		//AssertEquals("Parse Failed", 1234.5678, Double.Parse("1234.5678"));
	}

	public void TestToString() {
		//ToString is not yet Implemented......
		//AssertEquals("ToString Failed", "1234.9999", d_pos.ToString());
	}

}
}
