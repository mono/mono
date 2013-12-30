// Compiler options: -r:test-875-lib.dll -r:test-875-2-lib.dll

using N;

public class Test: Lib
{
	public static void Main ()
	{
		new Test ();
	}
}
