using System;
using System.Runtime.InteropServices;

class T2
{
	public enum E2: sbyte
	{
		A = Test.E.d,
		B = Test.E.a,
		C = Test.Constant
	}
	
}

class Test
{
	public const UnmanagedType UnmanagedType_80 = (UnmanagedType) 80;	
	public const sbyte Constant = (sbyte)T2.E2.A;
	
	public enum E: sbyte
	{
		a = -3,
		b = d,
		c = T2.E2.B,
		d,
		e,
		f = -Constant,
		g = checked (3 * 4),
		h = unchecked ((sbyte)(250 + 10))
	}
		
	public static void Main ()
	{
		Console.WriteLine (E.d.ToString ());
		Console.WriteLine (Constant.ToString ());
		object o = E.a;
		Console.WriteLine (E.a);
		Console.WriteLine (System.Reflection.BindingFlags.NonPublic);
	}
}
