public static class D
{
	static bool? debugging = null;
	static int? extra = 0;

	public static void Main ()
	{
		debugging |= true;
		
		extra |= 55;
	}
}
