//
// fixed
//
class Location {
	static public int Null {
		get {
			return 1;
		}
	}
}

class X {
	Location Location;
	X ()
	{
		int a = Location.Null;
	}

	static void Main () {}
}
	
	
	
