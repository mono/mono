//
// Mono.Data.TdsClient.TdsError.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
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

using Mono.Data.Tds.Protocol;
using System;

namespace Mono.Data.TdsClient {
        public sealed class TdsError
	{
		#region Fields
		
		byte theClass = 0x0;
		int lineNumber;
		string message;
		int number;
		string procedure;
		string server;
		string source;
		byte state;

		#endregion // Fields

		#region Constructors

		internal TdsError (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
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

		public byte Class {
			get { return theClass; }
		}

		public int LineNumber {
			get { return lineNumber; }
		}

		public string Message {
			get { return message; }
		}

		public int Number {
			get { return number; }
		}

		public string Procedure {
			get { return procedure; }
		}

		public string Server {
			get { return server; }
		}

		public string Source {
			get { return source; }
		}

		public byte State {
			get { return state; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
