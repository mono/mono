using System;
using System.Runtime.InteropServices;

public class Test
{
	public static int Main ()
	{
		object [] o = typeof (IFoo).GetMethod ("get_Item").GetParameters () [0].GetCustomAttributes (false);
		if (o.Length != 1)
			return 1;

		o = typeof (IFoo).GetMethod ("set_Item").GetParameters () [0].GetCustomAttributes (false);
		if (o.Length != 1)
			return 2;

		return 0;
	}

	public interface IFoo
	{
		int this [[MarshalAs (UnmanagedType.Struct)]object vt0BasedIdxOrId] { get; set; }
	}
}
