using System;
using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
	public static int Main ()
	{
		Type t = typeof (Control);
		MethodInfo m = t.GetMethod ("set_Foo");

		if (m.GetParameters ()[0].Name != "value")
			return 1;

		return 0;
	}
}

class Control
{
	public virtual int Foo
	{
		[param: MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (int))]
		set
		{
		}
	}
}
