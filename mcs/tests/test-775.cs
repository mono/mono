using System;

interface A
{
	event EventHandler Member;
}

interface B : A
{
	new event EventHandler Member;
}

interface BA : B, A { }

public class C : BA
{
	public EventHandler _AMember;
	public EventHandler _Member;

	event EventHandler A.Member
	{
		add { _AMember += value; Console.WriteLine ("Setting A Event"); }
		remove { _AMember -= value; }
	}

	public event EventHandler Member
	{
		add { _Member += value; Console.WriteLine ("Setting Direct Event"); }
		remove { _Member -= value; }
	}
}

public class Test
{
	public static int Main ()
	{
		return new Test ().TestMe ();
	}

	public int TestMe ()
	{
		C c = new C ();
		Console.WriteLine ("Trying to set EventHandler Directly - should set DirectEvent ");
		c.Member += new EventHandler (f);
		if (c._Member == null)
			return 0;

		c._Member = null;
		Console.WriteLine ("Trying to set EventHandler through A interface - Should set A Event");
		((A) c).Member += new EventHandler (f);
		if (c._AMember == null)
			return 1;

		c._AMember = null;
		Console.WriteLine ("Trying to set EventHandler through BA interface - Should set Direct Event");
		((BA) c).Member += new EventHandler (f);
		if (c._Member == null)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}

	void f (object sender, EventArgs e) { }
}

