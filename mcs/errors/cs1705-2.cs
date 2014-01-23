// CS1705: Assembly `CS1705-lib, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' references `CS1705-lib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3' which has a higher version number than imported assembly `CS1705-lib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=36f3ae7e947792e3'
// Line: 0
// Compiler options: -r:CS1705-lib.dll -r:dlls/second/CS1705-lib.dll -keyfile:key.snk

class C
{
	public static void Main ()
	{
		A.Test (new B ());
	}
}
