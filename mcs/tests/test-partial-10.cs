// Compiler options: -langversion:default

using System;

namespace Test2
{
	public interface Base
	{ }

	public partial class Foo : Base
	{
		public static int f = 10;
	}

	public partial class Foo : Base
	{
		public static int f2 = 9;		
	}
}

namespace Test3
{
	public interface Base
	{ }

	public partial struct Foo : Base
	{
		public static int f = 10;
	}

	public partial struct Foo : Base
	{
		public static int f2 = 9;
	}
}

class X
{
	public static int Main ()
	{
		if (Test2.Foo.f != 10)
			return 1;
		
		if (Test2.Foo.f2 != 9)
			return 1;
		
		if (Test3.Foo.f != 10)
			return 1;

		if (Test3.Foo.f2 != 9)
			return 1;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
