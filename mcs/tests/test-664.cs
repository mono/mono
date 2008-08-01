class C
{
	static bool Test (string value)
	{
		switch (value) {
			case "1.0":
			case "2.0":
			case "3.0":
			case "4.0":
			case "5.0":
			case "6.0":
			case "7.0":
			case null:
				return true;
		}
		return false;
	}
	
	public static int Main ()
	{
		if (!Test (null))
			return 1;
		
		if (!Test ("6.0"))
			return 2;

		if (Test ("0"))
			return 3;

		return 0;
	}
}