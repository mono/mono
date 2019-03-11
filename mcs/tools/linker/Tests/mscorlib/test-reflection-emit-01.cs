using System;
using System.Reflection;

class X
{
	public static int Main ()
	{
		Type appdomain_type = null;
		Type refemit_gate = null;
		var corlib = typeof (int).Assembly;
		foreach (var type in corlib.GetTypes ()) {
			if (type.Namespace == "System.Reflection.Emit")
				throw new ApplicationException ($"Expected {type.FullName} to be linked out.");

			switch (type.FullName) {
			case "System.AppDomain":
				appdomain_type = type;
				break;
			case "Mono.ReflectionEmitGate":
				refemit_gate = type;
				break;
			}
		}

		if (appdomain_type == null)
			throw new ApplicationException ("Missing `System.AppDomain` class.");
		if (refemit_gate == null)
			throw new ApplicationException ("Missing `Mono.ReflectionEmitGate` class.");

		bool found_type_resolve = false;
		bool found_assembly_resolve = false;
		foreach (var method in appdomain_type.GetMethods (BindingFlags.NonPublic | BindingFlags.Instance)) {
			switch (method.Name) {
			case "DoTypeResolve":
				found_type_resolve = true;
				break;
			case "DoAssemblyResolve":
				found_assembly_resolve = true;
				break;
			case "DoTypeBuilderResolve":
				throw new ApplicationException ($"Expected {method} to be linked out.");
			}
		}

		if (!found_type_resolve)
			throw new ApplicationException ("Missing `AppDomain.DoTypeResolve` method.");
		if (!found_assembly_resolve)
			throw new ApplicationException ("Missing `AppDomain.DoAssemblyResolve` method.");

		return 0;
	}
}
