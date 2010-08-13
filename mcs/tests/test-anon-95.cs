using System;

public delegate int D ();

public abstract class A
{
	protected abstract event D Event;
}

public class B : A
{
	protected override event D Event;

	protected int Run ()
	{
		return Event ();
	}
}

public class C : B
{
	int Test (int i)
	{
		Action a = () => base.Event += () => i;
		a ();
		return Run ();
	}

	public static int Main ()
	{
		if (new C ().Test (9) != 9)
			return 1;

		return 0;
	}
}