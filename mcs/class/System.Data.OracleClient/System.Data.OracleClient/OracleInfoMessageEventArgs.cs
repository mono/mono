//
// OracleInfoMessageEventArgs.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2003
//
// Licensed under the MIT/X11 License.
//

using System;
using System.IO;
using System.Data.SqlTypes;

namespace System.Data.OracleClient {
	public sealed class OracleInfoMessageEventArgs : EventArgs
	{
		#region Fields

		int code;
		string message;

		#endregion // Fields

		#region Constructors

		internal OracleInfoMessageEventArgs (OracleException exception)
		{
			code = exception.Code;
			message = exception.Message;
		}

		#endregion // Constructors

		#region Properties

		public int Code {
			get { return code; }
		}

		public string Message {
			get { return message; }
		}

		public string Source {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
