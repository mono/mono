using System;
using BitSet = antlr.collections.impl.BitSet;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TokenStreamBasicFilter.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*This object is a TokenStream that passes through all
	*  tokens except for those that you tell it to discard.
	*  There is no buffering of the tokens.
	*/
	public class TokenStreamBasicFilter : TokenStream
	{
		/*The set of token types to discard */
		protected internal BitSet discardMask;
		
		/*The input stream */
		protected internal TokenStream input;
		
		public TokenStreamBasicFilter(TokenStream input)
		{
			this.input = input;
			discardMask = new BitSet();
		}
		public virtual void  discard(int ttype)
		{
			discardMask.add(ttype);
		}
		public virtual void  discard(BitSet mask)
		{
			discardMask = mask;
		}
		public virtual Token nextToken()
		{
			Token tok = input.nextToken();
			while (tok != null && discardMask.member(tok.Type))
			{
				tok = input.nextToken();
			}
			return tok;
		}
	}
}