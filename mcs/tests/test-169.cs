//
// Test for overloaded properties.
//
using System;

public class basec {
	public virtual string Message {
		get {
			return "base";
		}
	}
}

public class der : basec {
	public override string Message {
		get {
			return "der";
		}
	}
}

class Base {
        int thingy = 0;
        public virtual int Thingy {
                get { return thingy; }
                set { thingy = value; }
        }
}

class Derived : Base {
        public int BaseThingy {
                get { return Thingy; }
        }

        public override int Thingy {
                // override the set constructor
                set { }
        }
}

class D {

	public static int Main ()
	{
		//
		// These tests just are compilation tests, the new property code
		// will excercise these
		//
		der d = new der ();
		if (d.Message != "der")
			return 1;

		basec b = new basec ();
		if (b.Message != "base")
			return 2;

		Derived dd = new Derived ();
		dd.Thingy = 10;
		if (dd.BaseThingy != 0)
			return 3;

		Console.WriteLine ("Test ok");
		return 0;
	}
}
