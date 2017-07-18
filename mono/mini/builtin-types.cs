#define ARCH_32
// #define NINT_JIT_OPTIMIZED

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


public class BuiltinTests {
	static int test_0_nint_ctor ()
	{
		var x = new nint (10);
		var y = new nint (x);
		var z = new nint (new nint (20));
		if ((int)x != 10)
			return 1;
		if ((int)y != 10)
			return 2;
		if ((int)z != 20)
			return 3;
		return 0;
	}

	static int test_0_nint_casts ()
	{
		var x = (nint)10;
		var y = (nint)20L;

		if ((int)x != 10)
			return 1;
		if ((long)x != 10L)
			return 2;
		if ((int)y != 20)
			return 3;
		if ((long)y != 20L)
			return 4;
		return 0;
	}

	static int test_0_nint_plus ()
	{
		var x = (nint)10;
		var z = +x;
		if ((int)z != 10)
			return 1;
		return 0;
	}

	static int test_0_nint_neg ()
	{
		var x = (nint)10;
		var z = -x;
		if ((int)z != -10)
			return 1;
		return 0;
	}

	static int test_0_nint_comp ()
	{
		var x = (nint)10;
		var z = ~x;
		if ((int)z != ~10)
			return 1;
		return 0;
	}

	static int test_0_nint_inc ()
	{
		var x = (nint)10;
		++x;
		if ((int)x != 11)
			return 1;
		return 0;
	}

	static int test_0_nint_dec ()
	{
		var x = (nint)10;
		--x;
		if ((int)x != 9)
			return 1;
		return 0;
	}

	static int test_0_nint_add ()
	{
		var x = (nint)10;
		var y = (nint)20;
		var z = x + y;
		if ((int)z != 30)
			return 1;
		return 0;
	}

	static int test_0_nint_sub ()
	{
		var x = (nint)10;
		var y = (nint)20;
		var z = x - y;
		if ((int)z != -10)
			return 1;
		return 0;
	}

	static int test_0_nint_mul ()
	{
		var x = (nint)10;
		var y = (nint)20;
		var z = x * y;
		if ((int)z != 200)
			return 1;
		return 0;
	}

	static int test_0_nint_div ()
	{
		var x = (nint)30;
		var y = (nint)3;
		var z = x / y;
		if ((int)z != 10)
			return 1;
		return 0;
	}

	static int test_0_nint_rem ()
	{
		var x = (nint)22;
		var y = (nint)10;
		var z = x % y;
		if ((int)z != 2)
			return 1;
		return 0;
	}

	static int test_0_nint_and ()
	{
		var x = (nint)0x30;
		var y = (nint)0x11;
		var z = x & y;
		if ((int)z != 0x10)
			return 1;
		return 0;
	}

	static int test_0_nint_or ()
	{
		var x = (nint)0x0F;
		var y = (nint)0xF0;
		var z = x | y;
		if ((int)z != 0xFF)
			return 1;
		return 0;
	}

	static int test_0_nint_xor ()
	{
		var x = (nint)0xFF;
		var y = (nint)0xF0;
		var z = x ^ y;
		if ((int)z != 0x0F)
			return 1;
		return 0;
	}

	static int test_0_nint_shl ()
	{
		var x = (nint)10;
		var z = x << 2;
		if ((int)z != 40)
			return 1;
		return 0;
	}

	static int test_0_nint_shr ()
	{
		var x = (nint)10;
		var z = x >> 2;
		if ((int)z != 2)
			return 1;
		return 0;
	}

	static int test_0_nint_cmp_same_val ()
	{
		var x = (nint)10;
		var y = (nint)10;
		if (!(x == y))
			return 1;
		if (x != y)
			return 2;
		if (x < y)
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (!(x >= y))
			return 6;
		return 0;
	}

	static int test_0_nint_cmp_small_val ()
	{
		var x = (nint)5;
		var y = (nint)10;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (!(x < y))
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (x >= y)
			return 6;
		return 0;
	}

	static int test_0_nint_cmp_large_val ()
	{
		var x = (nint)20;
		var y = (nint)10;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (x < y)
			return 3;
		if (!(x > y))
			return 4;
		if (x <= y)
			return 1;
		if (!(x >= y))
			return 1;
		return 0;
	}

	// static int test_0_nint_call_boxed_equals ()
	// {
	// 	object x = new nint (10);
	// 	object y = new nint (10);
	// 	if (!x.Equals (y))
	// 		return 1;
	// 	return 0;
	// }

	static int test_0_nint_call_boxed_funs ()
	{
		object x = new nint (10);
		object y = new nint (10);
		if (x.GetHashCode () == 0)
			return 2;
		if (x.ToString () != "10")
			return 3;
		return 0;
	}

	public int test_0_nint_unboxed_member_calls ()
	{
		var x = (nint)10;
		if (!x.Equals (x))
			return 1;
		if (x != nint.Parse ("10"))
			return 2;
		return 0;
	}

	static int test_0_nuint_ctor ()
	{
		var x = new nuint (10u);
		var y = new nuint (x);
		var z = new nuint (new nuint (20u));
		if ((uint)x != 10)
			return 1;
		if ((uint)y != 10)
			return 2;
		if ((uint)z != 20)
			return 3;
		return 0;
	}

	static int test_0_nuint_casts ()
	{
		var x = (nuint)10;
		var y = (nuint)20L;

		if ((uint)x != 10)
			return 1;
		if ((ulong)x != 10L)
			return 2;
		if ((uint)y != 20)
			return 3;
		if ((ulong)y != 20L)
			return 4;
		return 0;
	}

	static int test_0_nuint_plus ()
	{
		var x = (nuint)10;
		var z = +x;
		if ((uint)z != 10)
			return 1;
		return 0;
	}

	// static int test_0_nuint_neg ()
	// {
	// 	var x = (nuint)10;
	// 	var z = -x;
	// 	if ((uint)z != -10)
	// 		return 1;
	// 	return 0;
	// }

	static int test_0_nuint_comp ()
	{
		var x = (nuint)10;
		var z = ~x;
		if ((uint)z != ~10u)
			return 1;
		return 0;
	}

	static int test_0_nuint_inc ()
	{
		var x = (nuint)10;
		++x;
		if ((uint)x != 11)
			return 1;
		return 0;
	}

	static int test_0_nuint_dec ()
	{
		var x = (nuint)10;
		--x;
		if ((uint)x != 9)
			return 1;
		return 0;
	}

	static int test_0_nuint_add ()
	{
		var x = (nuint)10;
		var y = (nuint)20;
		var z = x + y;
		if ((uint)z != 30)
			return 1;
		return 0;
	}

	static int test_0_nuint_sub ()
	{
		var x = (nuint)20;
		var y = (nuint)5;
		var z = x - y;
		if ((uint)z != 15)
			return 1;
		return 0;
	}

	static int test_0_nuint_mul ()
	{
		var x = (nuint)10;
		var y = (nuint)20;
		var z = x * y;
		if ((uint)z != 200)
			return 1;
		return 0;
	}

	static int test_0_nuint_div ()
	{
		var x = (nuint)30;
		var y = (nuint)3;
		var z = x / y;
		if ((uint)z != 10)
			return 1;
		return 0;
	}

	static int test_0_nuint_rem ()
	{
		var x = (nuint)22;
		var y = (nuint)10;
		var z = x % y;
		if ((uint)z != 2)
			return 1;
		return 0;
	}

	static int test_0_nuint_and ()
	{
		var x = (nuint)0x30;
		var y = (nuint)0x11;
		var z = x & y;
		if ((uint)z != 0x10)
			return 1;
		return 0;
	}

	static int test_0_nuint_or ()
	{
		var x = (nuint)0x0F;
		var y = (nuint)0xF0;
		var z = x | y;
		if ((uint)z != 0xFF)
			return 1;
		return 0;
	}

	static int test_0_nuint_xor ()
	{
		var x = (nuint)0xFF;
		var y = (nuint)0xF0;
		var z = x ^ y;
		if ((uint)z != 0x0F)
			return 1;
		return 0;
	}

	static int test_0_nuint_shl ()
	{
		var x = (nuint)10;
		var z = x << 2;
		if ((uint)z != 40)
			return 1;
		return 0;
	}

	static int test_0_nuint_shr ()
	{
		var x = (nuint)10;
		var z = x >> 2;
		if ((uint)z != 2)
			return 1;
		return 0;
	}

	static int test_0_nuint_cmp_same_val ()
	{
		var x = (nuint)10;
		var y = (nuint)10;
		if (!(x == y))
			return 1;
		if (x != y)
			return 2;
		if (x < y)
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (!(x >= y))
			return 6;
		return 0;
	}

	static int test_0_nuint_cmp_small_val ()
	{
		var x = (nuint)5;
		var y = (nuint)10;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (!(x < y))
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (x >= y)
			return 6;
		return 0;
	}

	static int test_0_nuint_cmp_large_val ()
	{
		var x = (nuint)20;
		var y = (nuint)10;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (x < y)
			return 3;
		if (!(x > y))
			return 4;
		if (x <= y)
			return 1;
		if (!(x >= y))
			return 1;
		return 0;
	}

	// static int test_0_nuint_call_boxed_equals ()
	// {
	// 	object x = new nuint (10);
	// 	object y = new nuint (10);
	// 	if (!x.Equals (y))
	// 		return 1;
	// 	return 0;
	// }

	static int test_0_nuint_call_boxed_funs ()
	{
		object x = new nuint (10u);
		object y = new nuint (10u);
		if (x.GetHashCode () == 0)
			return 2;
		if (x.ToString () != "10")
			return 3;
		return 0;
	}

	public int test_0_nuint_unboxed_member_calls ()
	{
		var x = (nuint)10;
		if (!x.Equals (x))
			return 1;
		if (x != nuint.Parse ("10"))
			return 2;
		return 0;
	}

	static int test_0_nfloat_ctor ()
	{
		var x = new nfloat (10.0f);
		var y = new nfloat (x);
		var z = new nfloat (new nfloat (20f));
		if ((float)x != 10f)
			return 1;
		if ((float)y != 10f)
			return 2;
		if ((float)z != 20f)
			return 3;
		return 0;
	}

	static int test_0_nfloat_casts ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)20;

		if ((float)x != 10f)
			return 1;
		if ((double)x != 10)
			return 2;
		if ((float)y != 20f)
			return 3;
		if ((double)y != 20)
			return 4;
		return 0;
	}

	static int test_0_nfloat_plus ()
	{
		var x = (nfloat)10f;
		var z = +x;
		if ((float)z != 10f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_neg ()
	{
		var x = (nfloat)10f;
		var z = -x;
		if ((float)z != -10f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_inc ()
	{
		var x = (nfloat)10f;
		++x;
		if ((float)x != 11f) {
			Console.WriteLine ((float)x);
			return 1;
		}
		return 0;
	}

	static int test_0_nfloat_dec ()
	{
		var x = (nfloat)10f;
		--x;
		if ((float)x != 9f) {
			Console.WriteLine ((float)x);
			return 1;
		}
		return 0;
	}

	static int test_0_nfloat_add ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)20f;
		var z = x + y;
		if ((float)z != 30f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_sub ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)20f;
		var z = x - y;
		if ((float)z != -10f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_mul ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)20f;
		var z = x * y;
		if ((float)z != 200f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_div ()
	{
		var x = (nfloat)30f;
		var y = (nfloat)3f;
		var z = x / y;
		if ((float)z != 10f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_rem ()
	{
		var x = (nfloat)22f;
		var y = (nfloat)10f;
		var z = x % y;
		if ((float)z != 2f)
			return 1;
		return 0;
	}

	static int test_0_nfloat_cmp_same_val ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)10f;
		if (!(x == y))
			return 1;
		if (x != y)
			return 2;
		if (x < y)
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (!(x >= y))
			return 6;
		return 0;
	}

	static int test_0_nfloat_cmp_small_val ()
	{
		var x = (nfloat)5f;
		var y = (nfloat)10f;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (!(x < y))
			return 3;
		if (x > y)
			return 4;
		if (!(x <= y))
			return 5;
		if (x >= y)
			return 6;
		return 0;
	}

	static int test_0_nfloat_cmp_large_val ()
	{
		var x = (nfloat)20f;
		var y = (nfloat)10f;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (x < y)
			return 3;
		if (!(x > y))
			return 4;
		if (x <= y)
			return 1;
		if (!(x >= y))
			return 1;
		return 0;
	}

	static int test_0_nfloat_cmp_left_nan ()
	{
		var x = (nfloat)float.NaN;
		var y = (nfloat)10f;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (x < y)
			return 3;
		if (x > y)
			return 4;
		if (x <= y)
			return 1;
		if (x >= y)
			return 1;
		return 0;
	}


	static int test_0_nfloat_cmp_right_nan ()
	{
		var x = (nfloat)10f;
		var y = (nfloat)float.NaN;
		if (x == y)
			return 1;
		if (!(x != y))
			return 2;
		if (x < y)
			return 3;
		if (x > y)
			return 4;
		if (x <= y)
			return 1;
		if (x >= y)
			return 1;
		return 0;
	}
	// static int test_0_nfloat_call_boxed_equals ()
	// {
	// 	object x = new nfloat (10f);
	// 	object y = new nfloat (10f);
	// 	if (!x.Equals (y))
	// 		return 1;
	// 	return 0;
	// }

	static int test_0_nfloat_call_boxed_funs ()
	{
		object x = new nfloat (10f);
		object y = new nfloat (10f);
		if (x.GetHashCode () == 0)
			return 2;
		if (x.ToString () != "10")
			return 3;
		return 0;
	}

	public int test_0_nfloat_unboxed_member_calls ()
	{
		var x = (nfloat)10f;
		if (!x.Equals (x))
			return 1;
		if (x != nfloat.Parse ("10"))
			return 2;
		return 0;
	}

	public static int Main (String[] args) {
		return TestDriver.RunTests (typeof (BuiltinTests), args);
	}
}


// !!! WARNING - GENERATED CODE - DO NOT EDIT !!!
//
// Generated by NativeTypes.tt, a T4 template.
//
// NativeTypes.cs: basic types with 32 or 64 bit sizes:
//
//   - nint
//   - nuint
//   - nfloat
//
// Authors:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2013 Xamarin, Inc. All rights reserved.
//

namespace System
{
	[Serializable]
	public struct nint : IFormattable, IConvertible, IComparable, IComparable<nint>, IEquatable <nint>
	{
		public nint (nint v) { this.v = v.v; }
		public nint (Int32 v) { this.v = v; }

#if ARCH_32
		public static readonly nint MaxValue = Int32.MaxValue;
		public static readonly nint MinValue = Int32.MinValue;

		public Int32 v;

		public nint (Int64 v) { this.v = (Int32)v; }
#else
		public static readonly nint MaxValue = Int32.MaxValue;
		public static readonly nint MinValue = Int32.MinValue;

		Int64 v;

		public nint (Int64 v) { this.v = v; }
#endif

#if ARCH_32
#	if NINT_JIT_OPTIMIZED
		public static implicit operator Int32 (nint v) { throw new NotImplementedException (); }
		public static implicit operator nint (Int32 v) { throw new NotImplementedException (); }
		public static implicit operator Int64 (nint v) { throw new NotImplementedException (); }
		public static explicit operator nint (Int64 v) { throw new NotImplementedException (); }
#	else
		public static implicit operator Int32 (nint v) { return v.v; }
		public static implicit operator nint (Int32 v) { return new nint (v); }
		public static implicit operator Int64 (nint v) { return (Int64)v.v; }
		public static explicit operator nint (Int64 v) { return new nint (v); }
#	endif
#else
#	if NINT_JIT_OPTIMIZED
		public static explicit operator Int32 (nint v) { throw new NotImplementedException (); }
		public static implicit operator nint (Int32 v) { throw new NotImplementedException (); }
		public static implicit operator Int64 (nint v) { throw new NotImplementedException (); }
		public static implicit operator nint (Int64 v) { throw new NotImplementedException (); }
#	else
		public static explicit operator Int32 (nint v) { return (Int32)v.v; }
		public static implicit operator nint (Int32 v) { return new nint (v); }
		public static implicit operator Int64 (nint v) { return v.v; }
		public static implicit operator nint (Int64 v) { return new nint (v); }
#	endif
#endif

#if NINT_JIT_OPTIMIZED
		public static nint operator + (nint v) { throw new NotImplementedException (); }
		public static nint operator - (nint v) { throw new NotImplementedException (); }
		public static nint operator ~ (nint v) { throw new NotImplementedException (); }
#else
		public static nint operator + (nint v) { return new nint (+v.v); }
		public static nint operator - (nint v) { return new nint (-v.v); }
		public static nint operator ~ (nint v) { return new nint (~v.v); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nint operator ++ (nint v) { throw new NotImplementedException (); }
		public static nint operator -- (nint v) { throw new NotImplementedException (); }
#else
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static nint operator ++ (nint v) { return new nint (v.v + 1); }
		public static nint operator -- (nint v) { return new nint (v.v - 1); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nint operator + (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator - (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator * (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator / (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator % (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator & (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator | (nint l, nint r) { throw new NotImplementedException (); }
		public static nint operator ^ (nint l, nint r) { throw new NotImplementedException (); }

		public static nint operator << (nint l, int r) { throw new NotImplementedException (); }
		public static nint operator >> (nint l, int r) { throw new NotImplementedException (); }
#else
		public static nint operator + (nint l, nint r) { return new nint (l.v + r.v); }
		public static nint operator - (nint l, nint r) { return new nint (l.v - r.v); }
		public static nint operator * (nint l, nint r) { return new nint (l.v * r.v); }
		public static nint operator / (nint l, nint r) { return new nint (l.v / r.v); }
		public static nint operator % (nint l, nint r) { return new nint (l.v % r.v); }
		public static nint operator & (nint l, nint r) { return new nint (l.v & r.v); }
		public static nint operator | (nint l, nint r) { return new nint (l.v | r.v); }
		public static nint operator ^ (nint l, nint r) { return new nint (l.v ^ r.v); }

		public static nint operator << (nint l, int r) { return new nint (l.v << r); }
		public static nint operator >> (nint l, int r) { return new nint (l.v >> r); }
#endif

#if NINT_JIT_OPTIMIZED
		public static bool operator == (nint l, nint r) { throw new NotImplementedException (); }
		public static bool operator != (nint l, nint r) { throw new NotImplementedException (); }
		public static bool operator <  (nint l, nint r) { throw new NotImplementedException (); }
		public static bool operator >  (nint l, nint r) { throw new NotImplementedException (); }
		public static bool operator <= (nint l, nint r) { throw new NotImplementedException (); }
		public static bool operator >= (nint l, nint r) { throw new NotImplementedException (); }
#else
		public static bool operator == (nint l, nint r) { return l.v == r.v; }
		public static bool operator != (nint l, nint r) { return l.v != r.v; }
		public static bool operator <  (nint l, nint r) { return l.v < r.v; }
		public static bool operator >  (nint l, nint r) { return l.v > r.v; }
		public static bool operator <= (nint l, nint r) { return l.v <= r.v; }
		public static bool operator >= (nint l, nint r) { return l.v >= r.v; }
#endif

		public int CompareTo (nint value) { return v.CompareTo (value.v); }
		public int CompareTo (object value) { return v.CompareTo (value); }
		public bool Equals (nint obj) { return v.Equals (obj.v); }
		public override bool Equals (object obj) { return v.Equals (obj); }
		public override int GetHashCode () { return v.GetHashCode (); }

#if ARCH_32
		public static nint Parse (string s, IFormatProvider provider) { return Int32.Parse (s, provider); }
		public static nint Parse (string s, NumberStyles style) { return Int32.Parse (s, style); }
		public static nint Parse (string s) { return Int32.Parse (s); }
		public static nint Parse (string s, NumberStyles style, IFormatProvider provider) {
			return Int32.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nint result)
		{
			Int32 v;
			var r = Int32.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nint result)
		{
			Int32 v;
			var r = Int32.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#else
		public static nint Parse (string s, IFormatProvider provider) { return Int64.Parse (s, provider); }
		public static nint Parse (string s, NumberStyles style) { return Int64.Parse (s, style); }
		public static nint Parse (string s) { return Int64.Parse (s); }
		public static nint Parse (string s, NumberStyles style, IFormatProvider provider) {
			return Int64.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nint result)
		{
			Int64 v;
			var r = Int64.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nint result)
		{
			Int64 v;
			var r = Int64.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#endif

		public override string ToString () { return v.ToString (); }
		public string ToString (IFormatProvider provider) { return v.ToString (provider); }
		public string ToString (string format) { return v.ToString (format); }
		public string ToString (string format, IFormatProvider provider) { return v.ToString (format, provider); }

		public TypeCode GetTypeCode () { return v.GetTypeCode (); }

		bool     IConvertible.ToBoolean  (IFormatProvider provider) { return ((IConvertible)v).ToBoolean (provider); }
		byte     IConvertible.ToByte     (IFormatProvider provider) { return ((IConvertible)v).ToByte (provider); }
		char     IConvertible.ToChar     (IFormatProvider provider) { return ((IConvertible)v).ToChar (provider); }
		DateTime IConvertible.ToDateTime (IFormatProvider provider) { return ((IConvertible)v).ToDateTime (provider); }
		decimal  IConvertible.ToDecimal  (IFormatProvider provider) { return ((IConvertible)v).ToDecimal (provider); }
		double   IConvertible.ToDouble   (IFormatProvider provider) { return ((IConvertible)v).ToDouble (provider); }
		short    IConvertible.ToInt16    (IFormatProvider provider) { return ((IConvertible)v).ToInt16 (provider); }
		int      IConvertible.ToInt32    (IFormatProvider provider) { return ((IConvertible)v).ToInt32 (provider); }
		long     IConvertible.ToInt64    (IFormatProvider provider) { return ((IConvertible)v).ToInt64 (provider); }
		sbyte    IConvertible.ToSByte    (IFormatProvider provider) { return ((IConvertible)v).ToSByte (provider); }
		float    IConvertible.ToSingle   (IFormatProvider provider) { return ((IConvertible)v).ToSingle (provider); }
		ushort   IConvertible.ToUInt16   (IFormatProvider provider) { return ((IConvertible)v).ToUInt16 (provider); }
		uint     IConvertible.ToUInt32   (IFormatProvider provider) { return ((IConvertible)v).ToUInt32 (provider); }
		ulong    IConvertible.ToUInt64   (IFormatProvider provider) { return ((IConvertible)v).ToUInt64 (provider); }

		object IConvertible.ToType (Type targetType, IFormatProvider provider) {
			return ((IConvertible)v).ToType (targetType, provider);
		}
	}

	[Serializable]
	public struct nuint : IFormattable, IConvertible, IComparable, IComparable<nuint>, IEquatable <nuint>
	{
		public nuint (nuint v) { this.v = v.v; }
		public nuint (UInt32 v) { this.v = v; }

#if ARCH_32
		public static readonly nuint MaxValue = UInt32.MaxValue;
		public static readonly nuint MinValue = UInt32.MinValue;

		UInt32 v;

		public nuint (UInt64 v) { this.v = (UInt32)v; }
#else
		public static readonly nuint MaxValue = UInt32.MaxValue;
		public static readonly nuint MinValue = UInt32.MinValue;

		UInt64 v;

		public nuint (UInt64 v) { this.v = v; }
#endif

#if ARCH_32
#	if NINT_JIT_OPTIMIZED
		public static implicit operator UInt32 (nuint v) { throw new NotImplementedException (); }
		public static implicit operator nuint (UInt32 v) { throw new NotImplementedException (); }
		public static implicit operator UInt64 (nuint v) { throw new NotImplementedException (); }
		public static explicit operator nuint (UInt64 v) { throw new NotImplementedException (); }
#	else
		public static implicit operator UInt32 (nuint v) { return v.v; }
		public static implicit operator nuint (UInt32 v) { return new nuint (v); }
		public static implicit operator UInt64 (nuint v) { return (UInt64)v.v; }
		public static explicit operator nuint (UInt64 v) { return new nuint (v); }
#	endif
#else
#	if NINT_JIT_OPTIMIZED
		public static explicit operator UInt32 (nuint v) { throw new NotImplementedException (); }
		public static implicit operator nuint (UInt32 v) { throw new NotImplementedException (); }
		public static implicit operator UInt64 (nuint v) { throw new NotImplementedException (); }
		public static implicit operator nuint (UInt64 v) { throw new NotImplementedException (); }
#	else
		public static explicit operator UInt32 (nuint v) { return (UInt32)v.v; }
		public static implicit operator nuint (UInt32 v) { return new nuint (v); }
		public static implicit operator UInt64 (nuint v) { return v.v; }
		public static implicit operator nuint (UInt64 v) { return new nuint (v); }
#	endif
#endif

#if NINT_JIT_OPTIMIZED
		public static nuint operator + (nuint v) { throw new NotImplementedException (); }
		public static nuint operator ~ (nuint v) { throw new NotImplementedException (); }
#else
		public static nuint operator + (nuint v) { return new nuint (+v.v); }
		public static nuint operator ~ (nuint v) { return new nuint (~v.v); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nuint operator ++ (nuint v) { throw new NotImplementedException (); }
		public static nuint operator -- (nuint v) { throw new NotImplementedException (); }
#else
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static nuint operator ++ (nuint v) { return new nuint (v.v + 1); }
		public static nuint operator -- (nuint v) { return new nuint (v.v - 1); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nuint operator + (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator - (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator * (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator / (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator % (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator & (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator | (nuint l, nuint r) { throw new NotImplementedException (); }
		public static nuint operator ^ (nuint l, nuint r) { throw new NotImplementedException (); }

		public static nuint operator << (nuint l, int r) { throw new NotImplementedException (); }
		public static nuint operator >> (nuint l, int r) { throw new NotImplementedException (); }
#else
		public static nuint operator + (nuint l, nuint r) { return new nuint (l.v + r.v); }
		public static nuint operator - (nuint l, nuint r) { return new nuint (l.v - r.v); }
		public static nuint operator * (nuint l, nuint r) { return new nuint (l.v * r.v); }
		public static nuint operator / (nuint l, nuint r) { return new nuint (l.v / r.v); }
		public static nuint operator % (nuint l, nuint r) { return new nuint (l.v % r.v); }
		public static nuint operator & (nuint l, nuint r) { return new nuint (l.v & r.v); }
		public static nuint operator | (nuint l, nuint r) { return new nuint (l.v | r.v); }
		public static nuint operator ^ (nuint l, nuint r) { return new nuint (l.v ^ r.v); }

		public static nuint operator << (nuint l, int r) { return new nuint (l.v << r); }
		public static nuint operator >> (nuint l, int r) { return new nuint (l.v >> r); }
#endif

#if NINT_JIT_OPTIMIZED
		public static bool operator == (nuint l, nuint r) { throw new NotImplementedException (); }
		public static bool operator != (nuint l, nuint r) { throw new NotImplementedException (); }
		public static bool operator <  (nuint l, nuint r) { throw new NotImplementedException (); }
		public static bool operator >  (nuint l, nuint r) { throw new NotImplementedException (); }
		public static bool operator <= (nuint l, nuint r) { throw new NotImplementedException (); }
		public static bool operator >= (nuint l, nuint r) { throw new NotImplementedException (); }
#else
		public static bool operator == (nuint l, nuint r) { return l.v == r.v; }
		public static bool operator != (nuint l, nuint r) { return l.v != r.v; }
		public static bool operator <  (nuint l, nuint r) { return l.v < r.v; }
		public static bool operator >  (nuint l, nuint r) { return l.v > r.v; }
		public static bool operator <= (nuint l, nuint r) { return l.v <= r.v; }
		public static bool operator >= (nuint l, nuint r) { return l.v >= r.v; }
#endif

		public int CompareTo (nuint value) { return v.CompareTo (value.v); }
		public int CompareTo (object value) { return v.CompareTo (value); }
		public bool Equals (nuint obj) { return v.Equals (obj.v); }
		public override bool Equals (object obj) { return v.Equals (obj); }
		public override int GetHashCode () { return v.GetHashCode (); }

#if ARCH_32
		public static nuint Parse (string s, IFormatProvider provider) { return UInt32.Parse (s, provider); }
		public static nuint Parse (string s, NumberStyles style) { return UInt32.Parse (s, style); }
		public static nuint Parse (string s) { return UInt32.Parse (s); }
		public static nuint Parse (string s, NumberStyles style, IFormatProvider provider) {
			return UInt32.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nuint result)
		{
			UInt32 v;
			var r = UInt32.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nuint result)
		{
			UInt32 v;
			var r = UInt32.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#else
		public static nuint Parse (string s, IFormatProvider provider) { return UInt64.Parse (s, provider); }
		public static nuint Parse (string s, NumberStyles style) { return UInt64.Parse (s, style); }
		public static nuint Parse (string s) { return UInt64.Parse (s); }
		public static nuint Parse (string s, NumberStyles style, IFormatProvider provider) {
			return UInt64.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nuint result)
		{
			UInt64 v;
			var r = UInt64.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nuint result)
		{
			UInt64 v;
			var r = UInt64.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#endif

		public override string ToString () { return v.ToString (); }
		public string ToString (IFormatProvider provider) { return v.ToString (provider); }
		public string ToString (string format) { return v.ToString (format); }
		public string ToString (string format, IFormatProvider provider) { return v.ToString (format, provider); }

		public TypeCode GetTypeCode () { return v.GetTypeCode (); }

		bool     IConvertible.ToBoolean  (IFormatProvider provider) { return ((IConvertible)v).ToBoolean (provider); }
		byte     IConvertible.ToByte     (IFormatProvider provider) { return ((IConvertible)v).ToByte (provider); }
		char     IConvertible.ToChar     (IFormatProvider provider) { return ((IConvertible)v).ToChar (provider); }
		DateTime IConvertible.ToDateTime (IFormatProvider provider) { return ((IConvertible)v).ToDateTime (provider); }
		decimal  IConvertible.ToDecimal  (IFormatProvider provider) { return ((IConvertible)v).ToDecimal (provider); }
		double   IConvertible.ToDouble   (IFormatProvider provider) { return ((IConvertible)v).ToDouble (provider); }
		short    IConvertible.ToInt16    (IFormatProvider provider) { return ((IConvertible)v).ToInt16 (provider); }
		int      IConvertible.ToInt32    (IFormatProvider provider) { return ((IConvertible)v).ToInt32 (provider); }
		long     IConvertible.ToInt64    (IFormatProvider provider) { return ((IConvertible)v).ToInt64 (provider); }
		sbyte    IConvertible.ToSByte    (IFormatProvider provider) { return ((IConvertible)v).ToSByte (provider); }
		float    IConvertible.ToSingle   (IFormatProvider provider) { return ((IConvertible)v).ToSingle (provider); }
		ushort   IConvertible.ToUInt16   (IFormatProvider provider) { return ((IConvertible)v).ToUInt16 (provider); }
		uint     IConvertible.ToUInt32   (IFormatProvider provider) { return ((IConvertible)v).ToUInt32 (provider); }
		ulong    IConvertible.ToUInt64   (IFormatProvider provider) { return ((IConvertible)v).ToUInt64 (provider); }

		object IConvertible.ToType (Type targetType, IFormatProvider provider) {
			return ((IConvertible)v).ToType (targetType, provider);
		}
	}

	[Serializable]
	public struct nfloat : IFormattable, IConvertible, IComparable, IComparable<nfloat>, IEquatable <nfloat>
	{
		public nfloat (nfloat v) { this.v = v.v; }
		public nfloat (Single v) { this.v = v; }

#if ARCH_32
		public static readonly nfloat MaxValue = Single.MaxValue;
		public static readonly nfloat MinValue = Single.MinValue;
		public static readonly nfloat Epsilon = Single.Epsilon;
		public static readonly nfloat NaN = Single.NaN;
		public static readonly nfloat NegativeInfinity = Single.NegativeInfinity;
		public static readonly nfloat PositiveInfinity = Single.PositiveInfinity;

		Single v;

		public nfloat (Double v) { this.v = (Single)v; }
#else
		public static readonly nfloat MaxValue = Single.MaxValue;
		public static readonly nfloat MinValue = Single.MinValue;
		public static readonly nfloat Epsilon = Double.Epsilon;
		public static readonly nfloat NaN = Double.NaN;
		public static readonly nfloat NegativeInfinity = Double.NegativeInfinity;
		public static readonly nfloat PositiveInfinity = Double.PositiveInfinity;

		Double v;

		public nfloat (Double v) { this.v = v; }
#endif

#if ARCH_32
#	if NINT_JIT_OPTIMIZED
		public static implicit operator Single (nfloat v) { throw new NotImplementedException (); }
		public static implicit operator nfloat (Single v) { throw new NotImplementedException (); }
		public static implicit operator Double (nfloat v) { throw new NotImplementedException (); }
		public static explicit operator nfloat (Double v) { throw new NotImplementedException (); }
#	else
		public static implicit operator Single (nfloat v) { return v.v; }
		public static implicit operator nfloat (Single v) { return new nfloat (v); }
		public static implicit operator Double (nfloat v) { return (Double)v.v; }
		public static explicit operator nfloat (Double v) { return new nfloat (v); }
#	endif
#else
#	if NINT_JIT_OPTIMIZED
		public static explicit operator Single (nfloat v) { throw new NotImplementedException (); }
		public static implicit operator nfloat (Single v) { throw new NotImplementedException (); }
		public static implicit operator Double (nfloat v) { throw new NotImplementedException (); }
		public static implicit operator nfloat (Double v) { throw new NotImplementedException (); }
#	else
		public static explicit operator Single (nfloat v) { return (Single)v.v; }
		public static implicit operator nfloat (Single v) { return new nfloat (v); }
		public static implicit operator Double (nfloat v) { return v.v; }
		public static implicit operator nfloat (Double v) { return new nfloat (v); }
#	endif
#endif

#if NINT_JIT_OPTIMIZED
		public static nfloat operator + (nfloat v) { throw new NotImplementedException (); }
		public static nfloat operator - (nfloat v) { throw new NotImplementedException (); }
#else
		public static nfloat operator + (nfloat v) { return new nfloat (+v.v); }
		public static nfloat operator - (nfloat v) { return new nfloat (-v.v); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nfloat operator ++ (nfloat v) { throw new NotImplementedException (); }
		public static nfloat operator -- (nfloat v) { throw new NotImplementedException (); }
#else
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static nfloat operator ++ (nfloat v) { return new nfloat (v.v + 1); }
		public static nfloat operator -- (nfloat v) { return new nfloat (v.v - 1); }
#endif

#if NINT_JIT_OPTIMIZED
		public static nfloat operator + (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static nfloat operator - (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static nfloat operator * (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static nfloat operator / (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static nfloat operator % (nfloat l, nfloat r) { throw new NotImplementedException (); }
#else
		public static nfloat operator + (nfloat l, nfloat r) { return new nfloat (l.v + r.v); }
		public static nfloat operator - (nfloat l, nfloat r) { return new nfloat (l.v - r.v); }
		public static nfloat operator * (nfloat l, nfloat r) { return new nfloat (l.v * r.v); }
		public static nfloat operator / (nfloat l, nfloat r) { return new nfloat (l.v / r.v); }
		public static nfloat operator % (nfloat l, nfloat r) { return new nfloat (l.v % r.v); }
#endif

#if NINT_JIT_OPTIMIZED
		public static bool operator == (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static bool operator != (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static bool operator <  (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static bool operator >  (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static bool operator <= (nfloat l, nfloat r) { throw new NotImplementedException (); }
		public static bool operator >= (nfloat l, nfloat r) { throw new NotImplementedException (); }
#else
		public static bool operator == (nfloat l, nfloat r) { return l.v == r.v; }
		public static bool operator != (nfloat l, nfloat r) { return l.v != r.v; }
		public static bool operator <  (nfloat l, nfloat r) { return l.v < r.v; }
		public static bool operator >  (nfloat l, nfloat r) { return l.v > r.v; }
		public static bool operator <= (nfloat l, nfloat r) { return l.v <= r.v; }
		public static bool operator >= (nfloat l, nfloat r) { return l.v >= r.v; }
#endif

		public int CompareTo (nfloat value) { return v.CompareTo (value.v); }
		public int CompareTo (object value) { return v.CompareTo (value); }
		public bool Equals (nfloat obj) { return v.Equals (obj.v); }
		public override bool Equals (object obj) { return v.Equals (obj); }
		public override int GetHashCode () { return v.GetHashCode (); }

#if ARCH_32
		public static bool IsNaN              (nfloat f) { return Single.IsNaN (f); }
		public static bool IsInfinity         (nfloat f) { return Single.IsInfinity (f); }
		public static bool IsPositiveInfinity (nfloat f) { return Single.IsPositiveInfinity (f); }
		public static bool IsNegativeInfinity (nfloat f) { return Single.IsNegativeInfinity (f); }

		public static nfloat Parse (string s, IFormatProvider provider) { return Single.Parse (s, provider); }
		public static nfloat Parse (string s, NumberStyles style) { return Single.Parse (s, style); }
		public static nfloat Parse (string s) { return Single.Parse (s); }
		public static nfloat Parse (string s, NumberStyles style, IFormatProvider provider) {
			return Single.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nfloat result)
		{
			Single v;
			var r = Single.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nfloat result)
		{
			Single v;
			var r = Single.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#else
		public static bool IsNaN              (nfloat f) { return Double.IsNaN (f); }
		public static bool IsInfinity         (nfloat f) { return Double.IsInfinity (f); }
		public static bool IsPositiveInfinity (nfloat f) { return Double.IsPositiveInfinity (f); }
		public static bool IsNegativeInfinity (nfloat f) { return Double.IsNegativeInfinity (f); }

		public static nfloat Parse (string s, IFormatProvider provider) { return Double.Parse (s, provider); }
		public static nfloat Parse (string s, NumberStyles style) { return Double.Parse (s, style); }
		public static nfloat Parse (string s) { return Double.Parse (s); }
		public static nfloat Parse (string s, NumberStyles style, IFormatProvider provider) {
			return Double.Parse (s, style, provider);
		}

		public static bool TryParse (string s, out nfloat result)
		{
			Double v;
			var r = Double.TryParse (s, out v);
			result = v;
			return r;
		}

		public static bool TryParse (string s, NumberStyles style, IFormatProvider provider, out nfloat result)
		{
			Double v;
			var r = Double.TryParse (s, style, provider, out v);
			result = v;
			return r;
		}
#endif

		public override string ToString () { return v.ToString (); }
		public string ToString (IFormatProvider provider) { return v.ToString (provider); }
		public string ToString (string format) { return v.ToString (format); }
		public string ToString (string format, IFormatProvider provider) { return v.ToString (format, provider); }

		public TypeCode GetTypeCode () { return v.GetTypeCode (); }

		bool     IConvertible.ToBoolean  (IFormatProvider provider) { return ((IConvertible)v).ToBoolean (provider); }
		byte     IConvertible.ToByte     (IFormatProvider provider) { return ((IConvertible)v).ToByte (provider); }
		char     IConvertible.ToChar     (IFormatProvider provider) { return ((IConvertible)v).ToChar (provider); }
		DateTime IConvertible.ToDateTime (IFormatProvider provider) { return ((IConvertible)v).ToDateTime (provider); }
		decimal  IConvertible.ToDecimal  (IFormatProvider provider) { return ((IConvertible)v).ToDecimal (provider); }
		double   IConvertible.ToDouble   (IFormatProvider provider) { return ((IConvertible)v).ToDouble (provider); }
		short    IConvertible.ToInt16    (IFormatProvider provider) { return ((IConvertible)v).ToInt16 (provider); }
		int      IConvertible.ToInt32    (IFormatProvider provider) { return ((IConvertible)v).ToInt32 (provider); }
		long     IConvertible.ToInt64    (IFormatProvider provider) { return ((IConvertible)v).ToInt64 (provider); }
		sbyte    IConvertible.ToSByte    (IFormatProvider provider) { return ((IConvertible)v).ToSByte (provider); }
		float    IConvertible.ToSingle   (IFormatProvider provider) { return ((IConvertible)v).ToSingle (provider); }
		ushort   IConvertible.ToUInt16   (IFormatProvider provider) { return ((IConvertible)v).ToUInt16 (provider); }
		uint     IConvertible.ToUInt32   (IFormatProvider provider) { return ((IConvertible)v).ToUInt32 (provider); }
		ulong    IConvertible.ToUInt64   (IFormatProvider provider) { return ((IConvertible)v).ToUInt64 (provider); }

		object IConvertible.ToType (Type targetType, IFormatProvider provider) {
			return ((IConvertible)v).ToType (targetType, provider);
		}
	}
}