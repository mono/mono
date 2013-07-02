// CS0161: `T.Main()': not all code paths return a value
// Line: 6
// CSC bug: The error is not reported even if it should as in other unreachable cases

class T {
	public static int Main ()
	{
		switch (1) {
		case 1:
			return 0;
		default:
			break;
		}
	}
}
