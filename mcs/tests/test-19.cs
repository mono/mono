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

	public delegate int Foo (int i, int j);
	
	private void Thread_func () {
		Console.WriteLine ("Inside the thread !");
	}

	public int Func (int i, int j)
	{
		return i+j;
	}

	public void Bar ()
	{
		Foo my_func = new Foo (Func);

		int result = my_func (2, 4);

		Console.WriteLine ("Answer is : " + result);
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

		t.Bar ();

		Console.WriteLine ("Test passes");

		return 0;
	}
}
