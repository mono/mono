using System;

class Item
{
	internal static object Field = "Test";
}

class Caller
{
	public string this[string x]
	{
		get { return x; }
	}

	public int this[int x]
	{
		get { return x; }
	}

	public void Foo ()
	{
		var v = Item.Field.ToString ();
	}

	public static void Main ()
	{
	}
}
