// CS0136: A local variable named `i' cannot be declared in this scope because it would give a different meaning to `i', which is already used in a `child' scope to denote something else
// Line: 9
class X {
	void b ()
	{
		{
			string i;
		}
		int i;
	}
}


