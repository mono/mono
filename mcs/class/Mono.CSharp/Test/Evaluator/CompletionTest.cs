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