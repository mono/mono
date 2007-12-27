class A<T>
{
	void Foo (B<T>.E arg)
	{
	}
}

class B<U> : B
{
}

class B
{
	public class E
	{
	}
}

class C
{
	public static void Main ()
	{
	}
}

