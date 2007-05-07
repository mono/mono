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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

#if NET_2_0
using System.Collections.Generic;
#endif

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
		public void preserveSigMethod () {
		}

		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void synchronizedMethod () {
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
		public void ReturnTypeMarshalAs () {
		}

		[Test]
		[Category ("TargetJvmNotWorking")]
		public void ReturnTypePseudoCustomAttributes () {
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

#if NET_2_0
		[Test]
		public void InvokeThreadAbort () {
			MethodInfo method = typeof (MethodInfoTest).GetMethod ("AbortIt");
			try {
				method.Invoke (null, new object [0]);
			}
			catch (ThreadAbortException ex) {
				Thread.ResetAbort ();
			}
		}

		public static void AbortIt () {
			Thread.CurrentThread.Abort ();
		}
#endif			

		[Test] // bug #76541
		public void ToStringByRef ()
		{
			Assert.AreEqual ("Void HeyHey(System.String ByRef, System.String ByRef)",
				this.GetType ().GetMethod ("HeyHey").ToString ());
		}

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

#if NET_2_0
#if !TARGET_JVM // MethodBody is not supported for TARGET_JVM
		[Test]
		public void GetMethodBody_Abstract () {
			MethodBody mb = typeof (ICloneable).GetMethod ("Clone").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Runtime () {
			MethodBody mb = typeof (AsyncCallback).GetMethod ("Invoke").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Pinvoke () {
			MethodBody mb = typeof (MethodInfoTest).GetMethod ("dllImportMethod").GetMethodBody ();
			Assert.IsNull (mb);
		}

		[Test]
		public void GetMethodBody_Icall () {
			foreach (MethodInfo mi in typeof (object).GetMethods (BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
				if ((mi.GetMethodImplementationFlags () & MethodImplAttributes.InternalCall) != 0) {
					MethodBody mb = mi.GetMethodBody ();
					Assert.IsNull (mb);
				}
		}

		public static void locals_method () {
			byte[] b = new byte [10];

			unsafe {
				/* This generates a pinned local */
				fixed (byte *p = &b [0]) {
				}
			}
		}

		[Test]
		public void GetMethodBody () {
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

		public int return_parameter_test () {
			return 0;
		}

		[Test]
		public void ReturnParameter () {
			ParameterInfo pi = typeof (MethodInfoTest).GetMethod ("return_parameter_test").ReturnParameter;

			Assert.AreEqual (typeof (int), pi.ParameterType);
			Assert.AreEqual (-1, pi.Position);
			// This fails on MS
			//Assert.AreEqual (True, pi.IsRetval);
		}

#if !TARGET_JVM // ReflectionOnly is not supported yet on TARGET_JVM
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InvokeOnRefOnlyAssembly ()
		{
			Assembly a = Assembly.ReflectionOnlyLoad (typeof (MethodInfoTest).Assembly.FullName);
			Type t = a.GetType (typeof (RefOnlyMethodClass).FullName);
			MethodInfo m = t.GetMethod ("RefOnlyMethod", BindingFlags.Static | BindingFlags.NonPublic);
			
			m.Invoke (null, new object [0]);
		}
#endif // TARGET_JVM

		[Test]
		public void InvokeGenericVtype ()
		{
			KeyValuePair<string, uint> kvp = new KeyValuePair<string, uint> ("a", 21);
			Type type = kvp.GetType ();
			Type [] arguments = type.GetGenericArguments ();
			MethodInfo method = typeof (MethodInfoTest).GetMethod ("Go");
			MethodInfo generic_method = method.MakeGenericMethod (arguments);
			kvp = (KeyValuePair<string, uint>)generic_method.Invoke (null, new object [] { kvp });

			Assert.AreEqual ("a", kvp.Key);
			Assert.AreEqual (21, kvp.Value);
		}

        public static KeyValuePair<T1, T2> Go <T1, T2> (KeyValuePair <T1, T2> kvp)
        {
			return kvp;
        }

		public void MakeGenericMethodArgsMismatchFoo<T> () {}
	    
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MakeGenericMethodArgsMismatch ()
		{
			MethodInfo gmi = this.GetType ().GetMethod (
				"MakeGenericMethodArgsMismatchFoo")
				.MakeGenericMethod ();
		}

		public static int? pass_nullable (int? i)
		{
			return i;
		}

		[Test]
		public void NullableTests ()
		{
			MethodInfo mi = typeof (MethodInfoTest).GetMethod ("pass_nullable");
			Assert.AreEqual (102, mi.Invoke (null, new object [] { 102 }), "#1");
			Assert.AreEqual (null, mi.Invoke (null, new object [] { null }), "#2");
		}

		public static void foo_generic<T> () {
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

		class A<T> {

			public static void Foo<T2> (T2 i) {
			}

			public static void Bar () {
			}

			public class B {
				public static void Baz () {
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

		class GenericHelper<T>
		{
			public void Test (T t)
			{ }
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
#endif
}

