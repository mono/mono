// CS0117: `A' does not contain a definition for `Method'
// Line: 8

public class E : A
{
	void Test ()
	{
		base.Method ();
	}
}

static class S
{
	public static void Method (this A a)
	{
	}
}

public class A
{
}