public class Foo<T,U>
{
	public void Hello (Foo<T,U> foo)
	{
	}

	public virtual void Test ()
	{
		Hello (this);
	}
}

public class Bar<T> : Foo<T,long>
{
	public void Test (Foo<T,long> foo)
	{
		Hello (foo);
	}
}

public class Baz<T> : Foo<T,string>
{
	public override void Test ()
	{
		Hello (this);
	}
}

class X
{
	static void Main ()
	{ }
}
