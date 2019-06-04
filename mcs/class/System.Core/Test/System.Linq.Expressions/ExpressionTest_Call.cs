//
// ExpressionTest_Call.cs
//
// Author:
//   Federico Di Gregorio <fog@initd.org>
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	[Category("SRE")]
	public class ExpressionTest_Call {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Call ((Type)null, "TestMethod", null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Call (typeof (MemberClass), null, null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Arg4WrongType ()
		{
			Expression.Call (typeof (MemberClass), "StaticMethod", null, Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InstanceMethod ()
		{
			Expression.Call (typeof (MemberClass), "TestMethod", null, Expression.Constant (1));
		}

		[Test]
		public void StaticMethod ()
		{
			Expression.Call (typeof (MemberClass), "StaticMethod", null, Expression.Constant (1));
		}

		[Test]
		public void StaticGenericMethod ()
		{
			Expression.Call (typeof (MemberClass), "StaticGenericMethod", new [] { typeof (int) }, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgMethodNull ()
		{
			Expression.Call (Expression.Constant (new object ()), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgInstanceNullForNonStaticMethod ()
		{
			Expression.Call (null, typeof (object).GetMethod ("ToString"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InstanceTypeDoesntMatchMethodDeclaringType ()
		{
#if MOBILE
			// ensure that String.Intern won't be removed by the linker
			string s = String.Intern (String.Empty);
#endif
			Expression.Call (Expression.Constant (1), typeof (string).GetMethod ("Intern"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgumentCountDoesnMatchParameterLength ()
		{
			Expression.Call (Expression.Constant (new object ()), typeof (object).GetMethod ("ToString"), Expression.Constant (new object ()));
		}

		public class Foo {
			public void Bar (string s)
			{
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodHasNullArgument ()
		{
			Expression.Call (Expression.New (typeof (Foo)), typeof (Foo).GetMethod ("Bar"), null as Expression);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgumentDoesntMatchParameterType ()
		{
			Expression.Call (Expression.New (typeof (Foo)), typeof (Foo).GetMethod ("Bar"), Expression.Constant (42));
		}

		[Test]
		public void CallToString ()
		{
			var call = Expression.Call (Expression.Constant (new object ()), typeof (object).GetMethod ("ToString"));
			Assert.AreEqual ("value(System.Object).ToString()", call.ToString ());
		}

		[Test]
		public void CallStringIsNullOrEmpty ()
		{
			var call = Expression.Call (null, typeof (string).GetMethod ("IsNullOrEmpty"), Expression.Constant (""));
			Assert.AreEqual ("IsNullOrEmpty(\"\")", call.ToString ());
		}

		[Test]
		[Category ("NotDotNet")] // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=339351
		[ExpectedException (typeof (ArgumentException))]
		public void CallStaticMethodWithInstanceArgument ()
		{
			Expression.Call (
				Expression.Parameter (GetType (), "t"),
				GetType ().GetMethod ("Identity"),
				Expression.Constant (null));
		}

		public static object Identity (object o)
		{
			return o;
		}

		[Test]
		public void CompileSimpleStaticCall ()
		{
			var p = Expression.Parameter (typeof (object), "o");
			var lambda = Expression.Lambda<Func<object, object>> (Expression.Call (GetType ().GetMethod ("Identity"), p), p);

			var i = lambda.Compile ();

			Assert.AreEqual (2, i (2));
			Assert.AreEqual ("Foo", i ("Foo"));
		}

		[Test]
		public void CompileSimpleInstanceCall ()
		{
			var p = Expression.Parameter (typeof (string), "p");
			var lambda = Expression.Lambda<Func<string, string>> (
				Expression.Call (
					p, typeof (string).GetMethod ("ToString", Type.EmptyTypes)),
				p);

			var ts = lambda.Compile ();

			Assert.AreEqual ("foo", ts ("foo"));
			Assert.AreEqual ("bar", ts ("bar"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void CheckTypeArgsIsNotUsedForParameterLookup ()
		{
			Expression.Call (GetType (), "EineMethod", new [] { typeof (string), typeof (int) }, "foo".ToConstant (), 2.ToConstant ());
		}

		public static void EineGenericMethod<X, Y> (string foo, int bar)
		{
		}

		[Test]
		public void CheckTypeArgsIsUsedForGenericArguments ()
		{
			var m = Expression.Call (GetType (), "EineGenericMethod", new [] { typeof (string), typeof (int) }, "foo".ToConstant (), 2.ToConstant ());
			Assert.IsNotNull (m.Method);
			Assert.AreEqual ("Void EineGenericMethod[String,Int32](System.String, Int32)", m.Method.ToString ());
		}

		public struct EineStrukt {

			public string Foo;

			public EineStrukt (string foo)
			{
				Foo = foo;
			}

			public string GimmeFoo ()
			{
				return Foo;
			}
		}

		[Test]
		public void CallMethodOnStruct ()
		{
			var param = Expression.Parameter (typeof (EineStrukt), "s");
			var foo = Expression.Lambda<Func<EineStrukt, string>> (
				Expression.Call (param, typeof (EineStrukt).GetMethod ("GimmeFoo")), param).Compile ();

			var s = new EineStrukt ("foo");
			Assert.AreEqual ("foo", foo (s));
		}

		public static int OneStaticMethod ()
		{
			return 42;
		}

		[Test]
		public void CallMethodOnDateTime ()
		{
			var left = Expression.Call (Expression.Constant (DateTime.Now), typeof(DateTime).GetMethod ("AddDays"), Expression.Constant (-5.0));
			var right = Expression.Constant (DateTime.Today.AddDays (1));
			var expr = Expression.GreaterThan (left, right);

			var eq = Expression.Lambda<Func<bool>> (expr).Compile ();
			Assert.IsFalse (eq ());
		}

		[Test]
		[Category ("NotDotNet")] // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=339351
		[ExpectedException (typeof (ArgumentException))]
		public void CallStaticMethodOnNonSenseInstanceExpression ()
		{
			Expression.Call (
				Expression.Constant ("la la la"),
				this.GetType ().GetMethod ("OneStaticMethod"));
		}

		public static int DoSomethingWith (ref int a)
		{
			return a + 4;
		}

		public static string DoAnotherThing (ref int a, string s)
		{
			return s + a;
		}

		[Test]
		public void CallStaticMethodWithRefParameter ()
		{
			var p = Expression.Parameter (typeof (int), "i");

			var c = Expression.Lambda<Func<int, int>> (
				Expression.Call (GetType ().GetMethod ("DoSomethingWith"), p), p).Compile ();

			Assert.AreEqual (42, c (38));
		}

		[Test]
		public void CallStaticMethodWithRefParameterAndOtherParameter ()
		{
			var i = Expression.Parameter (typeof (int), "i");
			var s = Expression.Parameter (typeof (string), "s");

			var lamda = Expression.Lambda<Func<int, string, string>> (
				Expression.Call (GetType ().GetMethod ("DoAnotherThing"), i, s), i, s).Compile ();

			Assert.AreEqual ("foo42", lamda (42, "foo"));
		}

#if !FULL_AOT_RUNTIME
		[Test]
		public void CallDynamicMethod_ToString ()
		{
			// Regression test for #49686
			var m = new DynamicMethod ("intIntId", typeof (int), new Type [] { typeof (int) });
			var ilg = m.GetILGenerator ();
			ilg.Emit (OpCodes.Ldarg_0);
			ilg.Emit (OpCodes.Ret);

			var i = Expression.Parameter (typeof (int), "i");
			var e = Expression.Call (m, i);

			Assert.IsNotNull (e.ToString ());
		}

		[Test]
		public void CallDynamicMethod_CompileInvoke ()
		{
			var m = new DynamicMethod ("intIntId", typeof (int), new Type [] { typeof (int) });
			var ilg = m.GetILGenerator ();
			ilg.Emit (OpCodes.Ldarg_0);
			ilg.Emit (OpCodes.Ret);

			var i = Expression.Parameter (typeof (int), "i");
			var e = Expression.Call (m, i);

			var lambda = Expression.Lambda<Func<int, int>> (e, i).Compile ();
			Assert.AreEqual (42, lambda (42));
		}
#endif

		public static int Bang (Expression i)
		{
			return (int) (i as ConstantExpression).Value;
		}
		static bool fout_called = false;

		public static int FooOut (out int x)
		{
			fout_called = true;
			return x = 0;
		}

		[Test]
		public void Connect282729 ()
		{
			// test from https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=282729

			var p = Expression.Parameter (typeof (int), "p");
			var lambda = Expression.Lambda<Func<int, int>> (
				Expression.Call (
					GetType ().GetMethod ("FooOut"),
					Expression.ArrayIndex(
						Expression.NewArrayBounds (
							typeof(int),
							1.ToConstant ()),
						0.ToConstant ())),
				p).Compile ();

			Assert.AreEqual (0, lambda (0));
			Assert.IsTrue (fout_called);
		}

		public static int FooOut2 (out int x)
		{
			x = 2;
			return 3;
		}

		[Test]
		public void Connect290278 ()
		{
			// test from https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=290278

			var p = Expression.Parameter (typeof (int [,]), "p");
			var lambda = Expression.Lambda<Func<int [,], int>> (
				Expression.Call (
					GetType ().GetMethod ("FooOut2"),
					Expression.ArrayIndex (p, 0.ToConstant (), 0.ToConstant ())),
				p).Compile ();

			int [,] data = { { 1 } };

			Assert.AreEqual (3, lambda (data));
			Assert.AreEqual (2, data [0, 0]);
		}

		public static void FooRef (ref string s)
		{
		}

		[Test]
		public void Connect297597 ()
		{
			// test from https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=297597

			var strings = new string [1];

			var lambda = Expression.Lambda<Action> (
				Expression.Call (
					GetType ().GetMethod ("FooRef"),
					Expression.ArrayIndex (
						Expression.Constant (strings), 0.ToConstant ()))).Compile ();

			lambda ();
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=319190
		[Category ("NotWorkingLinqInterpreter")]
		[Category ("NotWasm")]
		public void Connect319190 ()
		{
			var lambda = Expression.Lambda<Func<bool>> (
				Expression.TypeIs (
					Expression.New (typeof (TypedReference)),
					typeof (object))).Compile ();

			Assert.IsTrue (lambda ());
		}

		public static int Truc ()
		{
			return 42;
		}

		[Test]
		public void Connect282702 ()
		{
			var lambda = Expression.Lambda<Func<Func<int>>> (
				Expression.Convert (
					Expression.Call (
						typeof (Delegate).GetMethod ("CreateDelegate", new [] { typeof (Type), typeof (object), typeof (MethodInfo) }),
						Expression.Constant (typeof (Func<int>), typeof (Type)),
						Expression.Constant (null, typeof (object)),
						Expression.Constant (GetType ().GetMethod ("Truc"))),
					typeof (Func<int>))).Compile ();

			Assert.AreEqual (42, lambda ().Invoke ());
		}

		[Test]
		public void CallQueryableWhere ()
		{
			var queryable = new [] { 1, 2, 3 }.AsQueryable ();

			var parameter = Expression.Parameter (typeof (int), "i");
			var lambda = Expression.Lambda<Func<int, bool>> (
				Expression.LessThan (parameter, Expression.Constant (2)),
				parameter);

			var selector = Expression.Quote (lambda);

			var call = Expression.Call (
				typeof (Queryable),
				"Where",
				new [] { typeof (int) },
				queryable.Expression,
				selector);

			Assert.IsNotNull (call);
			Assert.IsNotNull (call.Method);
		}

		[Test]
		public void CallAsQueryable () // #537768
		{
			var constant = Expression.Constant (
				new List<string> (),
				typeof (IEnumerable<string>));

			var call = Expression.Call (
				typeof (Queryable),
				"AsQueryable",
				new [] { typeof (string) },
				constant);

			Assert.IsNotNull (call);
			Assert.AreEqual (1, call.Arguments.Count);
			Assert.AreEqual (constant, call.Arguments [0]);

			var method = call.Method;

			Assert.AreEqual ("AsQueryable", method.Name);
			Assert.IsTrue (method.IsGenericMethod);
			Assert.AreEqual (typeof (string), method.GetGenericArguments () [0]);
		}


		[Test]
		public void CallQueryableSelect () // #536637
		{
			var parameter = Expression.Parameter (typeof (string), "s");
			var string_length = Expression.Property (parameter, typeof (string).GetProperty ("Length"));
			var lambda = Expression.Lambda (string_length, parameter);

			var strings = new [] { "1", "22", "333" };

			var call = Expression.Call (
				typeof (Queryable),
				"Select",
				new [] { typeof (string), typeof (int) },
				Expression.Constant (strings.AsQueryable ()),
				lambda);

			Assert.IsNotNull (call);

			var method = call.Method;

			Assert.AreEqual ("Select", method.Name);
			Assert.IsTrue (method.IsGenericMethod);
			Assert.AreEqual (typeof (string), method.GetGenericArguments () [0]);
			Assert.AreEqual (typeof (int), method.GetGenericArguments () [1]);
		}

		[Test]
		public void CallNullableGetValueOrDefault () // #568989
		{
#if MOBILE
			// ensure that int?.GetValueOrDefault won't be removed by the linker
			Assert.AreEqual (0, ((int?)0).GetValueOrDefault (3));
#endif

			var value = Expression.Parameter (typeof (int?), "value");
			var default_parameter = Expression.Parameter (typeof (int), "default");

			var getter = Expression.Lambda<Func<int?, int, int>> (
				Expression.Call (
					value,
					"GetValueOrDefault",
					Type.EmptyTypes,
					default_parameter),
				value,
				default_parameter).Compile ();

			Assert.AreEqual (2, getter (null, 2));
			Assert.AreEqual (4, getter (4, 2));
		}

		[Test]
		public void CallToStringOnEnum () // #625367
		{
			var lambda = Expression.Lambda<Func<string>> (
				Expression.Call (
					Expression.Constant (TypeCode.Boolean, typeof (TypeCode)),
					typeof (object).GetMethod ("ToString"))).Compile ();

			Assert.AreEqual ("Boolean", lambda ());
		}

		public static void AcceptsIEnumerable(IEnumerable<object> o)
		{
		}

		[Test]
		public void CallIQueryableMethodWithNewArrayBoundExpression () // #2304
		{
			Expression.Call (
				GetType ().GetMethod ("AcceptsIEnumerable", BindingFlags.Public | BindingFlags.Static),
				Expression.NewArrayBounds (typeof (object), Expression.Constant (0)));
		}
	}
}
