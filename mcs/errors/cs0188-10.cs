// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 10

struct S
{
	public int x;
	
	S (dynamic d)
	{
		Foo (d);
		x = 44;
	}

	void Foo (int a)
	{
	}
}
