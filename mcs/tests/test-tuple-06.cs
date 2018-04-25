class Base
{
	public virtual (int, long rest) Foo ()
	{
		return (1, 2);
	}
}

class Test : Base
{
	public override (int, long rest) Foo ()
	{
		return (3, 4);
	}

	public static void Main ()
	{	
	}
}