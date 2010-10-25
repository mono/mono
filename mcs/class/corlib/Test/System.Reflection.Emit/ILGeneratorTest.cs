//
// ILGeneratorTest.cs - NUnit Test Cases for the ILGenerator class
//
// Marek Safar (marek.safar@seznam.cz)
//
// (C) Novell, Inc.  http://www.novell.com

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class ILGeneratorTest
	{
		TypeBuilder tb;
		ILGenerator il_gen;

		void DefineBasicMethod ()
		{
			MethodBuilder mb = tb.DefineMethod("F",
				MethodAttributes.Public, typeof(string), null);
			il_gen = mb.GetILGenerator ();
		}

		[SetUp]
		public void SetUp ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.ILGeneratorTest";

			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (
				assemblyName, AssemblyBuilderAccess.Run);

			ModuleBuilder module = assembly.DefineDynamicModule ("module1");
			tb = module.DefineType ("T", TypeAttributes.Public);
		}

		[Test]
		public void DeclareLocal_LocalType_Null ()
		{
			DefineBasicMethod ();

			try {
				il_gen.DeclareLocal (null);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNotNull (ex.ParamName, "#A5");
				Assert.AreEqual ("localType", ex.ParamName, "#A");
			}

#if NET_2_0
			try {
				il_gen.DeclareLocal (null, false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNotNull (ex.ParamName, "#B5");
				Assert.AreEqual ("localType", ex.ParamName, "#B6");
			}
#endif
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

		[Test] // bug #81431
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

		[Test] // Emit (OpCode, ConstructorInfo)
#if NET_2_0
		[Category ("NotDotNet")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304610
#endif
		public void Emit3_Constructor_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Newobj, (ConstructorInfo) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
		}

#if NET_2_0
		[Test] // Emit (OpCode, ConstructorInfo)
		[Category ("NotWorking")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304610
		public void Emit3_Constructor_Null_MS ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Newobj, (ConstructorInfo) null);
				Assert.Fail ("#1");
			} catch (NullReferenceException) {
			}
		}
#endif

		[Test] // Emit (OpCode, FieldInfo)
		public void Emit5_Field_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Ldsfld, (FieldInfo) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
		}

		[Test] // Emit (OpCode, Label [])
		[Category ("NotDotNet")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304610
		public void Emit10_Labels_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (Label []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("labels", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("NotWorking")] // MS bug: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304610
		public void Emit10_Labels_Null_MS ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (Label []) null);
				Assert.Fail ("#1");
			} catch (NullReferenceException) {
			}
		}

		[Test] // Emit (OpCode, LocalBuilder)
		public void Emit11_Local_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (LocalBuilder) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("local", ex.ParamName, "#6");
			}
		}

		[Test] // Emit (OpCode, MethodInfo)
		public void Emit12_Method_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (MethodInfo) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("meth", ex.ParamName, "#6");
			}
		}

		[Test] // Emit (OpCode, SignatureHelper)
		public void Emit14_Signature_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (SignatureHelper) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
		}

		[Test] // Emit (OpCode, String)
		public void Emit16_String_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (String) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // Emit (OpCode, Type)
		public void Emit16_Type_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.Emit (OpCodes.Switch, (Type) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void EmitCall_MethodInfo_Null ()
		{
			DefineBasicMethod ();
			try {
				il_gen.EmitCall (OpCodes.Call, (MethodInfo) null, null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("methodInfo", ex.ParamName, "#6");
			}
		}

		[Test]
		public void TestFilterEmittingWithHandlerExecution ()
		{
			DefineTestFilterMethod ();
			Type dynt = tb.CreateType ();
			
			MethodInfo tf = dynt.GetMethod ("TestFilter");
			Assert.IsTrue ((bool) tf.Invoke (null, new object [] { true }));
		}

#if NET_2_0
		delegate void FooFoo ();

		static void Foo ()
		{
		}

		[Test]
		public void TestEmitCalliWithNullReturnType ()
		{
			MethodBuilder mb = tb.DefineMethod ("F",
				MethodAttributes.Public | MethodAttributes.Static, null, new Type [] { typeof (IntPtr) });
			mb.SetImplementationFlags (MethodImplAttributes.NoInlining);
			il_gen = mb.GetILGenerator ();
			il_gen.Emit (OpCodes.Ldarg_0);
			il_gen.EmitCalli (OpCodes.Calli, CallingConvention.StdCall, null, Type.EmptyTypes);
			il_gen.Emit (OpCodes.Ret);
	
			Type dynt = tb.CreateType ();
			dynt.GetMethod ("F", BindingFlags.Public | BindingFlags.Static).Invoke (
				null, new object [] { Marshal.GetFunctionPointerForDelegate (new FooFoo (Foo)) });
		}
#endif

#if NET_2_0
		//Test for #509131
		[Test]
		public void TestEmitCallIgnoresOptionalArgsForNonVarargMethod ()
		{
			DefineBasicMethod ();
			try {
				il_gen.EmitCall (OpCodes.Call, typeof (object).GetMethod ("GetHashCode"), new Type[] { typeof (string) });
			} catch (InvalidOperationException ex) {
				Assert.Fail ("#1");
			}
		}
#else
		[Test]
		public void TestEmitCallThrowsOnOptionalArgsForNonVarargMethod ()
		{
			DefineBasicMethod ();
			try {
				il_gen.EmitCall (OpCodes.Call, typeof (object).GetMethod ("GetHashCode"), new Type[] { typeof (string) });
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
			}
		}
#endif

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

		[Test]
		public void TestEmitLocalInfoWithNopOpCode ()
		{
			var method_builder = tb.DefineMethod ("linop", MethodAttributes.Public | MethodAttributes.Static, typeof (bool), Type.EmptyTypes);
			il_gen = method_builder.GetILGenerator ();

			var local = il_gen.DeclareLocal (typeof (int));
			il_gen.Emit (OpCodes.Nop, local);
			il_gen.Emit (OpCodes.Ldc_I4_1);
			il_gen.Emit (OpCodes.Ret);

			var type = tb.CreateType ();
			var method = type.GetMethod ("linop");

			Assert.IsNotNull (method);
			Assert.IsTrue ((bool) method.Invoke (null, new object [0]));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LdObjByRef () {
			DefineBasicMethod ();
			ILGenerator ig = il_gen;

			ig.Emit (OpCodes.Ldtoken, typeof (int).MakeByRefType ());
		}



		[Test] //bug #649017
		public void GtdEncodingAsOpenInstance () {
	        AssemblyName asmname = new AssemblyName ();
	        asmname.Name = "test";
	        AssemblyBuilder asmbuild = Thread.GetDomain ().DefineDynamicAssembly (asmname, AssemblyBuilderAccess.RunAndSave);
	        ModuleBuilder modbuild = asmbuild.DefineDynamicModule ("modulename", "test.exe");
	
	        TypeBuilder myType = modbuild.DefineType ("Sample", TypeAttributes.Public);
	
	        string[] typeParamNames = { "TFirst" };
	        myType.DefineGenericParameters (typeParamNames);
	
	        var nested = myType.DefineNestedType ("nested");
	        nested.DefineGenericParameters (typeParamNames);
	
	        var m = myType.DefineMethod ("test", MethodAttributes.Public);
	        m.SetParameters (myType);
	
	        var ilgen = m.GetILGenerator ();
	        ilgen.Emit (OpCodes.Castclass, nested);
	        ilgen.Emit (OpCodes.Castclass, typeof (List<>));
	        ilgen.Emit (OpCodes.Ldtoken, nested);
	        ilgen.Emit (OpCodes.Ldtoken, typeof (List<>));
	
	        var baked = myType.CreateType ();
	        nested.CreateType ();
	
			var method = baked.GetMethod ("test");
			var body = method.GetMethodBody ();
			/*
			The resulting IL is:
			[ 0] 0x74 token:uint
			[ 5] 0x74 token:uint
			[10] 0xd0 token:uint
			[10] 0xd0 token:uint
			The first two tokens must be to typespecs and the last two to typeref/typedef*/
			var il = body.GetILAsByteArray ();
		
			Assert.AreEqual (20, il.Length, "#1");
			Assert.AreEqual (0x1B, il [4]); //typespec
			Assert.AreEqual (0x1B, il [9]); //typespec
			Assert.AreEqual (0x02, il [14]); //typedef
			Assert.AreEqual (0x01, il [19]); //typeref
		}
	}
}
