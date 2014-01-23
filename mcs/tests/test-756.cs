// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

class UnsafeTest
{
	[StructLayout (LayoutKind.Sequential)]
	public unsafe struct Foo
	{
		public Bar* bar;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct Bar
	{
		public Foo foo;
	}

	unsafe public static void Main ()
	{
		Console.WriteLine (sizeof (Foo));
	}
}
