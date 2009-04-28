// Compiler options: -unsafe

public unsafe struct A
{
	public B* pB;
}

public unsafe struct B
{
	public A* pA;
}

public class C
{
	public static void Main ()
	{
	}
}