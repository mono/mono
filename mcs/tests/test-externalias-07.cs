// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll

using System;

namespace NS
{
	extern alias MyAssembly01;
	
	public class MyClass
	{
		public static int GetInt ()
		{
			return MyAssembly01::GlobalClass.StaticMethod ();
		}
	}
}

public class Test
{
	public static int Main ()
	{
		if (NS.MyClass.GetInt () != 1)
			return 1;
		
		return 0;
	}
}

