using System;
using Hashtable		= System.Collections.Hashtable;
using Stack    		= System.Collections.Stack;
	
namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: TokenStreamSelector.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*A token stream MUX (multiplexor) knows about n token streams
	*  and can multiplex them onto the same channel for use by token
	*  stream consumer like a parser.  This is a way to have multiple
	*  lexers break up the same input stream for a single parser.
	*	Or, you can have multiple instances of the same lexer handle
	*  multiple input streams; this works great for includes.
	*/
	public class TokenStreamSelector : TokenStream
	{
		/*The set of inputs to the MUX */
		protected internal Hashtable inputStreamNames;
		
		/*The currently-selected token stream input */
		protected internal TokenStream input;
		
		/*Used to track stack of input streams */
		protected internal Stack streamStack = new Stack();
		
		public TokenStreamSelector() : base()
		{
			inputStreamNames = new Hashtable();
		}
		public virtual void  addInputStream(TokenStream stream, string key)
		{
			inputStreamNames[key] = stream;
		}
		/*Return the stream from tokens are being pulled at
		*  the moment.
		*/
		public virtual TokenStream getCurrentStream()
		{
			return input;
		}
		public virtual TokenStream getStream(string sname)
		{
			TokenStream stream = (TokenStream) inputStreamNames[sname];
			if (stream == null)
			{
				throw new System.ArgumentException("TokenStream " + sname + " not found");
			}
			return stream;
		}
		public virtual Token nextToken()
		{
			// return input.nextToken();
			// keep looking for a token until you don't
			// get a retry exception.
			 for (; ; )
			{
				try
				{
					return input.nextToken();
				}
				//catch (TokenStreamRetryException r)
				catch
				{
					// just retry "forever"
				}
			}
		}
		public virtual TokenStream pop()
		{
			TokenStream stream = (TokenStream) streamStack.Pop();
			select(stream);
			return stream;
		}
		public virtual void  push(TokenStream stream)
		{
			streamStack.Push(input); // save current stream
			select(stream);
		}
		public virtual void  push(string sname)
		{
			streamStack.Push(input);
			select(sname);
		}
		/*Abort recognition of current Token and try again.
		*  A stream can push a new stream (for include files
		*  for example, and then retry(), which will cause
		*  the current stream to abort back to this.nextToken().
		*  this.nextToken() then asks for a token from the
		*  current stream, which is the new "substream."
		*/
		public virtual void  retry()
		{
			throw new TokenStreamRetryException();
		}
		/*Set the stream without pushing old stream */
		public virtual void  select(TokenStream stream)
		{
			input = stream;
		}
		public virtual void  select(string sname)
		{
			input = getStream(sname);
		}
	}
}