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
	}
}
