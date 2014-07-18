using System.Reflection;
using System.Diagnostics;
using System;

[assembly: AssemblyVersion ("2011.04.0.0")]
[assembly: AssemblyFileVersion ("2011.02.0.0")]

class X
{
	public static int Main ()
	{
		Assembly executingAssembly = Assembly.GetAssembly (typeof(X));
		FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (executingAssembly.Location);
		if (fvi.FileVersion != "2011.02.0.0")
			return 1;

		return 0;
	}
}