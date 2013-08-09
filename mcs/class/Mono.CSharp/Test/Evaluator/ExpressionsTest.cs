//
// ExpressionsTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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
using NUnit.Framework;
using Mono.CSharp;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace MonoTests.EvaluatorTest
{
	[TestFixture]
	public class ExpressionsTest : EvaluatorFixture
	{
		[Test]
		public void StatementExpression_1 ()
		{
			Evaluator.Run ("System.Console.WriteLine (100)");
		}

		[Test]
		public void StatementExpression_2 ()
		{
			Evaluator.Run ("var a = new int [] {1,2,3}; var b = a.Length;");
		}

		[Test]
		public void AnonymousType ()
		{
			Evaluator.Run ("var foo = new { Bar = \"baz\" };");
		}

		[Test]
		public void Simple ()
		{
			object res;
			res = Evaluator.Evaluate ("\"foo\" == \"bar\";");
			Assert.AreEqual (false, res, "CompareString");

			res = Evaluator.Evaluate ("var a = 1; a+2;");
			Assert.AreEqual (3, res, "CompareInt");

			res = Evaluator.Evaluate ("2 * 4;");
			Assert.AreEqual (8, res, "Multiply");
		}

		[Test]
		public void UsingAfterError ()
		{
			try {
				Evaluator.Evaluate ("File.OpenRead (\"/etc/passwd\");");
				Assert.Fail ("#1");
			} catch {
			}

			Evaluator.Run ("using System.IO;");
			Evaluator.Evaluate ("File.Exists (\"/etc/passwd\");");
		}

		[Test]
		public void WithTypeBuilders ()
		{
			object res;
			Evaluator.Run ("var a = new { A = 1 };");
			res = Evaluator.Evaluate ("a.ToString ();");
			Assert.AreEqual ("{ A = 1 }", res, "#1");
		}

		[Test]
		public void LinqExpressions ()
		{
			Evaluator.Run ("using System; using System.Linq;");

			Evaluator.Run ("var a = new int[]{1,2,3};");

			object res = Evaluator.Evaluate ("from x in a select x + 1;");
			CollectionAssert.AreEqual (new int[] { 2, 3, 4 }, ((IEnumerable<int>) res).ToArray ());
		}

		[Test]
		public void LinqExpressionStatements ()
		{
			Evaluator.Run ("using System; using System.Linq;");

			Evaluator.Run ("var first_scope = new int [] {1,2,3};");
			Evaluator.Run ("var second_scope = from x in first_scope select x;");
		}

		[Test]
		public void ReferenceLoading ()
		{
			Evaluator.ReferenceAssembly (typeof (ExpressionsTest).Assembly);
			object res = Evaluator.Evaluate ("typeof (MonoTests.EvaluatorTest.ExpressionsTest) != null;");
			Assert.AreEqual (true, res, "#1");
		}

		[Test]
		public void PartialExpression ()
		{
			object eval_result;
			bool result_set;
			string sres = Evaluator.Evaluate ("1+", out eval_result, out result_set);
			Assert.IsFalse (result_set, "No result should have been set");
			Assert.AreEqual ("1+", sres, "The result should have been the input string, since we have a partial input");
		}

#if NET_4_0
		[Test]
		public void DynamicStatement ()
		{
			Evaluator.Run ("dynamic d = 1;");
			Evaluator.Run ("d = 'a';");
			Evaluator.Run ("d.GetType ();");
		}
#endif

#if NET_4_5
		[Test]
		public void AwaitExpression ()
		{
			Evaluator.WaitOnTask = true;
			var res = Evaluator.Evaluate("var res = await System.Threading.Tasks.Task.FromResult (1) + await System.Threading.Tasks.Task.FromResult (2);");
			res = Evaluator.Evaluate ("res;");
			Assert.AreEqual (3, res, "#1");
		}
#endif
	}
}