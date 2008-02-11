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
		[Category ("NotWorking")]
		public void CreateDelegate2 ()
		{
			E e;

			B b = new B ();

			// static method
			try {
				Delegate.CreateDelegate (typeof (E), b, "Run");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// matching instance method
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Execute");
			Assert.IsNotNull (e, "#B1");
			Assert.AreEqual (4, e (new C ()), "#B2");

			C c = new C ();

			// static method
			try {
				Delegate.CreateDelegate (typeof (E), c, "Run");
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}

			// matching instance method
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Execute");
			Assert.IsNotNull (e, "#D1");
			Assert.AreEqual (4, e (new C ()), "#D2");
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
		[Category ("NotWorking")]
		public void CreateDelegate3 ()
		{
			E e;

			// matching static method
			e = (E) Delegate.CreateDelegate (typeof (E), typeof (B), "Run");
			Assert.IsNotNull (e, "#A1");
			Assert.AreEqual (5, e (new C ()), "#A2");

			// instance method
			try {
				Delegate.CreateDelegate (typeof (E), typeof (B), "Execute");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			// matching static method
			e = (E) Delegate.CreateDelegate (typeof (E), typeof (C), "Run");
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (5, e (new C ()), "#C2");

			// instance method
			try {
				Delegate.CreateDelegate (typeof (E), typeof (C), "Execute");
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsNull (ex.ParamName, "#D5");
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
		[Category ("NotWorking")]
		public void CreateDelegate4 ()
		{
			E e;

			B b = new B ();

			// static method, exact case, ignore case
			try {
				Delegate.CreateDelegate (typeof (E), b, "Run", true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// static method, exact case, do not ignore case
			try {
				Delegate.CreateDelegate (typeof (E), b, "Run", false);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}

			// instance method, exact case, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Execute", true);
			Assert.IsNotNull (e, "#C1");
			Assert.AreEqual (4, e (new C ()), "#C2");

			// instance method, exact case, do not ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Execute", false);
			Assert.IsNotNull (e, "#D1");
			Assert.AreEqual (4, e (new C ()), "#D2");

			// instance method, case mismatch, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), b, "ExecutE", true);
			Assert.IsNotNull (e, "#E1");
			Assert.AreEqual (4, e (new C ()), "#E2");

			// instance method, case mismatch, do not igore case
			try {
				Delegate.CreateDelegate (typeof (E), b, "ExecutE", false);
				Assert.Fail ("#F1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#F2");
				Assert.IsNull (ex.InnerException, "#F3");
				Assert.IsNotNull (ex.Message, "#F4");
				Assert.IsNull (ex.ParamName, "#F5");
			}

			C c = new C ();

			// static method, exact case, ignore case
			try {
				Delegate.CreateDelegate (typeof (E), c, "Run", true);
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
				Assert.IsNull (ex.ParamName, "#G5");
			}

			// static method, exact case, do not ignore case
			try {
				Delegate.CreateDelegate (typeof (E), c, "Run", false);
				Assert.Fail ("#H1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#H2");
				Assert.IsNull (ex.InnerException, "#H3");
				Assert.IsNotNull (ex.Message, "#H4");
				Assert.IsNull (ex.ParamName, "#H5");
			}

			// instance method, exact case, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Execute", true);
			Assert.IsNotNull (e, "#I1");
			Assert.AreEqual (4, e (new C ()), "#I2");

			// instance method, exact case, do not ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Execute", false);
			Assert.IsNotNull (e, "#J1");
			Assert.AreEqual (4, e (new C ()), "#J2");

			// instance method, case mismatch, ignore case
			e = (E) Delegate.CreateDelegate (typeof (E), c, "ExecutE", true);
			Assert.IsNotNull (e, "#K1");
			Assert.AreEqual (4, e (new C ()), "#K2");

			// instance method, case mismatch, do not ignore case
			try {
				Delegate.CreateDelegate (typeof (E), c, "ExecutE", false);
				Assert.Fail ("#L1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#L2");
				Assert.IsNull (ex.InnerException, "#L3");
				Assert.IsNotNull (ex.Message, "#L4");
				Assert.IsNull (ex.ParamName, "#L5");
			}
		}

		[Test] // CreateDelegate (Type, Object, String, Boolean)
		public void CreateDelegate4_Method_Null ()
		{
			C c = new C ();
			try {
				Delegate.CreateDelegate (typeof (D), c, (string) null, true);
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
		[Category ("NotWorking")]
		public void CreateDelegate9_BindFailures ()
		{
			E e;

			B b = new B ();

			// static method, exact case, ignore case, throw on bind failure
			try {
				Delegate.CreateDelegate (typeof (E), b, "Run", true, true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			// static method, exact case, ignore case, do not throw on bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), b, "Run", true, false);
			Assert.IsNull (e, "#B");

			// instance method, case mismatch, do not igore case, throw on bind failure
			try {
				Delegate.CreateDelegate (typeof (E), b, "ExecutE",
					false, true);
				Assert.Fail ("#C1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsNull (ex.ParamName, "#C5");
			}

			// instance method, case mismatch, do not igore case, do not throw on bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), b, "ExecutE",
				false, false);
			Assert.IsNull (e, "#D");

			C c = new C ();

			// static method, exact case, ignore case, throw on bind failure
			try {
				Delegate.CreateDelegate (typeof (E), c, "Run", true, true);
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
				Assert.IsNotNull (ex.Message, "#E4");
				Assert.IsNull (ex.ParamName, "#E5");
			}

			// static method, exact case, ignore case, do not throw on bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), c, "Run", true, false);
			Assert.IsNull (e, "#F");

			// instance method, case mismatch, do not ignore case, throw on bind failure
			try {
				Delegate.CreateDelegate (typeof (E), c, "ExecutE", false, true);
				Assert.Fail ("#G1");
			} catch (ArgumentException ex) {
				// Error binding to target method
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#G2");
				Assert.IsNull (ex.InnerException, "#G3");
				Assert.IsNotNull (ex.Message, "#G4");
				Assert.IsNull (ex.ParamName, "#G5");
			}

			// instance method, case mismatch, do not ignore case, do not throw on bind failure
			e = (E) Delegate.CreateDelegate (typeof (E), c, "ExecutE",
				false, false);
			Assert.IsNull (e, "#H");
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
#endif
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

			int Execute (C c)
			{
				return 4;
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

			public void M ()
			{
			}

			public void N (C c)
			{
			}

			public static void S (C c)
			{
			}
		}

		public delegate void D (C c);
		public delegate int E (C c);
	}
}
