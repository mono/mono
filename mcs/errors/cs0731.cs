// CS0731: The type forwarder for type `A' in assembly `CS0731-2-lib, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' has circular dependency
// Line: 9
// Compiler options: -r:CS0731-1-lib.dll -r:CS0731-2-lib.dll

class Test
{
	static void Main ()
	{
		new A ();
	}
}