// CS0140: The label `L1' is a duplicate
// Line: 11 

namespace CS0140Test
{
	public class A
	{
		static int Main ()
		{
			L1: int a=0;
			L1: a++;
			if (a < 3)
			goto L1;
			return 0;
		}
	}
}
