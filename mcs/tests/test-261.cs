using System;

class T {
        T Me { get { calls ++; return this; } }
	T GetMe () { foo ++; return this; }
        int blah = 0, calls = 0, foo = 0, bar = 0;

	static int Test (T t)
	{
                t.Me.Me.blah ++;
		t.GetMe ().GetMe ().bar++;
		if (t.blah != 1)
			return 1;
		if (t.bar != 1)
			return 2;
		if (t.calls != 2)
			return 3;
		if (t.foo != 2)
			return 4;
		return 0;
	}

	public static int Main ()
	{
		T t = new T ();
		int result = Test (t);
		Console.WriteLine ("RESULT: {0}", result);
		return result;
        }
}
