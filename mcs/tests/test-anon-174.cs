delegate int F (int i);
class Foo {
	static int i;
	static void Main ()
	{
		F f = delegate (int i) { return i; };
		i = 0;
	}
}
