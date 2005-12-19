// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll

extern alias MyAssembly01;
using System;

using SameNamespace = MyAssembly01;

public class Test
{
	static int Main ()
	{
		SameNamespace.GlobalClass.StaticMethod ();
		return 0;
	}
}

