// Compiler options: -langversion:default

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

namespace X
{
	using Foo;

	public partial class Test
	{
		public static int FooWorld ()
		{
			return Hello.World;
		}
	}
}

namespace X
{
	using Bar;

	public partial class Test
	{
		public static int BarWorld ()
		{
			return Hello.World;
		}
	}
}

class Y
{
	public static int Main ()
	{
		if (X.Test.FooWorld () != 8)
			return 1;
		if (X.Test.BarWorld () != 9)
			return 2;
		return 0;
	}
}
