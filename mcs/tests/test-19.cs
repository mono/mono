using System;
using System.Threading;
using System.Reflection;

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

	static bool MyFilter (MemberInfo mi, object criteria)
	{
		Console.WriteLine ("You passed in : " + criteria);
		return true;
	}
	
	public static int Main ()
	{
		I.GetTextFn _ = I.GetText;

	Console.WriteLine ("Value: " + I.GetText);
		X x = new X ();

		Thread thr = new Thread (new ThreadStart (x.Thread_func));

		thr.Start ();
		Console.WriteLine ("Inside main ");
		thr.Join ();

		Console.WriteLine (_("Hello"));

		x.Bar ();

		MemberFilter filter = new MemberFilter (MyFilter);

		Type t = x.GetType ();

		MemberInfo [] mi = t.FindMembers (MemberTypes.Method, BindingFlags.Static | BindingFlags.NonPublic,
						  Type.FilterName, "MyFilter");

		Console.WriteLine ("FindMembers called, mi = " + mi);
		Console.WriteLine ("   Count: " + mi.Length);
		if (!filter (mi [0], "MyFilter"))
			return 1;

		//
		// This test is used to call into a delegate defined in a separate
		// namespace, but which is still not a nested delegate inside a class
		//
		NameSpace.TestDelegate td = new NameSpace.TestDelegate (multiply_by_three);

		if (td (8) != 24)
			return 30;

		//
		// Check the names that were used to define the delegates
		//
		if (td.GetType ().FullName != "NameSpace.TestDelegate")
			return 31;

		if (_.GetType ().FullName != "I+GetTextFn")
			return 32;
		
		Console.WriteLine ("Test passes");

		return 0;
	}

	static int multiply_by_three (int v)
	{
		return v * 3;
	}
	
}

namespace NameSpace {

	public delegate int TestDelegate (int a);

}
	
