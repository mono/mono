// CS0162: Unreachable code detected
// Line: 11
// Compiler options: -warnaserror -warn:2

class Error
{
	void Test ()
	{
		switch (10)
		{
			case 9:
				break;
		}
	}

}
