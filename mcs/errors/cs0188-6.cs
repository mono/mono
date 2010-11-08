// CS0188: The `this' object cannot be used before all of its fields are assigned to
// Line: 10

struct B
{
	public int a;

	public B (int foo)
	{
		Test (this);
		a = 1;
	}

	static void Test (B b)
	{
	}
}