// CS0012: The type `A.Nested' is defined in an assembly that is not referenced. Consider adding a reference to assembly `CS0012-lib-missing, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
// Line: 9
// Compiler options: -r:CS0012-11-lib.dll

class Test
{
	public static void Main ()
	{
		new B ().Test ();
	}
}
