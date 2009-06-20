//
// PropertyBuilderTest.cs - NUnit Test Cases for the PropertyBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class PropertyBuilderTest
{	
    private TypeBuilder tb;

	private ModuleBuilder module;

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.PropertyBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
	    tb = module.DefineType("class1", 
							   TypeAttributes.Public);
	}

	[Test]
	public void TestProperties () {

		FieldBuilder propField = tb.DefineField("propField",
												typeof(int),
												FieldAttributes.Private);

		PropertyBuilder property = 
			tb.DefineProperty ("prop", PropertyAttributes.HasDefault, typeof (int), new Type [0] { });
		property.SetConstant (44);

		MethodBuilder propertyGet = tb.DefineMethod("GetProp",
													MethodAttributes.Public,
													typeof(int),
													new Type[] { });

		{
			ILGenerator il = propertyGet.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, propField);
			il.Emit(OpCodes.Ret);
		}

		MethodBuilder propertySet = tb.DefineMethod("SetProp",
													MethodAttributes.Public,
													null,
													new Type[] { typeof(int) });
		{
			ILGenerator il = propertySet.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, propField);
			il.Emit(OpCodes.Ret);
		}

		property.SetGetMethod(propertyGet);
		property.SetSetMethod(propertySet);

		Type t = tb.CreateType ();

		PropertyInfo[] props = t.GetProperties (BindingFlags.Public|BindingFlags.Instance);
		Assert.AreEqual (1, props.Length);

		PropertyInfo p = t.GetProperty ("prop");

		Assert.AreEqual (PropertyAttributes.HasDefault, p.Attributes);
		Assert.AreEqual (true, p.CanRead);
		Assert.AreEqual (true, p.CanWrite);
		Assert.AreEqual ("prop", p.Name);
		Assert.AreEqual (MemberTypes.Property, p.MemberType);
		Assert.AreEqual (typeof (int), p.PropertyType);
		MethodInfo[] methods = p.GetAccessors ();
		Assert.AreEqual (2, methods.Length);
		Assert.IsNotNull (p.GetGetMethod ());
		Assert.IsNotNull (p.GetSetMethod ());

		object o = Activator.CreateInstance (t);
		p.SetValue (o, 42, null);
		Assert.AreEqual (42, p.GetValue (o, null));
	}
}
}
