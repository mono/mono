class Item
{
}

class ItemCollection<T> where T : Item
{
	public void Bind<U> (ItemCollection<U> sub) where U : T
	{
	}
}

class a
{
	public static void Main ()
	{
		var ic = new ItemCollection<Item> ();
		ic.Bind (ic);
	}
}
