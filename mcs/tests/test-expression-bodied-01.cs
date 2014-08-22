using System;

class C
{
	int value;
	string f1 = "f-1";
	string f2 = "f=2";

	public static string Test1 (string a, string b) => a + "|" + b;
	void Test2 (int x) => value = x;
	Func<int> Test3 (int a) => () => a;

	public static implicit operator string (C c) => "op";

	protected string Prop => f1 + " " + f2;
	static Func<string> Prop2 => () => "n1";

	public int this[int arg1, int arg2] => arg2 - arg1;


	int Check ()
	{
		if (Test1 ("1", "5") != "1|5")
			return 1;

		Test2 (6);
		if (value != 6)
			return 2;

		if (Test3 (9) () != 9)
			return 3;

		string s = this;
		if (s != "op")
			return 4;

		if (Prop != "f-1 f=2")
			return 5;

		if (Prop2 () != "n1")
			return 6;

		if (this [13, 20] != 7)
			return 7;

		return 0;
	}

    static int Main()
    {
    	var c = new C ();
        return c.Check ();
    }
}