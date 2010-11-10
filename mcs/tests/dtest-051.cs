using System;

class C
{
	int value = 1;
	
	public int this [int arg] {
		get { return this.value; }
		set { this.value = value + arg; }
	}
	
	public static int Main ()
	{
		C c = new C ();
		dynamic d = c;
		int index = 1;

		var x = ++d[++index];

		if (index != 2)
			return 1;
		
		if (c.value != 4)
			return 2;
		
		if (x != 2)
			return 3;
		
		return 0;
	}
}