using System;
using System.Reflection;

[assembly: AssemblyKeyFileAttribute ("test-715.snk")]
[assembly: AssemblyFlags (AssemblyNameFlags.EnableJITcompileOptimizer | AssemblyNameFlags.Retargetable)]

class MyClass
{
	public static int Main ()
	{
		Assembly thisAsm = Assembly.GetExecutingAssembly ();
		AssemblyName name = thisAsm.GetName (false);

		if (name.Flags != (AssemblyNameFlags.EnableJITcompileOptimizer | AssemblyNameFlags.Retargetable | AssemblyNameFlags.PublicKey))
			return 1;

		byte[] key = name.GetPublicKey ();
		if (key.Length != 160)
			return 2;

		if (key [56] != 170)
			return 3;

		if (name.HashAlgorithm != System.Configuration.Assemblies.AssemblyHashAlgorithm.SHA1)
			return 4;

//		if (name.ProcessorArchitecture != ProcessorArchitecture.MSIL)
//			return 5;

		if (name.VersionCompatibility != System.Configuration.Assemblies.AssemblyVersionCompatibility.SameMachine)
			return 6;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
