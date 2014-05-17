// CS0165: Use of unassigned local variable `a'
// Line: 14

class X
{
	public static void Main ()
	{
		int i = 3;
		switch (i) {
		case 1:
			float a = 7.0f;
			break;
		default:
			float b = a + 99.0f;
			break;
		}
	}
}