using System;
using System.Collections.Generic;

class Repro
{
	class Runner<T>
	{
		public Runner (Action<T> action, T t) { }
	}

	static void AssertFoo<T> (IList<T> list)
	{
		new Runner<int> (delegate {
			foreach (T item in list) { }
		}, 42);
	}

	public static void Main ()
	{
	}
}
