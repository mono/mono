using System;
using System.Threading;

class I {

	public delegate string GetTextFn (string a);

	static public GetTextFn GetText;

	static string fn (string s)
	{
		return "(" + s + ")";
	}
	
	static I ()
	{
		GetText = new GetTextFn (fn);
	}
}

class X {
	
	private void Thread_func () {
		Console.WriteLine ("Inside the thread !");
	}
	
	public static int Main ()
	{
		I.GetTextFn _ = I.GetText;

		X t = new X ();

		Thread thr = new Thread (new ThreadStart (t.Thread_func));

		thr.Start ();
		Console.WriteLine ("Inside main ");
		thr.Join ();

		Console.WriteLine (_("Hello"));

		return 0;
	}
}
