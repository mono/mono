//
// MethodBuilderTest.cs - NUnit Test Cases for the MethodBuilder class
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
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class MethodBuilderTest : Assertion
{	
    private TypeBuilder genClass;

	private ModuleBuilder module;

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.MethodBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
		genClass = module.DefineType(genTypeName (), 
									 TypeAttributes.Public);
	}

	static int methodIndexer = 0;

	static int typeIndexer = 0;

	// Return a unique method name
	private string genMethodName () {
		return "m" + (methodIndexer ++);
	}

	// Return a unique type name
	private string genTypeName () {
		return "class" + (typeIndexer ++);
	}

	[Test]
	public void TestAttributes () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void), new Type [0]);

		AssertEquals ("Attributes works", 
					  MethodAttributes.Public, mb.Attributes);
	}

	[Test]
	public void TestCallingConvention () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type[0]);
		AssertEquals ("CallingConvetion defaults to Standard+HasThis",
					  CallingConventions.Standard | CallingConventions.HasThis,
					  mb.CallingConvention);

		MethodBuilder mb3 = genClass.DefineMethod (
			genMethodName (), 0, CallingConventions.VarArgs, typeof (void), new Type[0]);
		AssertEquals ("CallingConvetion works",
					  CallingConventions.VarArgs | CallingConventions.HasThis,
					  mb3.CallingConvention);

		MethodBuilder mb4 = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Static, CallingConventions.Standard,
			typeof (void), new Type [0]);
		AssertEquals ("Static implies !HasThis",
					  CallingConventions.Standard,
					  mb4.CallingConvention);
	}

	[Test]
	public void TestDeclaringType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type[0]);

		AssertEquals ("DeclaringType works",
					  genClass, mb.DeclaringType);
	}

	[Test]
	public void TestInitLocals () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type[0]);

		Assert ("InitLocals defaults to true", mb.InitLocals);
		mb.InitLocals = false;
		Assert ("InitLocals is settable", !mb.InitLocals);
	}
	
	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestMethodHandleIncomplete () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		RuntimeMethodHandle handle = mb.MethodHandle;
	}

	[Test]
	[ExpectedException (typeof(NotSupportedException))]
	public void TestMethodHandleComplete () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);
		mb.CreateMethodBody (new byte[2], 1);
		genClass.CreateType ();

		RuntimeMethodHandle handle = mb.MethodHandle;
	}

	[Test]
	public void TestName () {
		string name = genMethodName ();
		MethodBuilder mb = genClass.DefineMethod (
			name, 0, typeof (void), new Type [0]);

		AssertEquals ("Name works", name, mb.Name);
	}

	[Test]
	public void TestReflectedType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("ReflectedType works", 
					  genClass, mb.ReflectedType);
	}

	[Test]
	public void TestReturnType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (Console), new Type [0]);

		AssertEquals ("ReturnType works", typeof (Console),
					  mb.ReturnType);

		
		MethodBuilder mb2 = genClass.DefineMethod (
			genMethodName (), 0, null, new Type [0]);

		Assert ("void ReturnType works", (mb2.ReturnType == null) || (mb2.ReturnType == typeof (void)));
	}

	[Test]
	public void TestReturnTypeCustomAttributes () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (Console), new Type [0]);

		AssertEquals ("ReturnTypeCustomAttributes must be null", null,
					  mb.ReturnTypeCustomAttributes);
	}

	/*
	public void TestSignature () {
		MethodBuilder mb = genClass.DefineMethod (
			"m91", 0, typeof (Console), new Type [1] { typeof (Console) });

		Console.WriteLine (mb.Signature);
	}
	*/

	[Test]
	public void TestCreateMethodBody () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		// Clear body
		mb.CreateMethodBody (null, 999);

		// Check arguments 1.
		try {
			mb.CreateMethodBody (new byte[1], -1);
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}

		// Check arguments 2.
		try {
			mb.CreateMethodBody (new byte[1], 2);
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}

		mb.CreateMethodBody (new byte[2], 1);

		// Could only be called once
		try {
			mb.CreateMethodBody (new byte[2], 1);
			Fail ();
		} catch (InvalidOperationException) {
		}

		// Can not be called on a created type
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb2 = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);
		ILGenerator ilgen = mb2.GetILGenerator ();
		ilgen.Emit (OpCodes.Ret);
		tb.CreateType ();

		try {
			mb2.CreateMethodBody (new byte[2], 1);
			Fail ();
		} catch (InvalidOperationException) {
		}
	}

	[Test]
	[ExpectedException (typeof(InvalidOperationException))]
	public void TestDefineParameterInvalidIndexComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void),
			new Type[2] {
			typeof(int), typeof(int)
		});
		mb.CreateMethodBody (new byte[2], 1);
		tb.CreateType ();
		mb.DefineParameter (-5, ParameterAttributes.None, "param1");
	}

	[Test]
	[ExpectedException (typeof(InvalidOperationException))]
	public void TestDefineParameterValidIndexComplete ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void),
			new Type[2] {
			typeof(int), typeof(int)
		});
		mb.CreateMethodBody (new byte[2], 1);
		tb.CreateType ();
		mb.DefineParameter (1, ParameterAttributes.None, "param1");
	}

	[Test]
	public void TestDefineParameter () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [2] { typeof(int), typeof(int) });

		// index out of range

		// This fails on mono because the mono version accepts a 0 index
		/*
		try {
			mb.DefineParameter (0, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
		*/

		try {
			mb.DefineParameter (3, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}

		// Normal usage
		mb.DefineParameter (1, 0, "param1");
		mb.DefineParameter (1, 0, "param1");
		mb.DefineParameter (2, 0, null);

		mb.CreateMethodBody (new byte[2], 1);
		tb.CreateType ();
		try {
			mb.DefineParameter (1, 0, "param1");
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
#if NET_2_0
	// MS.NET 2.x no longer allows a zero length method body
	// to be emitted
	[ExpectedException (typeof (InvalidOperationException))]
#endif
	public void ZeroLengthBodyTest1 ()
	{
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [2] { typeof(int), typeof(int) });
		mb.CreateMethodBody (new byte[2], 0);
		genClass.CreateType ();
	}

	// A zero length method body can be created
	[Test]
	public void ZeroLengthBodyTest2 ()
	{
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [2] { typeof(int), typeof(int) });
		mb.CreateMethodBody (new byte[2], 0);
	}

	[Test]
	public void TestHashCode ()
	{
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		string methodName = genMethodName ();
		MethodBuilder mb = tb.DefineMethod (
			methodName, 0, typeof (void),
			new Type[2] {
			typeof(int), typeof(int)
		});
		AssertEquals ("Hashcode of method should be equal to hashcode of method name",
			methodName.GetHashCode (), mb.GetHashCode ());
	}

	[Test]
	public void TestGetBaseDefinition () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("GetBaseDefinition works",
					  mb.GetBaseDefinition (), mb);
	}

	[Test]
	public void TestGetILGenerator () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		// The same instance is returned on the second call
		ILGenerator ilgen1 = mb.GetILGenerator ();
		ILGenerator ilgen2 = mb.GetILGenerator ();

		AssertEquals ("The same ilgen is returned on the second call",
					  ilgen1, ilgen2);

		// Does not work on unmanaged code
		MethodBuilder mb2 = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);		
		try {
			mb2.SetImplementationFlags (MethodImplAttributes.Unmanaged);
			mb2.GetILGenerator ();
			Fail ();
		} catch (InvalidOperationException) {
		}
		try {
			mb2.SetImplementationFlags (MethodImplAttributes.Native);
			mb2.GetILGenerator ();
			Fail ();
		} catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestMethodImplementationFlags () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("MethodImplementationFlags defaults to Managed+IL",
					  MethodImplAttributes.Managed | MethodImplAttributes.IL,
					  mb.GetMethodImplementationFlags ());

		mb.SetImplementationFlags (MethodImplAttributes.OPTIL);

		AssertEquals ("SetImplementationFlags works",
					  MethodImplAttributes.OPTIL, 
					  mb.GetMethodImplementationFlags ());

		mb.CreateMethodBody (new byte[2], 1);
		mb.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			mb.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	[Test]
	public void TestGetModule () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("GetMethod works", module, 
					  mb.GetModule ());
	}

	[Test]
	public void TestGetParameters () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [1] {typeof(void)});

		/*
		 * According to the MSDN docs, this method should fail with a
		 * NotSupportedException. In reality, it throws an 
		 * InvalidOperationException under MS .NET, and returns the 
		 * requested data under mono.
		 */
		/*
		try {
			mb.GetParameters ();
			Fail ("#161");
		} catch (InvalidOperationException ex) {
			Console.WriteLine (ex);
		}
		*/
	}

	[Test]
	public void TestGetToken () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [1] {typeof(void)});

		mb.GetToken ();
	}

	[Test]
	public void TestInvoke () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [1] {typeof(int)});

		try {
			mb.Invoke (null, new object [1] { 42 });
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			mb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	[Test]
	[ExpectedException (typeof (NotSupportedException))]
	public void TestIsDefined () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [1] {typeof(int)});
		mb.IsDefined (null, true);
	}

	[Test]
	public void TestGetCustomAttributes () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [1] {typeof(int)});
		
		try {
			mb.GetCustomAttributes (true);
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			mb.GetCustomAttributes (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	[Test]
	public void TestSetCustomAttribute () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		string name = genMethodName ();
		MethodBuilder mb = tb.DefineMethod (
			name, MethodAttributes.Public, typeof (void), 
			new Type [1] {typeof(int)});

		// Null argument
		try {
			mb.SetCustomAttribute (null);
			Fail ();
		} catch (ArgumentNullException) {
		}

		byte[] custAttrData = { 1, 0, 0, 0, 0};
		Type attrType = Type.GetType
			("System.Reflection.AssemblyKeyNameAttribute");
		Type[] paramTypes = new Type[1];
		paramTypes[0] = typeof(String);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor(paramTypes);

		mb.SetCustomAttribute (ctorInfo, custAttrData);

		// Test MethodImplAttribute
		mb.SetCustomAttribute (new CustomAttributeBuilder (typeof (MethodImplAttribute).GetConstructor (new Type[1] { typeof (short) }), new object[1] {(short)MethodImplAttributes.Synchronized}));
		mb.GetILGenerator ().Emit (OpCodes.Ret);

		Type t = tb.CreateType ();

		AssertEquals ("Setting MethodImplAttributes works",
					  t.GetMethod (name).GetMethodImplementationFlags (),
					  MethodImplAttributes.Synchronized);

		// Null arguments again
		try {
			mb.SetCustomAttribute (null, new byte[2]);
			Fail ();
		} catch (ArgumentNullException) {
		}

		try {
			mb.SetCustomAttribute (ctorInfo, null);
			Fail ();
		} catch (ArgumentNullException) {
		}
	}

	[AttributeUsage (AttributeTargets.Parameter)]
	class PrivateAttribute : Attribute {

		public PrivateAttribute () {
		}
	}

	[Test]
	public void GetCustomAttributes () {
		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod ("foo", MethodAttributes.Public, 
											typeof (void),
											new Type [1] {typeof(int)});
		mb.GetILGenerator ().Emit (OpCodes.Ret);

		Type attrType = typeof (ObsoleteAttribute);
		ConstructorInfo ctorInfo =
			attrType.GetConstructor (new Type [] { typeof (String) });

		mb.SetCustomAttribute (new CustomAttributeBuilder (ctorInfo, new object [] { "FOO" }));

		// Check that attributes not accessible are not returned
		mb.SetCustomAttribute (new CustomAttributeBuilder (typeof (PrivateAttribute).GetConstructor (new Type [0]), new object [] { }));

		Type t = tb.CreateType ();

		// Try the created type
		{
			MethodInfo mi = t.GetMethod ("foo");
			object[] attrs = mi.GetCustomAttributes (true);

			AssertEquals (1, attrs.Length);
			Assert (attrs [0] is ObsoleteAttribute);
			AssertEquals ("FOO", ((ObsoleteAttribute)attrs [0]).Message);
		}

		// Try the type builder
		{
			MethodInfo mi = tb.GetMethod ("foo");
			object[] attrs = mi.GetCustomAttributes (true);

			AssertEquals (1, attrs.Length);
			Assert (attrs [0] is ObsoleteAttribute);
			AssertEquals ("FOO", ((ObsoleteAttribute)attrs [0]).Message);
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityAlreadyCreated () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void),
			new Type [0]);
		ILGenerator ilgen = mb.GetILGenerator ();
		ilgen.Emit (OpCodes.Ret);
		genClass.CreateType ();

		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void TestAddDeclarativeSecurityNullPermissionSet () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void), 
			new Type [0]);
		mb.AddDeclarativeSecurity (SecurityAction.Demand, null);
	}

	[Test]
	public void TestAddDeclarativeSecurityInvalidAction () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void), 
			new Type [0]);

		SecurityAction[] actions = new SecurityAction [] { 
			SecurityAction.RequestMinimum,
			SecurityAction.RequestOptional,
			SecurityAction.RequestRefuse };
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);

		foreach (SecurityAction action in actions) {
			try {
				mb.AddDeclarativeSecurity (action, set);
				Fail ();
			}
			catch (ArgumentException) {
			}
		}
	}

	[Test]
	[ExpectedException (typeof (InvalidOperationException))]
	public void TestAddDeclarativeSecurityDuplicateAction () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void), 
			new Type [0]);
		PermissionSet set = new PermissionSet (PermissionState.Unrestricted);
		mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
		mb.AddDeclarativeSecurity (SecurityAction.Demand, set);
	}

	[AttributeUsage (AttributeTargets.Parameter)]
	class ParamAttribute : Attribute {

		public ParamAttribute () {
		}
	}

	[Test]
	public void TestDynamicParams () {
		string mname = genMethodName ();

		MethodBuilder mb = genClass.DefineMethod (
			mname, MethodAttributes.Public, typeof (void), 
			new Type [] { typeof (int), typeof (string) });
		ParameterBuilder pb = mb.DefineParameter (1, ParameterAttributes.In, "foo");
		pb.SetConstant (52);
		pb.SetCustomAttribute (new CustomAttributeBuilder (typeof (ParamAttribute).GetConstructors () [0], new object [] { }));
		ParameterBuilder pb2 = mb.DefineParameter (2, 0, "bar");
		pb2.SetConstant ("foo");
		mb.GetILGenerator ().Emit (OpCodes.Ret);

		Type t = genClass.CreateType ();
		MethodInfo m = t.GetMethod (mname);
		ParameterInfo[] pi = m.GetParameters ();

		AssertEquals ("foo", pi [0].Name);
		AssertEquals (true, pi [0].IsIn);
		AssertEquals (52, pi [0].DefaultValue);
		object[] cattrs = pi [0].GetCustomAttributes (true);

		AssertEquals ("foo", pi [1].DefaultValue);
		

		/* This test does not run under MS.NET: */
		/*
		  AssertEquals (1, cattrs.Length);
		  AssertEquals (typeof (ParamAttribute), cattrs [0].GetType ());
		*/
	}

#if NET_2_0
	[Test]
	public void SetCustomAttribute_DllImport1 () {
		string mname = genMethodName ();

		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			mname, MethodAttributes.Public, typeof (void), 
			new Type [] { typeof (int), typeof (string) });

		// Create an attribute with default values
		mb.SetCustomAttribute (new CustomAttributeBuilder(typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) }), new object[] { "kernel32" }));

		Type t = tb.CreateType ();

		DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

		AssertEquals (CallingConvention.Winapi, attr.CallingConvention);
		AssertEquals (mname, attr.EntryPoint);
		AssertEquals ("kernel32", attr.Value);
		AssertEquals (false, attr.ExactSpelling);
		AssertEquals (true, attr.PreserveSig);
		AssertEquals (false, attr.SetLastError);
		AssertEquals (false, attr.BestFitMapping);
		AssertEquals (false, attr.ThrowOnUnmappableChar);
	}

	[Test]
	public void SetCustomAttribute_DllImport2 () {
		string mname = genMethodName ();

		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			mname, MethodAttributes.Public, typeof (void), 
			new Type [] { typeof (int), typeof (string) });

		CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] {typeof (DllImportAttribute).GetField ("EntryPoint"), typeof (DllImportAttribute).GetField ("CallingConvention"), typeof (DllImportAttribute).GetField ("CharSet"), typeof (DllImportAttribute).GetField ("ExactSpelling"), typeof (DllImportAttribute).GetField ("PreserveSig")}, new object [] { "bar", CallingConvention.StdCall, CharSet.Unicode, true, false });
		mb.SetCustomAttribute (cb);

		Type t = tb.CreateType ();

		DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

		AssertEquals (CallingConvention.StdCall, attr.CallingConvention);
		AssertEquals (CharSet.Unicode, attr.CharSet);
		AssertEquals ("bar", attr.EntryPoint);
		AssertEquals ("foo", attr.Value);
		AssertEquals (true, attr.ExactSpelling);
		AssertEquals (false, attr.PreserveSig);
		AssertEquals (false, attr.SetLastError);
		AssertEquals (false, attr.BestFitMapping);
		AssertEquals (false, attr.ThrowOnUnmappableChar);
	}

	[Test]
	public void SetCustomAttribute_DllImport3 () {
		string mname = genMethodName ();

		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			mname, MethodAttributes.Public, typeof (void), 
			new Type [] { typeof (int), typeof (string) });

		// Test attributes with three values (on/off/missing)
		CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] { typeof (DllImportAttribute).GetField ("BestFitMapping"), typeof (DllImportAttribute).GetField ("ThrowOnUnmappableChar")}, new object [] { false, false });
		mb.SetCustomAttribute (cb);

		Type t = tb.CreateType ();

		DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

		AssertEquals (false, attr.BestFitMapping);
		AssertEquals (false, attr.ThrowOnUnmappableChar);
	}

	[Test]
	public void SetCustomAttribute_DllImport4 () {
		string mname = genMethodName ();

		TypeBuilder tb = module.DefineType (genTypeName (), TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			mname, MethodAttributes.Public, typeof (void), 
			new Type [] { typeof (int), typeof (string) });

		CustomAttributeBuilder cb = new CustomAttributeBuilder (typeof (DllImportAttribute).GetConstructor (new Type [] {typeof (String)}), new object [] { "foo" }, new FieldInfo [] { typeof (DllImportAttribute).GetField ("SetLastError"), typeof (DllImportAttribute).GetField ("BestFitMapping"), typeof (DllImportAttribute).GetField ("ThrowOnUnmappableChar")}, new object [] { true, true, true });
		mb.SetCustomAttribute (cb);

		Type t = tb.CreateType ();

		DllImportAttribute attr = (DllImportAttribute)((t.GetMethod (mname).GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

		AssertEquals (true, attr.SetLastError);
		AssertEquals (true, attr.BestFitMapping);
		AssertEquals (true, attr.ThrowOnUnmappableChar);
	}
#endif
}
}
