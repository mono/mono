//
// Check whether we're correctly handling `this'.
//
//

public class Foo<T,U>
{
	public void Hello (Foo<T,U> foo)
	{
	}

	public virtual void Test ()
	{
		//
		// Again, this must be encoded as a TypeSpec (Foo<!0,!1>)
		// instead of a TypeDef.
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
	public static void Main ()
	{ }
}
