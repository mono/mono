delegate int Foo<in T> (T t);

public class Test
{
	static int Main ()
	{
		string message = "Hello World!";
		Foo<object> foo = (o) => o.GetHashCode ();
		if (Bar (foo, message) != message.GetHashCode ())
			return 1;

		return 0;
	}

	static int Bar (Foo<string> foo, string s)
	{
		return foo(s);
	}
}
