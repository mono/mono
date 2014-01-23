class T {
	static int ret_code = 0;
    
	public static int Main ()
	{
		try {
			T t = null;
			t.Foo ();
		} catch {
			return ret_code;
		}
		ret_code = 1;
		return ret_code;
	}
	
	void Foo () {
		if (this == null) {
			System.Console.WriteLine ("This isnt anything!?!?");
			ret_code = 1;
		}
	}
}