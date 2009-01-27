using System;

public class Tests
{
	public virtual ServiceType GetService<ServiceType> (params object[] args) where ServiceType : class
	{
		Console.WriteLine ("asdafsdafs");
		return null;
	}

	public static int Main ()
	{
		new Tests ().GetService<Tests> ();
		return 0;
	}
}
