using System;

public class Test {

	public static int Main ()
	{
		int a = 2;
		int res = 0;
		switch(a) {
		case 1:
			res += 1;
		label:
			res += 3;
			break;
		case 2:
			res += 2;
			goto label;
		}

		if (res != 5)
			return 1;

		return 0;
	}
}