//
// ILGeneratorTest.cs - NUnit Test Cases for the ILGenerator class
//
// Marek Safar (marek.safar@seznam.cz)
//
// (C) Novell, Inc.  http://www.novell.com

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit {

	[TestFixture]
	public class ILGeneratorTest {

		ILGenerator il_gen;

		[SetUp]
		public void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.ILGeneratorTest";

			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Run);

			ModuleBuilder module = assembly.DefineDynamicModule ("module1");
			TypeBuilder _tb = module.DefineType ("GetType", TypeAttributes.Public);

			MethodBuilder myMethod = _tb.DefineMethod("Function1",
				MethodAttributes.Public, typeof(String), null);

			il_gen = myMethod.GetILGenerator();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeclareLocal_NULL ()
		{
			il_gen.DeclareLocal (null);
		}
	}
}
