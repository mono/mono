//
// AssemblyBuilderTest.cs - NUnit Test Cases for the AssemblyBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//


using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{

[TestFixture]
public class AssemblyBuilderTest : Assertion
{	
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class FooAttribute : Attribute
	{
		public FooAttribute (string arg)
		{
		}
	}

	static int nameIndex = 0;

	static AppDomain domain;

	[SetUp]
	protected void SetUp () {
		domain = Thread.GetDomain ();
	}

	private AssemblyName genAssemblyName () {
		AssemblyName assemblyName = new AssemblyName();
		assemblyName.Name = "MonoTests.System.Reflection.Emit.AssemblyBuilderTest" + (nameIndex ++);
		return assemblyName;
	}

	private AssemblyBuilder genAssembly () {
		return domain.DefineDynamicAssembly (genAssemblyName (),
											 AssemblyBuilderAccess.RunAndSave);
	}

	private MethodInfo genEntryFunction (AssemblyBuilder assembly) {
		ModuleBuilder module = assembly.DefineDynamicModule("module1");
		TypeBuilder tb = module.DefineType ("A");
		MethodBuilder mb = tb.DefineMethod ("A",
			MethodAttributes.Static, typeof (void), new Type [0]);
		mb.GetILGenerator ().Emit (OpCodes.Ret);
		return mb;
	}

	public void TestCodeBase () {
		AssemblyBuilder ab = genAssembly ();

		try {
			string codebase = ab.CodeBase;
			Fail ();
		}
		catch (NotSupportedException) {
		}
	}

	public void TestEntryPoint () {
		AssemblyBuilder ab = genAssembly ();

		AssertEquals ("EntryPoint defaults to null",
					  null, ab.EntryPoint);

		MethodInfo mi = genEntryFunction (ab);
		ab.SetEntryPoint (mi);

		AssertEquals ("EntryPoint works", mi, ab.EntryPoint);
	}

	public void TestSetEntryPoint () {
		AssemblyBuilder ab = genAssembly ();

		// Check invalid arguments
		try {
			ab.SetEntryPoint (null);
			Fail ();
		}
		catch (ArgumentNullException) {
		}

		// Check method from other assembly
		try {
			ab.SetEntryPoint (typeof (AssemblyBuilderTest).GetMethod ("TestSetEntryPoint"));
			Fail ();
		}
		catch (InvalidOperationException) {
		}
	}

	public void TestIsDefined () {
		AssemblyBuilder ab = genAssembly ();

		CustomAttributeBuilder cab = new CustomAttributeBuilder (typeof (FooAttribute).GetConstructor (new Type [1] {typeof (string)}), new object [1] { "A" });
		ab.SetCustomAttribute (cab);

		AssertEquals ("IsDefined works",
					  true, ab.IsDefined (typeof (FooAttribute), false));
		AssertEquals ("IsDefined works",
					  false, ab.IsDefined (typeof (AssemblyVersionAttribute), false));
	}

	
}
}

