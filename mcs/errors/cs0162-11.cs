// CS0162: Unreachable code detected
// Line: 12
// Compiler options: -warnaserror -warn:2

class Program
{
	static int Main ()
	{
		int ctc_f = 0;

		if ((++ctc_f == 0 && false)) {
			return 1;
		}
		
		return 0;
	}
}

