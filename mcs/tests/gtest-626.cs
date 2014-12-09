using System;

class Program
{
	public class Foo
	{
		public static bool MG (Foo t)
		{
			return false;
		}
	}

	public class Bar<T>
	{
		public static Bar<T> Create (Func<T, bool> a)
		{
			return null;
		}

		public static Bar<T> Create (Func<T, double> a, Func<T, bool> b = null)
		{
			throw new ApplicationException ();
		}
	}

	static void Main ()
	{
		var x = Bar<Foo>.Create (Foo.MG);
	}
}
