//
// Mono.Data.Tds.Protocol.TdsInternalInfoMessageEventArgs.cs
//
// Authors:
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

namespace Mono.Data.Tds.Protocol {
	public class TdsInternalInfoMessageEventArgs : EventArgs
	{
		#region Fields

		TdsInternalErrorCollection errors;

		#endregion // Fields

		#region Constructors
		
		public TdsInternalInfoMessageEventArgs (TdsInternalErrorCollection errors)
		{
			this.errors = errors;
		}

		public TdsInternalInfoMessageEventArgs (TdsInternalError error)
		{
			this.errors = new TdsInternalErrorCollection ();
			errors.Add (error);
		}

		#endregion // Constructors

		#region Properties

		public TdsInternalErrorCollection Errors {
			get { return errors; }
		}

		public byte Class {
			get { return errors[0].Class; }
		}

		public int LineNumber {
			get { return errors[0].LineNumber; }
		}

		public string Message {
			get { return errors[0].Message; }
		}

		public int Number {
			get { return errors[0].Number; }
		}

		public string Procedure {
			get { return errors[0].Procedure; }
		}

		public string Server {
			get { return errors[0].Server; }
		}
		
		public string Source {
			get { return errors[0].Source; }
		}

		public byte State {
			get { return errors[0].State; }
		}

		#endregion // Properties

		#region Methods

		public int Add (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			return errors.Add (new TdsInternalError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		#endregion // Methods
	}
}
