// testcase from #58290

delegate void Foo ();
class A {
	public event Foo Bar;

	public static void m1 () { }
 
	public static int Main ()
	{
		A a = new A();
		a.Bar += new Foo (m1);
		a.Bar -= new Foo (m1);
		return (a.Bar == null) ? 0 : 1;
	}
}

