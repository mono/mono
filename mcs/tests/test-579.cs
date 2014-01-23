// Compiler options: -warnaserror

public class TestCase
{
	public static int Main ()
	{
		if (Test1 () != 0)
			return 1;

		if (Test2 () != 0)
			return 2;

		return 0;
	}

	static int Test1 ()
	{
		int i = 0;
		{
			goto A;
			A:
				i += 3;
		}
		{
			goto A;
			A:
				i *= 4;
		}
		
		if (i != 12)
			return 1;
			
		return 0;
	}

	static int Test2 ()
	{
		int i = 0;

		while (true) {
			{
				goto A;
				A:
					i += 3;
				break;
			}
		}

		if (i != 3)
			return 1;

		return 0;
	}

	static int Test3 ()
	{
		int i = 0;

		do {
			{
				goto A;
				A:
					i += 3;
				goto X;
				X:
				break;
			}
#pragma warning disable 162, 429
		} while (i > 0);
#pragma warning restore 162, 429
		
		if (i != 3)
			return 1;

		return 0;
	}
}
