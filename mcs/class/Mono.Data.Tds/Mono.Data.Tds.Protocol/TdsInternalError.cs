//
// Mono.Data.Tds.Protocol.TdsInternalError.cs
//
// Authors:
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace Mono.Data.Tds.Protocol {
	public sealed class TdsInternalError 
	{
		#region Fields

		byte theClass;
		int lineNumber;
		string message;
		int number;
		string procedure;
		string server;
		string source;
		byte state;

		#endregion // Fields

		#region Constructors

		public TdsInternalError (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			this.theClass = theClass;
			this.lineNumber = lineNumber;
			this.message = message;
			this.number = number;
			this.procedure = procedure;
			this.server = server;
			this.source = source;
			this.state = state;
		}

		#endregion // Constructors

		#region Properties

		public Byte Class {
			get { return theClass; }
			set { theClass = value; }
		}

		public int LineNumber {
			get { return lineNumber; }
			set { lineNumber = value; }
		}

		public string Message {
			get { return message; }
			set { message = value; }
		}

		public int Number {
			get { return number; }
			set { number = value; }
		}

		public string Procedure {
			get { return procedure; }
			set { procedure = value; }
		}

		public string Server {
			get { return server; }
			set { server = value;}
		}

		public string Source {
			get { return source; }
			set { source = value; }
		}

		public byte State {
			get { return state; }
			set { state = value; }
		}

		#endregion // Properties
	}
}

