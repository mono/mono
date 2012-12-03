class X {
	static string a = "static string";
	string b = a + "string";
	
	X () {}
	X (int x) {}
	
	public static int Main () {
		if (new X ().b != "static stringstring")
			return 1;
		
		if (new X (1).b != "static stringstring")
			return 2;
		return 0;
	}
}