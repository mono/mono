class C
{
	public void MethodRef (ref int a)
	{
		a += 10;
	}
	
	public void MethodOut (out ushort a)
	{
		a = 40;
	}
}

public class Test
{
	static void M (ref dynamic[] d, ref object[] o)
	{
	}
	
	public static int Main ()
	{
		dynamic d = new C ();
		int i = 1;
		
		d.MethodRef (ref i);
		if (i != 11)
			return 1;

		ushort u = 9;
		d.MethodOut (out u);
		if (u != 40)
			return 2;
		
		object[] o = null;
		M (ref o, ref o);
		
		return 0;
	}
}
