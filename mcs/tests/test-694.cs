// Compiler options: -d:X

#undef X

#if X
private // must be ignored
#else
public
#endif
class Test
{
	public static int Main ()
	{
		return 0;
	}
}
