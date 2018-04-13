using System;

class C : B
{

}

public class B
{
	public static void Main ()
	{
		C c = new C ();

		if (c is B b)
		{
			Console.WriteLine (b == null);
		}
	}
}