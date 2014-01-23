using System;

class Foo {
	static int t_count = 0, f_count = 0;

	public static int Main ()
	{
		Console.WriteLine (t && f);
		if (t_count != 1)
			return 1;
		if (f_count != 1)
			return 2;
		Console.WriteLine ();
		Console.WriteLine (t && t);
		if (t_count != 3)
			return 3;
		if (f_count != 1)
			return 4;
		Console.WriteLine ();
		return 0;
	}
	
	static MyBool t { get { Console.WriteLine ("t"); t_count++; return new MyBool (true); }}
	static MyBool f { get { Console.WriteLine ("f"); f_count++; return new MyBool (false); }}
}

public struct MyBool {
	bool v;
	
	public MyBool (bool v) { this.v = v; }
	
	public static MyBool operator & (MyBool x, MyBool y) {
		return new MyBool (x.v & y.v);  
	}
	
	public static MyBool operator | (MyBool x, MyBool y) {
		return new MyBool (x.v | y.v);  
	}
	
	public static bool operator true (MyBool x) {
		return x.v;  
	}
	
	public static bool operator false (MyBool x) {
		return ! x.v;  
	}
	
	public override string ToString () { return v.ToString (); }
}
