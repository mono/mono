using System;
using Stream			= System.IO.Stream;
using TextReader		= System.IO.TextReader;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: LexerSharedInputState.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*This object contains the data associated with an
	*  input stream of characters.  Multiple lexers
	*  share a single LexerSharedInputState to lex
	*  the same input stream.
	*/
	public class LexerSharedInputState
	{
		protected internal int column = 1;
		protected internal int line = 1;
		protected internal int tokenStartColumn = 1;
		protected internal int tokenStartLine = 1;
		protected internal InputBuffer input;
		
		/*What file (if known) caused the problem? */
		protected internal string filename;
		
		public int guessing = 0;
		
		public LexerSharedInputState(InputBuffer inbuf)
		{
			input = inbuf;
		}
		
		public LexerSharedInputState(Stream inStream) : this(new ByteBuffer(inStream))
		{
		}
		
		public LexerSharedInputState(TextReader inReader) : this(new CharBuffer(inReader))
		{
		}
		
		public virtual void  reset()
		{
			column = 1;
			line = 1;
			tokenStartColumn = 1;
			tokenStartLine = 1;
			guessing = 0;
			filename = null;
			input.reset();
		}
	}
}