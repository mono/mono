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

namespace System.Data.Odbc
{
	[Serializable]
	public sealed class OdbcError
	{
		string _message;
		string _source;
		string _state;
		int _nativeerror;

		#region Constructors

		internal OdbcError (string Source)
		{
			_nativeerror = 1;
			_source = Source;
			_message = "Error in " + _source;
			_state = "";
		}

		internal OdbcError (string Source, OdbcHandleType HandleType, IntPtr Handle)
		{
			short buflen = 256, txtlen = 0;
			OdbcReturn ret = OdbcReturn.Success;
			byte [] buf_MsgText = new byte [buflen];
			byte [] buf_SqlState = new byte [buflen];
			bool NeedsDecode = true;
			_source = Source;
			switch (HandleType)
			{
				case OdbcHandleType.Dbc:
					ret = libodbc.SQLError(IntPtr.Zero, Handle, IntPtr.Zero, buf_SqlState,
						ref _nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Stmt:
					ret = libodbc.SQLError(IntPtr.Zero, IntPtr.Zero, Handle, buf_SqlState,
						ref _nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Env:
					ret = libodbc.SQLError(Handle, IntPtr.Zero, IntPtr.Zero, buf_SqlState,
						ref _nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				default:
					_nativeerror = 1;
					_source = Source;
					_message = "Error in " + _source;
					_state = "";
					NeedsDecode = false;
					break;
			}
			if (NeedsDecode)
			{
				if (ret != OdbcReturn.Success)
				{
					_nativeerror = 1;
					_source = Source;
					_message = "Unable to retreive error information from ODBC driver manager";
					_state = "";
				}
				else
				{
					_state = System.Text.Encoding.Default.GetString (buf_SqlState).Replace ((char) 0, ' ').Trim ();
					_message = System.Text.Encoding.Default.GetString (buf_MsgText).Replace ((char) 0, ' ').Trim ();
				}
			}
		}

		#endregion // Constructors
		
		#region Properties

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public int NativeError
		{
			get
			{
				return _nativeerror;
			}
		}

		public string Source
		{
			get
			{
				return _source;
			}
		}

		public string SQLState
		{
			get
			{
				return _state;
			}
		}

		#endregion // Properties
		
		#region methods
		
		public override string ToString () 
		{
			return Message;
		}	
			
		#endregion

	}

}
