// CS0728: Possibly incorrect assignment to `s' which is the argument to a using or lock statement
// Line: 12
// Compiler options: -warnaserror

public class Foo
{
	public static void Test (ref string s)
	{
		lock (s) {
			lock (s) {}
			s = null;
		}
	}
}
