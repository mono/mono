using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.CSharp;
using System.IO;

namespace MonoTests.Visit
{
	[TestFixture]
	public class ASTVisitorTest
	{
		class TestVisitor : StructuralVisitor
		{
		}

		[SetUp]
		public void Setup ()
		{
		}

		[Test]
		public void Simple ()
		{
			//string content = @"class A { }";
			string content = @"

class Foo
{
	void Bar ()
	{
completionList.Add (""delegate"" + sb, ""md-keyword"", GettextCatalog.GetString (""Creates anonymous delegate.""), ""delegate"" + sb + "" {"" + Document.Editor.EolMarker + stateTracker.Engine.ThisLineIndent + TextEditorProperties.IndentString + ""|"" + Document.Editor.EolMarker + stateTracker.Engine.ThisLineIndent +""};"");
	}
}"
	;


			var stream = new MemoryStream (Encoding.UTF8.GetBytes (content));

			var ctx = new CompilerContext (new CompilerSettings (), new AssertReportPrinter ());

			ModuleContainer module = new ModuleContainer (ctx);
			var file = new SourceFile ("test", "asdfas", 0);
			CSharpParser parser = new CSharpParser (
				new SeekableStreamReader (stream, Encoding.UTF8),
				new CompilationSourceFile (module, file),
				ctx.Report,
				new ParserSession ());

			RootContext.ToplevelTypes = module;
			Location.Initialize (new List<SourceFile> { file });
			parser.parse ();

			Assert.AreEqual (0, ctx.Report.Errors);

			module.Accept (new TestVisitor ());
		}
	}
}
