// cs0136.cs: A local variable named `i' cannot be declared in this scope because it would give a different meaning to `i', which is already used in a `parent' scope to denote something else
// Line: 8
class X {
	void b ()
	{
		int i;
		{
			string i;
		}
	}
}


