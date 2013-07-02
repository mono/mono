// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll -r:MyAssembly02=test-externalias-01-lib.dll -r:test-externalias-01-lib.dll

extern alias MyAssembly01;
extern alias MyAssembly02;
using System;

// this is from test-externalias-01-lib.dll
using Namespace1;

public class Test
{
	public static int Main ()
	{
		// This shouldn't produce a clash
		if (MyClass1.StaticMethod () != 2)
			return 1;
		if (GlobalClass.StaticMethod () != 2)
			return 1;

		if (MyAssembly01::GlobalClass.StaticMethod () != 1)
			return 1;
		if (MyAssembly02::GlobalClass.StaticMethod () != 2)
			return 1;

		return 0;
	}
}

