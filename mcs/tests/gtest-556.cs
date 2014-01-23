// Compiler options: -r:gtest-556-lib.dll

using System;

class A2
{
	public class N<T>
	{
		public static N<T> Method ()
		{
			return default (N<T>);
		}
	}
}

class Test
{
	public static int Main ()
	{
		A2.N<short> b1 = A2.N<short>.Method ();
		A.N<byte> b2 = A.N<byte>.Method ();

		return 0;
	}
}
