// Compiler options: -warnaserror

// Checks redundant CS0642 warning

public class C
{
	public static void Main ()
	{
		int v;
		for (v = 1; v >= 0; v--) ;
		uint [] b = null;
		if (b != null)
			return;
	}
}
