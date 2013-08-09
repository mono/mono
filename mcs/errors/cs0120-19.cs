// CS0120: An object reference is required to access non-static member `MainClass.Callback()'
// Line: 9

using System;

class MainClass : BaseClass
{
	public MainClass (string a, Action callback)
		: base (a, () => Callback ())
	{
	}

	private void Callback ()
	{
	}
}

public class BaseClass
{
	public BaseClass (string a, int b)
	{
	}

	public BaseClass (string a, Action callback)
	{
	}
}