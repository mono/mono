// cs0159.cs: No default target for goto default
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
