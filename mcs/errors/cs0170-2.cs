// CS0170: Use of possibly unassigned field `c'
// Line: 11

struct A
{
	private long b;
	private float c;

	public A (int foo)
	{
		b = (long) c;
		c = 1;
	}
}