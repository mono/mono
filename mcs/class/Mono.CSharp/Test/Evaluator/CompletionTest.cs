//
// Completion.cs
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

namespace MonoTests.EvaluatorTest
{
	[TestFixture]
	public class Completion : EvaluatorFixture
	{
		[Test]
		public void SimpleSystemNamespace ()
		{
			Evaluator.Run ("using System;");

			string prefix;
			string[] res;
			res = Evaluator.GetCompletions ("ConsoleK", out prefix);
			Assert.AreEqual (new string[] { "ey", "eyInfo" }, res, "#1");

			res = Evaluator.GetCompletions ("Converte", out prefix);
			Assert.AreEqual (new string[] { "r" }, res, "#2");

			res = Evaluator.GetCompletions ("Sys", out prefix);
			Assert.AreEqual (new string[] { "tem", "temException" }, res, "#3");

			res = Evaluator.GetCompletions ("System.Int3", out prefix);
			Assert.AreEqual (new string[] { "2" }, res, "#4");
		}

		[Test]
		public void Initializers ()
		{
			string prefix;
			string[] res;
			res = Evaluator.GetCompletions ("new System.Text.StringBuilder () { Ca", out prefix);
			Assert.AreEqual (new string[] { "pacity" }, res, "#1");

			res = Evaluator.GetCompletions ("new System.Text.StringBuilder () { ", out prefix);
			Assert.AreEqual (new string[] { "Capacity", "Length", "MaxCapacity" }, res, "#2");
		}

		[Test]
		public void StringLocalVariable ()
		{
			string prefix;
			var res = Evaluator.GetCompletions ("string a.", out prefix);
			Assert.IsNull (res);
		}
	}
}