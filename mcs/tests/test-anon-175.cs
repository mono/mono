delegate int F (int i);
class Foo {
	static int i;
	static void Main ()
	{
		i = 0;
		F f = delegate (int i) { return i; };
	}
}
