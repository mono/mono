using System;

namespace ConsoleApplication1
{
	class Program
	{
		public static void Main ()
		{
			object o = new object ();
			Inner<object>.Compare (o, o);
		}
	}

	public class Inner<T> where T : class
	{
		public static void Compare (object obj, T value)
		{
			if (obj != value) { }
		}
	}
}
