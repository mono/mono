using static A;
using static B;

class A
{
	public class TestMe
	{
	}

	public static int TestMe1 ()
	{
		return 0;
	}
}

class B
{
	public static int TestMe2 ()
	{
		return 0;
	}

	public class TestMe1
	{
	}
}

class C
{
	public static void Main ()
	{
		new TestMe1 ();
	}
}