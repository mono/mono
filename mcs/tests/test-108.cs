class X {

	public static int Main ()
	{
		int i = 0;
		
		if (false){
			i = 1;
			return 1;
		}

		if (true)
			i = 2;
		else
			i = 3;

		if (i != 2)
			return 5;
		
		while (true){
			i++;
			if (i == 10)
				break;
		}

		while (false){
			i--;
			return 3;
		}

		if (i != 10)
			return 2;

		do {
			if (i++ == 20)
				break;
		} while (true);

		if (i != 21)
			return 4;
		
		return 0;
	}
}
