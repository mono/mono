// CS0576: Namespace `global::' contains a definition with same name as alias `A'
// Line: 12

using A = System;

namespace A.Foo
{
	class X
	{
		public static void Main ()
		{
			A.GG ();
		}
	}
}