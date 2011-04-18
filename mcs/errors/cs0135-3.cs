// CS0135: `i' conflicts with a declaration in a child block
// Line: 9

delegate int F (int i);
class Foo {
	static int i;
	static void Main ()
	{
		i = 0;
		F f = delegate (int i) { return i; };
	}
}
