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
		private CodeGeneratorOptions _options;

		[SetUp]
		public virtual void SetUp ()
		{
			_options = new CodeGeneratorOptions ();
		}

		protected abstract ICodeGenerator CodeGenerator
		{
			get;
		}

		protected virtual string NewLine
		{
			get { return "\n"; }
		}

		protected CodeGeneratorOptions Options
		{
			get { return _options; }
		}

		protected string GenerateCodeFromType (CodeTypeDeclaration type)
		{
			return GenerateCodeFromType (type, _options);
		}

		protected virtual string GenerateCodeFromType (CodeTypeDeclaration type, CodeGeneratorOptions options)
		{
			using (StringWriter writer = new StringWriter ()) {
				writer.NewLine = NewLine;
				CodeGenerator.GenerateCodeFromType (type, writer, options);
				writer.Close ();
				return writer.ToString ();
			}
		}
	}
}
