// cs0283-2.cs: The type `S' cannot be declared const
// Line: 12

struct S
{
}

class C
{
	public void Foo ()
	{
		const S s = new S();
	}
}