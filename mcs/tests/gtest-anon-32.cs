using System;
using System.Collections.Generic;

public class Program {

	public static void Assert (Action<int> action)
	{
		action (42);
	}

	public static void Foo<T> (IList<T> list)
	{
		Assert (i => {
			T [] backup = new T [list.Count];
		});
	}

	public static void Main (string [] args)
	{
		Foo (args);
	}
}