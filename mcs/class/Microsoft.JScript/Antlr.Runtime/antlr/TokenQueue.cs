using System;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TokenQueue.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	/*A private circular buffer object used by the token buffer */

	class TokenQueue
	{
		/*Physical circular buffer of tokens */
		private Token[] buffer;
		/*buffer.length-1 for quick modulos */
		private int sizeLessOne;
		/*physical index of front token */
		private int offset;
		/*number of tokens in the queue */
		protected internal int nbrEntries;
		
		public TokenQueue(int minSize)
		{
			// Find first power of 2 >= to requested size
			int size;
			if (minSize < 0)
			{
				init(16); // pick some value for them
				return ;
			}
			// check for overflow
			if (minSize >= (int.MaxValue / 2))
			{
				init(int.MaxValue); // wow that's big.
				return ;
			}
			 for (size = 2; size < minSize; size *= 2)
			{
				;
			}
			init(size);
		}
		
		/*Add token to end of the queue
		* @param tok The token to add
		*/
		public void  append(Token tok)
		{
			if (nbrEntries == buffer.Length)
			{
				expand();
			}
			buffer[(offset + nbrEntries) & sizeLessOne] = tok;
			nbrEntries++;
		}
		
		/*Fetch a token from the queue by index
		* @param idx The index of the token to fetch, where zero is the token at the front of the queue
		*/
		public Token elementAt(int idx)
		{
			return buffer[(offset + idx) & sizeLessOne];
		}
		
		/*Expand the token buffer by doubling its capacity */
		private void  expand()
		{
			Token[] newBuffer = new Token[buffer.Length * 2];
			// Copy the contents to the new buffer
			// Note that this will store the first logical item in the
			// first physical array element.
			 for (int i = 0; i < buffer.Length; i++)
			{
				newBuffer[i] = elementAt(i);
			}
			// Re-initialize with new contents, keep old nbrEntries
			buffer = newBuffer;
			sizeLessOne = buffer.Length - 1;
			offset = 0;
		}
		
		/*Initialize the queue.
		* @param size The initial size of the queue
		*/
		private void  init(int size)
		{
			// Allocate buffer
			buffer = new Token[size];
			// Other initialization
			sizeLessOne = size - 1;
			offset = 0;
			nbrEntries = 0;
		}
		
		/*Clear the queue. Leaving the previous buffer alone.
		*/
		public void  reset()
		{
			offset = 0;
			nbrEntries = 0;
		}
		
		/*Remove token from front of queue */
		public void  removeFirst()
		{
			offset = (offset + 1) & sizeLessOne;
			nbrEntries--;
		}
	}
}