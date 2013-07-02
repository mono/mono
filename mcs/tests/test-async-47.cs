using System;

class C
{
	public static void Main ()
	{
		new C ().Test ();
	}

	void Test ()
	{
		var v = new View ();
		v.Click += async (o, e) => {
			var b = new Builder ();
			b.SetButton (() => {
				Console.WriteLine (this);
			});
		};
		v.Run ();
	}
}

class View
{
	public event EventHandler Click;

	public void Run ()
	{
		Click (null, null);
	}
}

class Builder
{
	public void SetButton (Action h)
	{
		h ();
	}
}
