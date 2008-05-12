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
	[TestFixture]
	public class MethodBaseTest
	{
#if NET_2_0
		[Test] // GetMethodFromHandle (RuntimeMethodHandle)
		[Category ("NotWorking")]
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
#endif

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

#if NET_2_0
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
#endif
	}
}
