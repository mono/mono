// testcase from #58290

delegate void Foo ();
class A {
	public event Foo Bar;

	public static void m1 () { }
 
	public static void Main ()
	{
		A a = new A();
		a.Bar += new Foo (m1);
		a.Bar -= new Foo (m1);
		System.Diagnostics.Debug.Assert (a.Bar == null);
	}
}

