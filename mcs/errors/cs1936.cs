// CS1936: An implementation of `Select' query expression pattern for source type `Item' could not be found
// Line: 16


using System.Linq;

class Item
{
}

public static class Test
{
	static void Main ()
	{
		var v = new Item ();
		var foo = from a in v select a;
	}
}
