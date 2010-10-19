using System;
using System.Reflection;
using System.Runtime.InteropServices;

struct S
{
}

public class G<T>
{

	public object M1 (T o = default (T))
	{
		return o;
	}

	public object M2 ([Optional] T o)
	{
		return o;
	}
}

public class C
{
	public static object Test ([Optional] dynamic a)
	{
		return a;
	}
	
	void TestS (S s = default (S))
	{
	}
	
	object TestD (dynamic o = null)
	{
		return o;
	}

	public static int Main ()
	{
		if (Test () != Missing.Value)
			return 1;
		
		dynamic d = new C ();
		d.TestS ();
		
		if (d.TestD () != null)
			return 2;
			
		d = new G<string> ();
		if (d.M1 () != null)
			return 3;
			
		if (d.M2 () != null)
			return 4;
		
		return 0;
	}
}
