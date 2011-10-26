// CS0146: Circular base class dependency involving `Generic<P>.Status' and `Generic<P>.Status'
// Line: 6

public class Generic<P>
{
	public class Status : Status
	{
		Status (Foo foo) : base (foo)
		{
		}
	}
}
