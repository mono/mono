// CS0445: Cannot modify the result of an unboxing conversion
// Line: 8

struct S
{
	public void Do (object o)
	{
		((S) o)[1] = 4;
	}

	int this[int arg] { set { } }
}

