using System;

namespace A { class CAttribute : Attribute { } }
namespace B { class CAttribute : Attribute { } }

namespace Foo
{
	using A;
	using B;

	using C = A.CAttribute;

	[C]
	class Foo
	{
		static void Main ()
		{
		}
	}
}
