namespace N
{
	public class TestG
	{
		public static void Foo<T> ()
		{
		}
	}
}

class NonGeneric { }
class Generic<T> { }

class m
{
	public global::NonGeneric compiles_fine (global::NonGeneric i, out global::NonGeneric o)
	{
		o = new global::NonGeneric ();
		return new global::NonGeneric ();
	}

	public global::Generic<int> does_not_compile (global::Generic<int> i, out global::Generic<int> o)
	{
		o = new global::Generic<int> ();
		return new global::Generic<int> ();
	}

	public static void Main ()
	{
		global::N.TestG.Foo<int> ();
	}
}
