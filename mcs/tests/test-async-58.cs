using System;
using System.Threading.Tasks;

public class A
{
	public int Get ()
	{
		return 1;
	}
}

public class B : A
{
	public async Task<int> GetAsync ()
	{
		return base.Get ();
	}
	
	static void Main ()
	{
		new B ().GetAsync ().Wait ();
	}
}