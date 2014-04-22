// CS0170: Use of possibly unassigned field `c'
// Line: 11

struct A
{
	public long b;
	public float c;

	public A (int foo)
	{
		b = (long) c;
		c = 1;
	}
}