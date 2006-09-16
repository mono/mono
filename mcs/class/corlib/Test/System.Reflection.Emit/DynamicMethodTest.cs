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

		[Test]
		public void Constructor1_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest).Module);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void Constructor2_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest));
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void Constructor3_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest).Module, true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void Constructor4_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest), true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void Constructor5_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					MethodAttributes.Public | MethodAttributes.Static,
					CallingConventions.Standard,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest).Module, true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

		[Test]
		public void Constructor6_Name_Null ()
		{
			try {
				new DynamicMethod (null,
					MethodAttributes.Public | MethodAttributes.Static,
					CallingConventions.Standard,
					typeof (void),
					new Type[] { typeof (string) },
					typeof (DynamicMethodTest), true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.AreEqual ("name", ex.ParamName, "#3");
				Assert.IsNull (ex.InnerException, "#4");
			}
		}

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
		private delegate void DoNothing (string msg);

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

		[Test]
		public void ReturnType_Null ()
		{
			DynamicMethod hello = new DynamicMethod ("Hello",
				null,
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest).Module, true);
			Assert.AreEqual (typeof (void), hello.ReturnType, "#1");

			ILGenerator helloIL = hello.GetILGenerator ();
			helloIL.Emit (OpCodes.Ret);

			DoNothing dn = (DoNothing) hello.CreateDelegate (typeof (DoNothing));
			dn ("whatever");

			object[] invokeArgs = { "Hello, World!" };
			object objRet = hello.Invoke (null, invokeArgs);
			Assert.IsNull (objRet, "#2");
		}

		[Test]
		public void Name_Empty ()
		{
			DynamicMethod hello = new DynamicMethod (string.Empty,
				typeof (int),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest).Module);
			Assert.AreEqual (string.Empty, hello.Name, "#1");

			DynamicMethod write = new DynamicMethod ("Write",
				typeof (int),
				new Type[] { typeof (string) },
				typeof (DynamicMethodTest));

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
			Assert.AreEqual (2, ret, "#2");

			object[] invokeArgs = { "Hello, World!" };
			object objRet = hello.Invoke (null, invokeArgs);
			Assert.AreEqual (2, objRet, "#3");
		}
	}
}

#endif
