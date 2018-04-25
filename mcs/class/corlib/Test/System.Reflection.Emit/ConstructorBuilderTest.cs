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
public class ConstructorBuilderTest
{
	private TypeBuilder genClass;
	private ModuleBuilder module;

	private static int typeIndexer = 0;

	[SetUp]
	public void SetUp ()
	{
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.ConstructorBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		genClass = module.DefineType(genTypeName (), TypeAttributes.Public);
	}

	// Return a unique type name
	private string genTypeName ()
	{
		return "class" + (typeIndexer ++);
	}

	[Test]
	public void Attributes ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			MethodAttributes.Public, 0, new Type [0]);

		Assert.IsTrue ((cb.Attributes & MethodAttributes.Public) != 0, "#1");
		Assert.IsTrue ((cb.Attributes & MethodAttributes.SpecialName) != 0, "#2");
	}

	[Test]
	public void CallingConvention ()
	{
		/* This does not work under MS.NET
		ConstructorBuilder cb3 = genClass.DefineConstructor (
			0, CallingConventions.VarArgs, new Type [0]);
		Assert.AreEqual (CallingConventions.VarArgs | CallingConventions.HasThis,
			cb3.CallingConvention, "#1");
		*/

		ConstructorBuilder cb4 = genClass.DefineConstructor (
			 MethodAttributes.Static, CallingConventions.Standard, new Type [0]);
		Assert.AreEqual (CallingConventions.Standard,
			cb4.CallingConvention, "#2");
	}

	[Test]
	public void DeclaringType ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type[0]);

		Assert.AreSame (genClass, cb.DeclaringType);
	}

	[Test]
	public void InitLocals ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type[0]);

		Assert.IsTrue (cb.InitLocals, "#1");
		cb.InitLocals = false;
		Assert.IsFalse (cb.InitLocals, "#2");
	}
	
	[Test]
	public void MethodHandle ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [0]);
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			RuntimeMethodHandle handle = cb.MethodHandle;
			Assert.Fail ("#A1:" + handle);
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			RuntimeMethodHandle handle = cb.MethodHandle;
			Assert.Fail ("#B1:" + handle);
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void Name ()
	{
		ConstructorBuilder cb;
		
		cb = genClass.DefineConstructor (0, 0, new Type [0]);
		Assert.AreEqual (".ctor", cb.Name, "#1");
		cb = genClass.DefineConstructor (MethodAttributes.Static, 0, new Type [0]);
		Assert.AreEqual (".cctor", cb.Name, "#2");
	}

	[Test]
	public void TestReflectedType ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		Assert.AreSame (genClass, cb.ReflectedType);
	}

	[Test]
	public void ReturnType ()
	{
		ConstructorBuilder cb;
		
		cb = genClass.DefineConstructor (0, 0, new Type [] { typeof (string) });
		Assert.IsNull (cb.ReturnType, "#1");
		cb = genClass.DefineConstructor (MethodAttributes.Static, 0, new Type [0]);
		Assert.IsNull (cb.ReturnType, "#2");
	}

	[Test]
	public void DefineParameter_Position_Negative ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [2] { typeof (int), typeof (int) });

		try {
			cb.DefineParameter (-1, ParameterAttributes.None, "param1");
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.ActualValue, "#3");
			Assert.IsNull (ex.InnerException, "#4");
			Assert.IsNotNull (ex.Message, "#5");
			Assert.IsNotNull (ex.ParamName, "#6");
		}
	}

	[Test]
	public void DefineParameter_Position_Max ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [2] { typeof (int), typeof (int) });

		try {
			cb.DefineParameter (3, 0, "param1");
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException ex) {
			// Specified argument was out of the range of valid values
			Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
			Assert.IsNull (ex.ActualValue, "#3");
			Assert.IsNull (ex.InnerException, "#4");
			Assert.IsNotNull (ex.Message, "#5");
			Assert.IsNotNull (ex.ParamName, "#6");
		}
	}

	[Test]
	public void DefineParameter_Position_Zero ()
	{
		// https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=341439
		// https://msdn.microsoft.com/en-us/library/system.reflection.emit.constructorbuilder.defineparameter(v=vs.110).aspx
		// "If you specify 0 (zero) for iSequence, this method returns
		// a ParameterBuilder instead of throwing an exception. There
		// is nothing useful that you can do with this
		// ParameterBuilder."

		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [2] { typeof (int), typeof (int) });

		var pb = cb.DefineParameter (0, ParameterAttributes.In, "param1");
		Assert.IsNotNull (pb);
	}

	[Test]
	public void DefineParameter ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [2] { typeof(int), typeof(int) });

		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (2, 0, null);

		cb.GetILGenerator ().Emit (OpCodes.Ret);
		genClass.CreateType ();

		try {
			cb.DefineParameter (1, 0, "param1");
			Assert.Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Unable to change after type has been created
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // GetCustomAttributes (Boolean)
	public void GetCustomAttributes1 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.GetCustomAttributes (true);
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.GetCustomAttributes (true);
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test] // GetCustomAttributes (Type, Boolean)
	public void GetCustomAttributes2 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.GetCustomAttributes (null, true);
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.GetCustomAttributes (null, true);
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void TestMethodImplementationFlags ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [0]);

		Assert.AreEqual (MethodImplAttributes.Managed | MethodImplAttributes.IL,
			cb.GetMethodImplementationFlags (), "#A1");
		cb.SetImplementationFlags (MethodImplAttributes.OPTIL);
		Assert.AreEqual (MethodImplAttributes.OPTIL,
			cb.GetMethodImplementationFlags (), "#A2");

		// Can not be called on a created type
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		ConstructorBuilder cb2 = tb.DefineConstructor (
			 0, 0, new Type [0]);

		cb2.GetILGenerator ().Emit (OpCodes.Ret);
		cb2.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			cb2.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Assert.Fail ("#B1");
		} catch (InvalidOperationException ex) {
			// Unable to change after type has been created
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void GetModule ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [0]);

		Assert.AreSame (module, cb.GetModule ());
	}

	[Test]
	public void GetParameters_Complete1 ()
	{
		ConstructorBuilder cb;
		ParameterInfo [] parameters;

		cb = genClass.DefineConstructor (MethodAttributes.Public,
			CallingConventions.Standard,
			new Type [] { typeof (int), typeof (string), typeof (bool) });
		cb.DefineParameter (3, ParameterAttributes.In, "param3a");
		cb.DefineParameter (3, ParameterAttributes.In, "param3b");
		cb.DefineParameter (2, ParameterAttributes.Out, "param2");
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		genClass.CreateType ();

		parameters = cb.GetParameters ();
		Assert.IsNotNull (parameters, "#A1");
		Assert.AreEqual (3, parameters.Length, "#A2");

		Assert.AreEqual (ParameterAttributes.None, parameters [0].Attributes, "#B1");
		Assert.IsNull (parameters [0].Name, "#B2");
		Assert.AreEqual (typeof (int), parameters [0].ParameterType, "#B3");
		Assert.AreEqual (0, parameters [0].Position, "#B4");

		Assert.AreEqual (ParameterAttributes.Out, parameters [1].Attributes, "#C1");
		Assert.AreEqual ("param2", parameters [1].Name, "#C2");
		Assert.AreEqual (typeof (string), parameters [1].ParameterType, "#C3");
		Assert.AreEqual (1, parameters [1].Position, "#C4");

		Assert.AreEqual (ParameterAttributes.In, parameters [2].Attributes, "#D1");
		Assert.AreEqual ("param3b", parameters [2].Name, "#D2");
		Assert.AreEqual (typeof (bool), parameters [2].ParameterType, "#D3");
		Assert.AreEqual (2, parameters [2].Position, "#D4");
	}

	[Test]
	public void GetParameters_Complete2 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			MethodAttributes.Public,
			CallingConventions.Standard, null);
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		genClass.CreateType ();

		ParameterInfo [] parameters = cb.GetParameters ();
		Assert.IsNotNull (parameters, "#1");
		Assert.AreEqual (0, parameters.Length, "#2");
	}

	[Test]
	public void GetParameters_Incomplete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [2] { typeof (int), typeof (string) });
		cb.DefineParameter (1, ParameterAttributes.In, "param1");
		cb.DefineParameter (2, ParameterAttributes.In, "param2");
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.GetParameters ();
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// Type has not been created
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	public void GetToken ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] { typeof(int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		MethodToken tokenA = cb.GetToken ();
		Assert.IsFalse (tokenA == MethodToken.Empty, "#1");

		genClass.CreateType ();

		MethodToken tokenB = cb.GetToken ();
		Assert.AreEqual (tokenA, tokenB, "#2");
	}

	[Test] // Invoke (Object [])
	public void Invoke1 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] { typeof(int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.Invoke (new object [1] { 42 });
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.Invoke (new object [1] { 42 });
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test] // Invoke (Object, Object [])
	public void Invoke2 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.Invoke (null, new object [1] { 42 });
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.Invoke (null, new object [1] { 42 });
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test] // Invoke (BindingFlags, Binder, Object [], CultureInfo)
	public void Invoke3 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.Invoke (0, null, new object [1] { 42 }, null);
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.Invoke (0, null, new object [1] { 42 }, null);
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test] // Invoke (Object, BindingFlags, Binder, Object [], CultureInfo)
	public void Invoke4 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test]
	public void IsDefined ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.IsDefined (null, true);
			Assert.Fail ("#A1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
		}

		genClass.CreateType ();

		try {
			cb.IsDefined (null, true);
			Assert.Fail ("#B1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
		}
	}

	[Test] // SetCustomAttribute (ConstructorInfo, Byte [])
	public void SetCustomAttribute1 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

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
			cb.SetCustomAttribute (null, new byte [2]);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("con", ex.ParamName, "#B5");
		}

		try {
			cb.SetCustomAttribute (ctorInfo, null);
			Assert.Fail ("#C1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.AreEqual ("binaryAttribute", ex.ParamName, "#C5");
		}
	}

	[Test] // SetCustomAttribute (CustomAttributeBuilder)
	public void SetCustomAttribute2 ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			MethodAttributes.Public, CallingConventions.Standard,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		TypeBuilder attrTb = module.DefineType ("TestAttribute",
			TypeAttributes.Public, typeof (Attribute));
		ConstructorBuilder attrCb = attrTb.DefineDefaultConstructor (
			MethodAttributes.Public);

		CustomAttributeBuilder cab = new CustomAttributeBuilder (
			attrCb, new object [0]);
		cb.SetCustomAttribute (cab);
		attrTb.CreateType ();
		
		Type emittedType  = genClass.CreateType ();
		ConstructorInfo ci = emittedType.GetConstructor (
			new Type [1] { typeof (int) });

		Assert.IsNotNull (ci, "#1");
		object [] cas = ci.GetCustomAttributes (false);
		Assert.IsNotNull (cas, "#2");
		Assert.AreEqual (1, cas.Length, "#3");
		Assert.AreEqual ("TestAttribute", cas [0].GetType ().FullName, "#4");
	}

	[Test]
	public void SetCustomAttribute2_CustomBuilder_Null ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			MethodAttributes.Public, CallingConventions.Standard,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		try {
			cb.SetCustomAttribute ((CustomAttributeBuilder) null);
			Assert.Fail ("#A1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.AreEqual ("customBuilder", ex.ParamName, "#A5");
		}

		genClass.CreateType ();

		try {
			cb.SetCustomAttribute ((CustomAttributeBuilder) null);
			Assert.Fail ("#B1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.AreEqual ("customBuilder", ex.ParamName, "#B5");
		}
	}

	[Test]
	public void GetCustomAttributes_Emitted ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, 
			new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });

		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		Type t = genClass.CreateType ();

		// Try the created type
		{
			ConstructorInfo ci = t.GetConstructors () [0];
			object[] attrs = ci.GetCustomAttributes (true);

			Assert.AreEqual (1, attrs.Length, "#A1");
			Assert.IsTrue (attrs [0] is ObsoleteAttribute, "#A2");
			Assert.AreEqual ("FOO", ((ObsoleteAttribute)attrs [0]).Message, "#A3");
		}

		// Try the type builder
		{
			ConstructorInfo ci = genClass.GetConstructors () [0];
			object[] attrs = ci.GetCustomAttributes (true);

			Assert.AreEqual (1, attrs.Length, "#B1");
			Assert.IsTrue (attrs [0] is ObsoleteAttribute, "#B2");
			Assert.AreEqual ("FOO", ((ObsoleteAttribute)attrs [0]).Message, "#B3");
		}
	}

	[Test] // GetCustomAttributes (Boolean)
	public void GetCustomAttributes1_Complete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		genClass.CreateType ();

		try {
			cb.GetCustomAttributes (false);
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // GetCustomAttributes (Boolean)
	public void GetCustomAttributes1_Incomplete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		try {
			cb.GetCustomAttributes (false);
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // GetCustomAttributes (Type, Boolean)
	public void GetCustomAttributes2_Complete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		genClass.CreateType ();

		try {
			cb.GetCustomAttributes (attrType, false);
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test] // GetCustomAttributes (Type, Boolean)
	public void GetCustomAttributes2_Incomplete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0,
			new Type [1] { typeof (int) });
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });
		cb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		try {
			cb.GetCustomAttributes (attrType, false);
			Assert.Fail ("#1");
		} catch (NotSupportedException ex) {
			// The invoked member is not supported in a dynamic
			// module
			Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	// Same as in MethodBuilderTest
	[Test]
	[Category ("MobileNotWorking")] // No declarative security in the mobile profile
	public void AddDeclarativeSecurity_Complete ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		ILGenerator ilgen = cb.GetILGenerator ();
		ilgen.Emit (OpCodes.Ret);
		genClass.CreateType ();

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			Assert.Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Type has not been created
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}

	[Test]
	[Category ("MobileNotWorking")] // No declarative security in the mobile profile
	public void AddDeclarativeSecurity_PSet_Null ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("pset", ex.ParamName, "#5");
		}
	}

	[Test]
	[Category ("MobileNotWorking")] // No declarative security in the mobile profile
	public void AddDeclarativeSecurity_Action_Invalid ()
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
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.ActualValue, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("action", ex.ParamName, "#5");
			}
		}
	}

	[Test]
	[Category ("MobileNotWorking")] // No declarative security in the mobile profile
	public void AddDeclarativeSecurity_Action_Duplicate ()
	{
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		try {
			cb.AddDeclarativeSecurity (SecurityAction.Demand, set);
			Assert.Fail ("#1");
		} catch (InvalidOperationException ex) {
			// Type has not been created
			Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
		}
	}
}
}
