//
// System.Reflection.MethodBase Test Cases
//
// Authors:
//  Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
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

using System;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System.Reflection
{
	public class Generic<T> {
		public void Foo () {
		}

		public void GenericFoo<K> () {

		}
	}

	public class AnotherGeneric<T> {
		public void Foo () {
		}	
	}

	public class SimpleClass {
		public void GenericFoo<K> () {

		}
	}


	[TestFixture]
	public class MethodBaseTest
	{
		public static MethodInfo Where<T> (T a) {
			return (MethodInfo) MethodBase.GetCurrentMethod ();
		}

		public class Foo<K>
		{
			public static MethodInfo Where<T> (T a, K b) {
				return (MethodInfo) MethodBase.GetCurrentMethod ();
			}
		}

		[Test]
		public void GetCurrentMethodDropsAllGenericArguments ()
		{
			MethodInfo a = Where<int> (10);
			MethodInfo b = Foo<int>.Where <double> (10, 10);

			Assert.IsTrue (a.IsGenericMethodDefinition, "#1");
			Assert.IsTrue (b.IsGenericMethodDefinition, "#2");

			Assert.IsTrue (b.DeclaringType.IsGenericTypeDefinition, "#3");

			Assert.AreSame (a, typeof (MethodBaseTest).GetMethod ("Where"), "#4");
			Assert.AreSame (b, typeof (Foo<>).GetMethod ("Where"), "#5");
		}

		[Test] // GetMethodFromHandle (RuntimeMethodHandle)
		public void GetMethodFromHandle1_Handle_Generic ()
		{
			G<string> instance = new G<string> ();
			Type t = instance.GetType ();
			MethodBase mb1 = t.GetMethod ("M");
			RuntimeMethodHandle mh = mb1.MethodHandle;
			RuntimeTypeHandle th = t.TypeHandle;

			try {
				MethodBase.GetMethodFromHandle (mh);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Cannot resolve method Void M(System.__Canon)
				// because the declaring type of the method
				// handle MonoTests.System.Reflection.MethodBaseTest+G`1[T]
				// is generic. Explicitly provide the declaring type to
				// GetMethodFromHandle
			}
		}

		[Test] // GetMethodFromHandle (RuntimeMethodHandle)
		public void GetMethodFromHandle1_Handle_Zero ()
		{
			RuntimeMethodHandle mh = new RuntimeMethodHandle ();

			try {
				MethodBase.GetMethodFromHandle (mh);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Handle is not initialized
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		public void GetMethodFromHandle ()
		{
			Type t = typeof (object);
			RuntimeMethodHandle rmh = t.GetConstructor (Type.EmptyTypes).MethodHandle;
			MethodBase mb = MethodBase.GetMethodFromHandle (rmh);
			Assert.IsNotNull (mb, "#1");
			Assert.AreEqual (t, mb.DeclaringType, "#2");
			Assert.AreEqual (".ctor", mb.Name, "#3");
			ParameterInfo [] parameters = mb.GetParameters ();
			Assert.IsNotNull (parameters, "#4");
			Assert.AreEqual (0, parameters.Length, "#5");
		}

		[Test]
		public void GetMethodFromHandle_NonGenericType_DeclaringTypeZero ()
		{
			Type t = typeof (object);
			RuntimeMethodHandle rmh = t.GetConstructor (Type.EmptyTypes).MethodHandle;
			MethodBase mb = MethodBase.GetMethodFromHandle (rmh, new RuntimeTypeHandle ());
			Assert.IsNotNull (mb, "#1");
			Assert.AreEqual (t, mb.DeclaringType, "#2");
			Assert.AreEqual (".ctor", mb.Name, "#3");
			ParameterInfo [] parameters = mb.GetParameters ();
			Assert.IsNotNull (parameters, "#4");
			Assert.AreEqual (0, parameters.Length, "#5");
		}

		[Test] // GetMethodFromHandle (RuntimeMethodHandle, RuntimeTypeHandle)
		public void GetMethodFromHandle2_DeclaringType_Zero ()
		{
			RuntimeTypeHandle th = new RuntimeTypeHandle ();
			Type t = typeof (G<>);
			RuntimeMethodHandle mh = t.GetMethod ("M").MethodHandle;

			MethodBase mb = MethodBase.GetMethodFromHandle (mh, th);
			Assert.IsNotNull (mb, "#1");
			Assert.AreEqual (t, mb.DeclaringType, "#2");
			Assert.AreEqual ("M", mb.Name, "#3");
			ParameterInfo [] parameters = mb.GetParameters ();
			Assert.IsNotNull (parameters, "#4");
			Assert.AreEqual (1, parameters.Length, "#5");
			Assert.AreEqual (t.GetGenericArguments () [0] , parameters [0].ParameterType, "#6");
		}

		[Test] // GetMethodFromHandle (RuntimeMethodHandle, RuntimeTypeHandle)
		public void GetMethodFromHandle2_Handle_Generic ()
		{
			G<string> instance = new G<string> ();
			Type t = instance.GetType ();
			MethodBase mb1 = t.GetMethod ("M");
			RuntimeMethodHandle mh = mb1.MethodHandle;
			RuntimeTypeHandle th = t.TypeHandle;

			MethodBase mb2 = MethodBase.GetMethodFromHandle (mh, th);
			Assert.IsNotNull (mb2, "#1");
			Assert.AreEqual (t, mb2.DeclaringType, "#2");
			Assert.AreEqual ("M", mb2.Name, "#3");
			ParameterInfo [] parameters = mb2.GetParameters ();
			Assert.IsNotNull (parameters, "#4");
			Assert.AreEqual (1, parameters.Length, "#5");
			Assert.AreEqual (typeof (string), parameters [0].ParameterType, "#6");
		}

		[Test] // GetMethodFromHandle (RuntimeMethodHandle, RuntimeTypeHandle)
		public void GetMethodFromHandle2_Handle_Zero ()
		{
			RuntimeTypeHandle th = typeof (G<>).TypeHandle;
			RuntimeMethodHandle mh = new RuntimeMethodHandle ();

			try {
				MethodBase.GetMethodFromHandle (mh, th);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Handle is not initialized
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		public class G<T>
		{
			public void M (T t)
			{
			}
		}

		[Test]
		public void GetMethodFromHandle_Handle_Generic_Method ()
		{
			MethodInfo mi = typeof (SimpleClass).GetMethod ("GenericFoo");
			RuntimeMethodHandle handle = mi.MethodHandle;

			MethodBase res = MethodBase.GetMethodFromHandle (handle);
			Assert.AreEqual (mi, res, "#1");

			res = MethodBase.GetMethodFromHandle (handle, typeof (SimpleClass).TypeHandle);
			Assert.AreEqual (mi, res, "#2");
		}


		[Test]
		public void GetMethodFromHandle_Handle_Generic_Method_Instance ()
		{
			MethodInfo mi = typeof (SimpleClass).GetMethod ("GenericFoo").MakeGenericMethod (typeof (int));
			RuntimeMethodHandle handle = mi.MethodHandle;

			MethodBase res = MethodBase.GetMethodFromHandle (handle);
			Assert.AreEqual (mi, res, "#1");

			res = MethodBase.GetMethodFromHandle (handle, typeof (SimpleClass).TypeHandle);
			Assert.AreEqual (mi, res, "#2");
		}

		[Test]
		public void GetMethodFromHandle_Handle_Generic_Method_On_Generic_Class ()
		{
			MethodInfo mi = typeof (Generic<>).GetMethod ("GenericFoo");
			RuntimeMethodHandle handle = mi.MethodHandle;
			MethodBase res;

			try {
				MethodBase.GetMethodFromHandle (handle);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			mi = typeof (Generic<int>).GetMethod ("GenericFoo").MakeGenericMethod (typeof (int));
			handle = mi.MethodHandle;

			try {
				MethodBase.GetMethodFromHandle (handle);
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}


			mi = typeof (Generic<>).GetMethod ("GenericFoo").MakeGenericMethod (typeof (int));
			handle = mi.MethodHandle;

			try {
				MethodBase.GetMethodFromHandle (handle);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}


			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<int>).TypeHandle);
			Assert.AreEqual (typeof (Generic<int>), res.DeclaringType, "#4");

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<double>).TypeHandle);
			Assert.AreEqual (typeof (Generic<double>), res.DeclaringType, "#5");

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<>).TypeHandle);
			Assert.AreEqual (typeof (Generic<>), res.DeclaringType, "#6");

			try {
				MethodBase.GetMethodFromHandle(handle, typeof (AnotherGeneric<double>).TypeHandle);
				Assert.Fail ("#7");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void GetMethodFromHandle_Handle_Method_On_Generic_Class ()
		{
			MethodInfo mi = typeof (Generic<>).GetMethod ("Foo");
			RuntimeMethodHandle handle = mi.MethodHandle;
			MethodBase res;

			try {
				MethodBase.GetMethodFromHandle(handle);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<>).TypeHandle);
			Assert.AreEqual (res, mi, "#2");

			mi = typeof (Generic<int>).GetMethod ("Foo");
			handle = mi.MethodHandle;

			try {
				MethodBase.GetMethodFromHandle(handle);
				Assert.Fail ("#3");
			} catch (ArgumentException) {
			}

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<int>).TypeHandle);
			Assert.AreEqual (typeof (Generic<int>), res.DeclaringType, "#4");

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<double>).TypeHandle);
			Assert.AreEqual (typeof (Generic<double>), res.DeclaringType, "#5");

			res = MethodBase.GetMethodFromHandle(handle, typeof (Generic<>).TypeHandle);
			Assert.AreEqual (typeof (Generic<>), res.DeclaringType, "#6");

			try {
				MethodBase.GetMethodFromHandle(handle, typeof (AnotherGeneric<double>).TypeHandle);
				Assert.Fail ("#7");
			} catch (ArgumentException) {
			}
		}

		// test case adapted from http://www.chrishowie.com/2010/11/24/mutable-strings-in-mono/
		public class FakeString {
			public int length;
			public char start_char;
		}

		private static FakeString UnsafeConversion<T> (T thing) where T : FakeString
		{
			return thing;
		}

		[Test]
		public void MutableString ()
		{
			var m = typeof (MethodBaseTest).GetMethod ("UnsafeConversion", BindingFlags.NonPublic | BindingFlags.Static);
			try {
				var m2 = m.MakeGenericMethod (typeof (string));
				Assert.Fail ("MakeGenericMethod");
			}
			catch (ArgumentException) {
			}
		}
	}
}
