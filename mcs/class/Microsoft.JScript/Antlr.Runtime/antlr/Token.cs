using System;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: Token.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*A token is minimally a token type.  Subclasses can add the text matched
	*  for the token and line info. 
	*/

	public class Token //: ICloneable
	{
		private void  InitBlock()
		{
			type_ = INVALID_TYPE;
		}
		// constants
		public const int MIN_USER_TYPE = 4;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int INVALID_TYPE = 0;
		public const int EOF_TYPE = 1;
		public static readonly int SKIP = - 1;
		
		// each Token has at least a token type
		protected int type_;
		
		// the illegal token object
		public static Token badToken = new Token(INVALID_TYPE, "<no text>");
		
		public Token()
		{
			InitBlock();
			;
		}
		public Token(int t)
		{
			InitBlock();
			type_ = t;
		}
		public Token(int t, string txt)
		{
			InitBlock();
			type_ = t;
			setText(txt);
		}
		public virtual int getColumn()
		{
			return 0;
		}
		public virtual int getLine()
		{
			return 0;
		}
		public string getFilename() 
		{
			return null;
		}

		public void setFilename(string name) 
		{
		}

		public virtual string getText()
		{
			return "<no text>";
		}

		public int Type
		{
			get { return type_;  }
			set { type_ = value; }
		}

		public virtual void setType(int newType)	{ this.Type = newType; }

		public virtual void  setColumn(int c)
		{
			;
		}
		public virtual void  setLine(int l)
		{
			;
		}
		public virtual void  setText(string t)
		{
			;
		}
		override public string ToString()
		{
			return "[\"" + getText() + "\",<" + type_ + ">]";
		}
	}
}