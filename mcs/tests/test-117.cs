using System;
using System.Collections;

public interface IFoo {

}

public class Blah : IFoo {

	Hashtable table;

	public Blah ()
	{
		table = new Hashtable ();
	}

	public static int Main ()
	{
		Blah b = new Blah ();

		b.table.Add ("Ravi", (IFoo) b);

		return 0;
	}
		
	

}
