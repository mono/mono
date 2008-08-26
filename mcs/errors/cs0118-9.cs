// CS0118: `System' is a `namespace' but a `type' was expected
// Line: 9

public class C
{
	public object Test (object a)
	{
		return (System)(a);
	}
}
