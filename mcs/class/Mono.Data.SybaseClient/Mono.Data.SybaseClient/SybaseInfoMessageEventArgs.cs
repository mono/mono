//
// Mono.Data.SybaseClient.SybaseInfoMessageEventArgs.cs
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

using Mono.Data.Tds.Protocol;
using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseInfoMessageEventArgs : EventArgs
	{
		#region Fields

		SybaseErrorCollection errors = new SybaseErrorCollection ();

		#endregion // Fields

		#region Constructors

		internal SybaseInfoMessageEventArgs (TdsInternalErrorCollection tdsErrors)
		{
			foreach (TdsInternalError e in tdsErrors)
				errors.Add (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		#endregion // Constructors

		#region Properties

		public SybaseErrorCollection Errors {
			get { return errors; }
		}	

		public string Message {
			get { return errors[0].Message; }
		}	

		public string Source {
			get { return errors[0].Source; }
		}

		#endregion // Properties

		#region Methods

		public override string ToString () 
		{
			return Message;
		}

		#endregion // Methods
	}
}
