using System;
using System.Linq;
using System.Reflection;

class Program
{
	public static int Main ()
	{
		var setter = typeof (MyClass).GetMember("set_Item")[0] as MethodInfo;
		var sp = setter.GetParameters ();
		var first = sp [0].GetCustomAttributes ();
		var value = sp [2].GetCustomAttributes ();

		if (first.Count () != 0)
			return 1;

		if (value.Count () != 1)
			return 2;
			
		return 0;
	}
}

[AttributeUsage(AttributeTargets.All)]
public class MyAttribute2Attribute : Attribute
{
}

public class MyClass
{
	public string this[int index1, int index2]
	{
		get
		{
			return "";
		}

		[param: MyAttribute2]
		set
		{
		}
	}
}