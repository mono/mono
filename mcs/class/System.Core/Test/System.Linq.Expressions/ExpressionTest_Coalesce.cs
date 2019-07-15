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
//
// Authors:
//		Federico Di Gregorio <fog@initd.org>
//		Jb Evain <jbevain@novell.com>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	[Category("SRE")]
	public class ExpressionTest_Coalesce
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Coalesce (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Coalesce (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NonNullLeftParameter ()
		{
			// This throws because they are both doubles, which are never
			Expression.Coalesce (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Incompatible_Arguments ()
		{
			// The artuments are not compatible
			Expression.Coalesce (Expression.Parameter (typeof (int?), "a"),
					     Expression.Parameter (typeof (bool), "b"));
		}

		[Test]
		public void IsCoalesceStringLifted ()
		{
			var coalesce = Expression.Coalesce (
				Expression.Parameter (typeof (string), "a"),
				Expression.Parameter (typeof (string), "b"));

			Assert.AreEqual ("(a ?? b)", coalesce.ToString ());

			Assert.IsFalse (coalesce.IsLifted);
			Assert.IsFalse (coalesce.IsLiftedToNull);
		}

		[Test]
		public void IsCoalesceNullableIntLifted ()
		{
			var coalesce = Expression.Coalesce (
				Expression.Parameter (typeof (int?), "a"),
				Expression.Parameter (typeof (int?), "b"));

			Assert.IsFalse (coalesce.IsLifted);
			Assert.IsFalse (coalesce.IsLiftedToNull);
		}

		[Test]
		public void CoalesceNullableInt ()
		{
			var a = Expression.Parameter (typeof (int?), "a");
			var b = Expression.Parameter (typeof (int?), "b");
			var coalesce = Expression.Lambda<Func<int?, int?, int?>> (
				Expression.Coalesce (a, b), a, b).Compile ();

			Assert.AreEqual ((int?) 1, coalesce (1, 2));
			Assert.AreEqual ((int?) null, coalesce (null, null));
			Assert.AreEqual ((int?) 2, coalesce (null, 2));
			Assert.AreEqual ((int?) 2, coalesce (2, null));
		}

		[Test]
		public void CoalesceString ()
		{
			var a = Expression.Parameter (typeof (string), "a");
			var b = Expression.Parameter (typeof (string), "b");
			var coalesce = Expression.Lambda<Func<string, string, string>> (
				Expression.Coalesce (a, b), a, b).Compile ();

			Assert.AreEqual ("foo", coalesce ("foo", "bar"));
			Assert.AreEqual (null, coalesce (null, null));
			Assert.AreEqual ("bar", coalesce (null, "bar"));
			Assert.AreEqual ("foo", coalesce ("foo", null));
		}

		[Test]
		public void CoalesceNullableToNonNullable ()
		{
			var a = Expression.Parameter (typeof (int?), "a");

			var node = Expression.Coalesce (a, Expression.Constant (99, typeof (int)));

			Assert.AreEqual (typeof (int), node.Type);
			Assert.IsFalse (node.IsLifted);
			Assert.IsFalse (node.IsLiftedToNull);

			var coalesce = Expression.Lambda<Func<int?, int>> (node, a).Compile ();

			Assert.AreEqual (5, coalesce (5));
			Assert.AreEqual (99, coalesce (null));
		}

		[Test]
		[Category ("NotDotNet")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=349822
		public void CoalesceUserDefinedConversion ()
		{
			var s = Expression.Parameter (typeof (string), "s");

			var coalesce = Expression.Lambda<Func<string, int>> (
				Expression.Coalesce (
					s,
					Expression.Constant (42),
					Expression.Lambda<Func<string, int>> (
						Expression.Call (typeof (int).GetMethod ("Parse", new [] { typeof (string) }), s), s)), s).Compile ();

			Assert.AreEqual (12, coalesce ("12"));
			Assert.AreEqual (42, coalesce (null));
		}

		struct Slot {
			int Value;

			public Slot (int v)
			{
				Value = v;
			}

			public static implicit operator int (Slot s)
			{
				return s.Value;
			}
		}

		[Test]
		public void CoalesceNullableSlotIntoInteger ()
		{
			var s = Expression.Parameter (typeof (Slot?), "s");

			var method = typeof (Slot).GetMethod ("op_Implicit");

			var coalesce = Expression.Lambda<Func<Slot?, int>> (
				Expression.Coalesce (
					s,
					Expression.Constant (-3),
					Expression.Lambda (
						Expression.Convert (s, typeof (int), method),
						s)), s).Compile ();

			Assert.AreEqual (-3, coalesce (null));
			Assert.AreEqual (42, coalesce (new Slot (42)));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongCoalesceConversionParameterCount ()
		{
			var s = Expression.Parameter (typeof (string), "s");
			var p = Expression.Parameter (typeof (string), "foo");

			Expression.Coalesce (
				s,
				42.ToConstant (),
				Expression.Lambda<Func<string, string, int>> (
					Expression.Call (typeof (int).GetMethod ("Parse", new [] { typeof (string) }), s), s, p));

		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WrongCoalesceConversionParameterType ()
		{
			var s = Expression.Parameter (typeof (string), "s");
			var i = Expression.Parameter (typeof (int), "i");

			Expression.Coalesce (
				s,
				42.ToConstant (),
				Expression.Lambda<Func<int, int>> (
					i, i));

		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WrongCoalesceConversionReturnType ()
		{
			var s = Expression.Parameter (typeof (string), "s");

			Expression.Coalesce (
				s,
				42.ToConstant (),
				Expression.Lambda<Func<string, string>> (
					s, s));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CoalesceVoidUserDefinedConversion ()
		{
			var s = Expression.Parameter (typeof (string), "s");

			Expression.Coalesce (
				s,
				42.ToConstant (),
				Expression.Lambda<Action<string>> (
					Expression.Call (typeof (int).GetMethod ("Parse", new [] { typeof (string) }), s), s));
		}
	}
}
