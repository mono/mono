class T {
	static void Main ()
	{
		try {
			T t = null;
			t.Foo ();
		} catch {
			System.Environment.Exit (0);
		}
		
		System.Environment.Exit (1);
	}
	
	void Foo () {
		if (this == null) {
			System.Console.WriteLine ("This isnt anything!?!?");
			System.Environment.Exit (1);
		}
	}
}