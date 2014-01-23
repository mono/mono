using System;

struct Blah : System.IDisposable {
	public void Dispose () {
		Console.WriteLine ("foo");
	}
}

class B  {
	public static void Main () {
		using (Blah b = new Blah ()) {
			Console.WriteLine ("...");
		}
	}
}
