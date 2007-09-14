namespace N
{
	partial class Foo
	{
	}
}

namespace N
{
	using System;

	partial class Foo
	{
		public Foo ()
		{
			Console.Write ("Hello, world.\n");
		}
		public static void Main ()
		{
		}
	}
}