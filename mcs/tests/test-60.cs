//
// Really lame test, but it should be fine for now
//
#if (X)
#endif

#if YY
#errro this should not be printed
#if X
#elif Y
#else
#endif
#else
class X {
	static int Main ()
	{
		return 0;
	}
}
#endif
