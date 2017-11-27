using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;

public class Tests
{
	public static int Main (String[] args) {
		AssemblyName an = new AssemblyName ("dynamicAssembly");
		var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
		ModuleBuilder mb = ab.DefineDynamicModule ("mainModule.dll");

		// define a generic class Foo<T> with public field T value
		TypeBuilder foob = mb.DefineType ("Foo", TypeAttributes.Public);
		GenericTypeParameterBuilder t = foob.DefineGenericParameters ("T")[0];
		FieldBuilder fb = foob.DefineField ("value", t, FieldAttributes.Public);

		Type foo = foob.CreateType ();
		var f = foo.GetField ("value");

		// define a static class Bar with generic method Bar.Get : Foo<'a> -> 'a
		TypeBuilder barb = mb.DefineType ("Bar", TypeAttributes.Public);

		var getB = barb.DefineMethod ("Get", MethodAttributes.Public | MethodAttributes.Static);
		GenericTypeParameterBuilder a = getB.DefineGenericParameters ("a")[0];
		getB.SetReturnType (a);
		Type fooT = foo.MakeGenericType ((Type) a);
		getB.SetParameters (fooT);


		// emit method body
		var ilGen = getB.GetILGenerator ();

		ilGen.Emit (OpCodes.Ldarg_0);
		ilGen.Emit (OpCodes.Ldfld, TypeBuilder.GetField (fooT, f));
		ilGen.Emit (OpCodes.Ret);

		var bar = barb.CreateType ();

		// emission complete
		var ga = bar.GetMethod ("Get").GetGenericArguments ()[0];
		var field = foo.MakeGenericType (ga).GetField ("value");

		Console.WriteLine ("Name: {0}", field.Name);
		Console.WriteLine ("Token: {0}", field.MetadataToken);

		return 0;
	}
}

