//
// Mono.Data.SybaseClient.SybaseError.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
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
using System.Data;
using System.Runtime.InteropServices;

namespace Mono.Data.SybaseClient {
	[MonoTODO]
	public sealed class SybaseError
	{
		#region Fields

		byte theClass = 0;
		int lineNumber = 0;
		string message = "";
		int number = 0;
		string procedure = "";
		string server = "";
		string source = "";
		byte state = 0;

		#endregion // Fields

		#region Constructors

		internal SybaseError(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
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

		[MonoTODO]
		public byte Class {
			get { return theClass; }
		}

		[MonoTODO]
		public int LineNumber {
			get { return lineNumber; }
		}

		[MonoTODO]
		public string Message {
			get { return message; }
		}
		
		[MonoTODO]
		public int Number {
			get { return number; }
		}

		[MonoTODO]
		public string Procedure {
			get { return procedure; }
		}

		[MonoTODO]
		public string Server {
			get { return server; }
		}

		[MonoTODO]
		public string Source {
			get { return source; }
		}

		[MonoTODO]
		public byte State {
			get { return state; }
		}

		#endregion // Properties

		#region Methods

		internal void SetClass (byte theClass) {
			this.theClass = theClass;
		}

		internal void SetLineNumber (int lineNumber) {
			this.lineNumber = lineNumber;
		}

		internal void SetMessage (string message) {
			this.message = message;
		}

		internal void SetNumber (int number) {
			this.number = number;
		}

		internal void SetProcedure (string procedure) {
			this.procedure = procedure;
		}

		internal void SetServer (string server) {
			this.server = server;
		}

		internal void SetSource (string source) {
			this.source = source;
		}

		internal void SetState (byte state) {
			this.state = state;
		}

		[MonoTODO]
		public override string ToString ()
		{
			String toStr;
			String stackTrace;
			stackTrace = " <Stack Trace>";
			// FIXME: generate the correct SQL error string
			toStr = "SybaseError:" + message + stackTrace;
			return toStr;
		}

		#endregion // Methods
	}
}
