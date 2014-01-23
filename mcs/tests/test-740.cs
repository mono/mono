// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

class FixedTest
{
	[StructLayout (LayoutKind.Explicit)]
	public unsafe struct Value
	{
		[FieldOffset (0)]
		public void* p;
		[FieldOffset (0)]
		public double n;
		[FieldOffset (0)]
		public long i;
		[FieldOffset (0)]
		public bool b;
	}

	[StructLayout (LayoutKind.Sequential, Pack = 4)]
	public unsafe struct TValue
	{
		public Value value;

		public TValue (long x)
		{
			value = new Value ();
			value.i = x;
		}

		public override string ToString ()
		{
			return value.i.ToString ();
		}
	}

	unsafe public static int Main ()
	{
		TValue[] values = new TValue[10];
		values[0] = new TValue (0L);
		values[1] = new TValue (1000L);
		values[2] = new TValue (1L);
		Console.WriteLine ("values: {0} {1} {2}", values[0], values[1], values[2]);
		fixed (TValue* vals = values) {
			Console.WriteLine ("fixed: {0} {1} {2}", vals[0], vals[1], vals[2]);
			if (vals[0].ToString () != "0")
				return 1;

			if (vals[1].ToString() != "1000")
				return 2;

			if (vals[2].ToString() != "1")
				return 3;
		}

		Console.WriteLine ("ok");
		return 0;
	}
}
