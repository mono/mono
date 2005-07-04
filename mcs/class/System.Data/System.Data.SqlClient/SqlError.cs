//
// System.Data.SqlClient.SqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Data.SqlClient {
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[Serializable]
	public sealed class SqlError
	{
		#region Fields

		byte errorClass = 0;
		int lineNumber = 0;
		string message = "";
		int number = 0;
		string procedure = "";
		string source = "";
		byte state = 0;

		[NonSerialized]		
		string server = "";
		

		#endregion // Fields

		#region Constructors

		internal SqlError (byte errorClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			this.errorClass = errorClass;
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
			get { return errorClass; }
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

		#endregion

		#region Methods

		public override string ToString ()
		{
			return Message;
		}

		#endregion
		
	}
}
