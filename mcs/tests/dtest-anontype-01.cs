class C
{
	public static void Main ()
	{
		var a = new {
			Field = Factory ()
		};
		
		a.Field.Test ();
	}
	
	void Test ()
	{
	}
	
	static dynamic Factory ()
	{
		return new C ();
	}
}
