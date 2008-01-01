using System;
using System.Reflection;

[assembly: AssemblyVersion ("7")]

class Program
{
	static int Main ()
	{
		Assembly a = Assembly.GetExecutingAssembly ();
		if (a.GetName ().Version != new Version (7, 0, 0, 0))
			return 1;
		return 0;
	}
}
