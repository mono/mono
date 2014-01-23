using System;

class Delegable {
	public event EventHandler MyDelegate;
}

class DelegateTest {
	public static void Main (string[] argv)
	{
		Console.WriteLine ("Test");

		Delegable db = new Delegable ();
		db.MyDelegate += delegate (object o, EventArgs args) {
			Console.WriteLine ("{0}", argv);
			Console.WriteLine ("{0}", db);
		};
	}
}


