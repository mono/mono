// CS1738: Named arguments must appear after the positional arguments
// Line: 14

class C
{
	int this [int a, string s] {
		get {
			return 1;
		}
	}
	
	void Test ()
	{
		var r = this [a : 1,  "out"];
	}
}
