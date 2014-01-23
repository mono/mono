using System.Collections;
abstract class A {
        protected abstract int this [int a] { get; }

	public int EmulateIndexer (int a)
	{
		return this [a];
	}
}

class B : A {
	protected override int this [int a] { get { return a;}  }

	public int M ()
	{
		return this [0];
	}
	
}
class X {
	int v1, v2;
	
	int this [int a] {
		get {
			if (a == 0)
				return v1;
			else
				return v2;
		}

		set {
			if (a == 0)
				v1 = value;
			else
				v2 = value;
		}
	}

	public static int Main ()
	{
		X x = new X ();

		x [0] = 1;
		if (x.v1 != 1)
			return 1;

		if (x [0] != 1)
			return 2;

		B bb = new B ();

		if (bb.EmulateIndexer (10) != 10)
			return 3;

		//
		// This tests that we properly set the return type for the setter
		// use pattern in the following indexer (see bug 36156)
		Hashtable a = new Hashtable ();
		int b = (int) (a [0] = 1);
		if (b != 1)
			return 4;
		return new B ().M ();
	}
}
