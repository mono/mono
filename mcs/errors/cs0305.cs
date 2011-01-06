// CS0305: Using the generic type `N1.A<T>' requires `1' type argument(s)
// Line: 12
namespace N1
{
	class A<T>
	{
	}
}

namespace N3
{
	using W = N1.A;

	class X
	{
		static void Main ()
		{
		}
	}
}
