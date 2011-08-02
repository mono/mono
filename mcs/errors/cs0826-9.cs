// CS0826: The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly
// Line: 8

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
