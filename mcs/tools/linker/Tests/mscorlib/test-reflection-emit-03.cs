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
				AssertFields (type, 28);
				break;
			case "System.Reflection.Emit.AssemblyBuilder":
				AssertFields (type, 45);
				break;
			case "System.Reflection.Emit.ConstructorBuilder":
				AssertFields (type, 14);
				break;
			case "System.Reflection.Emit.DynamicMethod":
				AssertFields (type, 19);
				break;
			case "System.Reflection.Emit.EnumBuilder":
				AssertFields (type, 4);
				break;
			case "System.Reflection.Emit.EventBuilder":
				AssertFields (type, 10);
				break;
			case "System.Reflection.Emit.FieldBuilder":
				AssertFields (type, 12);
				break;
			case "System.Reflection.Emit.GenericTypeParameterBuilder":
				AssertFields (type, 9);
				break;
			case "System.Reflection.Emit.ILExceptionBlock":
				AssertFields (type, 5);
				break;
			case "System.Reflection.Emit.ILExceptionInfo":
				AssertFields (type, 4);
				break;
			case "System.Reflection.Emit.ILGenerator":
				AssertFields (type, 18);
				break;
			case "System.Reflection.Emit.LocalBuilder":
				AssertFields (type, 7);
				break;
			case "System.Reflection.Emit.MethodBuilder":
				AssertFields (type, 27);
				break;
			case "System.Reflection.Emit.MonoResource":
				AssertFields (type, 0);
				break;
			case "System.Reflection.Emit.MonoWin32Resource":
				AssertFields (type, 0);
				break;
			case "System.Reflection.Emit.UnmanagedMarshal":
				AssertFields (type, 9);
				break;
			case "System.Reflection.Emit.ArrayType":
				AssertFields (type, 3);
				break;
			case "System.Reflection.Emit.ByRefType":
				AssertFields (type, 2);
				break;
			case "System.Reflection.Emit.PointerType":
				AssertFields (type, 2);
				break;
			case "System.Reflection.Emit.FieldOnTypeBuilderInst":
				AssertFields (type, 2);
				break;
			case "System.Reflection.Emit.MethodOnTypeBuilderInst":
				AssertFields (type, 4);
				break;
			case "System.Reflection.Emit.ConstructorOnTypeBuilderInst":
				AssertFields (type, 2);
				break;
			}
		}

		/*
		
		<type fullname="System.Reflection.Emit.ModuleBuilder" preserve="fields" feature="sre">
			<method name="Mono_GetGuid" feature="sre" />
		</type>
		<type fullname="System.Reflection.Emit.TypeBuilder" preserve="fields" feature="sre">
			<method name="SetCharSet" feature="sre" />
			<method name="IsAssignableTo" feature="sre" />
		</type>
		<type fullname="System.Reflection.Emit.UnmanagedMarshal" preserve="fields" feature="sre" >
			<method name="DefineCustom" feature="sre" />
			<method name="DefineLPArrayInternal" feature="sre" />
		</type>
 */

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

	static void AssertFields (Type type, int count)
	{
		var fields = type.GetFields (BindingFlags.Instance | BindingFlags.NonPublic);
		if (fields.Length != count) {
			Console.Error.WriteLine ($"Type `{type}` has {fields.Length} fields, but expected {count}.");
			// throw new ApplicationException ($"Type `{type}` has {fields.Length} fields, but expected {count}.");
		}
	}
}
