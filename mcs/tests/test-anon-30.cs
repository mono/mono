class X {
	public bool eh;
}

static class Program {
	delegate void D (X o);
	static event D E;
	
	public static void Main()
	{
		bool running = true;

		E = delegate(X o) {
			o.eh = false;
			running = false;
		};

		running = true;
		
	}
}
