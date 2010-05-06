public struct SchemaEntry<T>
{
	public static SchemaEntry<T> Zero;
}

public class C
{
	public static void Main ()
	{
		new SchemaEntry<short> ();
	}
}
