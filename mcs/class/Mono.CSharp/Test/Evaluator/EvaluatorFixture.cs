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
			evaluator = new Evaluator (new CompilerSettings (), new Report (new AssertReportPrinter ()));
		}

		public Evaluator Evaluator {
			get {
				return evaluator;
			}
 		}
 	}
 }