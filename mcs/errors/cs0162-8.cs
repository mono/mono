// CS0162: Unreachable code detected
// Line: 9
// Compiler options: -warnaserror -warn:2

class C
{
	public static int Main ()
	{
		if (true == false)
			return 1;
		
		return 2;
	}
}