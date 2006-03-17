using System;

public class Foo {

	public static void Main ()
	{
		new Foo (new object[] { "foo" });
	}

	public Foo (object[] array) : this (array, array[0]) {}

	public Foo (object[] array, object context)
	{
		if (array.GetType().IsArray)
			Console.WriteLine ("ok! array is correct type");
		else
			Console.WriteLine ("boo! array is of type {0}", array.GetType ());

		if (array[0] == context)
			Console.WriteLine ("ok! array[0] == context!");
		else
			Console.WriteLine ("boo! array[0] != context!");

		foreach (char ch in "123") {
			DoSomething += delegate (object obj, EventArgs args) {
				Console.WriteLine ("{0}:{1}:{2}", ch, array[0], context);
			};
		}
	}

	public event EventHandler DoSomething;
}
