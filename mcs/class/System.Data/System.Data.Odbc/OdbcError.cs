//
// System.Data.Odbc.OdbcError
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public class OdbcError
	{
		string message,source,sqlstate;
		int nativeerror;

		#region Constructors

		internal OdbcError(string Source)
		{
			nativeerror=1;
			source=Source;
			message="Error in "+source;
			sqlstate="";
		}

		internal OdbcError(string Source, OdbcHandleType HandleType, IntPtr Handle)
		{
			short buflen=256,txtlen=0;
			OdbcReturn ret=OdbcReturn.Success;
			byte[] buf_MsgText=new byte[buflen];
			byte[] buf_SqlState=new byte[buflen];
			bool NeedsDecode=true;
			this.source=Source;
			switch (HandleType)
			{
				case OdbcHandleType.Dbc:
					ret=libodbc.SQLError(IntPtr.Zero,Handle,IntPtr.Zero, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Stmt:
					ret=libodbc.SQLError(IntPtr.Zero,IntPtr.Zero,Handle, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				case OdbcHandleType.Env:
					ret=libodbc.SQLError(Handle,IntPtr.Zero,IntPtr.Zero, buf_SqlState,
						ref nativeerror, buf_MsgText, buflen, ref txtlen);
					break;
				default:
					nativeerror=1;
					source=Source;
					message="Error in "+source;
					sqlstate="";
					NeedsDecode=false;
					break;
			}
			if (NeedsDecode)
			{
				if (ret!=OdbcReturn.Success)
				{
					nativeerror=1;
					source=Source;
					message="Unable to retreive error information from ODBC driver manager";
					sqlstate="";
				}
				else
				{
					sqlstate=System.Text.Encoding.Default.GetString(buf_SqlState).Replace((char) 0,' ').Trim();;
					message=System.Text.Encoding.Default.GetString(buf_MsgText).Replace((char) 0,' ').Trim();;
				}
			}
		}

		#endregion // Constructors
		
		#region Properties

		public string Message
		{
			get
			{
				return message;
			}
		}

		public int NativeError
		{
			get
			{
				return nativeerror;
			}
		}

		public string Source
		{
			get
			{
				return source;
			}
		}

		public string SQLState
		{
			get
			{
				return sqlstate;
			}
		}

		#endregion // Properties

	}
}