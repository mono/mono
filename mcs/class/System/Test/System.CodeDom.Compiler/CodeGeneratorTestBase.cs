//
// Base class for CodeGenerator unit tests
//
// Authors:
// Gert Driesen (drieseng@users.sourceforge.net)
//
// (c) Novell
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.CodeDom.Compiler
{
	public abstract class CodeGeneratorTestBase
	{
		private StringWriter _writer;
		private CodeGeneratorOptions _options;

		[SetUp]
		public virtual void SetUp ()
		{
			_writer = new StringWriter ();
			_writer.NewLine = "\n";
			_options = new CodeGeneratorOptions ();
		}

		protected abstract ICodeGenerator CodeGenerator
		{
			get;
		}

		protected StringWriter Writer
		{
			get { return _writer; }
		}

		protected virtual string GenerateCodeFromType (CodeTypeDeclaration type)
		{
			CodeGenerator.GenerateCodeFromType (type, _writer, _options);
			_writer.Close ();
			return _writer.ToString ();
		}
	}
}
