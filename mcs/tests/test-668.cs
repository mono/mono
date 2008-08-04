#if!FOO
# if! BAR
class Bar { };
# endif
#endif

class Test {
	static void Main ()
	{
		new Bar ();
	}
}
