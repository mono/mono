delegate T Foo<out T> ();

public class Test
{
	public static int Main ()
	{
		string message = "Hello World!";
		Foo<string> foo = () => message;
		if (Bar (foo) != message.GetHashCode ())
			return 1;

		return 0;
	}

	static int Bar (Foo<object> foo)
	{
		return foo().GetHashCode ();
	}
}
