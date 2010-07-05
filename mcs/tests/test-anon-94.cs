using System;

class Program
{
	public class BaseClass
	{
		public int i;
		public virtual void Print () { Console.WriteLine ("BaseClass.Print"); i = 90; }
	}

	public class Derived : BaseClass
	{
		public override void Print ()
		{
			Action a = () => base.Print ();
			a ();
		}
	}

	public static int Main ()
	{
		var d = new Derived ();
		d.Print ();

		if (d.i != 90)
			return 1;

		return 0;
	}
}
