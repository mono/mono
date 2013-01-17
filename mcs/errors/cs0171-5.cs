// CS0171: Field `Test.v2' must be fully assigned before control leaves the constructor
// Line: 28

public struct V
{
	public int x;
}

struct Test
{
	public V v1;
	public V v2;

	public Test (int mm)
	{
		v1 = new V ();
	}

	public static void Main ()
	{
	}
}
