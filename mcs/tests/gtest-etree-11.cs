using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		new Test ().Invalid (4);
	}
}

public class Test
{
	public void Invalid (int item)
	{
		Expression<Action> e1 = () => Other (new int [] { item });
		e1.Compile () ();
	}

	public void Other (int [] i)
	{
	}
}