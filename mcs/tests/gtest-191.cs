using System;
using System.Collections.Generic;

namespace Test1
{
	public static class Test
	{
		public static IEnumerable<T> Replace<T> ()
		{
			yield break;
		}
	}
}

namespace Test2
{
	public class Test<S>
	{
		public static IEnumerable<T> Replace<T> ()
		{
			yield break;
		}
	}
}

namespace Test3
{
	public class Test<S>
	{
		public static IEnumerable<KeyValuePair<S,T>> Replace<T> (IEnumerable<T> a,
									 IEnumerable<S> b)
		{
			yield break;
		}
	}
}

namespace Test4
{
	public class Test
	{
		public static IEnumerable<T> Replace<T> ()
			where T : class
		{
			yield break;
		}
	}
}

namespace Test5
{
	public class Test
	{
		public static IEnumerable<T> Replace<T> (T t)
		{
			yield return t;
		}
	}
}

namespace Test6
{
	public class Test
	{
		public static IEnumerable<T> Replace<T> (T t)
		{
			T u = t;
			yield return u;
		}
	}
}

namespace Test7
{
	public class Test
	{
		public static IEnumerable<T[]> Replace<T> (T[] t)
		{
			T[] array = t;
			yield return array;
		}
	}
}

class X
{
	public static void Main ()
	{ }
}
