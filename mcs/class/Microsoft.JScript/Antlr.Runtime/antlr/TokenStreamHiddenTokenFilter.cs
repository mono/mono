using System;
using BitSet = antlr.collections.impl.BitSet;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TokenStreamHiddenTokenFilter.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*This object filters a token stream coming from a lexer
	* or another TokenStream so that only certain token channels
	* get transmitted to the parser.
	*
	* Any of the channels can be filtered off as "hidden" channels whose
	* tokens can be accessed from the parser.
	*/
	public class TokenStreamHiddenTokenFilter : TokenStreamBasicFilter, TokenStream
	{
		// protected BitSet discardMask;
		protected internal BitSet hideMask;
		
		private CommonHiddenStreamToken nextMonitoredToken;
		
		/*track tail of hidden list emanating from previous
		*  monitored token
		*/
		protected internal CommonHiddenStreamToken lastHiddenToken;
		
		protected internal CommonHiddenStreamToken firstHidden = null;
		
		public TokenStreamHiddenTokenFilter(TokenStream input) : base(input)
		{
			hideMask = new BitSet();
		}
		protected internal virtual void  consume()
		{
			nextMonitoredToken = (CommonHiddenStreamToken) input.nextToken();
		}
		private void  consumeFirst()
		{
			consume(); // get first token of input stream
			
			// Handle situation where hidden or discarded tokens
			// appear first in input stream
			CommonHiddenStreamToken p = null;
			// while hidden or discarded scarf tokens
			while (hideMask.member(LA(1).Type) || discardMask.member(LA(1).Type))
			{
				if (hideMask.member(LA(1).Type))
				{
					if (p == null)
					{
						p = LA(1);
					}
					else
					{
						p.setHiddenAfter(LA(1));
						LA(1).setHiddenBefore(p); // double-link
						p = LA(1);
					}
					lastHiddenToken = p;
					if (firstHidden == null)
					{
						firstHidden = p; // record hidden token if first
					}
				}
				consume();
			}
		}
		public virtual BitSet getDiscardMask()
		{
			return discardMask;
		}
		/*Return a ptr to the hidden token appearing immediately after
		*  token t in the input stream.
		*/
		public virtual CommonHiddenStreamToken getHiddenAfter(CommonHiddenStreamToken t)
		{
			return t.getHiddenAfter();
		}
		/*Return a ptr to the hidden token appearing immediately before
		*  token t in the input stream.
		*/
		public virtual CommonHiddenStreamToken getHiddenBefore(CommonHiddenStreamToken t)
		{
			return t.getHiddenBefore();
		}
		public virtual BitSet getHideMask()
		{
			return hideMask;
		}
		/*Return the first hidden token if one appears
		*  before any monitored token.
		*/
		public virtual CommonHiddenStreamToken getInitialHiddenToken()
		{
			return firstHidden;
		}
		public virtual void  hide(int m)
		{
			hideMask.add(m);
		}
		public virtual void  hide(BitSet mask)
		{
			hideMask = mask;
		}
		protected internal virtual CommonHiddenStreamToken LA(int i)
		{
			return nextMonitoredToken;
		}
		/*Return the next monitored token.
		*  Test the token following the monitored token.
		*  If following is another monitored token, save it
		*  for the next invocation of nextToken (like a single
		*  lookahead token) and return it then.
		*  If following is unmonitored, nondiscarded (hidden)
		*  channel token, add it to the monitored token.
		*
		*  Note: EOF must be a monitored Token.
		*/
		override public Token nextToken()
		{
			// handle an initial condition; don't want to get lookahead
			// token of this splitter until first call to nextToken
			if (LA(1) == null)
			{
				consumeFirst();
			}
			
			// we always consume hidden tokens after monitored, thus,
			// upon entry LA(1) is a monitored token.
			CommonHiddenStreamToken monitored = LA(1);
			// point to hidden tokens found during last invocation
			monitored.setHiddenBefore(lastHiddenToken);
			lastHiddenToken = null;
			
			// Look for hidden tokens, hook them into list emanating
			// from the monitored tokens.
			consume();
			CommonHiddenStreamToken p = monitored;
			// while hidden or discarded scarf tokens
			while (hideMask.member(LA(1).Type) || discardMask.member(LA(1).Type))
			{
				if (hideMask.member(LA(1).Type))
				{
					// attach the hidden token to the monitored in a chain
					// link forwards
					p.setHiddenAfter(LA(1));
					// link backwards
					if (p != monitored)
					{
						//hidden cannot point to monitored tokens
						LA(1).setHiddenBefore(p);
					}
					p = (lastHiddenToken = LA(1));
				}
				consume();
			}
			return monitored;
		}
	}
}