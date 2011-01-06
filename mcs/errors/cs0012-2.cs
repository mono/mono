// CS0012: The type `A1' is defined in an assembly that is not referenced. Consider adding a reference to assembly `CS0012-lib-missing, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
// Line: 10
// Compiler options: -r:CS0012-2-lib.dll

class Test
{
	public static void Main ()
	{
		var b = new B ();
		b.Test (null);
	}
}