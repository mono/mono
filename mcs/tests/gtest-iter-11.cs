using System;
using System.Collections;
using System.Collections.Generic;

class Foo {}
	
class Bar : Foo {
	public string Name { get; set; }
}

class Collection<T> : IEnumerable<T> where T : Foo {
	List<T> list = new List<T> ();

	public void Add (T t)
	{
		list.Add (t);
	}

	public IEnumerator<T> GetEnumerator ()
	{
		foreach (T t in list)
			yield return t;
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return GetEnumerator ();
	}
}

class BarCollection : Collection<Bar> {}

class Program {

	public static int Main ()
	{
		var collection = new BarCollection () {
			new Bar { Name = "a" },
			new Bar { Name = "b" },
			new Bar { Name = "c" },
		};

		foreach (var bar in collection)
			Console.WriteLine (bar.Name);
		
		return 0;
	}
}
