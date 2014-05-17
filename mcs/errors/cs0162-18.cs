// CS0162: Unreachable code detected
// Line: 10
// Compiler options: -warnaserror

public class X
{
	public static void Main ()
	{
		return;

		switch (8) {
		case 1:
		case 2:
			break;
		default:
			return;
		}

		return;
	}
}