using System;

class X {

	IntPtr Raw;
	
	void g_object_get (IntPtr obj, string name, out string val, IntPtr term)
	{
		val = null;
	}

	public void GetProperty (String name, out String val)
	{
		g_object_get (Raw, name, out val, new IntPtr (0));
	}

	void g_object_get (IntPtr obj, string name, out bool val, IntPtr term)
	{
		val = true;
	}
	
	public void GetProperty (String name, out bool val)
	{
		g_object_get (Raw, name, out val, new IntPtr (0));
	}

	public static int Main ()
	{
		return 0;
	}
	
}
