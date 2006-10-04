// CS0136: A local variable named `i' cannot be declared in this scope because it would give a different meaning to `i', which is already used in a `parent or current' scope to denote something else
// Line: 10
delegate string Fun (int i);

class X
{
	static void Main ()
	{
		for (int i = 0; i < 5; i++) {
			Fun m = delegate (int i) {
				return "<<" + i + ">>";
			};
		}
	}
}
