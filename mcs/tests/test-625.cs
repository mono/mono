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

	public static void Main () {}
}
	
	
	
