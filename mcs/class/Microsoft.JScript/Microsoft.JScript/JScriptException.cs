//
// JScriptException.cs:
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
using Microsoft.Vsa;
using System.Runtime.Serialization;

namespace Microsoft.JScript {

	[Serializable]
	public class JScriptException : ApplicationException, IVsaError
	{
		public JScriptException (JSError errorNumber)
		{
			throw new NotImplementedException ();
		}


		public string SourceMoniker {
			get { throw new NotImplementedException (); }
		}


		public int StartColumn {
			get { throw new NotImplementedException (); }
		}

	
		public int Column {
			get { throw new NotImplementedException (); }
		}


		public string Description {
			get { throw new NotImplementedException (); }
		}

		
		public int EndLine {
			get { throw new NotImplementedException (); }
		}


		public int EndColumn {
			get { throw new NotImplementedException (); }
		}


		public int Number {
			get { throw new NotImplementedException (); }
		}


		public int ErrorNumber {
			get { throw new NotImplementedException (); }
		}


		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}


		public int Line {
			get { throw new NotImplementedException (); }
		}


		public string LineText {
			get { throw new NotImplementedException (); }
		}


		public override string Message {
			get { throw new NotImplementedException (); }
		}


		public int Severity {
			get { throw new NotImplementedException (); }
		}


		public IVsaItem SourceItem {
			get { throw new NotImplementedException (); }
		}


		public override string StackTrace {
			get { throw new NotImplementedException (); }
		}
	}

	
	public class NoContextException : ApplicationException 
	{}
}
		
