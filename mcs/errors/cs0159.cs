// cs0159.cs: No such label `default:' within the scope of the goto statement
// Line:

class X {

	static int m (int n)
	{
		switch (n){
		case 0:
			goto default;

		case 1:
			return 1;
		}

		return 10;
	}
	
	static void Main ()
	{
		m (1);
	}
}
