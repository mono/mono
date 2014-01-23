// CS0246: The type or namespace name `M' could not be found. Are you missing an assembly reference?
// Line: 11

interface I<T>
{
	void G<TT> ();
}

class C
{
	void I<M>.G<M> ()
	{
	}
	
	public static void Main ()
	{
	}
}
