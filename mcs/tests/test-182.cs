//
// See bug 37473
//
using System;
struct TS {
	long ticks;
	public long Ticks {
		get {return ++ticks;}
	}
}
struct DT {
	TS t;
	public long Ticks {
		get {return t.Ticks;}
	}
}

class T {
	public static int Main () {
		DT t = new DT ();
		if (t.Ticks != 1)
			return 1;
		return 0;
	}
}
