// DelegateTest.cs - NUnit Test Cases for the System.Delegate class
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class DelegateTest
	{
#if NET_2_0
		
		public class GenericClass<T> {
			public void Method<K> (T t, K k) {}
		}

		public delegate void SimpleDelegate(int a, double b);


		[Test] //See bug #372406
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
#endif

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
		public void CreateDelegate1_Method_Instance ()
		{
			C c = new C ();
			MethodInfo mi = typeof (C).GetMethod ("M");
#if NET_2_0
			Delegate dg = Delegate.CreateDelegate (typeof (D), mi);
			Assert.AreSame (mi, dg.Method, "#1");
			Assert.IsNull (dg.Target, "#2");
			D d = (D) dg;
			d (c);
#else
			try {
				Delegate.CreateDelegate (typeof (D), mi);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Method must be a static method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("method", ex.ParamName, "#6");
			}
#endif
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

#if NET_2_0
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
		[Category ("TargetJvmNotWorking")]
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
		[Category ("NotWorking")]
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

		class Foo {

			public void Bar ()
			{
			}
		}

		Foo foo;
		event Action bar_handler;

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))] // #635349, #605936
		public void NewDelegateClosedOverNullReferenceInstanceMethod ()
		{
			bar_handler += foo.Bar;
		}

#endif

		[Test]
		public void GetHashCode_Constant () {
			Action del = delegate {
			};
			int hc1 = del.GetHashCode ();
			del ();
			int hc2 = del.GetHashCode ();
			Assert.AreEqual (hc1, hc2);
		}

		delegate string FooDelegate (Iface iface, string s);

		delegate string FooDelegate2 (B b, string s);

		public interface Iface
		{
			string retarg (string s);
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
