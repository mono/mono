//
// This test checks if we implement all the interfaces inherited
//

interface IA {
        void A ();
}

interface IB : IA {
        void B ();
}

interface IC : IA, IB {
	void C ();
}

interface ID : IC {
}

class AA : IC {
	bool a, b, c;
	public void A () { a = true; }
	public void B () { b = true; }
	public void C () { c = true; }

	public bool OK {
		get {
			return a && b && c;
		}
	}
}

class BB : ID{
	bool a, b, c;
	public void A () { a = true; System.Console.WriteLine ("A"); }
	public void B () { b = true; }
	public void C () { c = true; }

	public bool OK {
		get {
			return a && b && c;
		}
	}
}

class T: IB {
        public void A () {}
        public void B () {}

        public static int Main() {

		BB bb = new BB ();
		bb.A ();
		bb.B ();
		bb.C ();

		if (!bb.OK)
			return 1;

		AA aa = new AA ();
		aa.A ();
		aa.B ();
		aa.C ();
		if (!aa.OK)
			return 2;

		return 0;
	}
}
