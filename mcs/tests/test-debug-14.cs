using System;

class C
{
	public static void Main ()
	{
	}
	
	void Test_1()
	{
		Action a =
			() =>
		Console.WriteLine ();
	}
	
	void Test_2()
	{
		Action a =
			() =>
		{
			Console.WriteLine ();
		};
	}
	
	void Test_3()
	{
		Action a = delegate
		{
			Console.WriteLine ();
		};
	}
	
	void Test_Capturing_1(int arg)
	{
		Func<int> a =
			() => arg;
	}
}