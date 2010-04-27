// CS0246: The type or namespace name `M' could not be found. Are you missing a using directive or an assembly reference?
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
