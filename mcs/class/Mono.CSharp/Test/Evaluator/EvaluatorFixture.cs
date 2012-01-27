using NUnit.Framework;
using Mono.CSharp;

namespace MonoTests.EvaluatorTest
 {
 	public class EvaluatorFixture
	{
		Evaluator evaluator;

 		[SetUp]
 		public void Setup ()
 		{
			var ctx = new CompilerContext (new CompilerSettings (), new AssertReportPrinter ());
			evaluator = new Evaluator (ctx);
		}

		public Evaluator Evaluator {
			get {
				return evaluator;
			}
 		}
 	}
 }