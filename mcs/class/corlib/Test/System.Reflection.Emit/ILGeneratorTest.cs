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

		TypeBuilder tb;
		ILGenerator il_gen;

		static TypeBuilder DefineDynType ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.ILGeneratorTest";

			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Run);

			ModuleBuilder module = assembly.DefineDynamicModule ("module1");
			return module.DefineType ("T", TypeAttributes.Public);			
		}
		
		void DefineBasicMethod ()
		{
			MethodBuilder mb = tb.DefineMethod("F",
				MethodAttributes.Public, typeof(string), null);
			il_gen = mb.GetILGenerator ();
		}

		[SetUp]
		public void SetUp ()
		{			
			tb = DefineDynType ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DeclareLocal_NULL ()
		{
			DefineBasicMethod ();

			il_gen.DeclareLocal (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DefineFilterBodyWithTypeNotNull ()
		{
			DefineBasicMethod ();

			il_gen.BeginExceptionBlock ();
			il_gen.EmitWriteLine ("in try");
			il_gen.BeginExceptFilterBlock ();
			il_gen.EmitWriteLine ("in filter head");
			il_gen.BeginCatchBlock (typeof (Exception));
			il_gen.EmitWriteLine ("in filter body");
			il_gen.EndExceptionBlock ();
		}
		
		// Bug 81431
		[Test]
		public void FilterAndCatchBlock ()
		{
			DefineBasicMethod ();
			ILGenerator il = il_gen;
			il.BeginExceptionBlock ();
			il.BeginExceptFilterBlock ();
			il.BeginCatchBlock (null);
			il.BeginCatchBlock (typeof (SystemException));
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InvalidFilterBlock1 ()
		{
			DefineBasicMethod ();
			ILGenerator il = il_gen;
			il.BeginExceptionBlock ();
			il.BeginExceptFilterBlock ();
			il.EndExceptionBlock ();
		}
		
		[Test]
		public void ValidFilterBlock1 ()
		{
			DefineBasicMethod ();
			ILGenerator il = il_gen;
			il.BeginExceptionBlock ();
			il.BeginExceptFilterBlock ();
			il.BeginFaultBlock ();
			il.EndExceptionBlock ();
		}
		
		[Test]
		public void ValidFilterBlock2 ()
		{
			DefineBasicMethod ();
			ILGenerator il = il_gen;
			il.BeginExceptionBlock ();
			il.BeginExceptFilterBlock ();
			il.BeginFinallyBlock ();
			il.EndExceptionBlock ();
		}
		
		/// <summary>
		/// Try to emit something like that:
		///
		/// .method public static bool TestFilter (bool execute_handler)
		/// {
		/// 	.locals init(bool)
		/// 	try {
		/// 		newobj  instance void [mscorlib]System.Exception::.ctor()
		/// 		throw
		/// 	} filter {
		/// 		pop
		/// 		ldarg.0
		/// 		endfilter
		/// 	} {
		/// 		ldc.i4.1
		/// 		stloc.0
		/// 		leave quit
		/// 	}
		/// 	ldc.i4.0
		/// 	stloc.0
		/// quit:
		/// 	ldloc.0
		/// 	ret
		/// }
		///
		/// It should return true if the handler has been executed
		/// Otherwise, the exception should not be catched
		/// </summary>
		void DefineTestFilterMethod ()
		{
			MethodBuilder mb = tb.DefineMethod("TestFilter",
				MethodAttributes.Public | MethodAttributes.Static, typeof(bool), new Type [] { typeof (bool) });

			ConstructorInfo exCtor = typeof (Exception).GetConstructor (new Type [0]);

			il_gen = mb.GetILGenerator ();
			il_gen.DeclareLocal (typeof (bool));
			Label quit = il_gen.DefineLabel ();
			il_gen.BeginExceptionBlock ();
			il_gen.Emit (OpCodes.Newobj, exCtor);
			il_gen.Emit (OpCodes.Throw);
			il_gen.BeginExceptFilterBlock ();
			il_gen.Emit (OpCodes.Pop);
			il_gen.Emit (OpCodes.Ldarg_0);
			il_gen.BeginCatchBlock (null);
			il_gen.Emit (OpCodes.Ldc_I4_1);
			il_gen.Emit (OpCodes.Stloc_0);
			il_gen.Emit (OpCodes.Leave, quit);
			il_gen.EndExceptionBlock ();
			il_gen.Emit (OpCodes.Ldc_I4_0);
			il_gen.Emit (OpCodes.Stloc_0);
			il_gen.MarkLabel (quit);
			il_gen.Emit (OpCodes.Ldloc_0);
			il_gen.Emit (OpCodes.Ret);
		}

		[Test]
		public void TestFilterEmittingWithHandlerExecution ()
		{
			DefineTestFilterMethod ();
			Type dynt = tb.CreateType ();
			
			MethodInfo tf = dynt.GetMethod ("TestFilter");
			Assert.IsTrue ((bool) tf.Invoke (null, new object [] { true }));
		}

		[Test]
		[ExpectedException (typeof (Exception))]
		public void TestFilterEmittingWithoutHandlerExecution ()
		{
			DefineTestFilterMethod ();
			Type dynt = tb.CreateType ();
			
			MethodInfo tf = dynt.GetMethod ("TestFilter");
			try {
				tf.Invoke (null, new object [] { false });
			} catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
		}
	}
}
