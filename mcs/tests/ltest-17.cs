using System;

class TestCase
{
	string a;
	string b;
	string c;

	public void Testing ()
	{
		string z = a + b + "blah1" + c + "blah2";
		Action test = () => {
			string x = a;
		};
		test ();
	}

	public static void Main ()
	{
		new TestCase ().Testing ();
	}
}
