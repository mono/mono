// cs1548: Error while signing the assembly (file not found)
// Line: 6

using System.Reflection;

[assembly: AssemblyKeyFile ("file_not_found.snk")]

class MyClass {

	public static void Main (string [] args)
	{
	}
}
