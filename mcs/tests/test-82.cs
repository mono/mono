//
// Test to ensure that we correctly perform type lookups - thanks to Felix A.I
//
namespace N1
{	
	public enum A
	{
		A_1, A_2, A_3
	}

	namespace N2
	{	
		public class B
		{
			A member;

			void Method (ref A a)
			{
			}

			public static int Main ()
			{
				return 0;
			}
		}

	}
}

namespace N1.N3
{	
	public class B
	{
		A member;

		void Method (ref A a)
		{
		}
	}
}
