// CS0159: The label `default:' could not be found within the scope of the goto statement
// Line: 10

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
