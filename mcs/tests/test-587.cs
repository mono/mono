// Compiler options: -nowarn:0162

class Program
{
	static int Main ()
	{
		int ctc_f = 0;

		if ((++ctc_f == 0 && false)) {
			return 1;
		} else {
			if (ctc_f != 1) {
				return 2;
			}
			return 0;
		}
	}
}

