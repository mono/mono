// Bug #77963
public class Foo<K>
{
}

public class Bar<Q> : Foo<Bar<Q>.Baz>
{
        public class Baz
        {
        }
}

class X
{
	public static void Main ()
	{
		Bar<int> bar = new Bar<int> ();
		System.Console.WriteLine (bar);
	}
}
