// cs0255.cs: Can not use stackalloc in finally or catch
// Line: 10
unsafe class X {
	string s;

	static void Main ()
	{
		try {
		} catch {
		char *ptr = stackalloc char [10U];
}
	}
}	
