

using System;
using System.Reflection;

public interface IA
{
	void Foo (IA self);
}

public static class C
{
	public static TAttribute GetCustomAttribute<TAttribute> (this ICustomAttributeProvider self)
	{
		var attributes = self.GetCustomAttributes<TAttribute> ();
//		if (attributes == null || attributes.Length == 0)
//			return null;

		return attributes [0];
	}

	public static TAttribute [] GetCustomAttributes<TAttribute> (this ICustomAttributeProvider self)
	{
		return null;
	}	
	
	public static void Main ()
	{
	}
}
