// cs1548.cs: Error during assembly signing. The speficied file `file_not_found.snk' does not exist
// Line: 6

using System.Reflection;

[assembly: AssemblyKeyFile ("file_not_found.snk")]

class MyClass {

	public static void Main (string [] args)
	{
	}
}
