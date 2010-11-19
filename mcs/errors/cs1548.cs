// CS1548: Error during assembly signing. The specified key file `file_not_found.snk' does not exist
// Line: 0

using System.Reflection;

[assembly: AssemblyKeyFile ("file_not_found.snk")]

class MyClass {

	public static void Main (string [] args)
	{
	}
}
