using System;

class A
{
	public int id;
	
	public int this [int i] {
		set { Console.WriteLine ("set= " + i); id = value; }
		get { Console.WriteLine ("get= " + i); return id; }
	}
}

struct MyPoint
{
	public MyPoint (int x, int y)
	{
		X = x;
		Y = y;
	}
	
	public int X, Y;
}

class C
{
	static decimal Foo (decimal t, decimal a)
	{
		return a;
	}
	
	static string Bar (int a = 1, string s = "2", char c = '3')
	{
		return a.ToString () + s + c;
	}

	static int Test (int a, int b)
	{
		Console.WriteLine ("{0} {1}", a, b);
		return a * 3 + b * 7;
	}
	
	public static int Main ()
	{
		int h;
		if (Foo (a : h = 9, t : 3) != 9)
			return 1;
		
		if (h != 9)
			return 2;
		
		if (Bar (a : 1, s : "x", c : '2') != "1x2")
			return 3;
		
		if (Bar (s : "x") != "1x3")
			return 4;
		
		int i = 1;
		if (Test (a: i++, b: i++) != 17)
			return 5;
		
		if (i != 3)
			return 6;
		
		i = 1;
		if (Test (b: i++, a: i++) != 13)
			return 7;
		
		A a = new A ();
		i = 5;
		a [i:i++]++;
		
		if (a.id != 1)
			return 8;
		
		if (i != 6)
			return 9;
		
		MyPoint mp = new MyPoint (y : -1, x : 5);
		if (mp.Y != -1)
			return 10;
		
		Console.WriteLine ("ok");
		return 0;
	}
}