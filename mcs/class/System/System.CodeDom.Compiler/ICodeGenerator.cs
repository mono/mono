//
// System.CodeDom.Compiler ICodeGenerator Interface
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom.Compiler {
	using System.CodeDom;
	using System.IO;

	public interface ICodeGenerator {

	
		// <summary>
		//   Generates code for @expression on @output
		// </summary>
		void GenerateCodeFromExpression (TextWriter output, CodeExpression expression);

		void GenerateCodeFromNamespace (TextWriter output, CodeExpression expression);

		void GenerateCodeFromStatement (TextWriter output, CodeStatement expression);

		bool IsValidIdentifier (string value);

		void ValidateIdentifier (string value);
	}
}
