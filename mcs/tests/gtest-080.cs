public interface IFoo<X>
{ }

public class Test
{
	public void Hello<T> (IFoo<T> foo)
	{
		InsertAll (foo);
	}

	public void InsertAll<U> (IFoo<U> foo)
	{ }
}

class X
{
	public static void Main ()
	{ }
}
