// CS0100: The parameter name `a' is a duplicate
// Line: 6
//
// Author: 
// 	Alejandro Snchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Snchez Acosta
//

public class X 
{
	public void Add (int a, int a)
	{
		int c;
		c= a + a;
		Console.WriteLine (c);
	}

	static void Main ()
	{
		this.Add (3, 5);
	}
}

