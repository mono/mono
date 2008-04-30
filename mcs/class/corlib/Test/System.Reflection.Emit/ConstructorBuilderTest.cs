//
// ConstructorBuilderTest.cs - NUnit Test Cases for the ConstructorBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

// TODO:
//  - implement 'Signature' (what the hell it does???) and test it
//  - implement Equals and test it

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class ConstructorBuilderTest : Assertion
{
	private TypeBuilder genClass;
	private ModuleBuilder module;

	private static int typeIndexer = 0;

	[SetUp]
	protected void SetUp ()
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.ConstructorBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
		genClass = module.DefineType(genTypeName (), 
									 TypeAttributes.Public);
	}

	// Return a unique type name
	private string genTypeName ()
	{
		return "class" + (typeIndexer ++);
	}

	[Test]
	public void TestAttributes ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);

		Assert ("Attributes works", 
				(cb.Attributes & MethodAttributes.Public) != 0);
		Assert ("Attributes works", 
				(cb.Attributes & MethodAttributes.SpecialName) != 0);
	}

	[Test]
	public void TestCallingConvention ()
	{
		/* This does not work under MS.NET
		ConstructorBuilder cb3 = genClass.DefineConstructor (
			0, CallingConventions.VarArgs, new Type [0]);
		AssertEquals ("CallingConvetion works",
					  CallingConventions.VarArgs | CallingConventions.HasThis,
					  cb3.CallingConvention);
		*/

		ConstructorBuilder cb4 = genClass.DefineConstructor (
			 MethodAttributes.Static, CallingConventions.Standard, new Type [0]);
		AssertEquals ("Static implies !HasThis",
					  cb4.CallingConvention,
					  CallingConventions.Standard);
	}

	[Test]
	public void TestDeclaringType ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("DeclaringType works",
					  cb.DeclaringType, genClass);
	}

	[Test]
	public void TestInitLocals ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("InitLocals defaults to true", cb.InitLocals, true);
		cb.InitLocals = false;
		AssertEquals ("InitLocals is settable", cb.InitLocals, false);
	}
	
	[Test]
	public void TestMethodHandle ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		genClass.CreateType ();

		try {
			RuntimeMethodHandle handle = cb.MethodHandle;
			Fail ("#1:" + handle);
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestName ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("Name works", ".ctor", cb.Name);

		ConstructorBuilder cb2 = genClass.DefineConstructor (MethodAttributes.Static, 0, new Type [0]);
		AssertEquals ("Static constructors have the right name", ".cctor", cb2.Name);
	}

	[Test]
	public void TestReflectedType ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReflectedType works", 
					  genClass, cb.ReflectedType);
	}

	[Test]
	public void TestReturnType ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReturnType works", 
					  null, cb.ReturnType);
	}

	[Test]
	public void DefineParameter_Position_Negative ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof (int), typeof (int) });

		try {
			cb.DefineParameter (-1, ParameterAttributes.None, "param1");
			Fail ("#B1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid values
			AssertEquals ("#B2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#B3", ex.ActualValue);
			AssertNull ("#B4", ex.InnerException);
			AssertNotNull ("#B5", ex.Message);
			AssertNotNull ("#B6", ex.ParamName);
		}
	}

	[Test]
	public void DefineParameter_Position_Max ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof (int), typeof (int) });

		try {
			cb.DefineParameter (3, 0, "param1");
			Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid values
			AssertEquals ("#2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#3", ex.ActualValue);
			AssertNull ("#4", ex.InnerException);
			AssertNotNull ("#5", ex.Message);
			AssertNotNull ("#6", ex.ParamName);
		}
	}

	[Test]
#if NET_2_0
	[Category ("NotDotNet")]
#endif
	public void DefineParameter_Position_Zero ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof (int), typeof (int) });

		try {
			cb.DefineParameter (0, ParameterAttributes.In, "param1");
			Fail ("#A1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid values
			AssertEquals ("#A2", typeof (ArgumentOutOfRangeException), ex.GetType ());
			AssertNull ("#A3", ex.ActualValue);
			AssertNull ("#A4", ex.InnerException);
			AssertNotNull ("#A5", ex.Message);
			AssertNotNull ("#A6", ex.ParamName);
		}
	}

	[Test]
	public void TestDefineParameter ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof(int), typeof(int) });

		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (2, 0, null);

		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();

		try {
			cb.DefineParameter (1, 0, "param1");
			Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Unable to change after type has been created
			AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestGetCustomAttributes ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] {typeof(int)});

		try {
			cb.GetCustomAttributes (true);
			Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#A2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
		}

		try {
			cb.GetCustomAttributes (null, true);
			Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#B2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
		}
	}

	[Test]
	public void TestMethodImplementationFlags ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		AssertEquals ("MethodImplementationFlags defaults to Managed+IL",
					  cb.GetMethodImplementationFlags (),
					  MethodImplAttributes.Managed | MethodImplAttributes.IL);

		cb.SetImplementationFlags (MethodImplAttributes.OPTIL);

		AssertEquals ("SetImplementationFlags works",
					  cb.GetMethodImplementationFlags (),
					  MethodImplAttributes.OPTIL);

		// Can not be called on a created type
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb2 = tb.DefineConstructor (
			 0, 0, new Type [0]);

		cb2.GetILGenerator ().Emit (OpCodes.Ret);
		cb2.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			cb2.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Unable to change after type has been created
			AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestGetModule ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		AssertEquals ("GetModule works",
					  module, cb.GetModule ());
	}

	[Test]
	public void GetParameters_Complete1 ()
	{
		TypeBuilder tb;
		ConstructorBuilder cb;
		ParameterInfo [] parameters;
		
		tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		cb = tb.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard,
			new Type [] { typeof (int), typeof (string), typeof (bool) });
		cb.DefineParameter (3, ParameterAttributes.In, "param3a");
		cb.DefineParameter (3, ParameterAttributes.In, "param3b");
		cb.DefineParameter (2, ParameterAttributes.Out, "param2");
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();

		parameters = cb.GetParameters ();
		AssertNotNull ("#A1", parameters);
		AssertEquals ("#A2", 3, parameters.Length);

		AssertEquals ("#B1", ParameterAttributes.None, parameters [0].Attributes);
		AssertNull ("#B2", parameters [0].Name);
		AssertEquals ("#B3", typeof (int), parameters [0].ParameterType);
		AssertEquals ("#B4", 0, parameters [0].Position);

		AssertEquals ("#C1", ParameterAttributes.Out, parameters [1].Attributes);
		AssertEquals ("#C2", "param2", parameters [1].Name);
		AssertEquals ("#C3", typeof (string), parameters [1].ParameterType);
		AssertEquals ("#C4", 1, parameters [1].Position);

		AssertEquals ("#D1", ParameterAttributes.In, parameters [2].Attributes);
		AssertEquals ("#D2", "param3b", parameters [2].Name);
		AssertEquals ("#D3", typeof (bool), parameters [2].ParameterType);
		AssertEquals ("#D4", 2, parameters [2].Position);
	}

	[Test]
#if ONLY_1_1
	[Category ("NotDotNet")] // ArgumentNullException in GetParameters
#endif
	public void GetParameters_Complete2 ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (MethodAttributes.Public,
			CallingConventions.Standard, null);
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();

		ParameterInfo [] parameters = cb.GetParameters ();
		AssertNotNull ("#1", parameters);
		AssertEquals ("#2", 0, parameters.Length);
	}

	[Test]
	public void GetParameters_Incomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof (int), typeof (string) });
		cb.DefineParameter (1, ParameterAttributes.In, "param1");
		cb.DefineParameter (2, ParameterAttributes.In, "param2");
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.GetParameters ();
			Fail ("#1");
#if NET_2_0
		} catch (NotSupportedException ex) {
			// Type has not been created
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
#else
		} catch (InvalidOperationException ex) {
			// Type has not been created
			AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
#endif
	}

	[Test]
	public void TestGetToken ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [1] {typeof(void)});

		cb.GetToken ();
	}

	[Test] // Invoke (Object [])
	public void Invoke1 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] {typeof(int)});

		try {
			cb.Invoke (new object [1] { 42 });
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // Invoke (Object, Object [])
	public void Invoke2 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });

		try {
			cb.Invoke (null, new object [1] { 42 });
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // Invoke (BindingFlags, Binder, Object [], CultureInfo)
	public void Invoke3 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });

		try {
			cb.Invoke (0, null, new object [1] { 42 }, null);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // Invoke (Object, BindingFlags, Binder, Object [], CultureInfo)
	public void Invoke4 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });

		try {
			cb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestIsDefined ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});

		try {
			cb.IsDefined (null, true);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestSetCustomAttribute ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		// Null argument
		try {
			cb.SetCustomAttribute (null);
			Fail ("#A1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#A2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#A3", ex.InnerException);
			AssertNotNull ("#A4", ex.Message);
			AssertEquals ("#A5", "customBuilder", ex.ParamName);
		}

		byte[] custAttrData = { 1, 0, 0, 0, 0};
		Type attrType = Type.GetType
			("System.Reflection.AssemblyKeyNameAttribute");
		Type[] paramTypes = new Type[1];
		paramTypes[0] = typeof(String);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor(paramTypes);

		cb.SetCustomAttribute (ctorInfo, custAttrData);

		// Null arguments again
		try {
			cb.SetCustomAttribute (null, new byte[2]);
			Fail ("#B1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#B2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#B3", ex.InnerException);
			AssertNotNull ("#B4", ex.Message);
			AssertEquals ("#B5", "con", ex.ParamName);
		}

		try {
			cb.SetCustomAttribute (ctorInfo, null);
			Fail ("#C1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#C2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#C3", ex.InnerException);
			AssertNotNull ("#C4", ex.Message);
			AssertEquals ("#C5", "binaryAttribute", ex.ParamName);
		}
	}

	[Test]
	public void GetCustomAttributes_Emitted ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 MethodAttributes.Public, 0, 
			new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });

		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		Type t = tb.CreateType ();

		// Try the created type
		{
			ConstructorInfo ci = t.GetConstructors () [0];
			object[] attrs = ci.GetCustomAttributes (true);

			AssertEquals (1, attrs.Length);
			Assert (attrs [0] is ObsoleteAttribute);
			AssertEquals ("FOO", ((ObsoleteAttribute)attrs [0]).Message);
		}

		// Try the type builder
		{
			ConstructorInfo ci = tb.GetConstructors () [0];
			object[] attrs = ci.GetCustomAttributes (true);

			AssertEquals (1, attrs.Length);
			Assert (attrs [0] is ObsoleteAttribute);
			AssertEquals ("FOO", ((ObsoleteAttribute)attrs [0]).Message);
		}
	}

	[Test] // GetCustomAttributes (Boolean)
	public void GetCustomAttributes1_Complete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		tb.CreateType ();

		try {
			cb.GetCustomAttributes (false);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // GetCustomAttributes (Boolean)
	public void GetCustomAttributes1_Incomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		try {
			cb.GetCustomAttributes (false);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // GetCustomAttributes (Type, Boolean)
	public void GetCustomAttributes2_Complete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		tb.CreateType ();

		try {
			cb.GetCustomAttributes (attrType, false);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test] // GetCustomAttributes (Type, Boolean)
	public void GetCustomAttributes2_Incomplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		try {
			cb.GetCustomAttributes (attrType, false);
			Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			AssertEquals ("#2", typeof (NotSupportedException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	// Same as in MethodBuilderTest
	[Test]
	public void TestAddDeclarativeSecurityAlreadyCreated ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		ILGenerator ilgen = cb.GetILGenerator ();
		ilgen.Emit (OpCodes.Ret);
		genClass.CreateType ();

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		} catch (InvalidOperationException ex) {
			// Type has not been created
			AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}

	[Test]
	public void TestAddDeclarativeSecurityNullPermissionSet ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, null);
			Fail ("#1");
		} catch (ArgumentNullException ex) {
			AssertEquals ("#2", typeof (ArgumentNullException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
			AssertEquals ("#5", "pset", ex.ParamName);
		}
	}

	[Test]
	public void TestAddDeclarativeSecurityInvalidAction ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);

		SecurityAction[] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

		foreach (SecurityAction action in actions) {
			try {
				cb.AddDeclarativeSecurity (action, set);
				Fail ();
			} catch (ArgumentOutOfRangeException) {
			}
		}
	}

	[Test]
	public void TestAddDeclarativeSecurityDuplicateAction ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Type has not been created
			AssertEquals ("#2", typeof (InvalidOperationException), ex.GetType ());
			AssertNull ("#3", ex.InnerException);
			AssertNotNull ("#4", ex.Message);
		}
	}
}
}
