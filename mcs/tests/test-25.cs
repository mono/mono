//
// Test the various iteration constructs, breaks and continues
//
// FIXME: Add foreach and more tests.
//
using System;

class X {

	public static int Main ()
	{
		int i, j, t, k;
		
		for (i = 0; i < 10; i++){
			if (i == 5)
				break;
		}

		if (i != 5)
			return 1;

		t = 0;
		k = 0;
		for (i = 0; i < 10; i++){
			for (j = 0; j < 10; j++){
				if (j > 3)
					break;
				t++;

				if (j >= 1)
					continue;

				k++;
			}
		}

		if (t != 40)
			return 2;
		if (k != 10)
			return 3;


		t = 0;
		do {
			if (k == 5)
				continue;
			t++;
		} while (--k > 0);

		if (t != 9)
			return 4;

		t = 0;
		do {
			t++;
			if (t == 5)
				break;
		} while (k++ < 10);

		if (t != 5)
			return 5;

		return 0;
	}
}
