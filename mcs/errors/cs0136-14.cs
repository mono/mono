// CS0136: A local variable named `i' cannot be declared in this scope because it would give a different meaning to `i', which is already used in a `child' scope to denote something else
// Line: 15
delegate string Fun ();

class X
{
	static void Main ()
	{
		for (int j = 0; j < 5; j++) {
			Fun m = delegate {
				int i = j;
				return "<<" + i + ">>";
			};

			int i = j;
		}
	}
}
