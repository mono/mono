// CS0308: The non-generic type `N1.A' cannot be used with the type arguments
// Line: 11
namespace N1
{
	class A
	{ }
}

namespace N3
{
	using W = N1.A<int>;

	class X
	{
		static void Main ()
		{
		}
	}
}
