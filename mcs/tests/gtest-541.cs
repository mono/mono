public class Foo
{
}

public class Top<S> where S : Foo
{
	public class Base<T> where T : S
	{
		public class Derived<U> where U : T
		{
			public void Test ()
			{
			}
		}
	}
}

public class Test
{
	public static int Main ()
	{
		Top<Foo>.Base<Foo>.Derived<Foo> d = new Top<Foo>.Base<Foo>.Derived<Foo> ();
		d.Test ();
		return 0;
	}
}
