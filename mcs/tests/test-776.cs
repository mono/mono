using System;

class First
{
	public virtual object this [string name]
	{
		get { return "First"; }
		set { }
	}
}

class Second : First
{
	public override object this [string name]
	{
		get { return "Second"; }
		set { }
	}
}

class Third : Second
{
	public override object this [string name]
	{
		get { return base [name]; }
		set { }
	}
}

class a
{
	static int Main (string[] args)
	{
		First t = (First)new Third ();
		if (t ["test"] != "Second")
			return 1;
		
		return 0;
	}
}
