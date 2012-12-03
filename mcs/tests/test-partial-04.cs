// Compiler options: -langversion:default

namespace A
{
	interface IFoo
	{
		void Hello (IFoo foo);
	}
}

namespace B
{
	partial class Test
	{ }
}

namespace B
{
	using A;

	partial class Test : IFoo
	{
		void IFoo.Hello (IFoo foo)
		{ }
	}
}

class X
{
	public static void Main ()
	{ }
}
