class X
{
	public static void Main ()
	{
		A a = new A ();
		var x = (a.b?.c?.d as A)?.b;
	}
}

class A
{
	public B b { get; set; }
}

class B
{
	public C c { get; set; }
}

class C
{
	public D d { get; set; }
}

class D : A
{

}