// CS0136: A local variable named `j' cannot be declared in this scope because it would give a different meaning to `j', which is already used in a `parent or current' scope to denote something else
// Line: 7

class X {
	public static void Bar (int j, params int [] args)
	{
		foreach (int j in args)
			;
	}
}
