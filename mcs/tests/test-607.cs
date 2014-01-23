using System;
using System.Reflection;
using System.Configuration.Assemblies;

[assembly: AssemblyVersion ("7.0")]
[assembly: AssemblyAlgorithmId (AssemblyHashAlgorithm.MD5)]
[assembly: AssemblyFlagsAttribute(AssemblyNameFlags.EnableJITcompileOptimizer | AssemblyNameFlags.Retargetable)]

class Program
{
	public static int Main ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		var an = a.GetName ();
		Console.WriteLine (an.Version);
		if (a.GetName ().Version != new Version (7, 0, 0, 0))
			return 1;
		
		if (an.HashAlgorithm != AssemblyHashAlgorithm.MD5)
			return 2;
		
		Console.WriteLine (an.Flags);
		if (an.Flags != (AssemblyNameFlags.PublicKey | AssemblyNameFlags.EnableJITcompileOptimizer | AssemblyNameFlags.Retargetable))
			return 3;
		
		Console.WriteLine ("ok");
		return 0;
	}
}
