#if!FOO
# if! BAR
class Bar { };
# endif
#endif

class Test {
	public static void Main ()
	{
		new Bar ();
	}
}
