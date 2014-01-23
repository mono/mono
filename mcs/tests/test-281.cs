namespace Foo
{
	public class Hello
	{
		public static int World = 8;
	}
}

namespace Bar
{
	public class Hello
	{
		public static int World = 9;
	}
}

namespace Test
{
	using Foo;

	public class Test1
	{
		public static int World ()
		{
			return Hello.World;
		}
	}
}

namespace Test
{
	using Bar;

	public class Test2
	{
		public static int World ()
		{
			return Hello.World;
		}
	}
}

class X
{
	public static int Main ()
	{
		if (Test.Test1.World () != 8)
			return 1;
		if (Test.Test2.World () != 9)
			return 2;
		return 0;
	}
}
