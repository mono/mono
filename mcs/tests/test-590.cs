using System;

class X
{
	public static int Main ()
	{
		X x = new X ();
		return x.Do ("a", "b", "c");
	}

	string str = "start";

	string Foo ()
	{
		return "s";
	}

	string Prop
	{
		get { return str; }
		set { str = value; }
	}

	string this [int i]
	{
		get { return str; }
		set { str = value; }
	}

	int Do (string a, string b, string c)
	{
		str += Foo ();
		if (str != "starts")
			return 1;

		str += a + "," + b + "," + c;
		if (str != "startsa,b,c")
			return 2;

		Prop += a;
		if (str != "startsa,b,ca")
			return 3;

		Prop += a + "," + b + "," + c;
		if (str != "startsa,b,caa,b,c")
			return 4;

		this [0] += a + "," + b + "," + c;
		if (str != "startsa,b,caa,b,ca,b,c")
			return 5;

		return 0;
	}
}