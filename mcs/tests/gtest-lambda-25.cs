using System;

namespace MonoBugs
{
	public struct Foo<T>
	{
		public T Item;
	}

	public static class Bar
	{
		public static void DoStuff<T> (T item, Action<T> fn)
		{
			throw new ApplicationException ("failed");
		}

		public static void DoStuff<T> (T? item, Action<T> fn)
			where T : struct
		{
			fn (item.Value);
		}
	}

	public static class Program
	{
		public static void Main ()
		{
			Foo<int>? value = new Foo<int> { Item = 3 };
			Bar.DoStuff (value, x => Console.WriteLine (x.Item));
		}
	}
}
