using System;

public class Proef
{
	private EventHandler _OnDoSomething = null;

	public event EventHandler OnDoSomething
	{
		add
		{
			_OnDoSomething += value;
		}
		remove
		{
			_OnDoSomething -= value;
		}
	}

	static void Temp(object s, EventArgs e)
	{
	}

	public static void Main()
	{
		Proef temp = new Proef();
		temp.OnDoSomething += new EventHandler(Temp);
	}
}
