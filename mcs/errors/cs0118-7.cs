// CS0118: `Martin.Test' is a `namespace' but a `type' was expected
// Line: 15
using System;
using Foo;

namespace Foo
{
	public class Test : Attribute
	{
	}
}

namespace Martin.Test
{
	[Test]
	public class X
	{
	}
}
