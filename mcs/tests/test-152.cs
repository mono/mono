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


		v = 20;
		int r = 0;
		
		switch (v){
		case 20:
			r++;
			int j = 10;
			r += j;
			break;
		}
		if (r != 11)
			return 5;
		
                return 0;
        }
}
