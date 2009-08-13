using System;

class B<U>
{
}

partial class C<T> : B<T>
{
}

partial class C<T> : B<T>
{
}

class Test
{
	public static void Main ()
	{
	}
}
