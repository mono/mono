// cs0136: A local variable named 'i' cannot be declared in this scope because it would give a different meaning to 'i'
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


