// CS0307: The namespace `N.M' cannot be used with type arguments
// Line: 15

namespace N
{
	namespace M
	{
	}
}

class Test
{
	static void Main ()
	{
		var a = N.M<int> ();
	}
}
