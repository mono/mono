// Compiler options: -r:../class/lib/net_2_0/Mono.Cecil.dll

using System;
using Mono.Cecil;

class Test
{
	public static string A
	{
		get { return ""; }
	}

	public string B
	{
		get { return ""; }
	}

	public static int Main ()
	{
		var assembly = AssemblyDefinition.ReadAssembly (typeof (Test).Assembly.Location);
		var t = assembly.MainModule.GetType ("Test");
		foreach (var p in t.Properties)
		{
			switch (p.Name) {
			case "A":
				if (!p.HasThis)
					break;
				
				return 1;
			case "B":
				if (p.HasThis)
					break;
				
				return 2;
			default:
				return 3;
			}
		}
		
		return 0;
	}
}

