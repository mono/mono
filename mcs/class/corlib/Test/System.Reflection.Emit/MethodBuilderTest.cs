//
// MethodBuilderTest.cs - NUnit Test Cases for the MethodBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com

// TODO:
//  - implement 'Signature' (what the hell it does???) and test it
//  - implement Equals and test it
//  - AddDeclarativeSecurity

using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

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
		
		genClass = module.DefineType("class1", 
									 TypeAttributes.Public);
	}

	static int methodIndexer = 0;

	// Return a unique method name
	private string genMethodName () {
		return "m" + (methodIndexer ++);
	}

	public void TestAttributes () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), MethodAttributes.Public, typeof (void), new Type [0]);

		AssertEquals ("Attributes works", 
					  MethodAttributes.Public, mb.Attributes);
	}

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

	public void TestDeclaringType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type[0]);

		AssertEquals ("DeclaringType works",
					  genClass, mb.DeclaringType);
	}

	public void TestInitLocals () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type[0]);

		Assert ("InitLocals defaults to true", mb.InitLocals);
		mb.InitLocals = false;
		Assert ("InitLocals is settable", !mb.InitLocals);
	}

	public void TestMethodHandle () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		try {
			RuntimeMethodHandle handle = mb.MethodHandle;
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestName () {
		string name = genMethodName ();
		MethodBuilder mb = genClass.DefineMethod (
			name, 0, typeof (void), new Type [0]);

		AssertEquals ("Name works", name, mb.Name);
	}

	public void TestReflectedType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("ReflectedType works", 
					  genClass, mb.ReflectedType);
	}

	public void TestReturnType () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (Console), new Type [0]);

		AssertEquals ("ReturnType works", typeof (Console),
					  mb.ReturnType);

		
		MethodBuilder mb2 = genClass.DefineMethod (
			genMethodName (), 0, null, new Type [0]);

		AssertEquals ("ReturnType is null", null,
					  mb2.ReturnType);
	}

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

	public void TestCreateMethodBody () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		// Clear body
		mb.CreateMethodBody (null, 999);

		// Check arguments 1.
		try {
			mb.CreateMethodBody (new byte[1], -1);
			Fail ();
		} catch (ArgumentException) {
		}

		// Check arguments 2.
		try {
			mb.CreateMethodBody (new byte[1], 2);
			Fail ();
		} catch (ArgumentException) {
		}

		mb.CreateMethodBody (new byte[2], 1);

		// Could only be called once
		try {
			mb.CreateMethodBody (new byte[2], 1);
			Fail ();
		} catch (InvalidOperationException) {
		}

		// Can not be called on a created type
		TypeBuilder tb = module.DefineType ("class6", TypeAttributes.Public);
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

	public void TestDefineParameter () {
		TypeBuilder tb = module.DefineType ("class7", TypeAttributes.Public);
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

		// Can not be called on a created type
		mb.CreateMethodBody (new byte[2], 0);
		tb.CreateType ();
		try {
			mb.DefineParameter (1, 0, "param1");
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetBaseDefinition () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("GetBaseDefinition works",
					  mb.GetBaseDefinition (), mb);
	}

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

	public void TestMethodImplementationFlags () {
		TypeBuilder tb = module.DefineType ("class14", TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("MethodImplementationFlags defaults to Managed+IL",
					  MethodImplAttributes.Managed | MethodImplAttributes.IL,
					  mb.GetMethodImplementationFlags ());

		mb.SetImplementationFlags (MethodImplAttributes.OPTIL);

		AssertEquals ("SetImplementationFlags works",
					  MethodImplAttributes.OPTIL, 
					  mb.GetMethodImplementationFlags ());

		// Can not be called on a created type
		mb.CreateMethodBody (new byte[2], 0);
		mb.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			mb.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetModule () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [0]);

		AssertEquals ("GetMethod works", module, 
					  mb.GetModule ());
	}

	public void TestGetParameters () {
		TypeBuilder tb = module.DefineType ("class16", TypeAttributes.Public);
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

	public void TestGetToken () {
		TypeBuilder tb = module.DefineType ("class17", TypeAttributes.Public);
		MethodBuilder mb = tb.DefineMethod (
			genMethodName (), 0, typeof (void), new Type [1] {typeof(void)});

		mb.GetToken ();
	}

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

	public void TestIsDefined () {
		MethodBuilder mb = genClass.DefineMethod (
			genMethodName (), 0, typeof (void), 
			new Type [1] {typeof(int)});

		try {
			mb.IsDefined (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

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

	public void TestSetCustomAttribute () {
		TypeBuilder tb = module.DefineType ("class21", TypeAttributes.Public);
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
}
}
