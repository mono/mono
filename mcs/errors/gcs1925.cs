// CS1925: Cannot initialize object of type `string[]' with a collection initializer
// Line: 10

class C
{
	string[] values;
	
	static void Main ()
	{
		var v = new C { values = { "a" } };
	}
}
