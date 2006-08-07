using System;
using Mono.ObjectServices;

class Demo {
    int a;
	static void Main ()
	{
		Demo d = new Demo ();

		prints ("d", d);
		prints ("dd", new DD ());
	}

	static void prints (string s, object x)
	{
		Console.WriteLine ("size of " + s + ":" + ObjectInspector.GetMemoryUsage (x));
	}
}

class DD {
    Demo d = new Demo ();
    object [] o = new object [10];
    
    public DD ()
    {
	    o [0] = new Demo ();
	    o [5] = new Demo ();
    }
}
