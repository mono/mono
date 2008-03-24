//
// System.Data.Odbc.OdbcError
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcError
	{
		readonly string _message;
		string _source;
		readonly string _state;
		readonly int _nativeerror;

		#region Constructors

		internal OdbcError (OdbcConnection connection)
		{
			_nativeerror = 1;
			_source = connection.SafeDriver;
			_message = "Error in " + _source;
			_state = string.Empty;
		}

		internal OdbcError (string message, string state, int nativeerror)
		{
			_message = message;
			_state = state;
			_nativeerror = nativeerror;
		}

		#endregion // Constructors
		
		#region Properties

		public string Message {
			get { return _message; }
		}

		public int NativeError {
			get { return _nativeerror; }
		}

		public string Source {
			get { return _source; }
		}

		public string SQLState {
			get { return _state; }
		}

		#endregion // Properties
		
		#region methods
		
		public override string ToString ()
		{
			return Message;
		}

		internal void SetSource (string source)
		{
			_source = source;
		}

		#endregion

	}

}
