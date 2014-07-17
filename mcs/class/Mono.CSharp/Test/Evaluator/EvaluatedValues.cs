//
// EvaluatedValues.cs
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2014 Xamarin Inc. (http://www.xamarin.com)
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
using System.Collections.Generic;

using NUnit.Framework;

using Mono.CSharp;

namespace MonoTests.EvaluatorTest
{
	[TestFixture]
	public class EvaluatedValues : EvaluatorFixture
	{
		List<Evaluation> evaluations;

		public override void Setup ()
		{
			evaluations = new List<Evaluation> ();

			base.Setup ();

			Evaluator.NotifyAssignEvaluations = true;
			Evaluator.EvaluationListener = evaluations.Add;
		}

		void AssertEvaluation (string variableName, Type valueType, object value, Evaluation evaluation)
		{
			Assert.AreEqual (variableName, evaluation.VariableName);
			Assert.AreEqual (valueType, evaluation.ValueType);
			Assert.AreEqual (value, evaluation.Value);
		}

		[Test]
		public void SimpleAssign ()
		{
			Assert.IsTrue (Evaluator.Run ("var x = 150"));
			Assert.AreEqual (1, evaluations.Count);
			AssertEvaluation ("x", typeof(int), 150, evaluations [0]);
		}

		[Test]
		public void Increment ()
		{
			Assert.IsTrue (Evaluator.Run ("var x = 150; x++"));
			Assert.AreEqual (3, evaluations.Count);

			// var x = 150
			AssertEvaluation ("x", typeof(int), 150, evaluations [0]);

			// x++ (assign)
			AssertEvaluation ("x", typeof(int), 151, evaluations [1]);

			// x++ (optional assign / final evaluation value)
			AssertEvaluation (null, typeof(int), 151, evaluations [2]);
		}
	}
}