using System;
using System.Runtime.InteropServices;
using Stream		= System.IO.Stream;
using BinaryReader	= System.IO.BinaryReader;
using IOException	= System.IO.IOException;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: ByteBuffer.cs,v 1.1 2003/04/22 04:56:12 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*A Stream of characters fed to the lexer from a InputStream that can
	* be rewound via mark()/rewind() methods.
	* <p>
	* A dynamic array is used to buffer up all the input characters.  Normally,
	* "k" characters are stored in the buffer.  More characters may be stored during
	* guess mode (testing syntactic predicate), or when LT(i>k) is referenced.
	* Consumption of characters is deferred.  In other words, reading the next
	* character is not done by conume(), but deferred until needed by LA or LT.
	* <p>
	*
	* @see antlr.CharQueue
	*/
	// SAS: added this class to handle Binary input w/ FileInputStream
	
	public class ByteBuffer:InputBuffer
	{
		
		// char source
		[NonSerialized()]
		internal Stream input;
		
		
		/*Create a character buffer */
		public ByteBuffer(Stream input_) : base()
		{
			input = input_;
		}
		
		/*Ensure that the character buffer is sufficiently full */
		override public void  fill(int amount)
		{
			try
			{
				syncConsume();
				// Fill the buffer sufficiently to hold needed characters
				while (queue.nbrEntries < (amount + markerOffset))
				{
					// Append the next character
					int c = input.ReadByte();
					queue.append((char) ((c==-1) ? CharScanner.EOF_CHAR : c));
				}
			}
			catch (IOException io)
			{
				throw new CharStreamIOException(io);
			}
		}
	}
}