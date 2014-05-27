class X
{
	public static void Main ()
	{
		int i = 0;
		if (i == 1) {
			a:
			switch (i) {
			default:
				goto a;
			}
		} else if (i == 2) {
			a:
			switch (i) {
			default:
				goto a;
			}
		}
	}
}
