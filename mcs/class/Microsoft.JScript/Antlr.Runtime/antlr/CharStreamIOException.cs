using System;
using IOException = System.IO.IOException;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: CharStreamIOException.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/*
	* Wrap an IOException in a CharStreamException
	*/
	public class CharStreamIOException : CharStreamException
	{
		public IOException io;
		
		public CharStreamIOException(IOException io) : base(io.Message)
		{
			this.io = io;
		}
	}
}