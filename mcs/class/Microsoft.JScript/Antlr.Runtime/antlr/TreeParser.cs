using System;
using AST = antlr.collections.AST;
using BitSet = antlr.collections.impl.BitSet;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TreeParser.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public class TreeParser
	{
		/*The AST Null object; the parsing cursor is set to this when
		*  it is found to be null.  This way, we can test the
		*  token type of a node without having to have tests for null
		*  everywhere.
		*/
		public static ASTNULLType ASTNULL = new ASTNULLType();
		
		/*Where did this rule leave off parsing; avoids a return parameter */
		protected internal AST retTree_;
		
		/*guessing nesting level; guessing==0 implies not guessing */
		// protected int guessing = 0;
		
		/*Nesting level of registered handlers */
		// protected int exceptionLevel = 0;
		
		protected internal TreeParserSharedInputState inputState;
		
		/*Table of token type to token names */
		protected internal string[] tokenNames;
		
		/*AST return value for a rule is squirreled away here */
		protected internal AST returnAST;
		
		/*AST support code; parser and treeparser delegate to this object */
		protected internal ASTFactory astFactory = new ASTFactory();
		
		/*Used to keep track of indentdepth for traceIn/Out */
		protected internal int traceDepth = 0;
		
		public TreeParser()
		{
			inputState = new TreeParserSharedInputState();
		}
		/*Get the AST return value squirreled away in the parser */
		public virtual AST getAST()
		{
			return returnAST;
		}
		public virtual ASTFactory getASTFactory()
		{
			return astFactory;
		}
		public virtual string getTokenName(int num)
		{
			return tokenNames[num];
		}
		public virtual string[] getTokenNames()
		{
			return tokenNames;
		}
		protected internal virtual void  match(AST t, int ttype)
		{
			//System.out.println("match("+ttype+"); cursor is "+t);
			if (t == null || t == ASTNULL || t.Type != ttype)
			{
				throw new MismatchedTokenException(getTokenNames(), t, ttype, false);
			}
		}
		/*Make sure current lookahead symbol matches the given set
		* Throw an exception upon mismatch, which is catch by either the
		* error handler or by the syntactic predicate.
		*/
		public virtual void  match(AST t, BitSet b)
		{
			if (t == null || t == ASTNULL || !b.member(t.Type))
			{
				throw new MismatchedTokenException(getTokenNames(), t, b, false);
			}
		}
		protected internal virtual void  matchNot(AST t, int ttype)
		{
			//System.out.println("match("+ttype+"); cursor is "+t);
			if (t == null || t == ASTNULL || t.Type == ttype)
			{
				throw new MismatchedTokenException(getTokenNames(), t, ttype, true);
			}
		}

		/// <summary>
		/// @deprecated as of 2.7.2. This method calls System.exit() and writes
		/// directly to stderr, which is usually not appropriate when
		/// a parser is embedded into a larger application. Since the method is
		/// <code>static</code>, it cannot be overridden to avoid these problems.
		/// ANTLR no longer uses this method internally or in generated code.
		/// </summary>
		/// 
		[Obsolete("De-activated since version 2.7.2.6 as it cannot be overidden.", true)]
		public static void panic()
		{
			Console.Error.WriteLine("TreeWalker: panic");
			System.Environment.Exit(1);
		}
		/*Parser error-reporting function can be overridden in subclass */
		public virtual void  reportError(RecognitionException ex)
		{
			Console.Error.WriteLine(ex.ToString());
		}
		/*Parser error-reporting function can be overridden in subclass */
		public virtual void  reportError(string s)
		{
			Console.Error.WriteLine("error: " + s);
		}
		/*Parser warning-reporting function can be overridden in subclass */
		public virtual void  reportWarning(string s)
		{
			Console.Error.WriteLine("warning: " + s);
		}
		/*Specify an object with support code (shared by
		*  Parser and TreeParser.  Normally, the programmer
		*  does not play with this, using setASTNodeType instead.
		*/
		public virtual void  setASTFactory(ASTFactory f)
		{
			astFactory = f;
		}
		
		/*Specify the type of node to create during tree building */
		public virtual void  setASTNodeType(string nodeType)
		{
			setASTNodeClass(nodeType);
		}
		
		/*Specify the type of node to create during tree building */
		public virtual void  setASTNodeClass(string nodeType)
		{
			astFactory.setASTNodeType(nodeType);
		}
		
		public virtual void  traceIndent()
		{
			 for (int i = 0; i < traceDepth; i++)
				Console.Out.Write(" ");
		}
		public virtual void  traceIn(string rname, AST t)
		{
			traceDepth += 1;
			traceIndent();
			Console.Out.WriteLine("> " + rname + "(" + ((t != null) ? t.ToString() : "null") + ")" + ((inputState.guessing > 0) ? " [guessing]" : ""));
		}
		public virtual void  traceOut(string rname, AST t)
		{
			traceIndent();
			Console.Out.WriteLine("< " + rname + "(" + ((t != null) ? t.ToString() : "null") + ")" + ((inputState.guessing > 0) ? " [guessing]" : ""));
			traceDepth--;
		}
	}
}