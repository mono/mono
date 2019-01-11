//
// DynamicMethodTest.cs - NUnit Test Cases for the DynamicMethod class
//
// Gert Driesen (drieseng@users.sourceforge.net)
// Konrad Kruczynski
//
// (C) 2006 Novell


using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Linq;

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

		[Test]
		public void OwnerCantBeArray ()
		{
			TestOwner (typeof (int[]));
		}

		[Test]
		public void OwnerCantBeInterface ()
		{
			TestOwner (typeof (global::System.Collections.IEnumerable));
		}

		private void TestOwner (Type owner)
		{
			try {
				new DynamicMethod ("Name", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
				                   typeof(void), new Type[] { }, owner, true);
				Assert.Fail (string.Format ("Created dynamic method with owner being {0}.", owner));
			} catch (ArgumentException) {
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

		[Test]
		public void Circular_Refs () {
			DynamicMethod m1 = new DynamicMethod("f1", typeof(int), new Type[] { typeof (int) },
												 typeof(object));
			DynamicMethod m2 = new DynamicMethod("f2", typeof(int), new Type[] { typeof (int) },
												 typeof(object));

			ILGenerator il1 = m1.GetILGenerator();
			ILGenerator il2 = m2.GetILGenerator();

			Label l = il1.DefineLabel ();
			//il1.EmitWriteLine ("f1");
			il1.Emit (OpCodes.Ldarg_0);
			il1.Emit (OpCodes.Ldc_I4_0);
			il1.Emit (OpCodes.Bne_Un, l);
			il1.Emit (OpCodes.Ldarg_0);
			il1.Emit (OpCodes.Ret);
			il1.MarkLabel (l);
			il1.Emit (OpCodes.Ldarg_0);
			il1.Emit (OpCodes.Ldc_I4_1);
			il1.Emit (OpCodes.Sub);
			il1.Emit (OpCodes.Call, m2);
			il1.Emit (OpCodes.Ret);

			//il2.EmitWriteLine("f2");
			il2.Emit(OpCodes.Ldarg_0);
			il2.Emit(OpCodes.Call, m1);
			il2.Emit(OpCodes.Ret);

			m1.Invoke(null, new object[] { 5 });
		}

		// Disabl known warning, the Field is never used directly from C#
		#pragma warning disable 414
		class Host {
			static string Field = "foo";
		}
		#pragma warning restore 414
		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=297416
		public void TestOwnerMemberAccess ()
		{
			DynamicMethod method = new DynamicMethod ("GetField",
				typeof (string), new Type [0], typeof (Host));

			ILGenerator il = method.GetILGenerator ();
			il.Emit (OpCodes.Ldsfld, typeof (Host).GetField (
				"Field", BindingFlags.Static | BindingFlags.NonPublic));
			il.Emit (OpCodes.Ret);

			string ret = (string) method.Invoke (null, new object [] {});
			Assert.AreEqual ("foo", ret, "#1");
		}

		[Test]
		public void AnonHosted ()
		{
			DynamicMethod hello = new DynamicMethod ("Hello",
													 typeof (int),
													 new Type[] { typeof (string) });
			ILGenerator helloIL = hello.GetILGenerator ();
			helloIL.Emit (OpCodes.Ldc_I4_2);
			helloIL.Emit (OpCodes.Ret);

			HelloInvoker hi =
				(HelloInvoker) hello.CreateDelegate (typeof (HelloInvoker));
			int ret = hi ("Hello, World!");
			Assert.AreEqual (2, ret);

			object[] invokeArgs = { "Hello, World!" };
			object objRet = hello.Invoke (null, invokeArgs);
			Assert.AreEqual (2, objRet);
		}

		public delegate int IntInvoker();

		public class Foo<T> {
			public virtual int Test () { return 99; }
		} 

		[Test]
		public void ConstrainedPrexixDoesntCrash () //bug #529238
		{
			Type foo = typeof (Foo<int>);

			DynamicMethod dm = new DynamicMethod ("Hello", typeof (int), null);
			ILGenerator ilgen = dm.GetILGenerator ();
			ilgen.DeclareLocal (foo);
			ilgen.Emit (OpCodes.Newobj, foo.GetConstructor (new Type [0]));
			ilgen.Emit (OpCodes.Stloc_0);
			ilgen.Emit (OpCodes.Ldloca_S, 0);
			ilgen.Emit (OpCodes.Constrained, foo);
			ilgen.Emit (OpCodes.Callvirt, foo.GetMethod ("Test"));
			ilgen.Emit (OpCodes.Ret);

			IntInvoker hi = (IntInvoker) dm.CreateDelegate (typeof (IntInvoker));
			Assert.AreEqual (99, hi (), "#1");	
		}

		// #575955
		[Test]
		public void Module_GetMethod () {
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "foo";

			AssemblyBuilder assembly =
				AppDomain.CurrentDomain.DefineDynamicAssembly (
															   assemblyName, AssemblyBuilderAccess.RunAndSave);

			ModuleBuilder module = assembly.DefineDynamicModule ("foo.dll");

			var d = new DynamicMethod ("foo", typeof (int), new Type [] { typeof (int[,]) }, module);
			var ig = d.GetILGenerator ();
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldc_I4, 1);
			ig.Emit (OpCodes.Ldc_I4, 1);
			ig.Emit (OpCodes.Call, module.GetArrayMethod (typeof (int[,]), "Get", CallingConventions.Standard, typeof (int), new Type [] { typeof (int), typeof (int) }));
			ig.Emit (OpCodes.Ret);
		
			var del = (Func<int[,], int>)d.CreateDelegate (typeof (Func<int[,], int>));
			int[,] arr = new int [10, 10];
			arr [1, 1] = 5;
			Assert.AreEqual (5, del (arr));
		}

		[Test]
		[Category ("NotWorking")]
		public void InvalidUnicodeName ()
		{
			var name = new StringBuilder ().Append ('\udf45').Append ('\ud808');
			var method = new DynamicMethod (name.ToString (), typeof (bool), new Type [0]);
			var il = method.GetILGenerator ();
			il.Emit (OpCodes.Ldc_I4_1);
			il.Emit (OpCodes.Ret);

			var function = (Func<bool>) method.CreateDelegate (typeof (Func<bool>));

			Assert.IsTrue (function ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetMethodBody ()
		{
			var method = new DynamicMethod ("method", typeof (object), new Type [] { typeof (object) });

			var il = method.GetILGenerator ();
			il.Emit (OpCodes.Ldarg_0);
			il.Emit (OpCodes.Ret);

			var f = (Func<object, object>) method.CreateDelegate (typeof (Func<object, object>));
			f.Method.GetMethodBody ();
		}

		[Test]
		public void GetCustomAttributes ()
		{
			var method = new DynamicMethod ("method", typeof (void), new Type [] { });

			var methodImplAttrType = typeof (MethodImplAttribute);
			Assert.IsTrue (method.IsDefined (methodImplAttrType, true), "MethodImplAttribute is defined");

			// According to the spec, MethodImplAttribute is the
			// only custom attr that's present on a DynamicMethod.
			// And it's always a managed method with no inlining.
			var a1 = method.GetCustomAttributes (true);
			Assert.AreEqual (a1.Length, 1, "a1.Length == 1");
			Assert.AreEqual (a1[0].GetType (), methodImplAttrType, "a1[0] is a MethodImplAttribute");
			var options = (a1[0] as MethodImplAttribute).Value;
			Assert.IsTrue ((options & MethodImplOptions.NoInlining) != 0, "NoInlining is set");
			Assert.IsTrue ((options & MethodImplOptions.Unmanaged) == 0, "Unmanaged isn't set");


			// any other custom attribute type
			var extensionAttrType = typeof (ExtensionAttribute);
			Assert.IsFalse (method.IsDefined (extensionAttrType, true));
			Assert.AreEqual (Array.Empty<object>(), method.GetCustomAttributes (extensionAttrType, true));
		}

	public delegate object RetObj();
		[Test] //#640702
		public void GetCurrentMethodWorksWithDynamicMethods ()
		{
	        DynamicMethod dm = new DynamicMethod("Foo", typeof(object), null);
	        ILGenerator ilgen = dm.GetILGenerator();
	        ilgen.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
	        ilgen.Emit(OpCodes.Ret);
	        RetObj del = (RetObj)dm.CreateDelegate(typeof(RetObj));
		    MethodInfo res = (MethodInfo)del();
			Assert.AreEqual (dm.Name, res.Name, "#1");

		}

		[StructLayout (LayoutKind.Explicit)]
		struct SizeOfTarget {
			[FieldOffset (0)] public int X;
			[FieldOffset (4)] public int Y;
		}

		[Test]
		public void SizeOf ()
		{
			var method = new DynamicMethod ("", typeof (int), Type.EmptyTypes);
			var il = method.GetILGenerator ();
			il.Emit (OpCodes.Sizeof, typeof (SizeOfTarget));
			il.Emit (OpCodes.Ret);

			var func = (Func<int>) method.CreateDelegate (typeof (Func<int>));
			var point_size = func ();

			Assert.AreEqual (8, point_size);
		}

		class TypedRefTarget {
			public string Name;
		}

		class ExceptionHandling_Test_Support
		{
			public static Exception Caught;
			public static string CaughtStackTrace;

			public static void ThrowMe ()
			{
				Caught = null;
				CaughtStackTrace = null;
				throw new Exception("test");
			}

			public static void Handler (Exception e)
			{
				Caught = e;
				CaughtStackTrace = e.StackTrace.ToString ();
			}
		}

		[Test]
		public void ExceptionHandling ()
		{
			var method = new DynamicMethod ("", typeof(void), new[] { typeof(int) }, typeof (DynamicMethodTest));
			var ig = method.GetILGenerator ();

			ig.BeginExceptionBlock();
			ig.Emit(OpCodes.Call, typeof(ExceptionHandling_Test_Support).GetMethod("ThrowMe"));

			ig.BeginCatchBlock(typeof(Exception));
			ig.Emit(OpCodes.Call, typeof(ExceptionHandling_Test_Support).GetMethod("Handler"));
			ig.EndExceptionBlock();

			ig.Emit(OpCodes.Ret);

			var invoke = (Action<int>) method.CreateDelegate (typeof(Action<int>));
			invoke (456324);

			Assert.IsNotNull (ExceptionHandling_Test_Support.Caught, "#1");

			var lines = ExceptionHandling_Test_Support.CaughtStackTrace.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
			lines = lines.Where (l => !l.StartsWith ("[")).ToArray ();
			Assert.AreEqual (2, lines.Length, "#2");

			var st = new StackTrace (ExceptionHandling_Test_Support.Caught, 0, true);

			// Caught stack trace when dynamic method is gone
			Assert.AreEqual (ExceptionHandling_Test_Support.CaughtStackTrace, st.ToString (), "#3");

			// Catch handler stack trace inside dynamic method match
			Assert.AreEqual (ExceptionHandling_Test_Support.Caught.StackTrace, st.ToString (), "#4");
		}

		class ExceptionHandlingWithExceptionDispatchInfo_Test_Support
		{
			public static Exception Caught;
			public static string CaughtStackTrace;

			public static void ThrowMe ()
			{
				Caught = null;
				CaughtStackTrace = null;

				Exception e;
				try {
					throw new Exception("test");
				} catch (Exception e2) {
					e = e2;
				}

				var edi = ExceptionDispatchInfo.Capture(e);

				edi.Throw();
			}

			public static void Handler (Exception e)
			{
				var lines = e.StackTrace.Split (new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				// Ignore Metadata
				lines = lines.Where (l => !l.StartsWith ("[")).ToArray ();

				Assert.AreEqual (4, lines.Length, "#1");
				Assert.IsTrue (lines [1].Contains ("---"), "#2");
			}
		}

		[Test]
		public void ExceptionHandlingWithExceptionDispatchInfo ()
		{
			var method = new DynamicMethod ("", typeof(void), new[] { typeof(int) }, typeof (DynamicMethodTest));
			var ig = method.GetILGenerator ();

			ig.BeginExceptionBlock();
			ig.Emit(OpCodes.Call, typeof(ExceptionHandlingWithExceptionDispatchInfo_Test_Support).GetMethod("ThrowMe"));

			ig.BeginCatchBlock(typeof(Exception));
			ig.Emit(OpCodes.Call, typeof(ExceptionHandlingWithExceptionDispatchInfo_Test_Support).GetMethod("Handler"));
			ig.EndExceptionBlock();

			ig.Emit(OpCodes.Ret);

			var invoke = (Action<int>) method.CreateDelegate (typeof(Action<int>));
			invoke (444);
		}

		static Func<int> EmitDelegate (DynamicMethod dm) {
			ILGenerator il = dm.GetILGenerator ();
			var ret_val = il.DeclareLocal (typeof (int));
			var leave_label = il.DefineLabel ();

			//ret = 1;
			il.Emit (OpCodes.Ldc_I4, 1);
			il.Emit (OpCodes.Stloc, ret_val);

			// try {
			il.BeginExceptionBlock ();
			//	throw "hello";
			il.Emit (OpCodes.Ldstr, "hello");
			il.Emit (OpCodes.Throw, typeof (string));
			//	ret = 2
			il.Emit (OpCodes.Ldc_I4, 2);
			il.Emit (OpCodes.Stloc, ret_val);
			// }
			il.Emit (OpCodes.Leave, leave_label);
			//catch (string)
			il.BeginCatchBlock (typeof (string));
			il.Emit (OpCodes.Pop);
			//	ret = 3
			il.Emit (OpCodes.Ldc_I4, 3);
			il.Emit (OpCodes.Stloc, ret_val);
			//}
			il.Emit (OpCodes.Leave, leave_label);
			il.EndExceptionBlock ();

			il.MarkLabel (leave_label);
			//return ret;
			il.Emit (OpCodes.Ldloc, ret_val);
			il.Emit (OpCodes.Ret);

			var dele = (Func<int>)dm.CreateDelegate (typeof (Func<int>));
			return dele;
		}

		[Test] //see bxc #59334
		public void ExceptionWrapping ()
		{
			AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("ehatevfheiw"), AssemblyBuilderAccess.Run);
			AssemblyBuilder ab2 = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("ddf4234"), AssemblyBuilderAccess.Run);
			CustomAttributeBuilder cab = new CustomAttributeBuilder (
					typeof (RuntimeCompatibilityAttribute).GetConstructor (new Type [0]),
					new object [0],
					new PropertyInfo[] { typeof (RuntimeCompatibilityAttribute).GetProperty ("WrapNonExceptionThrows") },
					new object[] { true });
			ab2.SetCustomAttribute (cab);

			AssemblyBuilder ab3 = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName ("frfhfher"), AssemblyBuilderAccess.Run);
			//1 NamedArg. Property name: WrapNonExceptionThrows value: true (0x01) 
			byte[] blob = new byte[] { 0x01, 0x00, 0x01, 0x00, 0x54, 0x02, 0x16, 0x57, 0x72, 0x61, 0x70, 0x4E, 0x6F, 0x6E, 0x45, 0x78,
				0x63, 0x65, 0x70, 0x74, 0x69, 0x6F, 0x6E, 0x54, 0x68, 0x72, 0x6F, 0x77, 0x73, 0x01 };
			ab3.SetCustomAttribute (typeof (RuntimeCompatibilityAttribute).GetConstructor (new Type [0]), blob);
		
			DynamicMethod invoke_no_module = new DynamicMethod("throw_1", typeof (int), new Type [0]);
			DynamicMethod invoke_with_module = new DynamicMethod("throw_2", typeof (int), new Type [0], typeof (DynamicMethodTest).Module);
			DynamicMethod invoke_with_ab = new DynamicMethod("throw_3", typeof (int), new Type [0], ab.ManifestModule);
			DynamicMethod invoke_with_ab2 = new DynamicMethod("throw_4", typeof (int), new Type [0], ab2.ManifestModule);
			DynamicMethod invoke_with_ab3 = new DynamicMethod("throw_5", typeof (int), new Type [0], ab3.ManifestModule);

			int result = 0;
			try {
				int res = EmitDelegate (invoke_no_module)();
				Assert.AreEqual (3, res, "invoke_no_module bad return value");
			} catch (RuntimeWrappedException e) {
				Assert.Fail ("invoke_no_module threw RWE");
			}

			try {
				int res = EmitDelegate (invoke_with_module)();
				Assert.Fail ("invoke_with_module did not throw RWE");
			} catch (RuntimeWrappedException e) {
			}

			try {
				int res = EmitDelegate (invoke_with_ab)();
				Assert.AreEqual (3, res, "invoke_with_ab bad return value");
			} catch (RuntimeWrappedException e) {
				Assert.Fail ("invoke_with_ab threw RWE");
			}

			try {
				int res = EmitDelegate (invoke_with_ab2)();
				Assert.Fail ("invoke_with_ab2 did not throw RWE");
			} catch (RuntimeWrappedException e) {
			}

			try {
				int res = EmitDelegate (invoke_with_ab3)();
				Assert.Fail ("invoke_with_a3 did not throw RWE");
			} catch (RuntimeWrappedException e) {
			}			
		}

#if !MONODROID
		// RUNTIME: crash
		[Test]
		public void TypedRef ()
		{
			var method = new DynamicMethod ("", typeof (TypedRefTarget), new [] {typeof (TypedRefTarget)}, true);
			var il = method.GetILGenerator ();
			var tr = il.DeclareLocal (typeof (TypedReference));

			il.Emit (OpCodes.Ldarga, 0);
			il.Emit (OpCodes.Mkrefany, typeof (TypedRefTarget));
			il.Emit (OpCodes.Stloc, tr);

			il.Emit (OpCodes.Ldloc, tr);
			il.Emit (OpCodes.Call, GetType ().GetMethod ("AssertTypedRef", BindingFlags.NonPublic | BindingFlags.Static));

			il.Emit (OpCodes.Ldloc, tr);
			il.Emit (OpCodes.Refanyval, typeof (TypedRefTarget));
			il.Emit (OpCodes.Ldobj, typeof (TypedRefTarget));
			il.Emit (OpCodes.Ret);

			var f = (Func<TypedRefTarget, TypedRefTarget>) method.CreateDelegate (typeof (Func<TypedRefTarget, TypedRefTarget>));

			var target = new TypedRefTarget { Name = "Foo" };
			var rt = f (target);

			Assert.AreEqual (target, rt);
		}

		private static void AssertTypedRef (TypedReference tr)
		{
			Assert.AreEqual (typeof (TypedRefTarget), TypedReference.GetTargetType (tr));
		}
#endif

	    static Action GenerateProblematicMethod (bool add_extra, bool mismatch = false, bool use_vts = false)
	    {
			Type this_type = typeof(object);
			Type bound_type = typeof(object);
			if (mismatch) {
				this_type = typeof (string);
				bound_type = typeof (DynamicMethodTest);
			} else if (use_vts) {
				this_type = typeof (int);
				bound_type = typeof (long);
			}

	        Type[] args;
			if (add_extra)
				args = new[] { this_type };
			else
				args = new Type [0];

	        var mb = new DynamicMethod("Peek", null, args, bound_type, true);
	        var il = mb.GetILGenerator ();
	        il.Emit(OpCodes.Ret);
	        return (Action) mb.CreateDelegate(typeof(Action));
	    }

		[Test]
		public void ExtraArgGetsIgnored ()
		{
			GenerateProblematicMethod (true) ();
		}

		[Test]
		public void ExactNumberOfArgsWork ()
		{
			GenerateProblematicMethod (false) ();
		}

		[Test]
		public void ExtraArgWithMismatchedTypes ()
		{
			GenerateProblematicMethod (true, mismatch: true) ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ExtraArgWithValueType ()
		{
			GenerateProblematicMethod (true, use_vts: true) ();
		}
	}
}

