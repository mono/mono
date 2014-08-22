// CS7059: Executables cannot be satellite assemblies. Remove the attribute or keep it empty
// Line: 7

using System;
using System.Reflection;

[assembly: AssemblyCulture("es")]
[assembly: AssemblyVersion("1.2.3456.7")]

namespace NS 
{
	class MyClass 
	{
		static void Main ()
		{
			Console.WriteLine (typeof(MyClass).Assembly.FullName);
		}
	}
}
