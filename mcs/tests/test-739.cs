// Compiler options: -warnaserror

using System;
using System.Reflection;

sealed class X
{
	~X ()
	{
		Foo ();
	}
	
	public void Foo ()
	{
	}
	
	public static int Main ()
	{
		foreach (var m in typeof (X).GetMethods (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
			Console.WriteLine (m.Name);
			Console.WriteLine (m.Attributes);
			if (m.Attributes != (MethodAttributes.Virtual | MethodAttributes.Family | MethodAttributes.HideBySig))
				return 1;
		}
		
		return 0;
	}
}
