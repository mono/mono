namespace B
{
	using C;

	partial class Test <T>
		where T : IA, IB
	{ }
}

namespace B
{
	partial class Test <T>
		where T : C.IB, C.IA
	{ }
}

namespace B
{
	partial class Test <T>
	{ }
}

class X
{
	public static void Main ()
	{ }
}

namespace C {
	interface IA { }
	interface IB { }
}
