class A
{
	public virtual void Add (object o)
	{
	}
}

class B : A
{
	public virtual bool Add (object o)
	{
		return false;
	}
	
	public static void Main () {}
}