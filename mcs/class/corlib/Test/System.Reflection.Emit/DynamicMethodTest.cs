//
// DynamicMethodTest.cs - NUnit Test Cases for the DynamicMethod class
//
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell

#if NET_2_0

using System;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class DynamicMethodTest
	{
		private delegate int HelloInvoker (string msg);

		[Test] // bug #78253
		public void DynamicMethodReference ()
		{
			DynamicMethod hello = new DynamicMethod ("Hello",
				typeof (int),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest).Module);
			Assert.IsNull (hello.DeclaringType, "#1");

			DynamicMethod write = new DynamicMethod ("Write",
				typeof (int),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest));
			Assert.IsNull (hello.DeclaringType, "#2");

			MethodInfo invokeWrite = write.GetBaseDefinition ();

			ILGenerator helloIL = hello.GetILGenerator ();
			helloIL.Emit (OpCodes.Ldarg_0);
			helloIL.EmitCall (OpCodes.Call, invokeWrite, null);
			helloIL.Emit (OpCodes.Ret);

			ILGenerator writeIL = write.GetILGenerator ();
			writeIL.Emit (OpCodes.Ldc_I4_2);
			writeIL.Emit (OpCodes.Ret);

			HelloInvoker hi =
				(HelloInvoker) hello.CreateDelegate (typeof (HelloInvoker));
			int ret = hi ("Hello, World!");
			Assert.AreEqual (2, ret, "#3");

			object[] invokeArgs = { "Hello, World!" };
			object objRet = hello.Invoke (null, invokeArgs);
			Assert.AreEqual (2, objRet, "#4");
		}

		[Test]
		public void EmptyMethodBody ()
		{
			DynamicMethod hello = new DynamicMethod ("Hello",
				typeof (int),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest).Module);
			object[] invokeArgs = { "Hello, World!" };

			// no IL generator
			try {
				hello.Invoke (null, invokeArgs);
				Assert.Fail ("#1");
			} catch (InvalidOperationException) {
			}

			// empty method body
			hello.GetILGenerator ();
			try {
				hello.Invoke (null, invokeArgs);
				Assert.Fail ("#2");
			} catch (InvalidOperationException) {
			}
		}

		private delegate string ReturnString (string msg);

		private static string private_method (string s) {
			return s;
		}

		[Test]
		public void SkipVisibility ()
		{
			DynamicMethod hello = new DynamicMethod ("Hello",
				typeof (string),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest).Module, true);

			ILGenerator helloIL = hello.GetILGenerator ();
			helloIL.Emit (OpCodes.Ldarg_0);
			helloIL.EmitCall (OpCodes.Call, typeof (DynamicMethodTest).GetMethod ("private_method", BindingFlags.Static|BindingFlags.NonPublic), null);
			helloIL.Emit (OpCodes.Ret);

			ReturnString del =
				(ReturnString) hello.CreateDelegate (typeof (ReturnString));
			Assert.AreEqual ("ABCD", del ("ABCD"));
		}

	}
}

#endif
