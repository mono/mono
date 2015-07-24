using static A;
//using N = System.Int32;

class A
{
	public class N
	{
	}
}

class Test
{
	public static void Main ()
	{
		N n = default (N); // Am I Int32 or A.N
	}
}