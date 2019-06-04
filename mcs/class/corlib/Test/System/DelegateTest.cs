// DelegateTest.cs - NUnit Test Cases for the System.Delegate class
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
#if !MONOTOUCH && !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class DelegateTest
	{
		
		public class GenericClass<T> {
			public void Method<K> (T t, K k) {}
		}

		public delegate void SimpleDelegate(int a, double b);


		[Test] //See bug #372406
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #10539
#endif
		public void CreateDelegate1_Method_Private_Instance ()
		{
			C c = new C ();
			MethodInfo mi = typeof (C).GetMethod ("PrivateInstance", BindingFlags.NonPublic | BindingFlags.Instance);
			Delegate dg = Delegate.CreateDelegate (typeof (D), mi);
			Assert.AreSame (mi, dg.Method, "#1");
			Assert.IsNull (dg.Target, "#2");
			D d = (D) dg;
			d (c);
		}

		[Test] //Fixes a regression #377324
		public void GetMethodFromGenericClass ()
		{
			GenericClass<int> gclass = new GenericClass<int>();
			SimpleDelegate d = new SimpleDelegate (gclass.Method<double>);
			MethodInfo method = d.Method;
			MethodInfo target = typeof (GenericClass<int>).GetMethod ("Method").MakeGenericMethod(typeof(double));
			Assert.IsNotNull (method, "#1");
			Assert.AreEqual (target, method, "#2");
		}

		[Test] // CreateDelegate (Type, MethodInfo)
		public void CreateDelegate1_Method_Static ()
		{
			C c = new C ();
			MethodInfo mi = typeof (C).GetMethod ("S");
			Delegate dg = Delegate.CreateDelegate (typeof (D), mi);
			Assert.AreSame (mi, dg.Method, "#1");
			Assert.IsNull (dg.Target, "#2");
			D d = (D) dg;
			d (c);
		}

		[Test] // CreateDelegate (Type, MethodInfo)
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #14163
#endif
		public void CreateDelegate1_Method_Instance ()
		{
			C c = new C ();
			MethodInfo mi = typeof (C).GetMethod ("M");
			Delegate dg = Delegate.CreateDelegate (typeof (D), mi);
			Assert.AreSame (mi, dg.Method, "#1");
			Assert.IsNull (dg.Target, "#2");
			D d = (D) dg;
			d (c);
		}

		[Test] // CreateDelegate (Type, MethodInfo)
		public void CreateDelegate1_Method_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D), (MethodInfo) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, MethodInfo)
		public void CreateDelegate1_Type_Null ()
		{
			MethodInfo mi = typeof (C).GetMethod ("S");
			try {
				Delegate.CreateDelegate ((Type) null, mi);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2 ()
		{
			E e;

			e = (E) Delegate.CreateDelegate (typeof (E), new B (), "Execute");
			Assert.IsNotNull (e, "#A1");
			Assert.AreEqual (4, e (new C ()), "#A2");

			e = (E) Delegate.CreateDelegate (typeof (E), new C (), "Execute");
			Assert.IsNotNull (e, "#B1");
			Assert.AreEqual (4, e (new C ()), "#D2");

			e = (E) Delegate.CreateDelegate (typeof (E), new C (), "DoExecute");
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (102, e (new C ()), "#C2");
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_ArgumentsMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"StartExecute");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_CaseMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (), "ExecutE");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_DoesNotExist ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoesNotExist");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_Null ()
		{
			C c = new C ();
			try {
				Delegate.CreateDelegate (typeof (D), c, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_ReturnTypeMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoExecute");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Method_Static ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (), "Run");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Target_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D),null, "N");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("target", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String)
		public void CreateDelegate2_Type_Null ()
		{
			C c = new C ();
			try {
				Delegate.CreateDelegate ((Type) null, c, "N");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3 ()
		{
			E e;

			// matching static method
			e = (E) Delegate.CreateDelegate (typeof (E), typeof (B), "Run");
			Assert.IsNotNull (e, "#A1");
			Assert.AreEqual (5, e (new C ()), "#A2");

			// matching static method
			e = (E) Delegate.CreateDelegate (typeof (E), typeof (C), "Run");
			Assert.IsNotNull (e, "#B1");
			Assert.AreEqual (5, e (new C ()), "#B2");

			// matching static method
			e = (E) Delegate.CreateDelegate (typeof (E), typeof (C), "DoRun");
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (107, e (new C ()), "#C2");
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_ArgumentsMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B),
					"StartRun");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_CaseMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B), "RuN");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_DoesNotExist ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B),
					"DoesNotExist");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_Instance ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B), "Execute");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D), typeof (C), (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Method_ReturnTypeMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B),
					"DoRun");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Target_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D), (Type) null, "S");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("target", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Type, String)
		public void CreateDelegate3_Type_Null ()
		{
			try {
				Delegate.CreateDelegate ((Type) null, typeof (C), "S");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4 ()
		{
			E e;

			B b = new B ();

			// instance method, exact case, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Execute", true);
			Assert.IsNotNull (e, "#A1");
			Assert.AreEqual (4, e (new C ()), "#A2");

			// instance method, exact case, do not ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Execute", false);
			Assert.IsNotNull (e, "#B1");
			Assert.AreEqual (4, e (new C ()), "#B2");

			// instance method, case mismatch, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "ExecutE", true);
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (4, e (new C ()), "#C2");

			C c = new C ();

			// instance method, exact case, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Execute", true);
			Assert.IsNotNull (e, "#D1");
			Assert.AreEqual (4, e (new C ()), "#D2");

			// instance method, exact case, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "DoExecute", true);
			Assert.IsNotNull (e, "#E1");
			Assert.AreEqual (102, e (new C ()), "#E2");

			// instance method, exact case, do not ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Execute", false);
			Assert.IsNotNull (e, "#F1");
			Assert.AreEqual (4, e (new C ()), "#F2");

			// instance method, case mismatch, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "ExecutE", true);
			Assert.IsNotNull (e, "#G1");
			Assert.AreEqual (4, e (new C ()), "#G2");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_ArgumentsMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"StartExecute", false);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_CaseMismatch ()
		{
			// instance method, case mismatch, do not igore case
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"ExecutE", false);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_DoesNotExist ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoesNotExist", false);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D), new C (),
					(string) null, true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_ReturnTypeMismatch ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoExecute", false);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_Static ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (), "Run", true);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Target_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (D), null, "N", true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("target", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Type_Null ()
		{
			C c = new C ();
			try {
				Delegate.CreateDelegate ((Type) null, c, "N", true);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9 ()
		{
			E e;

			// do not ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"Execute", false, false);
			Assert.IsNotNull (e, "#A1");
			Assert.AreEqual (4, e (new C ()), "#A2");

			// do not ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"Execute", false, true);
			Assert.IsNotNull (e, "#B1");
			Assert.AreEqual (4, e (new C ()), "#B2");

			// ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"Execute", true, false);
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (4, e (new C ()), "#C2");

			// ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"Execute", true, true);
			Assert.IsNotNull (e, "#D1");
			Assert.AreEqual (4, e (new C ()), "#D2");

			// do not ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"Execute", false, false);
			Assert.IsNotNull (e, "#E1");
			Assert.AreEqual (4, e (new C ()), "#E2");

			// do not ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"Execute", false, true);
			Assert.IsNotNull (e, "#F1");
			Assert.AreEqual (4, e (new C ()), "#F2");

			// ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"Execute", true, false);
			Assert.IsNotNull (e, "#G1");
			Assert.AreEqual (4, e (new C ()), "#G2");

			// ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"Execute", true, true);
			Assert.IsNotNull (e, "#H1");
			Assert.AreEqual (4, e (new C ()), "#H2");

			// do not ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"DoExecute", false, false);
			Assert.IsNotNull (e, "#I1");
			Assert.AreEqual (102, e (new C ()), "#I2");

			// do not ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"DoExecute", false, true);
			Assert.IsNotNull (e, "#J1");
			Assert.AreEqual (102, e (new C ()), "#J2");

			// ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"DoExecute", true, false);
			Assert.IsNotNull (e, "#K1");
			Assert.AreEqual (102, e (new C ()), "#K2");

			// ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new C (),
				"DoExecute", true, true);
			Assert.IsNotNull (e, "#L1");
			Assert.AreEqual (102, e (new C ()), "#L2");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_ArgumentsMismatch ()
		{
			// throw bind failure
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"StartExecute", false, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// do not throw on bind failure
			E e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"StartExecute", false, false);
			Assert.IsNull (e, "#B");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_CaseMismatch ()
		{
			E e;

			// do not ignore case, throw bind failure
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"ExecutE", false, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// do not ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"ExecutE", false, false);
			Assert.IsNull (e, "#B");

			// ignore case, throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"ExecutE", true, true);
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (4, e (new C ()), "#C2");

			// ignore case, do not throw bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"ExecutE", true, false);
			Assert.IsNotNull (e, "#D1");
			Assert.AreEqual (4, e (new C ()), "#D2");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_DoesNotExist ()
		{
			// throw bind failure
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoesNotExist", false, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// do not throw on bind failure
			E e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"DoesNotExist", false, false);
			Assert.IsNull (e, "#B");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					(string) null, false, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_ReturnTypeMismatch ()
		{
			// throw bind failure
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"DoExecute", false, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// do not throw on bind failure
			E e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"DoExecute", false, false);
			Assert.IsNull (e, "#B");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Method_Static ()
		{
			// throw bind failure
			try {
				Delegate.CreateDelegate (typeof (E), new B (),
					"Run", true, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// do not throw on bind failure
			E e = (E) Delegate.CreateDelegate (typeof (E), new B (),
				"Run", true, false);
			Assert.IsNull (e, "#B");
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean, Boolean)
		public void CreateDelegate9_Target_Null ()
		{
			try {
				Delegate.CreateDelegate (typeof (E),(object) null,
					"Execute", true, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("target", ex.ParamName, "#6");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate9_Type_Null ()
		{
			try {
				Delegate.CreateDelegate ((Type) null, new B (),
					"Execute", true, false);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("type", ex.ParamName, "#6");
			}
		}

		class ParentClass
		{
		}

		class Subclass : ParentClass
		{
		}

		delegate ParentClass CoContraVariantDelegate (Subclass s);

		static Subclass CoContraVariantMethod (ParentClass s)
		{
			return null;
		}

		[Test]
		public void CoContraVariance ()
		{
			CoContraVariantDelegate d = (CoContraVariantDelegate)
				Delegate.CreateDelegate (typeof (CoContraVariantDelegate),
					typeof (DelegateTest).GetMethod ("CoContraVariantMethod",
					BindingFlags.NonPublic|BindingFlags.Static));
			d (null);
		}

		static object Box (object o)
		{
			return o;
		}

		delegate object Boxer ();

		[Test]
		public void BoxingCovariance ()
		{
			var boxer = (Boxer) Delegate.CreateDelegate (
				typeof (Boxer),
				42,
				GetType ().GetMethod ("Box", BindingFlags.NonPublic | BindingFlags.Static));

			Assert.IsNotNull (boxer);
			Assert.AreEqual (42, boxer ());
		}

		static object Nada (int o)
		{
			return (int) o;
		}

		delegate int WrongDelegate ();

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveDelegateTypeMismatch ()
		{
			Delegate boxer = new Boxer (() => new object ());
			Delegate.Remove (boxer, new WrongDelegate (() => 42));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongReturnTypeContravariance ()
		{
			Delegate.CreateDelegate (
				typeof (WrongDelegate),
				42,
				GetType ().GetMethod ("Nada", BindingFlags.NonPublic | BindingFlags.Static));
		}

		static int Identity (int i)
		{
			return i;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongReturnTypeContravariance_2 ()
		{
			Delegate.CreateDelegate (
				typeof (Boxer),
				42,
				GetType ().GetMethod ("Identity", BindingFlags.NonPublic | BindingFlags.Static));
		}

		delegate object CallTarget ();

		class Closure {}

		static object Target (Closure c)
		{
			return c;
		}

		[Test]
		[Category ("NotWasm")]
		public void NullFirstArgumentOnStaticMethod ()
		{
			CallTarget call = (CallTarget) Delegate.CreateDelegate (
				typeof (CallTarget),
				null,
				GetType ().GetMethod ("Target", BindingFlags.NonPublic | BindingFlags.Static));

			Assert.IsNotNull (call);
			Assert.IsNull (call.Target);
			Assert.IsNull (call ());
		}

		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #10539
#endif
		public void Virtual ()
		{
			// Delegate with abstract method, no target
			FooDelegate del = (FooDelegate)Delegate.CreateDelegate (typeof (FooDelegate), typeof (Iface).GetMethod ("retarg"));
			C c = new C ();
			Assert.AreEqual ("Hello", del (c, "Hello"));

			// Combination with normal delegates
			FooDelegate del2 = (FooDelegate)Delegate.CreateDelegate (typeof (FooDelegate), typeof (Iface).GetMethod ("retarg"));
			FooDelegate del3 = new FooDelegate (c.retarg2);
			FooDelegate del4 = (FooDelegate)Delegate.Combine (del2, del3);

			Assert.AreEqual ("Hello2", del4 (c, "Hello"));

			// Delegate with virtual method, no target
			FooDelegate2 del5 = (FooDelegate2)Delegate.CreateDelegate (typeof (FooDelegate2), typeof (B).GetMethod ("retarg3"));
			Assert.AreEqual ("Hello2", del5 (c, "Hello"));
		}

		int int_field;

		delegate int Del1 (DelegateTest dt, int i);

		public int method1 (int i) {
			return int_field + i;
		}

		[Test]
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #14163
#endif
		public void NullTarget_Instance ()
		{
			Del1 d = (Del1)Delegate.CreateDelegate (typeof (Del1), null, typeof (DelegateTest).GetMethod ("method1"));

			DelegateTest dt = new DelegateTest ();
			dt.int_field = 5;

			Assert.AreEqual (10, d (dt, 5));
		}

		delegate int Del2 (int i);

		public static int method2 (int i) {
			return i + 5;
		}

		[Test]
		public void NullTarget_Static ()
		{
			Del2 d = (Del2)Delegate.CreateDelegate (typeof (Del2), null, typeof (DelegateTest).GetMethod ("method2"));

			Assert.AreEqual (10, d (5));
		}

		delegate int Del3 (int i);

		public int method3 (int i) {
			return int_field + 5;
		}

		[Test]
		public void HasTarget_Instance ()
		{
			DelegateTest dt = new DelegateTest ();
			dt.int_field = 5;

			Del3 d = (Del3)Delegate.CreateDelegate (typeof (Del3), dt, typeof (DelegateTest).GetMethod ("method3"));

			Assert.AreEqual (10, d (5));
		}

		delegate int Del4 (int i);

		public static int method4 (string s, int i) {
			return Int32.Parse (s) + 5;
		}

		[Test]
		public void HasTarget_Static ()
		{
			Del4 d = (Del4)Delegate.CreateDelegate (typeof (Del4), "5", typeof (DelegateTest).GetMethod ("method4"));

			Assert.AreEqual (10, d (5));
		}

		public static long? StaticMethodToBeClosedOverNull (object o, long? bar)
		{
			Console.WriteLine ("o: {0}", o);
			return 5;
		}

		[Test] // #617161
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #10539
#endif
		public void ClosedOverNullReferenceStaticMethod ()
		{
			var del = (Func<long?,long?>) Delegate.CreateDelegate (
				typeof (Func<long?,long?>),
				null as object,
				this.GetType ().GetMethod ("StaticMethodToBeClosedOverNull"));

			Assert.IsNull (del.Target);

			Assert.AreEqual ((long?) 5, del (5));
		}

		public void InstanceMethodToBeClosedOverNull ()
		{
		}

		public void InstanceMethodIntToBeClosedOverNull (int i)
		{
		}

		[Test] // #475962
#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")] // #10539
#endif
		public void ClosedOverNullReferenceInstanceMethod ()
		{
			var action = (Action) Delegate.CreateDelegate (
				typeof (Action),
				null as object,
				this.GetType ().GetMethod ("InstanceMethodToBeClosedOverNull"));

			Assert.IsNull (action.Target);

			action ();

			var action_int = (Action<int>) Delegate.CreateDelegate (
				typeof (Action<int>),
				null as object,
				this.GetType ().GetMethod ("InstanceMethodIntToBeClosedOverNull"));

			Assert.IsNull (action.Target);

			action_int (42);
		}

		struct FooStruct {
			public int i, j, k, l;

			public int GetProp (int a, int b, int c, int d) {
				return i;
			}
		}

		delegate int ByRefDelegate (ref FooStruct s, int a, int b, int c, int d);

#if MONOTOUCH || FULL_AOT_RUNTIME
		[Category ("NotWorking")]
#endif
		[Test]
		public void CallVirtVType ()
		{
			var action = (ByRefDelegate)Delegate.CreateDelegate (typeof (ByRefDelegate), null, typeof (FooStruct).GetMethod ("GetProp"));
			var s = new FooStruct () { i = 42 };
			Assert.AreEqual (42, action (ref s, 1, 2, 3, 4));
		}

		class Foo {

			public void Bar ()
			{
			}
		}

		Foo foo;
		event Action bar_handler;

		[Test]
		[ExpectedException (typeof (ArgumentException))] // #635349, #605936
		public void NewDelegateClosedOverNullReferenceInstanceMethod ()
		{
			bar_handler += foo.Bar;
		}

		public void Banga ()
		{
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateDelegateOpenOnly ()
		{
			Delegate.CreateDelegate (
				typeof (Action),
				this.GetType ().GetMethod ("Banga"));
		}

		[Test] // #664205
		public void DynamicInvokeClosedStatic ()
		{
			var d1 = Delegate.CreateDelegate (typeof(Func<int>), null, typeof(DelegateTest).GetMethod ("DynamicInvokeClosedStaticDelegate_CB"));
			Assert.AreEqual (1, d1.DynamicInvoke (), "#1");

			var d2 = Delegate.CreateDelegate (typeof(Func<int>), "arg", typeof(DelegateTest).GetMethod ("DynamicInvokeClosedStaticDelegate_CB"));
			Assert.AreEqual (2, d2.DynamicInvoke (), "#2");
		}

		public static int DynamicInvokeClosedStaticDelegate_CB (string instance)
		{
			switch (instance) {
			case null:
				return 1;
			case "arg":
				return 2;
			default:
				Assert.Fail ();
				return -1;
			}
		}

		[Test]
		public void DynamicInvokeOpenInstanceDelegate ()
		{
			var d1 = Delegate.CreateDelegate (typeof (Func<DelegateTest, int>), typeof(DelegateTest).GetMethod ("DynamicInvokeOpenInstanceDelegate_CB"));
			Assert.AreEqual (5, d1.DynamicInvoke (new DelegateTest ()), "#1");

			var d3 = (Func<DelegateTest, int>) d1;
			Assert.AreEqual (5, d3 (null), "#2");
		}

		public int DynamicInvokeOpenInstanceDelegate_CB ()
		{
			return 5;
		}

		[Test]
		public void DynamicInvoke_InvalidArguments ()
		{
			Delegate d = new Func<int, int> (TestMethod);

			try {
				d.DynamicInvoke (null);
				Assert.Fail ("#1");
			} catch (TargetParameterCountException) {
			}

			try {
				d.DynamicInvoke (new object [0]);
				Assert.Fail ("#2");
			} catch (TargetParameterCountException) {
			}
		}

		public static int TestMethod (int i)
		{
			throw new NotSupportedException ();
		}

		public static void CreateDelegateOfStaticMethodBoundToNull_Helper (object[] args) {}

		[Test]
		public void CreateDelegateOfStaticMethodBoundToNull ()
		{
			Type t = typeof (Action);
			MethodInfo m = typeof (DelegateTest).GetMethod ("CreateDelegateOfStaticMethodBoundToNull_Helper");
			object firstArg = null;
	
			try {
				Delegate.CreateDelegate (t, m) ;
				Assert.Fail ("#1");
			} catch (ArgumentException) {  }
	
			try {
				Delegate.CreateDelegate(t, m, true);
				Assert.Fail ("#2");
			} catch (ArgumentException) {  }
	
			try {
				Delegate.CreateDelegate(t, m, false);
			} catch (ArgumentException) { Assert.Fail ("#3"); }
	
			try {
				Delegate.CreateDelegate(t, null, m);
			} catch (ArgumentException) { Assert.Fail ("#4"); }
	
			try {
				Delegate.CreateDelegate(t, null, m, true);
			} catch (ArgumentException) { Assert.Fail ("#5");  }
	
			try {
				Delegate.CreateDelegate(t, null, m, false);
			} catch (ArgumentException) { Assert.Fail ("#6"); }
		}

		[Test]
		public void GetHashCode_Constant () {
			Action del = delegate {
			};
			int hc1 = del.GetHashCode ();
			del ();
			int hc2 = del.GetHashCode ();
			Assert.AreEqual (hc1, hc2);
		}

		public interface CreateDelegateIFoo {
			int Test2 ();
		}
		
		public abstract class CreateDelegateFoo {
			public abstract int Test ();
		}
		
		public class CreateDelegateMid : CreateDelegateFoo {
			public override int Test () {
				return 1;
			}
		}
		
		public class CreateDelegateBar : CreateDelegateMid, CreateDelegateIFoo {
			public override int Test () {
				return 2;
			}
		
			public int Test2 () {
				return 3;
			}
		}
	
		public delegate int IntNoArgs ();

		[Test]
		public void CreateDelegateWithAbstractMethods ()
		{
			var f = new CreateDelegateBar ();
			var m = typeof (CreateDelegateFoo).GetMethod ("Test");
			var m2 = typeof (CreateDelegateMid).GetMethod ("Test");
			var m3 = typeof (CreateDelegateIFoo).GetMethod ("Test2");
	
			IntNoArgs a1 = (IntNoArgs)Delegate.CreateDelegate (typeof (IntNoArgs), f, m);
			IntNoArgs a2 = (IntNoArgs)Delegate.CreateDelegate (typeof (IntNoArgs), f, m2);
			IntNoArgs a3 = (IntNoArgs)Delegate.CreateDelegate (typeof (IntNoArgs), f, m3);

			Assert.AreEqual (2, a1 (), "#1");
			Assert.AreEqual (2, a2 (), "#2");
			Assert.AreEqual (3, a3 (), "#3");
		}
		

		delegate string FooDelegate (Iface iface, string s);

		delegate string FooDelegate2 (B b, string s);

		public interface Iface
		{
			string retarg (string s);
		}
#if !MONOTOUCH && !FULL_AOT_RUNTIME
		[Test]
		public void CreateDelegateWithLdFtnAndAbstractMethod ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "customMod";
			assemblyName.Version = new Version (1, 2, 3, 4);
	
			AssemblyBuilder assembly
				= Thread.GetDomain ().DefineDynamicAssembly (
					  assemblyName, AssemblyBuilderAccess.RunAndSave);
	
			ModuleBuilder module = assembly.DefineDynamicModule ("res", "res.dll");
	
			TypeBuilder tb = module.DefineType ("Test2", TypeAttributes.Public, typeof (object));
	
			{
				MethodBuilder mb =
					tb.DefineMethod ("test", MethodAttributes.Public | MethodAttributes.Static,
									 typeof (int), null);
				ILGenerator il = mb.GetILGenerator ();
	
				il.Emit (OpCodes.Newobj, typeof (CreateDelegateBar).GetConstructor (new Type [] { }));
				il.Emit (OpCodes.Ldftn, typeof (CreateDelegateIFoo).GetMethod ("Test2"));
				il.Emit (OpCodes.Newobj, typeof (IntNoArgs).GetConstructor (new Type [] { typeof (object), typeof (IntPtr) }));
				il.Emit (OpCodes.Call, typeof (IntNoArgs).GetMethod ("Invoke"));
				il.Emit (OpCodes.Ret);
			}
	
			Type t = tb.CreateType ();
	
			Object obj = Activator.CreateInstance (t, new object [0] { });
	
			int a = (int) t.GetMethod ("test").Invoke (obj, null);
			Assert.AreEqual (3, a, "#1");
		}
#endif
		public static int MethodWithIntParam (int x) {
			return 10;
		}

		[Test]
		[Category("NotWorking")]
		public void CantBindValueTypeToFirstArg () {
			try {
				Delegate.CreateDelegate (typeof (Delegate695978_2), 10, typeof (DelegateTest).GetMethod ("MethodWithIntParam"));
				Assert.Fail ("create delegate must fail");
			} catch (ArgumentException) {}
		}

		struct Struct695978 {
			public int value;
			public int test() { return value + 10; }
			public static int test2 (ref Struct695978 foo) { return foo.value + 20; }
		}

		delegate int Delegate695978_1 (ref Struct695978 _this);
		delegate int Delegate695978_2 ();
		delegate int Delegate695978_3 (Struct695978 _this);

		[Test] //tests for #695978
		[Category ("NotWorking")]
		public void DelegateWithValueTypeArguments ()
		{
			Struct695978 es = new Struct695978 ();
			es.value = 100;

			var ar1 = (Delegate695978_1)Delegate.CreateDelegate(typeof (Delegate695978_1), typeof (Struct695978).GetMethod("test"));
			Assert.IsNotNull (ar1);
			Assert.AreEqual (110, ar1 (ref es));

			var ar2 = (Delegate695978_2)Delegate.CreateDelegate(typeof (Delegate695978_2), null, typeof (Struct695978).GetMethod("test"));
			Assert.IsNotNull (ar2);
			Assert.AreEqual (110, ar2 ());

			ar1 = (Delegate695978_1) Delegate.CreateDelegate(typeof (Delegate695978_1), typeof (Struct695978).GetMethod("test2"));
			Assert.IsNotNull (ar1);
			Assert.AreEqual (120, ar1 (ref es));

			try {
				Delegate.CreateDelegate(typeof (Delegate695978_2), null, typeof (Struct695978).GetMethod("test2"));
				Assert.Fail ("#1");
			} catch (ArgumentException) {}


			ar2 = (Delegate695978_2) Delegate.CreateDelegate(typeof (Delegate695978_2), new Struct695978 (), typeof (Struct695978).GetMethod("test"));
			Assert.IsNotNull (ar2);
			Assert.AreEqual (120, ar2 ());

			try {
				Delegate.CreateDelegate(typeof (Delegate695978_2), new Struct695978 (), typeof (Struct695978).GetMethod("test2"));
				Assert.Fail ("#2");
			} catch (ArgumentException) {}

			try {
				Delegate.CreateDelegate(typeof (Delegate695978_3), typeof (Struct695978).GetMethod("test"));
				Assert.Fail ("#3");
			} catch (ArgumentException) {}
		}

		[Test]
		public void EnumBaseTypeConversion () {
			Func<int, int, bool> dm = Int32D2;
			var d =
				Delegate.CreateDelegate(typeof (Func<StringComparison, StringComparison, bool>), dm.Method) as
				Func<StringComparison, StringComparison, bool>; 
			Assert.IsTrue (d (0, 0));
		}

		[Test]
		public void EnumBaseTypeConversion2 () {
			Func<Enum22, int> dm = EnumArg;
			var d = (Func<int, int>)Delegate.CreateDelegate (typeof (Func<int, int>), dm.Method);
			Assert.AreEqual (1, d (1));
		}

		public enum Enum22 {
			none,
			one,
			two
		}

		public static int EnumArg (Enum22 e) {
			return (int)e;
		}

#if !MONOTOUCH && !FULL_AOT_RUNTIME
		public static void DynInvokeWithClosedFirstArg (object a, object b)
		{
		}

		[Test]
		public void DynamicInvokeClosedOverNullDelegate () {
			var dm = new DynamicMethod ("test", typeof (Delegate), null);
			var il = dm.GetILGenerator ();
			il.Emit (OpCodes.Ldnull);
			il.Emit (OpCodes.Ldftn, GetType ().GetMethod ("DynInvokeWithClosedFirstArg"));
			il.Emit (OpCodes.Newobj, typeof (Action<object>).GetConstructors ()[0]);
			il.Emit (OpCodes.Ret);

			var f = (Func <object>) dm.CreateDelegate (typeof (Func <object>));
			Action<object> ac = (Action<object>)f();
			ac.DynamicInvoke (new object[] { "oi" });
			ac.DynamicInvoke (new object[] { null });
		}

		[Test]
		public void DynamicInvokeFirstArgBoundDelegate () {
			var dm = new DynamicMethod ("test", typeof (Delegate), null);
			var il = dm.GetILGenerator ();
			il.Emit (OpCodes.Ldstr, "test");
			il.Emit (OpCodes.Ldftn, GetType ().GetMethod ("DynInvokeWithClosedFirstArg"));
			il.Emit (OpCodes.Newobj, typeof (Action<object>).GetConstructors ()[0]);
			il.Emit (OpCodes.Ret);

			var f = (Func <object>) dm.CreateDelegate (typeof (Func <object>));
			Action<object> ac = (Action<object>)f();
			ac.DynamicInvoke (new object[] { "oi" });
			ac.DynamicInvoke (new object[] { null });
		}
#endif

		public delegate void DoExecuteDelegate1 (C c);
		public delegate void DoExecuteDelegate2 (C c);

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DelegateCombineDifferentTypes () {
			var b = new B ();
			var del1 = new DoExecuteDelegate1 (b.DoExecute);
			var del2 = new DoExecuteDelegate2 (b.DoExecute);
			var del = Delegate.Combine (del1, del2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DelegateRemoveDifferentTypes () {
			var b = new B ();
			var del1 = new DoExecuteDelegate1 (b.DoExecute);
			var del2 = new DoExecuteDelegate2 (b.DoExecute);
			var del = Delegate.Remove (del1, del2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateDelegateThrowsAnArgumentExceptionWhenCalledWithAnOpenGeneric()
		{
			var m = GetType().GetMethod("AnyGenericMethod");
			Delegate.CreateDelegate(typeof(Action), this, m);
		}

		[Test]
		public void ReflectedTypeInheritedVirtualMethod ()
		{
			var a = new DerivedClass ();

			Action m = a.MyMethod;
			Assert.AreEqual (typeof (BaseClass), m.Method.ReflectedType);
		}

		class BaseClass
		{
			public virtual void MyMethod() {
				Console.WriteLine ("Base method");
			}
		}

		class DerivedClass : BaseClass
		{
		}

		public void AnyGenericMethod<T>()
		{
		}

		static bool Int32D2 (int x, int y)
		{
			return (x & y) == y; 
		}

		public class B {

			public virtual string retarg3 (string s) {
				return s;
			}

			static int Run (C x)
			{
				return 5;
			}

			public static void DoRun (C x)
			{
			}

			public static int StartRun (C x, B b)
			{
				return 6;
			}

			int Execute (C c)
			{
				return 4;
			}

			public void DoExecute (C c)
			{
			}

			public int StartExecute (C c, B b)
			{
				return 3;
			}
		}

		public class C : B, Iface
		{
			public string retarg (string s) {
				return s;
			}

			public string retarg2 (Iface iface, string s) {
				return s + "2";
			}

			public override string retarg3 (string s) {
				return s + "2";
			}

			static void Run (C x)
			{
			}

			public new static int DoRun (C x)
			{
				return 107;
			}

			void Execute (C c)
			{
			}

			public new int DoExecute (C c)
			{
				return 102;
			}

			public void M ()
			{
			}

			public void N (C c)
			{
			}

			public static void S (C c)
			{
			}

			private void PrivateInstance ()
			{
			}
		}

		public delegate void D (C c);
		public delegate int E (C c);
	}
}
