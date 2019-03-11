using System;
using System.Reflection;
using System.Reflection.Emit;

class X
{
	public static int Main ()
	{
		Type appdomain_type = null;
		Type refemit_gate = null;
		Type type_builder_type = null;
		var corlib = typeof (int).Assembly;
		foreach (var type in corlib.GetTypes ()) {
			switch (type.FullName) {
			case "System.AppDomain":
				appdomain_type = type;
				break;
			case "Mono.ReflectionEmitGate":
				refemit_gate = type;
				break;
			case "System.Reflection.Emit.TypeBuilder":
				type_builder_type = type;
				break;
			}
		}

		if (appdomain_type == null)
			throw new ApplicationException ("Missing `System.AppDomain` class.");
		if (refemit_gate == null)
			throw new ApplicationException ("Missing `Mono.ReflectionEmitGate` class.");
		if (type_builder_type == null)
			throw new ApplicationException ("Missing `System.Reflection.Emit.TypeBuilder` class.");

		if (type_builder_type != typeof (TypeBuilder))
			return 1;

		bool found_type_resolve = false;
		bool found_type_builder_resolve = false;
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
				found_type_builder_resolve = true;
				break;
			}
		}

		if (!found_type_resolve)
			throw new ApplicationException ("Missing `AppDomain.DoTypeResolve` method.");
		if (!found_type_builder_resolve)
			throw new ApplicationException ("Missing `AppDomain.DoTypeBuilderResolve` method.");
		if (!found_assembly_resolve)
			throw new ApplicationException ("Missing `AppDomain.DoAssemblyResolve` method.");

		return 0;
	}
}
