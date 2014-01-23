using System;
using System.Reflection;

class Tests {

	public static int Main () {
		return TestDriver.RunTests (typeof (Tests));
	}
	
	static int test_0_beq () {
		double a = 2.0;
		if (a != 2.0)
			return 1;
		return 0;
	}

	static int test_0_bne_un () {
		double a = 2.0;
		if (a == 1.0)
			return 1;
		return 0;
	}

	static int test_0_conv_r8 () {
		double a = 2;
		if (a != 2.0)
			return 1;
		return 0;
	}

	static int test_0_conv_i () {
		double a = 2.0;
		int i = (int)a;
		if (i != 2)
			return 1;
		uint ui = (uint)a;
		if (ui != 2)
			return 2;
		short s = (short)a;
		if (s != 2)
			return 3;
		ushort us = (ushort)a;
		if (us != 2)
			return 4;
		byte b = (byte)a;
		if (b != 2)
			return 5;
		return 0;
	}

	static int test_5_conv_r4 () {
		int i = 5;
		float f = (float)i;
		return (int)f;
	}

	static int test_5_double_conv_r4 () {
		double d = 5.0;
		float f = (float)d;
		return (int)f;
	}

	static int test_5_float_conv_r8 () {
		float f = 5.0F;
		double d = (double)f;
		return (int)d;
	}

	static int test_5_conv_r8 () {
		int i = 5;
		double f = (double)i;
		return (int)f;
	}

	static int test_5_add () {
		double a = 2.0;
		double b = 3.0;		
		return (int)(a + b);
	}

	static int test_5_sub () {
		double a = 8.0;
		double b = 3.0;		
		return (int)(a - b);
	}	

	static int test_24_mul () {
		double a = 8.0;
		double b = 3.0;		
		return (int)(a * b);
	}	

	static int test_4_div () {
		double a = 8.0;
		double b = 2.0;		
		return (int)(a / b);
	}	

	static int test_2_rem () {
		double a = 8.0;
		double b = 3.0;		
		return (int)(a % b);
	}	

	static int test_2_neg () {
		double a = -2.0;		
		return (int)(-a);
	}
	
	static int test_46_float_add_spill () {
		// we overflow the FP stack
		double a = 1;
		double b = 2;
		double c = 3;
		double d = 4;
		double e = 5;
		double f = 6;
		double g = 7;
		double h = 8;
		double i = 9;

		return (int)(1.0 + (a + (b + (c + (d + (e + (f + (g + (h + i)))))))));
	}

	static int test_362880_float_mul_spill () {
		// we overflow the FP stack
		double a = 1;
		double b = 2;
		double c = 3;
		double d = 4;
		double e = 5;
		double f = 6;
		double g = 7;
		double h = 8;
		double i = 9;

		return (int)(1.0 * (a * (b * (c * (d * (e * (f * (g * (h * i)))))))));
	}

	static int test_4_long_cast () {
		long a = 1000;
		double d = (double)a;
		long b = (long)d;
		if (b != 1000)
			return 0;
		return 4;
	}

	/* FIXME: This only works on little-endian machines */
	/*
	static unsafe int test_2_negative_zero () {
		int result = 0;
		double d = -0.0;
		float f = -0.0f;

		byte *ptr = (byte*)&d;
		if (ptr [7] == 0)
			return result;
		result ++;

		ptr = (byte*)&f;
		if (ptr [3] == 0)
			return result;
		result ++;

		return result;
	}
	*/

	static int test_16_float_cmp () {
		double a = 2.0;
		double b = 1.0;
		int result = 0;
		bool val;
		
		val = a == a;
		if (!val)
			return result;
		result++;

		val = (a != a);
		if (val)
			return result;
		result++;

		val = a < a;
		if (val)
			return result;
		result++;

		val = a > a;
		if (val)
			return result;
		result++;

		val = a <= a;
		if (!val)
			return result;
		result++;

		val = a >= a;
		if (!val)
			return result;
		result++;

		val = b == a;
		if (val)
			return result;
		result++;

		val = b < a;
		if (!val)
			return result;
		result++;

		val = b > a;
		if (val)
			return result;
		result++;

		val = b <= a;
		if (!val)
			return result;
		result++;

		val = b >= a;
		if (val)
			return result;
		result++;

		val = a == b;
		if (val)
			return result;
		result++;

		val = a < b;
		if (val)
			return result;
		result++;

		val = a > b;
		if (!val)
			return result;
		result++;

		val = a <= b;
		if (val)
			return result;
		result++;

		val = a >= b;
		if (!val)
			return result;
		result++;

		return result;
	}

	static int test_15_float_cmp_un () {
		double a = Double.NaN;
		double b = 1.0;
		int result = 0;
		bool val;
		
		val = a == a;
		if (val)
			return result;
		result++;

		val = a < a;
		if (val)
			return result;
		result++;

		val = a > a;
		if (val)
			return result;
		result++;

		val = a <= a;
		if (val)
			return result;
		result++;

		val = a >= a;
		if (val)
			return result;
		result++;

		val = b == a;
		if (val)
			return result;
		result++;

		val = b < a;
		if (val)
			return result;
		result++;

		val = b > a;
		if (val)
			return result;
		result++;

		val = b <= a;
		if (val)
			return result;
		result++;

		val = b >= a;
		if (val)
			return result;
		result++;

		val = a == b;
		if (val)
			return result;
		result++;

		val = a < b;
		if (val)
			return result;
		result++;

		val = a > b;
		if (val)
			return result;
		result++;

		val = a <= b;
		if (val)
			return result;
		result++;

		val = a >= b;
		if (val)
			return result;
		result++;

		return result;
	}

	static int test_15_float_branch () {
		double a = 2.0;
		double b = 1.0;
		int result = 0;
		
		if (!(a == a))
			return result;
		result++;

		if (a < a)
			return result;
		result++;

		if (a > a)
			return result;
		result++;

		if (!(a <= a))
			return result;
		result++;

		if (!(a >= a))
			return result;
		result++;

		if (b == a)
			return result;
		result++;

		if (!(b < a))
			return result;
		result++;

		if (b > a)
			return result;
		result++;

		if (!(b <= a))
			return result;
		result++;

		if (b >= a)
			return result;
		result++;

		if (a == b)
			return result;
		result++;

		if (a < b)
			return result;
		result++;

		if (!(a > b))
			return result;
		result++;

		if (a <= b)
			return result;
		result++;

		if (!(a >= b))
			return result;
		result++;

		return result;
	}

	static int test_15_float_branch_un () {
		double a = Double.NaN;
		double b = 1.0;
		int result = 0;
		
		if (a == a)
			return result;
		result++;

		if (a < a)
			return result;
		result++;

		if (a > a)
			return result;
		result++;

		if (a <= a)
			return result;
		result++;

		if (a >= a)
			return result;
		result++;

		if (b == a)
			return result;
		result++;

		if (b < a)
			return result;
		result++;

		if (b > a)
			return result;
		result++;

		if (b <= a)
			return result;
		result++;

		if (b >= a)
			return result;
		result++;

		if (a == b)
			return result;
		result++;

		if (a < b)
			return result;
		result++;

		if (a > b)
			return result;
		result++;

		if (a <= b)
			return result;
		result++;

		if (a >= b)
			return result;
		result++;

		return result;
	}

}

public class TestDriver {

	static public int RunTests (Type type, string[] args) {
		int failed = 0, ran = 0;
		int result, expected, elen;
		int i, j;
		string name;
		MethodInfo[] methods;
		bool do_timings = false;
		int tms = 0;
		DateTime start, end = DateTime.Now;

		if (args != null && args.Length > 0) {
			for (j = 0; j < args.Length; j++) {
				if (args [j] == "--time") {
					do_timings = true;
					string[] new_args = new string [args.Length - 1];
					for (i = 0; i < j; ++i)
						new_args [i] = args [i];
					j++;
					for (; j < args.Length; ++i, ++j)
						new_args [i] = args [j];
					args = new_args;
					break;
				}
			}
		}
		methods = type.GetMethods (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
		for (i = 0; i < methods.Length; ++i) {
			name = methods [i].Name;
			if (!name.StartsWith ("test_"))
				continue;
			if (args != null && args.Length > 0) {
				bool found = false;
				for (j = 0; j < args.Length; j++) {
					if (name.EndsWith (args [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					continue;
			}
			for (j = 5; j < name.Length; ++j)
				if (!Char.IsDigit (name [j]))
					break;
			expected = Int32.Parse (name.Substring (5, j - 5));
			start = DateTime.Now;
			result = (int)methods [i].Invoke (null, null);
			if (do_timings) {
				end = DateTime.Now;
				long tdiff = end.Ticks - start.Ticks;
				int mdiff = (int)tdiff/10000;
				tms += mdiff;
				Console.WriteLine ("{0} took {1} ms", name, mdiff);
			}
			ran++;
			if (result != expected) {
				failed++;
				Console.WriteLine ("{0} failed: got {1}, expected {2}", name, result, expected);
			}
		}
		
		if (do_timings) {
			Console.WriteLine ("Total ms: {0}", tms);
		}
		Console.WriteLine ("Regression tests: {0} ran, {1} failed in {2}", ran, failed, type);
		//Console.WriteLine ("Regression tests: {0} ran, {1} failed in [{2}]{3}", ran, failed, type.Assembly.GetName().Name, type);
		return failed;
	}
	static public int RunTests (Type type) {
		return RunTests (type, null);
	}
}

