//
// JSParser.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;

namespace Microsoft.JScript {

	public class JSParser {

		internal JScriptParser Parser;

		public JSParser (Context context)
		{
			JSScanner scanner = new JSScanner (context);
			Parser = new JScriptParser (scanner.Lexer);
		}


		public ScriptBlock Parse ()
		{
			ScriptBlock prog = new ScriptBlock ();

			prog = Parser.program ();
			return prog;
		}


		public Block ParseEvalBody ()
		{
			throw new NotImplementedException ();
		}


		public void Tokenize ()
		{
			throw new NotImplementedException ();
		}
	}


	public class ParserException : Exception 
	{}

	
	public class EndOfFile : ParserException 
	{}
}