using System;

interface A {
	void B ();
}

interface X {
	void B ();
}


class B : A, X {
	void X.B () {}
	void A.B () {}
	
}

namespace N {
	interface B {
	}
}

class M {
	static void N (object N)
	{
		object x = (N.B) N;
	}

	public static void Main ()
	{
	}
}

