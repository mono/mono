using System;
using System.Reflection;

public class MethodAttribute : Attribute {}
public class ReturnAttribute : Attribute {}

public class Test {
	[Method]
	[return: Return]
	public void Method () {}

	public static int Main () {

		Type t = typeof(Test);
		MethodInfo mi = t.GetMethod ("Method");
		ICustomAttributeProvider cap = mi.ReturnTypeCustomAttributes;

		if (cap != null) 
			return 0;			
		else
			return 1;
	}
}

