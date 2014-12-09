//
// DynamicMethodTest.cs - NUnit Test Cases for the DynamicMethod class
//
// Gert Driesen (drieseng@users.sourceforge.net)
// Konrad Kruczynski
//
// (C) 2006 Novell

#if NET_2_0

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

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
	}
}

#endif
