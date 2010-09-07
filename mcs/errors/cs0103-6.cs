// CS0103: The name `owner' does not exist in the current context
// Line: 6

class C
{
	public readonly object A = owner.Foo;

	public C ()
	{
		int owner = 1;
	}
}
