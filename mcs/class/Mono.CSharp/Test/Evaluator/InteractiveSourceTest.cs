using System;
using NUnit.Framework;
using Mono.CSharp;

namespace MonoTests.EvaluatorTest
{
	public class CatchInteractiveSourceFileErrorReportPrinter : ReportPrinter
	{
		public override void Print (AbstractMessage msg)
		{
			Assert.False (msg.Text == "Source file `{interactive}' could not be found");
		}
	}

	[TestFixture]
	public class ReportTest
	{
		CompilerSettings settings;
		Report report;
		Evaluator eval;

		[SetUp]
		public void Setup ()
		{
			settings = new CompilerSettings ();
			report = new Report (new CatchInteractiveSourceFileErrorReportPrinter ());
			eval = new Evaluator (settings, report);
		}

		[Test]
		public void CreateInteractiveSourceFileError ()
		{
			//creating a second Evaluator using the same CompilerSettings object creates a second '{interactive}' CompilationSourceFile
			//object in CompilerSettings.SourceFiles
			Evaluator secondEval = new Evaluator (settings, report);
			secondEval.Run ("int i = 0;");
		}
	}
}
