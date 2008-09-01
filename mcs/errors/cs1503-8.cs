// CS1503: Argument `#1' cannot convert `method group' expression to type `IInterface'
// Line: 15

public delegate void Del ();

public interface IInterface
{
	void Do ();
}

public static class Test
{
	public static void Do (IInterface val)
	{
		Do (val.Do);
	}
}
