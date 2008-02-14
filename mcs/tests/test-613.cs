//
// Checks that we do not short-circuit the bitwise and operation
// See bug: 359789
//
public class M {
	static bool called;
	
	public static bool g() {
		called = true;
		return false;
	}

	public static int Main() {
		called = false;
		System.Console.WriteLine (false & g());
		if (!called)
			return 1;

		called = false;
		System.Console.WriteLine (true | g());
		if (!called)
			return 1;
		return 0;
	}
}
