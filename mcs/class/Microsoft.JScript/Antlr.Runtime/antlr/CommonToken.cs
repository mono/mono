using System;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: CommonToken.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public class CommonToken : Token
	{
		// most tokens will want line and text information
		protected internal int line;
		protected internal string text = null;
		protected internal int col;
		
		public CommonToken()
		{
		}
		
		public CommonToken(int t, string txt)
		{
			type_ = t;
			setText(txt);
		}
		
		public CommonToken(string s)
		{
			text = s;
		}
		
		override public int getLine()
		{
			return line;
		}
		
		override public string getText()
		{
			return text;
		}
		
		override public void  setLine(int l)
		{
			line = l;
		}
		
		override public void  setText(string s)
		{
			text = s;
		}
		
		override public string ToString()
		{
			return "[\"" + getText() + "\",<" + type_ + ">,line=" + line + ",col=" + col + "]";
		}
		
		/*Return token's start column */
		override public int getColumn()
		{
			return col;
		}
		
		override public void  setColumn(int c)
		{
			col = c;
		}
	}
}