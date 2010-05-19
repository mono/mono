using System;

namespace Test
{
	public interface IFoo
	{
	}

	public class Foo : IFoo
	{
	}

	public interface IBase
	{
		T Get<T> (object o);
	}

	public class TestClass : IBase
	{
		public T Get<T> (object o)
			where T : IFoo
		{
			return default (T);
		}

		T IBase.Get<T> (object o)
		{
			return default (T);
		}

		public static void Main (string[] args)
		{
			Console.WriteLine (new TestClass ().Get<Foo> (null));
		}
	}
}