using System;

public class A
{
	protected int value = 9;
	
	public virtual int this [int i]
	{
		get { throw new NotImplementedException (); }
		set { this.value = i; }
	}
}

public class B : A
{
	public override int this [int i]
	{
		get { return value; }
	}
}

public class C : B
{
	public override int this [int i]
	{
		get { return base [i]; }
		set { base [i] = value; }
	}

	public static int Main ()
	{
		var c = new C ();
		var r = c [100]++;
		Console.WriteLine (r);
		if (r != 9)
			return 1;
			
		return 0;
	}
}
