using System;

class T
{
	public static int Main ()
	{
		Test1 ();
		Test2 ();
		Test3 (0, 1);
		Test4 ();
		
		switch (1) {
		case 1:
			return 0;
		default:
			break;
		}
	}

	static void Test1 ()
	{
		int g = 9;
	A:
		switch (g) {
		case 4:
			return;
		case 5:
			goto A;
		}

		switch (g) {
		case 9:
			break;
		}

		return;
	}
	
	static void Test2 ()
	{
		int a,b;
		int g = 9;
		if (g > 0) {
			a = 1;
			goto X;
		} else {
			b = 2;
			goto Y;
		}

	X:
		Console.WriteLine (a);
		return;
	Y:
		Console.WriteLine (b);
		return;
	}
	
	static uint Test3 (int self, uint data)
	{
		uint rid;
		switch (self) {
		case 0:
			rid = 2;
			switch (data & 3) {
			case 0:
				goto ret;
			default:
				goto exit;
			}
		default:
			goto exit;
		}
	ret:
		return rid;
	exit:
		return 0;
	}

	static void Test4 ()
	{
		bool v;
		try {
			throw new NotImplementedException ();
		} catch (System.Exception) {
			v = false;
		}
		
		Console.WriteLine (v);
	}
}
