//
// System.Reflection.MethodInfo Test Cases
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Threading;
using System.Reflection;
#if !MONOTOUCH
using System.Reflection.Emit;
#endif
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace A.B.C {
	public struct MethodInfoTestStruct {
		int p;
	}
}
namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class MethodInfoTest
	{
#if !TARGET_JVM
		[DllImport ("libfoo", EntryPoint="foo", CharSet=CharSet.Unicode, ExactSpelling=false, PreserveSig=true, SetLastError=true, BestFitMapping=true, ThrowOnUnmappableChar=true)]
		public static extern void dllImportMethod ();
#endif
		[MethodImplAttribute(MethodImplOptions.PreserveSig)]
		public void preserveSigMethod ()
		{
		}

		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void synchronizedMethod ()
		{
		}

		[Test]
		public void IsDefined_AttributeType_Null ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("foo");

			try {
				mi.IsDefined ((Type) null, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("attributeType", ex.ParamName, "#6");
			}
		}

		[Test]
		public void TestInvokeByRefReturnMethod ()
		{
			try {
				MethodInfo m = typeof (int[]).GetMethod ("Address");
				m.Invoke (new int[1], new object[] { 0 });
				Assert.Fail ("#1");
			} catch (NotSupportedException e) {
				Assert.AreEqual (typeof (NotSupportedException), e.GetType (), "#2");
				Assert.IsNull (e.InnerException, "#3");
				Assert.IsNotNull (e.Message, "#4");
			}
		}

#if NET_2_0
		[Test]
		public void PseudoCustomAttributes ()
		{
			Type t = typeof (MethodInfoTest);

			DllImportAttribute attr = (DllImportAttribute)((t.GetMethod ("dllImportMethod").GetCustomAttributes (typeof (DllImportAttribute), true)) [0]);

			Assert.AreEqual (CallingConvention.Winapi, attr.CallingConvention, "#1");
			Assert.AreEqual ("foo", attr.EntryPoint, "#2");
			Assert.AreEqual ("libfoo", attr.Value, "#3");
			Assert.AreEqual (CharSet.Unicode, attr.CharSet, "#4");
			Assert.AreEqual (false, attr.ExactSpelling, "#5");
			Assert.AreEqual (true, attr.PreserveSig, "#6");
			Assert.AreEqual (true, attr.SetLastError, "#7");
			Assert.AreEqual (true, attr.BestFitMapping, "#8");
			Assert.AreEqual (true, attr.ThrowOnUnmappableChar, "#9");

			PreserveSigAttribute attr2 = (PreserveSigAttribute)((t.GetMethod ("preserveSigMethod").GetCustomAttributes (true)) [0]);

			// This doesn't work under MS.NET
			/*
			  MethodImplAttribute attr3 = (MethodImplAttribute)((t.GetMethod ("synchronizedMethod").GetCustomAttributes (true)) [0]);
			*/
		}

		[return: MarshalAs (UnmanagedType.Interface)]
		public void ReturnTypeMarshalAs ()
		{
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ReturnTypePseudoCustomAttributes ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("ReturnTypeMarshalAs");

			Assert.IsTrue (mi.ReturnTypeCustomAttributes.GetCustomAttributes (typeof (MarshalAsAttribute), true).Length == 1);
		}
#endif

		public static int foo (int i, int j)
		{
			return i + j;
		}

		[Test]
		public void StaticInvokeWithObject ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("foo");
			
			mi.Invoke (new Object (), new object [] { 1, 2 });
		}

		[Test]
		public void ByRefInvoke ()
		{
			MethodInfo met = typeof(MethodInfoTest).GetMethod ("ByRefTest");
			object[] parms = new object[] {1};
			met.Invoke (null, parms);
			Assert.AreEqual (2, parms[0]);
		}

		public static void ByRefTest (ref int a1)
		{
			if (a1 == 1)
				a1 = 2;
		}

		static int byref_arg;

		public static void ByrefVtype (ref int i) {
			byref_arg = i;
			i = 5;
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // #A2 fails on MS.NET 1.x
#endif
		public void ByrefVtypeInvoke ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("ByrefVtype");

			object o = 1;
			object[] args = new object [] { o };
			mi.Invoke (null, args);
			Assert.AreEqual (1, byref_arg, "#A1");
			Assert.AreEqual (1, o, "#A2");
			Assert.AreEqual (5, args[0], "#A3");

			args [0] = null;
			mi.Invoke (null, args);
			Assert.AreEqual (0, byref_arg, "#B1");
			Assert.AreEqual (5, args[0], "#B2");
		}

		public void HeyHey (out string out1, ref string ref1)
		{
			out1 = null;
		}

		public void SignatureTest (__arglist)
		{
		}
		
		public static unsafe int* PtrFunc (int* a)
		{
			return (int*) 0;
		}

		[Test] // bug #81538
		public void InvokeThreadAbort ()
		{
			MethodInfo method = typeof (MethodInfoTest).GetMethod ("AbortIt");
			try {
				method.Invoke (null, new object [0]);
				Assert.Fail ("#1");
			}
#if NET_2_0
			catch (ThreadAbortException ex) {
				Thread.ResetAbort ();
				Assert.IsNull (ex.InnerException, "#2");
			}
#else
			catch (TargetInvocationException ex) {
				Thread.ResetAbort ();
				Assert.IsNotNull (ex.InnerException, "#2");
				Assert.AreEqual (typeof (ThreadAbortException), ex.InnerException.GetType (), "#3");
			}
#endif
		}

		public static void AbortIt ()
		{
			Thread.CurrentThread.Abort ();
		}

		[Test] // bug #76541
		public void ToStringByRef ()
		{
			Assert.AreEqual ("Void HeyHey(System.String ByRef, System.String ByRef)",
				this.GetType ().GetMethod ("HeyHey").ToString ());
		}
		
		[Test]
		public void ToStringArgList ()
		{
			Assert.AreEqual ("Void SignatureTest(...)",
				this.GetType ().GetMethod ("SignatureTest").ToString ());
		}

		[Test]
		public void ToStringWithPointerSignatures () //bug #409583
		{
			Assert.AreEqual ("Int32* PtrFunc(Int32*)", this.GetType ().GetMethod ("PtrFunc").ToString ());
		}


#if NET_2_0
		public struct SimpleStruct
		{
			int a;
		}

		public static unsafe SimpleStruct* PtrFunc2 (SimpleStruct* a, A.B.C.MethodInfoTestStruct *b)
		{
			return (SimpleStruct*) 0;
		}

		[Test]
		public void ToStringWithPointerSignaturesToNonPrimitiveType ()
		{
			Assert.AreEqual ("SimpleStruct* PtrFunc2(SimpleStruct*, A.B.C.MethodInfoTestStruct*)", 
				this.GetType ().GetMethod ("PtrFunc2").ToString ());
		}	
		[Test]
		public void ToStringGenericMethod ()
		{
			Assert.AreEqual ("System.Collections.ObjectModel.ReadOnlyCollection`1[T] AsReadOnly[T](T[])",
				typeof (Array).GetMethod ("AsReadOnly").ToString ());
		}
#endif

		class GBD_A         { public virtual     void f () {} }
		class GBD_B : GBD_A { public override    void f () {} }
		class GBD_C : GBD_B { public override    void f () {} }
		class GBD_D : GBD_C { public new virtual void f () {} }
		class GBD_E : GBD_D { public override    void f () {} }

		[Test]
		public void GetBaseDefinition ()
		{
			Assert.AreEqual (typeof (GBD_A), typeof (GBD_C).GetMethod ("f").GetBaseDefinition ().DeclaringType);
			Assert.AreEqual (typeof (GBD_D), typeof (GBD_D).GetMethod ("f").GetBaseDefinition ().DeclaringType);
			Assert.AreEqual (typeof (GBD_D), typeof (GBD_E).GetMethod ("f").GetBaseDefinition ().DeclaringType);
		}

		class TestInheritedMethodA {
			private void TestMethod ()
			{
			}

			public void TestMethod2 ()
			{
			}
		}

		class TestInheritedMethodB : TestInheritedMethodA {
		}

		[Test]
		public void InheritanceTestGetMethodTest ()
		{
			MethodInfo inheritedMethod = typeof(TestInheritedMethodB).GetMethod("TestMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo baseMethod = typeof(TestInheritedMethodB).GetMethod("TestMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Assert.AreSame (inheritedMethod, baseMethod);

			MethodInfo inheritedMethod2 = typeof(TestInheritedMethodB).GetMethod("TestMethod2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo baseMethod2 = typeof(TestInheritedMethodB).GetMethod("TestMethod2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			Assert.AreSame (inheritedMethod, baseMethod);
		}

#if NET_2_0
#if !TARGET_JVM // MethodBody is not supported for TARGET_JVM
		[Test]
		public void GetMethodBody_Abstract ()
		{
			MethodBody mb = typeof (ICloneable).GetMethod ("Clone").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Runtime ()
		{
			MethodBody mb = typeof (AsyncCallback).GetMethod ("Invoke").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Pinvoke ()
		{
			MethodBody mb = typeof (MethodInfoTest).GetMethod ("dllImportMethod").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Icall ()
		{
			foreach (MethodInfo mi in typeof (object).GetMethods (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
				if ((mi.GetMethodImplementationFlags () & MethodImplAttributes.InternalCall) != 0) {
					MethodBody mb = mi.GetMethodBody ();
					Assert.IsNull (mb);
				}
		}

		public static void locals_method ()
		{
			byte[] b = new byte [10];

			unsafe {
				/* This generates a pinned local */
				fixed (byte *p = &b [0]) {
				}
			}
		}

		[Test]
		public void GetMethodBody ()
		{
			MethodBody mb = typeof (MethodInfoTest).GetMethod ("locals_method").GetMethodBody ();

			Assert.IsTrue (mb.InitLocals, "#1");
			Assert.IsTrue (mb.LocalSignatureMetadataToken > 0, "#2");

			IList<LocalVariableInfo> locals = mb.LocalVariables;

			// This might break with different compilers etc.
			Assert.AreEqual (2, locals.Count, "#3");

			Assert.IsTrue ((locals [0].LocalType == typeof (byte[])) || (locals [1].LocalType == typeof (byte[])), "#4");
			if (locals [0].LocalType == typeof (byte[]))
				Assert.AreEqual (false, locals [0].IsPinned, "#5");
			else
				Assert.AreEqual (false, locals [1].IsPinned, "#6");
		}
#endif // TARGET_JVM

		public int return_parameter_test ()
		{
			return 0;
		}

		[Test]
		public void GetMethodFromHandle_Generic ()
		{
			MethodHandleTest<int> test = new MethodHandleTest<int> ();
			RuntimeMethodHandle mh = test.GetType ().GetProperty ("MyList")
				.GetGetMethod ().MethodHandle;
			MethodBase mb = MethodInfo.GetMethodFromHandle (mh,
				typeof (MethodHandleTest<int>).TypeHandle);
			Assert.IsNotNull (mb, "#1");
			List<int> list = (List<int>) mb.Invoke (test, null);
			Assert.IsNotNull (list, "#2");
			Assert.AreEqual (1, list.Count, "#3");
		}

		[Test]
		public void ReturnParameter ()
		{
			ParameterInfo pi = typeof (MethodInfoTest).GetMethod ("return_parameter_test").ReturnParameter;
			Assert.AreEqual (typeof (int), pi.ParameterType, "#1");
			Assert.AreEqual (-1, pi.Position, "#2");
			// MS always return false here
			//Assert.IsTrue (pi.IsRetval, "#3");
		}

#if !TARGET_JVM // ReflectionOnly is not supported yet on TARGET_JVM
		[Test]
			public void InvokeOnRefOnlyAssembly ()
		{
			Assembly a = Assembly.ReflectionOnlyLoad (typeof (MethodInfoTest).Assembly.FullName);
			Type t = a.GetType (typeof (RefOnlyMethodClass).FullName);
			MethodInfo m = t.GetMethod ("RefOnlyMethod", BindingFlags.Static | BindingFlags.NonPublic);
			try {
				m.Invoke (null, new object [0]);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The requested operation is invalid in the
				// ReflectionOnly context
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
#endif // TARGET_JVM

		[Test]
		[ExpectedException (typeof (TargetInvocationException))]
		public void InvokeInvalidOpExceptionThrow () {
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("ThrowMethod");
			mi.Invoke(null, null);
		}

		public static void ThrowMethod () {
			throw new InvalidOperationException ();
		}

		[Test]
		public void InvokeGenericVtype ()
		{
			KeyValuePair<string, uint> kvp = new KeyValuePair<string, uint> ("a", 21);
			Type type = kvp.GetType ();
			Type [] arguments = type.GetGenericArguments ();
			MethodInfo method = typeof (MethodInfoTest).GetMethod ("Go");
			MethodInfo generic_method = method.MakeGenericMethod (arguments);
			kvp = (KeyValuePair<string, uint>)generic_method.Invoke (null, new object [] { kvp });

			Assert.AreEqual ("a", kvp.Key, "#1");
			Assert.AreEqual (21, kvp.Value, "#2");
		}

		public static KeyValuePair<T1, T2> Go <T1, T2> (KeyValuePair <T1, T2> kvp)
		{
			return kvp;
		}

		[Test] // bug #81997
		public void InvokeGenericInst ()
		{
			List<string> str = null;

			object [] methodArgs = new object [] { str };
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("GenericRefMethod");
			mi.Invoke (null, methodArgs);
			Assert.IsNotNull (methodArgs [0], "#A1");
			Assert.IsNull (str, "#A2");
			Assert.IsTrue (methodArgs [0] is List<string>, "#A3");

			List<string> refStr = methodArgs [0] as List<string>;
			Assert.IsNotNull (refStr, "#B1");
			Assert.AreEqual (1, refStr.Count, "#B2");
			Assert.AreEqual ("test", refStr [0], "#B3");
		}

		public static void GenericRefMethod (ref List<string> strArg)
		{
			strArg = new List<string> ();
			strArg.Add ("test");
		}

		public void MakeGenericMethodArgsMismatchFoo<T> ()
		{
		}

		[Test]
		public void MakeGenericMethodArgsMismatch ()
		{
			MethodInfo gmi = this.GetType ().GetMethod (
				"MakeGenericMethodArgsMismatchFoo");
			try {
				gmi.MakeGenericMethod ();
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// The type or method has 1 generic parameter(s),
				// but 0 generic argument(s) were provided. A
				// generic argument must be provided for each
				// generic parameter
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		public void SimpleGenericMethod<TFoo, TBar> () {}

		[Test]
		public void MakeGenericMethodWithNullArray ()
		{
			MethodInfo gmi = this.GetType ().GetMethod ("SimpleGenericMethod");
			try {
				gmi.MakeGenericMethod ((Type []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("methodInstantiation", ex.ParamName, "#5");
			}
		}

		[Test]
		public void MakeGenericMethodWithNullValueInTypesArray ()
		{
			MethodInfo gmi = this.GetType ().GetMethod ("SimpleGenericMethod");
			try {
				gmi.MakeGenericMethod (new Type [] { typeof (int), null });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void MakeGenericMethodWithNonGenericMethodDefinitionMethod ()
		{
			MethodInfo gmi = this.GetType ().GetMethod ("SimpleGenericMethod");
			MethodInfo inst = gmi.MakeGenericMethod (typeof (int), typeof (double));
			try {
				inst.MakeGenericMethod (typeof (int), typeof (double));
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
			}
		}
#if !MONOTOUCH
		public TFoo SimpleGenericMethod2<TFoo, TBar> () { return default (TFoo); }
		/*Test for the uggly broken behavior of SRE.*/
		[Test]
		public void MakeGenericMethodWithSreTypeResultsInStupidMethodInfo ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "MonoTests.System.Reflection.Emit.MethodInfoTest";
			AssemblyBuilder assembly = Thread.GetDomain ().DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.RunAndSave, ".");
			ModuleBuilder module = assembly.DefineDynamicModule ("module1", "tst.dll");
			TypeBuilder tb = module.DefineType ("Test", TypeAttributes.Public);

			MethodInfo gmi = this.GetType ().GetMethod ("SimpleGenericMethod2");
			MethodInfo ins = gmi.MakeGenericMethod (typeof (int), tb);

			Assert.AreSame (tb, ins.GetGenericArguments () [1], "#1");
			/*broken ReturnType*/
			Assert.AreSame (gmi.GetGenericArguments () [0], ins.ReturnType, "#2");
		}
#endif
		public static int? pass_nullable (int? i)
		{
			return i;
		}

		[Test]
		[Category ("MobileNotWorking")] // bug #10266
		public void NullableTests ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("pass_nullable");
			Assert.AreEqual (102, mi.Invoke (null, new object [] { 102 }), "#1");
			Assert.AreEqual (null, mi.Invoke (null, new object [] { null }), "#2");

			// Test conversion of vtype to a nullable type for the this argument
			PropertyInfo pi = typeof (Nullable <int>).GetProperty ("HasValue");
			Assert.AreEqual (true, pi.GetGetMethod ().Invoke (10, null));
			PropertyInfo pi2 = typeof (Nullable <int>).GetProperty ("Value");
			Assert.AreEqual (10, pi2.GetGetMethod ().Invoke (10, null));
		}

		public static void foo_generic<T> ()
		{
		}

		[Test]
		public void IsGenericMethod ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("foo_generic");
			Assert.AreEqual (true, mi.IsGenericMethod, "#1");
			MethodInfo mi2 = mi.MakeGenericMethod (new Type[] { typeof (int) });
			Assert.AreEqual (true, mi2.IsGenericMethod, "#2");
			MethodInfo mi3 = typeof (GenericHelper<int>).GetMethod ("Test");
			Assert.AreEqual (false, mi3.IsGenericMethod, "#3");
		}

		class A<T>
		{
			public static void Foo<T2> (T2 i)
			{
			}

			public static void Bar ()
			{
			}

			public class B
			{
				public static void Baz ()
				{
				}
			}
		}

		[Test]
		public void ContainsGenericParameters ()
		{
			// Non-generic method in open generic type
			Assert.IsTrue (typeof (A<int>).GetGenericTypeDefinition ().GetMethod ("Bar").ContainsGenericParameters);
			// open generic method in closed generic type
			Assert.IsTrue (typeof (A<int>).GetMethod ("Foo").ContainsGenericParameters);
			// non-generic method in closed generic type
			Assert.IsFalse (typeof (A<int>).GetMethod ("Bar").ContainsGenericParameters);
			// closed generic method in closed generic type
			Assert.IsFalse (typeof (A<int>).GetMethod ("Foo").MakeGenericMethod (new Type [] { typeof (int) }).ContainsGenericParameters);
			// non-generic method in non-generic nested type of closed generic type
			Assert.IsFalse (typeof (A<int>.B).GetMethod ("Baz").ContainsGenericParameters);
			// non-generic method in non-generic nested type of open generic type
			Assert.IsTrue (typeof (A<int>.B).GetGenericTypeDefinition ().GetMethod ("Baz").ContainsGenericParameters);
		}

		[Test]
		public void IsGenericMethodDefinition ()
		{
			MethodInfo m1 = typeof (A<>).GetMethod ("Foo");
			Assert.IsTrue (m1.IsGenericMethod, "#A1");
			Assert.IsTrue (m1.IsGenericMethodDefinition, "#A2");

			MethodInfo m2 = typeof (A<int>).GetMethod ("Foo");
			Assert.IsTrue (m2.IsGenericMethod, "#B1");
			Assert.IsTrue (m2.IsGenericMethodDefinition, "#B2");

			MethodInfo m3 = m2.MakeGenericMethod (typeof (int));
			Assert.IsTrue (m3.IsGenericMethod, "#C1");
			Assert.IsFalse (m3.IsGenericMethodDefinition, "#C2");
		}

		[Test]
		public void GetGenericMethodDefinition ()
		{
			MethodInfo mi1 = typeof (MyList<>).GetMethod ("ConvertAll");
			MethodInfo mi2 = typeof (MyList<int>).GetMethod ("ConvertAll");

			Assert.AreEqual ("MonoTests.System.Reflection.MethodInfoTest+Foo`2[T,TOutput]",
					 mi1.GetParameters () [0].ParameterType.ToString (), "#A1");
			Assert.AreEqual ("MonoTests.System.Reflection.MethodInfoTest+Foo`2[System.Int32,TOutput]",
					 mi2.GetParameters () [0].ParameterType.ToString (), "#A2");
			Assert.IsTrue (mi1.IsGenericMethod, "#A3");
			Assert.IsTrue (mi1.IsGenericMethodDefinition, "#A4");
			Assert.IsTrue (mi2.IsGenericMethod, "#A5");
			Assert.IsTrue (mi2.IsGenericMethodDefinition, "#A6");

			MethodInfo mi3 = mi2.GetGenericMethodDefinition ();

			Assert.IsTrue (mi3.IsGenericMethod, "#B1");
			Assert.IsTrue (mi3.IsGenericMethodDefinition, "#B2");
			Assert.AreSame (mi2, mi3, "#B3");

			MethodInfo mi4 = mi2.MakeGenericMethod (typeof (short));
			Assert.IsTrue (mi4.IsGenericMethod, "#C1");
			Assert.IsFalse (mi4.IsGenericMethodDefinition, "#C2");
			Assert.AreSame (mi2, mi4.GetGenericMethodDefinition (), "#C3");
		}

		public void TestMethod123(int a, int b) {}

		[Test]
		public void GetParametersDontReturnInternedArray ()
		{
			var method = typeof (MethodInfoTest).GetMethod ("TestMethod123");
			var parms = method.GetParameters ();
			Assert.AreNotSame (parms, method.GetParameters (), "#1");

			parms [0] = null;
			Assert.IsNotNull (method.GetParameters () [0], "#2");
		}

		[Test]
		public void Bug354757 ()
		{
			MethodInfo gmd = (typeof (MyList <int>)).GetMethod ("ConvertAll");
			MethodInfo oi = gmd.MakeGenericMethod (gmd.GetGenericArguments ());
			Assert.AreSame (gmd, oi);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MakeGenericMethodRespectConstraints ()
		{
			var m = typeof (MethodInfoTest).GetMethod ("TestMethod");
			m.MakeGenericMethod (typeof (Type));
		}

		public void TestMethod <T> () where T : Exception
		{
		}

		public class MyList<T>
		{
			public TOutput ConvertAll<TOutput> (Foo<T,TOutput> arg)
			{
				return default (TOutput);
			}
			public T ConvertAll2 (MyList<T> arg)
			{
				return default (T);
			}
		}

		public class Foo<T,TOutput>
		{
		}

		class GenericHelper<T>
		{
			public void Test (T t)
			{
			}
		}
#endif
#if NET_4_0
		interface IMethodInvoke<out T>
		{
		    T Test ();
		}

		class MethodInvoke : IMethodInvoke<string>
		{
		    public string Test ()
		    {
		        return "MethodInvoke";
		    }
		}

		[Test]
		public void GetInterfaceMapWorksWithVariantIfaces ()
		{
			var m0 = typeof (IMethodInvoke<object>).GetMethod ("Test");
			var m1 = typeof (IMethodInvoke<string>).GetMethod ("Test");
			var obj = new MethodInvoke ();

			Assert.AreEqual ("MethodInvoke", m0.Invoke (obj, new Object [0]));
			Assert.AreEqual ("MethodInvoke", m1.Invoke (obj, new Object [0]));
		}
#endif

	}
	
#if NET_2_0
	// Helper class
	class RefOnlyMethodClass 
	{
		// Helper static method
		static void RefOnlyMethod ()
		{
		}
	}

	public class MethodHandleTest<T>
	{
		private List<T> _myList = new List<T> ();

		public MethodHandleTest ()
		{
			_myList.Add (default (T));
		}

		public List<T> MyList {
			get { return _myList; }
			set { _myList = value; }
		}
	}
#endif
}
