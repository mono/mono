// CS0192: A readonly field `A.a' cannot be passed ref or out (except in a constructor)
// Line: 15

class A
{
	public readonly int a;
	
	public void Inc (out int a)
	{
            a = 3;
	}
	
	public void IncCall ()
	{
		Inc (out a);
	}
}
