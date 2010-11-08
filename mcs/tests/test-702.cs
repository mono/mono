struct S
{
	int a;
	int b;
	
	public S (int i)
	{
		this = new S ();
	}
	
	public S (string s, int a)
	{
		this.a = a;
		this.b = 2;
	}
	
	static void Main ()
	{
		S s = new S (1);
	}
}