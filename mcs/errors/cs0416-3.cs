// CS0416: `C<T>.N': an attribute argument cannot use type parameters
// Line: 17

using System;

public class TestAttribute : Attribute
{
	public TestAttribute(Type type)
	{
	}
}

class C<T>
{
	class N
	{
		[Test(typeof(N))]
		public static void Foo()
		{
		}
	}
}
