class A<T>
{
	public class Context<U>
	{
		public delegate void D (T instance);
		public delegate void D2<V> ();
	}

	public class Constructor
	{
		public class Nested
		{
			public static void Test<U> (Context<U>.D d)
			{
				var c = new Constructor ();
				c.Before (d);
			}

			public static void Test<U, V> (Context<U>.D2<V> d)
			{
				var c = new Constructor ();
				c.Before (d);
			}
		}

		public void Before<U> (Context<U>.D d)
		{
		}

		public void Before<U, V> (Context<U>.D2<V> d)
		{
		}
	}
}

class C
{
	public static int Main ()
	{
		A<int>.Context<bool>.D d = null;
		A<int>.Constructor.Nested.Test (d);

		A<int>.Context<bool>.D2<string> d2 = null;
		A<int>.Constructor.Nested.Test (d2);

		return 0;
	}
}