//
// We used to generate incorrect code for breaks in infinite while loops
//
using System;

public class BreakTest
{
	static int ok = 0;
	
	public static void B ()
	{
		ok++;
                while (true)
                {
			ok++;
                        break;
                }
		ok++;
	}
	
        public static int Main()
        {
		B ();
		if (ok != 3)
			return 1;
		return 0;
        }
}
