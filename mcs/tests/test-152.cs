using System;

public class X
{
	public static int Main ()
	{
                int v = 10;
                int a;

                switch (v) {
                case 0:
                        int i = v + 1;
                        a = i;
                        break;
                default:
                        i = 5;
                        a = i;
                        break;
                }
                if (a != 5)
                        return 1;
                return 0;
        }
}
