// Compiler options: -warnaserror

public class TestCase
{
	static int Main ()
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
}
