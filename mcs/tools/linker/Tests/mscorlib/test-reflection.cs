using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.IO;

public class C
{
	public static int Main ()
	{
		const string ASSEMBLY_NAME = "TypeBuilderTest";
			
		AssemblyName assemblyName = new AssemblyName ();
		assemblyName.Name = ASSEMBLY_NAME;

		var assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetTempPath ());

		var module = assembly.DefineDynamicModule ("module1");

		TypeBuilder tb = module.DefineType ("bla", TypeAttributes.Public);
		GenericTypeParameterBuilder [] typeParams = tb.DefineGenericParameters ("T");

		ConstructorBuilder cb = tb.DefineDefaultConstructor (MethodAttributes.Public);

		Type t = tb.MakeGenericType (typeof (int));
		t.MakeArrayType ();

		Type created = tb.CreateType ();

		Type inst = created.MakeGenericType (typeof (object));		
		return 0;
	}
}
