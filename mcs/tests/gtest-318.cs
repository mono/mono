// Bug #77963
public class Foo<K>
{
}

public class Bar<Q> : Goo<Q>
{
        public class Baz
        {
        }
}

public class Goo<Q> : Foo<Bar<Q>.Baz>
{
}

class X
{
	public static void Main ()
	{
		Bar<int> bar = new Bar<int> ();
		System.Console.WriteLine (bar);
	}
}
