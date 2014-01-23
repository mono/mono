// Compiler options: -r:MyAssembly01=test-externalias-00-lib.dll -r:MyAssembly02=test-externalias-01-lib.dll

extern alias MyAssembly01;
extern alias MyAssembly02;
using System;

// It must be possible to define a ns with the same name
// of the aliases
namespace MyAssembly01
{
	public class Test
	{
	}

	namespace Namespace1
	{
		public class Test
		{
		}
	}
}

namespace MyAssembly02
{
	public class Test
	{
	}

	namespace Namespace1
	{
		public class Test
		{
		}
	}
}

public class Test
{
	public static void Main ()
	{
	}
}

