using System;

struct S
{
}

class C<U>
{
	static void Foo<T> (T value) where T : U
	{
	}

	public static void Test (S? s)
	{
		C<S?>.Foo (s);
		C<ValueType>.Foo (s);
		C<object>.Foo (s);
	}
}

class M
{
	public static void Main ()
	{
		C<int>.Test (null);
	}
}