// cs0136: A local variable named 'i' cannot be declared in this scope because it would give a different meaning to 'i'
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


