using System;

public class C
{
	public event EventHandler MyDelegate = delegate { };

	internal void DoSomething (bool bValue)
	{
		if (!bValue) {
			// It has to be here to check we are closing correctly top-block
			return;
		}
	}
	
	public static void Main ()
	{
	}
}


