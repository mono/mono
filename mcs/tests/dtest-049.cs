public class A
{
	public class N : B.N
	{
	}
}

public class B
{
	public class N
	{
		public void Test ()
		{
		}
	}
}

class C
{
	public static void Main ()
	{
		dynamic n = new A.N ();
		n.Test ();
	}
}
