using System;

using AST			= antlr.collections.AST;

namespace antlr
{
	/* ANTLR Translator Generator
	 * Project led by Terence Parr at http://www.jGuru.com
	 * Software rights: http://www.antlr.org/RIGHTS.html
	 *
	 * $Id: DumpASTVisitor.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	 */

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/// <summary>
	/// Summary description for DumpASTVisitor.
	/// </summary>
	/** Simple class to dump the contents of an AST to the output */
	public class DumpASTVisitor : ASTVisitor 
	{
		protected int level = 0;


		private void tabs() 
		{
			for (int i = 0; i < level; i++) 
			{
				Console.Out.Write("   ");
			}
		}

		public void visit(AST node) 
		{
			// Flatten this level of the tree if it has no children
			bool flatten = /*true*/ false;
			AST node2;
			for (node2 = node; node2 != null; node2 = node2.getNextSibling()) 
			{
				if (node2.getFirstChild() != null) 
				{
					flatten = false;
					break;
				}
			}

			for (node2 = node; node2 != null; node2 = node2.getNextSibling()) 
			{
				if (!flatten || node2 == node) 
				{
					tabs();
				}
				if (node2.getText() == null) 
				{
					Console.Out.Write("nil");
				}
				else 
				{
					Console.Out.Write(node2.getText());
				}

				Console.Out.Write(" [" + node2.Type + "] ");

				if (flatten) 
				{
					Console.Out.Write(" ");
				}
				else 
				{
					Console.Out.WriteLine("");
				}

				if (node2.getFirstChild() != null) 
				{
					level++;
					visit(node2.getFirstChild());
					level--;
				}
			}

			if (flatten) 
			{
				Console.Out.WriteLine("");
			}
		}
	}  
}


