using System;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TokenBuffer.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/*A Stream of Token objects fed to the parser from a Tokenizer that can
	* be rewound via mark()/rewind() methods.
	* <p>
	* A dynamic array is used to buffer up all the input tokens.  Normally,
	* "k" tokens are stored in the buffer.  More tokens may be stored during
	* guess mode (testing syntactic predicate), or when LT(i>k) is referenced.
	* Consumption of tokens is deferred.  In other words, reading the next
	* token is not done by conume(), but deferred until needed by LA or LT.
	* <p>
	*
	* @see antlr.Token
	* @see antlr.Tokenizer
	* @see antlr.TokenQueue
	*/
	
	public class TokenBuffer
	{
		
		// Token source
		protected internal TokenStream input;
		
		// Number of active markers
		internal int nMarkers = 0;
		
		// Additional offset used when markers are active
		internal int markerOffset = 0;
		
		// Number of calls to consume() since last LA() or LT() call
		internal int numToConsume = 0;
		
		// Circular queue
		internal TokenQueue queue;
		
		/*Create a token buffer */
		public TokenBuffer(TokenStream input_)
		{
			input = input_;
			queue = new TokenQueue(1);
		}
		
		/*Reset the input buffer to empty state */
		public void  reset()
		{
			nMarkers = 0;
			markerOffset = 0;
			numToConsume = 0;
			queue.reset();
		}
		
		/*Mark another token for deferred consumption */
		public void  consume()
		{
			numToConsume++;
		}
		
		/*Ensure that the token buffer is sufficiently full */
		private void  fill(int amount)
		{
			syncConsume();
			// Fill the buffer sufficiently to hold needed tokens
			while (queue.nbrEntries < (amount + markerOffset))
			{
				// Append the next token
				queue.append(input.nextToken());
			}
		}
		
		/*return the Tokenizer (needed by ParseView) */
		public virtual TokenStream getInput()
		{
			return input;
		}
		
		/*Get a lookahead token value */
		public int LA(int i)
		{
			fill(i);
			return queue.elementAt(markerOffset + i - 1).Type;
		}
		
		/*Get a lookahead token */
		public Token LT(int i)
		{
			fill(i);
			return queue.elementAt(markerOffset + i - 1);
		}
		
		/*Return an integer marker that can be used to rewind the buffer to
		* its current state.
		*/
		public int mark()
		{
			syncConsume();
			nMarkers++;
			return markerOffset;
		}
		
		/*Rewind the token buffer to a marker.
		* @param mark Marker returned previously from mark()
		*/
		public void  rewind(int mark)
		{
			syncConsume();
			markerOffset = mark;
			nMarkers--;
		}
		
		/*Sync up deferred consumption */
		private void  syncConsume()
		{
			while (numToConsume > 0)
			{
				if (nMarkers > 0)
				{
					// guess mode -- leave leading tokens and bump offset.
					markerOffset++;
				}
				else
				{
					// normal mode -- remove first token
					queue.removeFirst();
				}
				numToConsume--;
			}
		}
	}
}