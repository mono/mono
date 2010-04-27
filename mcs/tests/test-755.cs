class Item
{
	public static int Foo = 42;

	public class Builder
	{
		public int Foo
		{
			get { return Item.Foo; }
		}

		public object this[int field]
		{
			get { return null; }
		}

		public object this[int field, int i]
		{
			get { return null; }
		}
	}
}

class Program
{
	static void Main ()
	{
	}
}