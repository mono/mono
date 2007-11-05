using System;

public class Bar<U> where U : EventArgs
{
	internal void OnWorldDestroyed ()
	{
	}
}

public class Baz<U> where U : Bar<EventArgs>
{
	public void DestroyWorld (U bar)
	{
		bar.OnWorldDestroyed ();
	}
}

public class Bling
{
	public static void Main ()
	{
	}
}
