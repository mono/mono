// Compiler options: -warn:4 -define:A1
// This test should print only: #warning: `A1'

#if A1
# warning A1
#elif A2
# error A2
# if B2
# error A1->B2
# define A1B2
# else
# error A2->else
# endif
#else
# error else
#endif

#if E1
	#error E1
#elif E2
	#error E2
#else
	public class C
#endif
	{
		public static void Main () {}
	}