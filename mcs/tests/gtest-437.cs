// Compiler options: -unsafe

using System;
using System.Reflection;

class C
{
	public static int Main ()
	{
		object[] o = Assembly.GetExecutingAssembly ().GetCustomAttributes (typeof (System.Security.Permissions.SecurityPermissionAttribute), false);
		if (o.Length != 1)
			return 1;

		System.Security.Permissions.SecurityPermissionAttribute a = (System.Security.Permissions.SecurityPermissionAttribute) o[0];

		if (a.Action != System.Security.Permissions.SecurityAction.RequestMinimum)
			return 2;

		if (!a.SkipVerification)
			return 3;

		Console.WriteLine (a);
		return 0;
	}
}