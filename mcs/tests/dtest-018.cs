class XValue
{
	public long Y { get; set; }
}

class A
{
	public dynamic X = new XValue ();
	
	static int Main()
	{
		var v = new A { X = { Y = 467 } };
		if (v.X.Y != 467)
			return 1;
		
		return 0;
	}
}
