abstract class A
{
	public virtual void M (params B[] b)
	{
	}
}

class B : A
{
	public override void M (B[] b2)
	{
	}
}

class Test2
{
	public static void Main()
	{
		B b = new B();
		A a = b;
		a.M (b, b);
		b.M (b, b, b);
	}
}