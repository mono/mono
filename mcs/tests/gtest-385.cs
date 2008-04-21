using System;
using System.Reflection;

[AttributeUsage (AttributeTargets.All)]
public class DocAttribute : Attribute
{
	public DocAttribute () {}
	public DocAttribute (string s) {}
}

public delegate void Func<[Doc] TArg, [Doc ("ret!")] TRet> ();

class Test
{
	public static int Main ()
	{
		Type[] targs = typeof (Func<,>).GetGenericArguments ();
		if (targs[0].GetCustomAttributes (false).Length != 1)
			return 1;
		
		if (targs[1].GetCustomAttributes (false).Length != 1)
			return 2;
		
		Console.WriteLine ("OK");
		return 0;
	}
}

