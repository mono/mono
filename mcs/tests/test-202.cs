using X;
namespace X
{
	public class X
	{ }
}

namespace A.B.C
{
	public class D
	{ }
}

class Test
{
	public static int Main ()
	{
		A.B.C.D d = new A.B.C.D ();
		X.X x = new X.X ();
		return 0;
	}
}
