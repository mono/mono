// CS0229: Ambiguity between `A.N' and `B.N'
// Line: 26

using static A;
using static B;

class A
{
	public class N
	{
		public static void Foo ()
		{
		}
	}
}

class B
{
	public static int N;
}

class Test
{
	public static void Main ()
	{
		N.Foo ();
	}
}