//
// This test exhibits an old bug where we did not
// go into the underlying type for an enumeration, and
// hence implicit and explicit casts were not working when
// they were going from a type to an enum
//

namespace N1
{	
	public enum A
	{
		A_1, A_2, A_3
	}

	public class B
	{
		static bool ShortCasting ()
		{
			short i = 0;
			N1.A a = N1.A.A_1;

			i = (short) a;	//<- crash
			a = (N1.A)i;//<- used to fail, can't convert

			if (a != N1.A.A_1)
				return false;
			return true;
		}

		static bool IntCasting ()
		{
			int i = 0;
			N1.A a = N1.A.A_1;

			i = (int) a;//<- works fine
			a = (N1.A)i;//<- used to fail, can't convert

			if (a != N1.A.A_1)
				return false;
			return true;
		}
	
		public static int Main ()
		{
			if (!IntCasting ())
				return 1;
			if (!ShortCasting ())
				return 2;
			return 0;
		}

	}
}






