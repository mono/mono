// Compiler options: -r:MyAssembly_01=test-externalias-00-lib.dll

extern alias MyAssembly_01;
using System;

using SameNamespace = MyAssembly_01;

public class Test
{
	public static int Main ()
	{
		SameNamespace.GlobalClass.StaticMethod ();
		return 0;
	}
}

