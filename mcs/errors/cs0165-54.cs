// CS0165: Use of unassigned local variable `res'
// Line: 23

class A
{
	public B b;
}

class B
{
	public void Foo (int arg)
	{
	}
}

class X
{
	public static void Main ()
	{
		A a = null;
		int res;
		a?.b.Foo(res = 3);
		System.Console.WriteLine (res);
	}
}