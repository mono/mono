// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll -r:MyAssembly02=test-externalias-01-lib.dll

extern alias MyAssembly01;
extern alias MyAssembly02;
using System;

public class Test
{
	public static int Main ()
	{
		if (MyAssembly01::GlobalClass.StaticMethod () != 1)
			return 1;
		if (MyAssembly02::GlobalClass.StaticMethod () != 2)
			return 1;

		if (new MyAssembly01::GlobalClass ().InstanceMethod () != 1)
			return 1;
		if (new MyAssembly02::GlobalClass ().InstanceMethod () != 2)
			return 1;

		return 0;
	}
}

