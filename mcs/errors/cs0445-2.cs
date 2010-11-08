// CS0445: Cannot modify the result of an unboxing conversion
// Line: 10

struct S
{
	public int val { set {} }

	public void Do (object o) 
	{
		((S)o).val = 4;
	}
}

