class C
{
	static void Main()
	{
		object o = 1;
		dynamic d = 1;
		
		var a = new[] {
			new { X = o },
			new { X = d }
		};
	}
}
