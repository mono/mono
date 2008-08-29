// Compiler options: -r:test-675-lib.dll

using System;

public class B : A
{
	public override int GetHashCode ()
	{
		return 1;
	}
	
	public override bool Equals (object o)
	{
		return true;
	}
	
	public static bool operator == (B u1, B u2)
	{
		return true;
	}

	public static bool operator != (B u1, B u2)
	{
		return false;
	}
}

public class Test
{
	public static int Main ()
	{
		return 0;
	}
}
