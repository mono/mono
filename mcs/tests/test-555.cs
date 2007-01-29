using System;
using System.Reflection;
using System.Reflection.Emit;

public class Test
{
   public static int Main()
   {
	   AssemblyName assemblyName = new AssemblyName ();
	   assemblyName.Name = "TestAssembly";
	   AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);

	   ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule ("TestModule");
	   TypeBuilder typeBuilder = moduleBuilder.DefineType ("TestType", TypeAttributes.Public);

	   FieldBuilder fieldBuilder = typeBuilder.DefineField ("TestField",
															typeof (int),
															FieldAttributes.Private);


	   PropertyBuilder propertyBuilder = typeBuilder.DefineProperty ("TestProperty",
																	 PropertyAttributes.HasDefault,
																	 typeof (int),
																	 new Type[] { typeof (int) });

	   MethodBuilder getMethodBuilder = typeBuilder.DefineMethod ("TestGetMethod",
																  MethodAttributes.Public,
																  typeof (int),
																  new Type[] { });


	   ILGenerator IL = getMethodBuilder.GetILGenerator();

	   IL.Emit (OpCodes.Ldarg_0);
	   IL.Emit (OpCodes.Ldfld, fieldBuilder);
	   IL.Emit (OpCodes.Ret);


	   MethodBuilder setMethodBuilder = typeBuilder.DefineMethod ("TestSetMethod",
																  MethodAttributes.Public,
																  null,
																  new Type[] { typeof(int) });
	   IL = setMethodBuilder.GetILGenerator();

	   IL.Emit (OpCodes.Ldarg_0);
	   IL.Emit (OpCodes.Ldarg_1);
	   IL.Emit (OpCodes.Stfld, fieldBuilder);
	   IL.Emit (OpCodes.Ret);

	   propertyBuilder.SetGetMethod (getMethodBuilder);
	   propertyBuilder.SetSetMethod (setMethodBuilder);

	   typeBuilder.CreateType ();

       Type type = moduleBuilder.GetType ("TestType", true);

	   PropertyInfo propertyInfo = type.GetProperty ("TestProperty");

	   return 0;
   }
}
