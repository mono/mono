//
// System.Web.UI.DataBinderTests
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

//#define NUNIT // Comment out this one if you wanna play with the test without using NUnit

#if NUNIT
using NUnit.Framework;
#else
using System.Reflection;
#endif

using System.IO;
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Runtime.CompilerServices;

namespace MonoTests.System.Web.UI
{
#if NUNIT
	public class DataBinderTests : TestCase
	{
#else
	public class DataBinderTests
	{
#endif
#if NUNIT
		public static ITest Suite
		{
			get {
				return new TestSuite (typeof (PathTest));
			}
		}

		public DataBinderTests () : base ("MonoTests.System.Web.UI.DataBinderTests testcase") { }
		public DataBinderTests (string name) : base (name) { }

		protected override void SetUp ()
		{
#else
		static DataBinderTests ()
		{
#endif
			instance = new ClassInstance ("instance");
			instance.another = new ClassInstance ("another");
			echo = new StringEcho();
		}

		static ClassInstance instance;
		static StringEcho echo;

		public void TestEval1 ()
		{
			try {
				DataBinder.Eval (instance, "hello");
				Fail ("Eval1 #1 didn't throw exception");
			} catch (HttpException) {
			}

			object o = instance.Prop1;
			AssertEquals ("Eval1 #2", DataBinder.Eval (instance, "Prop1"), o);
			o = instance.Prop2;
			AssertEquals ("Eval1 #3", DataBinder.Eval (instance, "Prop2"), o);
			o = instance [0];
			AssertEquals ("Eval1 #4", DataBinder.Eval (instance, "[0]"), o);
			o = instance ["hi there!"];
			AssertEquals ("Eval1 #4", DataBinder.Eval (instance, "[\"hi there!\"]"), o);
		}

		public void TestEval2 ()
		{
			try {
				DataBinder.Eval (instance, "Another.hello");
				Fail ("Eval2 #1 didn't throw exception");
			} catch (HttpException) {
			}

			object o = instance.Another.Prop1;
			AssertEquals ("Eval2 #2", DataBinder.Eval (instance, "Another.Prop1"), o);
			o = instance.Another.Prop2;
			AssertEquals ("Eval2 #3", DataBinder.Eval (instance, "Another.Prop2"), o);
			o = instance.Another [0];
			AssertEquals ("Eval2 #4", DataBinder.Eval (instance, "Another[0]"), o);
			o = instance.Another ["hi there!"];
			AssertEquals ("Eval2 #4", DataBinder.Eval (instance, "Another[\"hi there!\"]"), o);
			AssertEquals ("Eval2 #5", DataBinder.Eval (instance,
								   "Another[\"hi there!\"] MS ignores this]"), o);

			// MS gets fooled with this!!!
			//AssertEquals ("Eval2 #4", DataBinder.Eval (instance, "Another[\"hi] there!\"]"), o);
		}

		public void TestEval3 ()
		{
			try {
				DataBinder.Eval (echo, "[0]");
				Fail ("Eval3 #1 didn't throw exception");
			} catch (ArgumentException) {
			}

			AssertEquals ("Eval3 #2", DataBinder.Eval (echo, "[test]"), "test");
			AssertEquals ("Eval3 #3", DataBinder.Eval (echo, "[\"test\"]"), "test");
			AssertEquals ("Eval3 #4", DataBinder.Eval (echo, "['test']"), "test");
			AssertEquals ("Eval3 #5", DataBinder.Eval (echo, "['test\"]"), "'test\"");
			AssertEquals ("Eval3 #6", DataBinder.Eval (echo, "[\"test']"), "\"test'");
		}


#if !NUNIT
		void Assert (string msg, bool result)
		{
			if (!result)
				Console.WriteLine (msg);
		}

		void AssertEquals (string msg, object expected, object real)
		{
			if (expected == null && real == null)
				return;

			if (expected != null && expected.Equals (real))
				return;

			Console.WriteLine ("{0}: expected: '{1}', got: '{2}'", msg, expected, real);
		}

		void Fail (string msg)
		{
			Console.WriteLine ("Failed: {0}", msg);
		}

		static void Main ()
		{
			DataBinderTests dbt = new DataBinderTests ();
			Type t = typeof (DataBinderTests);
			MethodInfo [] methods = t.GetMethods ();
			foreach (MethodInfo m in methods) {
				if (m.Name.Substring (0, 4) == "Test")
					m.Invoke (dbt, null);
			}
		}
#endif
	}

	class ClassInstance
	{
		public string hello = "Hello";
		public ClassInstance another;
		string prefix;

		public ClassInstance (string prefix)
		{
			this.prefix = prefix;
		}
		
		public object Prop1
		{
			get {
				return prefix + "This is Prop1";
			}
		}

		public object Prop2
		{
			get {
				return prefix + "This is Prop2";
			}
		}

		public object this [int index]
		{
			get {
				return prefix + "This is the indexer for int. Index: " + index;
			}
		}

		public object this [string index]
		{
			get {
				return prefix + "This is the indexer for string. Index: " + index;
			}
		}

		public ClassInstance Another
		{
			get {
				return another;
			}
		}
	}

	class StringEcho
	{
		public object this [string msg] {
			get { return msg; }
		}
	}
}

