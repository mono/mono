//
// ConstructorBuilderTest.cs - NUnit Test Cases for the ConstructorBuilder class
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

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class ConstructorBuilderTest : Assertion
{	
    private TypeBuilder genClass;

	private ModuleBuilder module;

	[SetUp]
	protected void SetUp () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.ConstructorBuilderTest";

		AssemblyBuilder assembly 
			= Thread.GetDomain().DefineDynamicAssembly(
				assemblyName, AssemblyBuilderAccess.Run);

		module = assembly.DefineDynamicModule("module1");
		
		genClass = module.DefineType("class1", 
									 TypeAttributes.Public);
	}

	public void TestAttributes () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 MethodAttributes.Public, 0, new Type [0]);

		AssertEquals ("Attributes works", 
					  cb.Attributes, MethodAttributes.Public | MethodAttributes.SpecialName);
	}

	public void TestCallingConvention () {
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

	public void TestDeclaringType () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("DeclaringType works",
					  cb.DeclaringType, genClass);
	}

	public void TestInitLocals () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type[0]);

		AssertEquals ("InitLocals defaults to true", cb.InitLocals, true);
		cb.InitLocals = false;
		AssertEquals ("InitLocals is settable", cb.InitLocals, false);
	}

	public void TestMethodHandle () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		try {
			RuntimeMethodHandle handle = cb.MethodHandle;
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestName () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("Name works", ".ctor", cb.Name);

		ConstructorBuilder cb2 = genClass.DefineConstructor (MethodAttributes.Static, 0, new Type [0]);
		AssertEquals ("Static constructors have the right name", ".cctor", cb2.Name);
	}

	public void TestReflectedType () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReflectedType works", 
					  genClass, cb.ReflectedType);
	}

	public void TestReturnType () {
		ConstructorBuilder cb = genClass.DefineConstructor (0, 0, new Type [0]);

		AssertEquals ("ReturnType works", 
					  null, cb.ReturnType);
	}

	public void TestDefineParameter () {
		TypeBuilder tb = module.DefineType ("class7", TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [2] { typeof(int), typeof(int) });

		// index out of range
		try {
			cb.DefineParameter (0, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}
		try {
			cb.DefineParameter (3, 0, "param1");
			Fail ();
		} catch (ArgumentOutOfRangeException) {
		}

		// Normal usage
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (1, 0, "param1");
		cb.DefineParameter (2, 0, null);

		// Can not be called on a created type
		cb.GetILGenerator ().Emit (OpCodes.Ret);
		tb.CreateType ();
		try {
			cb.DefineParameter (1, 0, "param1");
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetCustomAttributes () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			0, 0, new Type [1] {typeof(int)});

		try {
			cb.GetCustomAttributes (true);
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			cb.GetCustomAttributes (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestMethodImplementationFlags () {
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
		TypeBuilder tb = module.DefineType ("class14", TypeAttributes.Public);
		ConstructorBuilder cb2 = tb.DefineConstructor (
			 0, 0, new Type [0]);

		cb2.GetILGenerator ().Emit (OpCodes.Ret);
		cb2.SetImplementationFlags (MethodImplAttributes.Managed);
		tb.CreateType ();
		try {
			cb2.SetImplementationFlags (MethodImplAttributes.OPTIL);
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestGetModule () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, new Type [0]);

		AssertEquals ("GetModule works",
					  module, cb.GetModule ());
	}

	public void TestGetParameters () {
		TypeBuilder tb = module.DefineType ("class16", TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [1] {typeof(int)});
		cb.GetILGenerator ().Emit (OpCodes.Ret);

		// Can't be called before CreateType ()
		/* This does not work under mono
		try {
			cb.GetParameters ();
			Fail ();
		} catch (InvalidOperationException) {
		}
		*/

		tb.CreateType ();

		/* This does not work under MS.NET !
		cb.GetParameters ();
		*/
	}

	public void TestGetToken () {
		TypeBuilder tb = module.DefineType ("class17", TypeAttributes.Public);
		ConstructorBuilder cb = tb.DefineConstructor (
			 0, 0, new Type [1] {typeof(void)});

		cb.GetToken ();
	}

	public void TestInvoke () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});

		try {
			cb.Invoke (null, new object [1] { 42 });
			Fail ();
		} catch (NotSupportedException) {
		}

		try {
			cb.Invoke (null, 0, null, new object [1] { 42 }, null);
			Fail ();
		} catch (NotSupportedException) {
		}
	}

	public void TestIsDefined () {
		ConstructorBuilder cb = genClass.DefineConstructor (
			 0, 0, 
			new Type [1] {typeof(int)});

		try {
			cb.IsDefined (null, true);
			Fail ();
		} catch (NotSupportedException) {
		}
	}
}
}
