using System;
using AST = antlr.collections.AST;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: ASTPair.cs,v 1.1 2003/04/22 04:56:12 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/*ASTPair:  utility class used for manipulating a pair of ASTs
	* representing the current AST root and current AST sibling.
	* This exists to compensate for the lack of pointers or 'var'
	* arguments in Java.
	*/
	public class ASTPair
	{
		public AST root; // current root of tree
		public AST child; // current child to which siblings are added
		
		/*Make sure that child is the last sibling */
		public void  advanceChildToEnd()
		{
			if (child != null)
			{
				while (child.getNextSibling() != null)
				{
					child = child.getNextSibling();
				}
			}
		}
		
		/*Copy an ASTPair.  Don't call it clone() because we want type-safety */
		public virtual ASTPair copy()
		{
			ASTPair tmp = new ASTPair();
			tmp.root = root;
			tmp.child = child;
			return tmp;
		}
		
		override public string ToString()
		{
			string r = (root == null) ? "null" : root.getText();
			string c = (child == null) ? "null" : child.getText();
			return "[" + r + "," + c + "]";
		}
	}
}