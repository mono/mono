// CS0029: Cannot implicitly convert type `B [cs0029-26, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null -- *PATH*/cs0029-26.cs]' to `B [CS0029-26-lib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3 -- *PATH*/CS0029-26-lib.dll]'
// Line: 16
// Compiler options: -r:R1=CS0029-26-lib.dll

extern alias R1;

public class B
{
}

public class C
{
	public static void Main ()
	{
		B b1 = null;
		R1::B b2 = b1;
	}
}
