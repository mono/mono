using System;
using System.Reflection;

public delegate long MyDelegate ();

public interface X
{
	event MyDelegate Foo;
	int Prop { get; }
}

public class Y : X
{
	event MyDelegate X.Foo {
		add {
		}

		remove {
		}
	}
	
	int X.Prop {
		get { return 1; }
	}

	public event MyDelegate Foo;

	public static int Main ()
	{
		MethodInfo o = typeof (Y).GetMethod ("X.add_Foo", BindingFlags.NonPublic | BindingFlags.Instance);
		
		if (o == null)
			return 1;
		
		o = typeof (Y).GetMethod ("X.get_Prop", BindingFlags.NonPublic | BindingFlags.Instance);
		if (o == null)
			return 2;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
