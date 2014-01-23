// Compiler options: -r:gtest-047-lib.dll

// Important test: verify our namespace lookup rules
//
// There's a generic and a non-generic `List' type in two
// different namespaces: make sure we pick the correct one.

using Foo;
using Bar;

class X
{
	public static void Main ()
	{
		List<int> list = new List<int> ();
	}
}
