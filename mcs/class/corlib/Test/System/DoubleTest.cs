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
	private NumberFormatInfo Nfi = NumberFormatInfo.InvariantInfo;
	
	
	private string[] string_values = {
		"1", ".1", "1.1", "-12", "44.444432", ".000021121", 
		"   .00001", "  .223    ", "         -221.3233",
		" 1.7976931348623157e308 ", "+1.7976931348623157E308", "-1.7976931348623157e308",
		"4.9406564584124650e-324",
		"6.28318530717958647692528676655900577",
		"1e-05",
	};
	private double[] double_values = {
		1, .1, 1.1, -12, 44.444432, .000021121,
		.00001, .223, -221.3233,
		1.7976931348623157e308, 1.7976931348623157e308, -1.7976931348623157e308,
		4.9406564584124650e-324,
		6.28318530717958647692528676655900577,
		1e-05
	};

	public DoubleTest () {}
	

	protected override void SetUp ()
	{
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
		Assert("CompareTo Infinity failed", d_ninf.CompareTo(d_pinf) < 0);		

		Assert("CompareTo Failed01", d_neg.CompareTo(d_pos) < 0);
		Assert("CompareTo NaN Failed", d_nan.CompareTo(d_neg) < 0);				

		AssertEquals("CompareTo Failed02", 0, d_pos.CompareTo(d_pos2));		
		AssertEquals("CompareTo Failed03", 0, d_pinf.CompareTo(d_pinf));		
		AssertEquals("CompareTo Failed04", 0, d_ninf.CompareTo(d_ninf));		
		AssertEquals("CompareTo Failed05", 0, d_nan.CompareTo(d_nan));		

		Assert("CompareTo Failed06", d_pos.CompareTo(d_neg) > 0);		
		Assert("CompareTo Failed07", d_pos.CompareTo(d_nan) > 0);		
		Assert("CompareTo Failed08", d_pos.CompareTo(null) > 0);		
		
		try {
			d_pos.CompareTo(s);
			Fail("CompareTo should raise a System.ArgumentException");
		}
		catch (Exception e) {
			AssertEquals ("CompareTo should be a System.ArgumentException", typeof(ArgumentException), e.GetType());
		}		
		
	}

	public void TestEquals () {
		AssertEquals("Equals Failed", true, d_pos.Equals(d_pos2));
		AssertEquals("Equals Failed", false, d_pos.Equals(d_neg));
		AssertEquals("Equals Failed", false, d_pos.Equals(s));
		
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
		int i=0;
		try {
			for(i=0;i<string_values.Length;i++) {			
				AssertEquals("Parse Failed", double_values[i], Double.Parse(string_values[i]));
			}
		} catch (Exception e) {
			Fail("TestParse: i=" + i + " failed with e = " + e.ToString());
		}
		
		try {
			AssertEquals("Parse Failed NumberStyles.Float", 10.1111, Double.Parse(" 10.1111 ", NumberStyles.Float, Nfi));
		} catch (Exception e) {
			Fail("TestParse: Parse Failed NumberStyles.Float with e = " + e.ToString());
		}

		try {
			AssertEquals("Parse Failed NumberStyles.AllowThousands", 1234.5678, Double.Parse("1,234.5678", NumberStyles.Float | NumberStyles.AllowThousands, Nfi));
		} catch (Exception e) {
			Fail("TestParse: Parse Failed NumberStyles.AllowThousands with e = " + e.ToString());
		}
	
		try {
			Double.Parse(null);
			Fail("Parse should raise a ArgumentNullException");
		}
		catch (Exception e) {
			Assert("Parse should be a ArgumentNullException", typeof(ArgumentNullException) == e.GetType());
		}		

		try {
			Double.Parse("save the elk");
			Fail("Parse should raise a FormatException");
		}
		catch (Exception e) {
			Assert("Parse should be a FormatException", typeof(FormatException) == e.GetType());
		}		

		double ovf_plus = 0;
		try {
			ovf_plus = Double.Parse("1.79769313486232e308");
			Fail("Parse should have raised an OverflowException +");
		}
		catch (Exception e) {
			AssertEquals("Should be an OverflowException + for " + ovf_plus, typeof(OverflowException), e.GetType());
		}		

		try {
			Double.Parse("-1.79769313486232e308");
			Fail("Parse should have raised an OverflowException -");
		}
		catch (Exception e) {
			AssertEquals("Should be an OverflowException -", typeof(OverflowException), e.GetType());
		}		


	}

	public void TestToString() {
		//ToString is not yet Implemented......
		//AssertEquals("ToString Failed", "1234.9999", d_pos.ToString());
		double d;
		try {
			d = 3.1415;
			d.ToString ("X");
			Fail ("Should have thrown FormatException");
		} catch (FormatException) {
			/* do nothing, this is what we expect */
		} catch (Exception e) {
			Fail ("Unexpected exception e: " + e);
		}
	}

}
}
