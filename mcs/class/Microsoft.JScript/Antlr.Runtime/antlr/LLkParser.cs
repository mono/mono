using System;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: LLkParser.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*An LL(k) parser.
	*
	* @see antlr.Token
	* @see antlr.TokenBuffer
	* @see antlr.LL1Parser
	*/
	public class LLkParser : Parser
	{
		internal int k;
		
		public LLkParser(int k_)
		{
			k = k_;
		}
		public LLkParser(ParserSharedInputState state, int k_)
		{
			k = k_;
			inputState = state;
		}
		public LLkParser(TokenBuffer tokenBuf, int k_)
		{
			k = k_;
			setTokenBuffer(tokenBuf);
		}
		public LLkParser(TokenStream lexer, int k_)
		{
			k = k_;
			TokenBuffer tokenBuf = new TokenBuffer(lexer);
			setTokenBuffer(tokenBuf);
		}
		/*Consume another token from the input stream.  Can only write sequentially!
		* If you need 3 tokens ahead, you must consume() 3 times.
		* <p>
		* Note that it is possible to overwrite tokens that have not been matched.
		* For example, calling consume() 3 times when k=2, means that the first token
		* consumed will be overwritten with the 3rd.
		*/
		override public void  consume()
		{
			inputState.input.consume();
		}
		override public int LA(int i)
		{
			return inputState.input.LA(i);
		}
		override public Token LT(int i)
		{
			return inputState.input.LT(i);
		}
		private void  trace(string ee, string rname)
		{
			traceIndent();
			Console.Out.Write(ee + rname + ((inputState.guessing > 0)?"; [guessing]":"; "));
			 for (int i = 1; i <= k; i++)
			{
				if (i != 1)
				{
					Console.Out.Write(", ");
				}
				if ( LT(i)!=null ) {
					Console.Out.Write("LA(" + i + ")==" + LT(i).getText());
				}
				else 
				{
					Console.Out.Write("LA(" + i + ")==ull");
				}
			}
			Console.Out.WriteLine("");
		}
		override public void  traceIn(string rname)
		{
			traceDepth += 1;
			trace("> ", rname);
		}
		override public void  traceOut(string rname)
		{
			trace("< ", rname);
			traceDepth -= 1;
		}
	}
}