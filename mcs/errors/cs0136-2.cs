// cs0136.cs: local variable j can not be declared, because there is something with that name already
// Line: 5
class X {
	public static void Bar (int j, params int [] args)
	{
		foreach (int j in args)
			;
	}
}
