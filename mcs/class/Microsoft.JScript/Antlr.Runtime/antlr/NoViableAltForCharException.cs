using System;
using StringBuilder				= System.Text.StringBuilder;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: NoViableAltForCharException.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	public class NoViableAltForCharException : RecognitionException
	{
		public char foundChar;
		
		public NoViableAltForCharException(char c, CharScanner scanner) :
					base("NoViableAlt", scanner.getFilename(), scanner.getLine(), scanner.getColumn())
		{
			foundChar = c;
		}
		
		public NoViableAltForCharException(char c, string fileName, int line, int column) : 
					base("NoViableAlt", fileName, line, column)
		{
			foundChar = c;
		}
		
		/*
		* Returns a clean error message (no line number/column information)
		*/
		override public string Message
		{
			get
			{
				StringBuilder mesg = new StringBuilder("unexpected char: ");
			
				// I'm trying to mirror a change in the C++ stuff.
				// But java seems to lack something isprint-ish..
				// so we do it manually. This is probably to restrictive.
			
				if ((foundChar >= ' ') && (foundChar <= '~'))
				{
					mesg.Append('\'');
					mesg.Append(foundChar);
					mesg.Append('\'');
				}
				else
				{
					mesg.Append("0x");
				
					int t = (int) foundChar >> 4;
				
					if (t < 10)
						mesg.Append((char) (t | 0x30));
					else
						mesg.Append((char) (t + 0x37));
				
					t = (int) foundChar & 0xF;
				
					if (t < 10)
						mesg.Append((char) (t | 0x30));
					else
						mesg.Append((char) (t + 0x37));
				}
				return mesg.ToString();
			}
		}
	}
}