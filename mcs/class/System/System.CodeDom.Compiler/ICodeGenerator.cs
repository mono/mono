//
// System.CodeDom.Compiler ICodeGenerator Interface
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom.Compiler
{
	using System.CodeDom;
	using System.IO;

	public interface ICodeGenerator
	{
		//
		// Methods
		//
		string CreateEscapedIdentifier( string value );

		string CreateValidIdentifier( string value );

		void GenerateCodeFromCompileUnit( CodeCompileUnit compileUnit,
						  TextWriter output,
						  CodeGeneratorOptions options );

		void GenerateCodeFromExpression( CodeExpression expression,
						 TextWriter output, 
						 CodeGeneratorOptions options );

		void GenerateCodeFromNamespace( CodeNamespace ns,
						TextWriter output, 
						CodeGeneratorOptions options );

		void GenerateCodeFromStatement( CodeStatement statement,
						TextWriter output, 
						CodeGeneratorOptions options );

		void GenerateCodeFromType( CodeTypeDeclaration typeDeclaration,
					   TextWriter output,
					   CodeGeneratorOptions options );

		string GetTypeOutput( CodeTypeReference type );

		bool IsValidIdentifier( string value );

		bool Supports( GeneratorSupport supports );
 
		void ValidateIdentifier( string value );
	}
}
