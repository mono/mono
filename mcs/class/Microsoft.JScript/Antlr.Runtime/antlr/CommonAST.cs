using System;
using AST = antlr.collections.AST;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: CommonAST.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*Common AST node implementation */
	public class CommonAST : BaseAST
	{
		internal int ttype = Token.INVALID_TYPE;
		internal string text;
		
		
		protected CommonAST(CommonAST another)
		{
			// don't include child/sibling pointers in Clone()/dup()
			//down	= another.down;
			//right	= another.right;
			ttype	= another.ttype;
			text	= (another.text==null) ? null : String.Copy(another.text);
		}

		/*Get the token text for this node */
		override public string getText()
		{
			return text;
		}
		
		/*Get the token type for this node */
		override public int Type
		{
			get { return ttype;   }
			set { ttype = value; }
		}
		
		override public void  initialize(int t, string txt)
		{
			setType(t);
			setText(txt);
		}
		
		override public void  initialize(AST t)
		{
			setText(t.getText());
			setType(t.Type);
		}
		
		public CommonAST()
		{
		}
		
		public CommonAST(Token tok)
		{
			initialize(tok);
		}
		
		override public void  initialize(Token tok)
		{
			setText(tok.getText());
			setType(tok.Type);
		}
		/*Set the token text for this node */
		override public void  setText(string text_)
		{
			text = text_;
		}
		/*Set the token type for this node */
		override public void  setType(int ttype_)
		{
			this.Type = ttype_;
		}

		#region Implementation of ICloneable
		override public object Clone()
		{
			return new CommonAST(this);
		}
		#endregion
	}
}