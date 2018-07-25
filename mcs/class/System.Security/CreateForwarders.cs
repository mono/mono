using System;
using System.Text;
using System.Linq;
using System.Reflection;

class X
{
	const string OldPath = @"/Workspace/mono-blue/mcs/class/lib/net_4_x/System.Security.dll";
	const string NewPath = @"/Workspace/mono-egyptian-blue/mcs/class/lib/net_4_x/System.Security.dll";

	static void Main ()
	{
		var oldAsm = Assembly.LoadFile (OldPath);
		var newAsm = Assembly.LoadFile (NewPath);

		foreach (var type in oldAsm.ExportedTypes) {
			if (!type.IsPublic)
				continue;
			if (newAsm.ExportedTypes.Any (Comparer) || newAsm.DefinedTypes.Any (Comparer))
				continue;
			Console.WriteLine ($"[assembly: System.Runtime.CompilerServices.TypeForwardedToAttribute(typeof({type}))]");

			bool Comparer (Type t)
			{
				return t.FullName.Equals (type.FullName);
			}
		}
	}
}
