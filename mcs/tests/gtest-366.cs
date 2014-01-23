
public struct MyType
{
	int value;

	public MyType (int value)
	{
		this.value = value;
	}

	public static implicit operator int (MyType o)
	{
		return o.value;
	}
}

class Tester
{
	static void Assert<T> (T expected, T value)
	{
	}
	
	public static void Main ()
	{
		Assert (10, new MyType (10));
	}
}