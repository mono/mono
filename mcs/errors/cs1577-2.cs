// CS1577: Referenced assembly `CS1577-lib, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' does not have a strong name
// Line: 0
// Compiler options: -r:CS1577-lib.dll

[assembly: System.Reflection.AssemblyKeyFileAttribute ("cs1577.snk")]

class Test
{
	static void Main ()
	{
		C c = new C ();
	}
}
