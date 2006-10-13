using System;
using Mono.ObjectServices;

class Demo {

        int a;

	static void Main ()
	{
		Demo d = new Demo ();

		prints ("d", d);
		prints ("dd", new DD ());
		prints ("short str", "short");
		prints ("long str", "this is a longer string which we want to measure the size of");

		object[] obj_array = new object [100];

		prints ("obj array", obj_array);

		for (int i = 0; i < 100; i++)
			obj_array [i] = new Demo ();

		prints ("obj array w/ demos", obj_array);
	}

	static void prints (string s, object x)
	{
		Console.WriteLine ("size of " + s + ":" + ObjectInspector.GetMemoryUsage (x));
	}
}

class DD {
    Demo d = new Demo ();
    object [] o = new object [10];
    char [] ch = new char [10];
    int junk;   
 
    public DD ()
    {
	    o [0] = new Demo ();
	    o [5] = new Demo ();
    }
}
