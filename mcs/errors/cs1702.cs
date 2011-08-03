// CS1702: Assuming assembly reference `CS1702-lib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3' matches assembly `CS1702-lib, Version=1.0.1.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3'. You may need to supply runtime policy
// Line: 0
// Compiler options: -warnaserror -r:CS1702-lib.dll -r:dlls/second/CS1702-lib.dll

class C
{
	public static void Main ()
	{
		A.Test (new B ());
	}
}
