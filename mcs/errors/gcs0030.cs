// CS0030: Cannot convert type `T' to `X'
// Line: 8
class Foo<T>
	where T : System.ICloneable
{
	public X Test (T t)
	{
		return (X) t;
	}
}

class X
{
	static void Main ()
	{ }
}
