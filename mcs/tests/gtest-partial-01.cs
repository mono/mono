using System;

class B<U>
{
}

partial class C<T> : B<T>
{
	T t1;
}

partial class C<T> : B<T>
{
	T t2;
}

class Test
{
	public static void Main ()
	{
	}
}
