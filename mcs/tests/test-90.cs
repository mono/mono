//
// This test just makes sure that we can compile C.A, there used to be
// a bug in the compiler that was doing the lookups in the wrong namespace
//
// 
namespace N1
{	
	public enum A
	{
		A_1, A_2, A_3
	}

	public interface B
	{
		N1.A myProp 
		{
			get;
			set;   // <-- This always worked.
		}
	}

	public interface C
	{
		A myProp
		{
			get;
			set;  // <-- This used to fail.
		}
	}

	public class Blah {
		public static int Main  ()
		{
			return 0;
		}
	}
}
