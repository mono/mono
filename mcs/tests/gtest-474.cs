class A<X>
{
	public const A<int> Value = B<int>.Value;
}

class B<T>
{
	public const A<T> Value = default (A<T>);
}

class C
{
	public static void Main ()
	{
		new B<int> ();
	}
}
