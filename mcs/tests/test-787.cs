using System;

public class A : Attribute
{
	public virtual string Prop {
		set {}
		get { return null; }
	}

	public int Field ()
	{
		return 0;
	}
}

public class B : A
{
	public override string Prop {
		set {}
		get { return "b"; }
	}

	public new int Field;	
}

[B (Prop = "a", Field = 3)]
public class Test
{
	public static void Main ()
	{
	}
}
