// CS1701: Assuming assembly reference `CS1701-lib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3' matches assembly `CS1701-lib, Version=2.0.1.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3'. You may need to supply runtime policy
// Line: 9
// Compiler options: -warnaserror -r:CS1701-lib.dll -r:dlls/second/CS1701-lib.dll

class C
{
	public static void Main ()
	{
		A.Test (new B ());
	}
}
