using System;
using System.Reflection;

public class Generic<T>
{
	public delegate void Delegate(Generic<T> proxy, T value);
}

class X
{
	public static int Main ()
	{
		Type t = typeof (Generic<bool>);
		MemberInfo[] mi = t.FindMembers (MemberTypes.NestedType,
						 BindingFlags.Static | BindingFlags.Public |
						 BindingFlags.DeclaredOnly, null, null);

		return mi.Length - 1;
	}
}
