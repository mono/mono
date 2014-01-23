class Program
{
	public static int Main ()
	{
		int ctc_f = 0;

		if ((++ctc_f == 0 && false)) {
			return 1;
		} else if (false && +ctc_f == 0) {
			return 2;
		} else {
			if (ctc_f != 1) {
				return 3;
			}
			
			return 0;
		}
	}
}

