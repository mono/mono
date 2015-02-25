using System;

class MainClass
{
	public static void Main ()
	{
		SomeMethod (() => {
			Func<int,int> f = b => b;
		retry:
			goto retry;
		});
	}

	static void SomeMethod (Action a)
	{
	}
}
