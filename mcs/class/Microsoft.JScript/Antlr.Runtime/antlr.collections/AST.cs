using System;
using IEnumerator = System.Collections.IEnumerator;

using Token = antlr.Token;
	
namespace antlr.collections
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: AST.cs,v 1.1 2003/04/22 04:59:15 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*Minimal AST node interface used by ANTLR AST generation
	* and tree-walker.
	*/
	public interface AST : ICloneable
	{
		/*Add a (rightmost) child to this node */
		void  addChild(AST c);
		bool Equals(AST t);
		bool EqualsList(AST t);
		bool EqualsListPartial(AST t);
		bool EqualsTree(AST t);
		bool EqualsTreePartial(AST t);
		IEnumerator findAll(AST tree);
		IEnumerator findAllPartial(AST subtree);
		/*Get the first child of this node; null if no children */
		AST getFirstChild();
		/*Get	the next sibling in line after this one */
		AST getNextSibling();
		/*Get the token text for this node */
		string getText();
		/*Get the token type for this node */
		int Type	{ get; set;}
		/// <summary>
		/// Get number of children of this node; if leaf, returns 0
		/// </summary>
		/// <returns>Number of children</returns>
		int getNumberOfChildren();
		void  initialize(int t, string txt);
		void  initialize(AST t);
		void  initialize(Token t);
		/*Set the first child of a node. */
		void  setFirstChild(AST c);
		/*Set the next sibling after this one. */
		void  setNextSibling(AST n);
		/*Set the token text for this node */
		void  setText(string text);
		/*Set the token type for this node */
		void  setType(int ttype);
		string ToString();
		string ToStringList();
		string ToStringTree();
	}
	
}