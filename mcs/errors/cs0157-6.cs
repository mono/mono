// CS0157: Control cannot leave the body of a finally clause
// Line: 10

class Foo {
	static void Main () { int i; foo (out i); }
	static void foo (out int i)
	{
		try {}
		finally {
			return;
		}
	}
}
