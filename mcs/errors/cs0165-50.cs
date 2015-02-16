// CS0165: Use of unassigned local variable `u'
// Line: 15

class X
{
	public static void Main ()
	{
		int i = 0;
		int u;
		switch (i) {
			case 1:
				A1:
				goto case 2;
			case 2:
				i = u;
				goto case 3;
			case 3:
				goto case 4;
			case 4:
				goto A1;
		}
	}
}