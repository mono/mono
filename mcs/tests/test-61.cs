//
// This tests checks that we allow the `get' and `set' keywords inside
// the get and set blocks.  It does the same for Events special remove
// and add keywords.
//
class X {
	int Property {
		get {
			int get;
			get = 1;
			return get;
		}
		set {
			int set;
			set = value;
		}
	}

	int P2 {
		get { return 0; }
	}

	int P3 {
		set {  }
	}

	public delegate void MyEvent ();
	
	public event MyEvent XX {
		add { int add = 1; }
		remove { int remove = 1; }
	}

	public static int Main ()
	{
		return 0;
	}
}

