//
// System.CodeDOM.Compiler ICodeGenerator Interface
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

public interface ICodeGenerator {

	using System.CodeDOM;
	using System.IO;
	
	// <summary>
	//   Generates code for @expression on @output
	// </summary>
	void GenerateCodeFromClass (TextWriter output, CodeClass expression);

	void GenerateCodeFromExpression (TextWriter output, CodeExpression expression);

	void GenerateCodeFromNamespace (TextWriter output, CodeExpression expression);

	void GenerateCodeFromStatement (TextWriter output, CodeStatement expression);

	bool IsValidIdentifier (string value);

	void ValidateIdentifier (string value);
}
