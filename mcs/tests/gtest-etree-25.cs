using System;
using System.Linq.Expressions;

class Foo
{
	public void OnBaz (IBaz baz)
	{
	}
}

interface IBar
{
	void RunOnBaz (Action<IBaz> action);
}

interface IBaz
{
}

class C : IBar
{
	public void RunOnBaz (Action<IBaz> action)
	{
		action (null);
	}
	
    public static int Main ()
    {
		var foo = new Foo ();

		Expression<Action<IBar>> e = bar => bar.RunOnBaz (foo.OnBaz);
		e.Compile () (new C ());
		
		return 0;
    }
}

