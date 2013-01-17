// 
// Fixed, incorporated into test
//
using System;

using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Explicit)]
struct A {
	[FieldOffset (0)]
	public int a;
	[FieldOffset (0)]
	public byte b1;
	[FieldOffset (1)]
	public byte b2;
	[FieldOffset (2)]
	public byte b3;
	[FieldOffset (3)]
	public byte b4;
}

class X {
	public static void Main ()
	{
		A a = new A ();

		a.a = 0x12345678;

		Console.WriteLine ("b1: " + a.b1);
		Console.WriteLine ("b2: " + a.b2);
		Console.WriteLine ("b3: " + a.b3);
		Console.WriteLine ("b4: " + a.b4);
		
	}
}
	
