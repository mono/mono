class X {

	static int Main ()
	{
		string s;

		s.Split ('a');
		try {
			s.Split ();
		} catch {
		}
		
		return 0;
	}
}
	
