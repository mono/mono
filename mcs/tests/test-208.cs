using System;

interface A
{
	string this [string s] { get; }
}

interface B : A
{
	void Test ();
}

class X : B
{
	public string this [string s] {
		get {
			return s;
		}
	}

	public void Test ()
	{ }
}

public class Y
{
	public static void Main ()
	{
		B b = new X ();

		string s = b ["test"];
	}
}

