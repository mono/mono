using System;

delegate object FactoryDelegate ();

public class C
{
	FactoryDelegate var;
	int counter;

	FactoryDelegate this [string s]
	{
		set { var = value; }
		get { return var; }
	}

	public void X ()
	{
		this ["ABC"] = delegate () {
			++counter;
			Console.WriteLine ("A");
			return "Return";
		};
	}

	public static int Main ()
	{
		C o = new C ();
		o.X ();

		Console.WriteLine ("B");
		Console.WriteLine (o ["ABC"] ());
		
				Console.WriteLine (o.counter);
		if (o.counter != 1)
			return 1;

		return 0;
	}
}
