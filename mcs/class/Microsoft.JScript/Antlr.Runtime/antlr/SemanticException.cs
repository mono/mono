using System;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: SemanticException.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public class SemanticException : RecognitionException
	{
		public SemanticException(string s) : base(s)
		{
		}
		
		[Obsolete("Replaced by SemanticException(string, string, int, int) since version 2.7.2.6", false)]
		public SemanticException(String s, String fileName, int line) :
					this(s, fileName, line, -1)
		{
			
		}

		public SemanticException(string s, string fileName, int line, int column) :
					base(s, fileName, line, column)
		{
		}
	}
}