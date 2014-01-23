using System;
using System.Reflection;

class AAttribute : Attribute
{
}

class Program
{
	[field: A]
	public int Prop { get; set; }

	public static int Main ()
	{
		var f = typeof (Program).GetFields (BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		if (f[0].GetCustomAttribute<AAttribute> () == null)
			return 1;

		return 0;
	}
}

