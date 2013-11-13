// CS0104: `X' is an ambiguous reference between `A.X' and `B.X'
// Line: 25

namespace A
{
	class X { }
}

namespace B
{
	class X { }
}

namespace C
{
	using System;
	using A;
	using B;

	class Test 
	{
		static void Main ()
		{
			Foo (delegate {
				X x;
			});
		}
		
		static void Foo (Action a)
		{
		}
	}
}
