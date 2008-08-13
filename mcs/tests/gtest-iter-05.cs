using System;
using System.Collections.Generic;

public class Test
{
	public static void Main ()
	{
		Foo<int> nav = new Foo<int> ();
		IEnumerable<int> t = TestRoutine<int> (new int [] { 1 }, nav);
		new List<int> (t);
	}

	public static IEnumerable<T> TestRoutine<T> (IEnumerable<T> a, Foo<T> f)
	{
		f.CreateItem<int> ();
		foreach (T n in a) {
			yield return n;
		}
	}

}
public class Foo<T>
{
	public void CreateItem<G> ()
	{
	}
}
