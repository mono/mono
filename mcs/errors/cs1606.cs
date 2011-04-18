// CS1606: Could not sign the assembly. ECMA key can only be used to delay-sign assemblies
// Line: 0

using System.Reflection;

[assembly: AssemblyKeyFile ("cs1606.pub")]

// note that cs1606.pub is the ECMA key (same as mcs/class/ecma.pub)
// this is the same as cs1548-3.cs (Missing private key in strongname file)
// expect the the error code is different for the ECMA key

class MyClass {

	public static void Main (string [] args)
	{
	}
}
