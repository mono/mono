//
// fixed
//
interface I {
	void a ();
}

abstract class X : I {
	public abstract void a ();
}

class Y : X {
	override public void a () {
		System.Console.WriteLine ("Hello!");
		return;
	}

	public static void Main () {
		Y y = new Y ();

		((I) y ).a ();
	}
}
