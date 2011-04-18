// CS1502: The best overloaded method match for `X.foo(ref int)' has some invalid arguments
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
