public class Location {
	public static readonly Location UnknownLocation = new 
Location();

	private Location() {
	}
}

public abstract class Element {
	private Location _location = Location.UnknownLocation;

	protected virtual Location Location {
		get { return _location; }
		set { _location = value; }
	}
}

public class T {
	public static void Main () { }
}
