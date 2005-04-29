//
// JSParser.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

namespace Microsoft.JScript {

	public class JSParser {

		internal Parser Parser;
		internal Context context;

		public JSParser (Context context)
		{
			this.context = context;
			Parser = new Parser ();
		}


		public ScriptBlock Parse ()
		{
			string filename = context.Document.Name;
			StreamReader r = new StreamReader (filename);
			return (ScriptBlock) Parser.Parse (r, filename, 0);
		}


		public Block ParseEvalBody ()
		{
			throw new NotImplementedException ();
		}


		internal void Tokenize ()
		{
			throw new NotImplementedException ();
		}
	}


	public class ParserException : Exception 
	{}

	
	public class EndOfFile : ParserException 
	{}
}
