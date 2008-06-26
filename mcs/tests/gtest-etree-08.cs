using System;
using System.Linq.Expressions;

class Foo
{
	int ThisMethod ()
	{
		return 33;
	}
	
	public int Goo (bool hoo)
	{
		bool local_hoo = hoo;

		Expression<Func<bool>> a = () => hoo;
		if (a.Compile ()())
			return 1;
		
		if (true) {
			Expression<Func<bool>> b = () => local_hoo;
			if (b.Compile ()())
				return 2;
		}
		
		Expression<Func<int>> c = () => ThisMethod ();
		if (c.Compile ()() != 33)
			return 3;
		
		Console.WriteLine ("OK");
		return 0;
	}

	public static int Main ()
	{
		var f = new Foo ();
		return f.Goo (false);
	}
}
