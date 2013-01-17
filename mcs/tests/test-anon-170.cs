using System;

public class MyClass
{
	Func<int> ll;

	int Test (int a)
	{
		return 1;
	}

	public void Run ()
	{
		Action<int> a = l => {
			ll = () => l;
		};

		a (1);

		Action<int> a2 = l => {
			ll = () => l;
		};

		a2 (2);
	}

	public void Run2 ()
	{
		Action<int> a = l => {
			ll = () => Test (l);
		};

		a (1);

		Action<int> a2 = l => {
			ll = () => Test (l);
		};

		a2 (1);
	}

	public static void Main ()
	{
		var mc = new MyClass ();
		mc.Run ();
		mc.Run2 ();
	}
}