//
// JSParser.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

namespace Microsoft.JScript
{
	using System;

	public class JSParser
	{
		public JSParser (Context context)
		{
			throw new NotImplementedException ();
		}


		public ScriptBlock Parse ()
		{
			throw new NotImplementedException ();
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