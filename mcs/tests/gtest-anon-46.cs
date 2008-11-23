using System;

public class TheClass
{
	static void Foo<T> (T t, Func<T, T> f)
	{
		Func<Func<T>> d = () => {
			if (t != null) {
				return () => f (t);
			}
			
			return null;
		};
		d ();
	}
	
	public static void Main ()
	{
		Foo (1, null);
	}
}
