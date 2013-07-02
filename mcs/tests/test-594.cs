using System;
using System.Reflection;

[assembly: AssemblyVersion ("2.3.*")]

public class Test
{
	public static int Main ()
	{
		var v = typeof (Test).Assembly.GetName ().Version;
		if (v.Major != 2)
			return 1;
		
		if (v.Minor != 3)
			return 2;

		if (v.Build < 1)
			return 3;
		
		if (v.Revision < 1)
			return 4;

		return 0;
	}
}
