// cs1502.cs: The method has incorrect arguments, passing out to something expecting ref
// Line: 8
class X {
	public void foo (ref int blah) {}

	public void bar (ref int baz)
	{
		foo(out baz);
	}

	static void Main ()
	{
	}
}
