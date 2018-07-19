// CS0136: A local variable named `s' cannot be declared in this scope because it would give a different meaning to `s', which is already used in a `parent or current' scope to denote something else
// Line: 10

internal class Program
{
	public static void Main ()
	{
		object o = null;
		if (o is string s) {
			int s = 1;
		}
	}
}