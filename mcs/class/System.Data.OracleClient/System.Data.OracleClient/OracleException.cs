// 
// OracleException.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Daniel Morgan, 2002
// Copyright (C) Tim Coleman , 2003
//
// Licensed under the MIT/X11 License.
//

using System;

namespace System.Data.OracleClient {
	public sealed class OracleException : SystemException
	{
		#region Fields

		int code;
		string message;

		#endregion // Fields

		#region Constructors

		internal OracleException (int code, string message)
		{
			this.code = code;
			this.message = message;
		}

		#endregion // Constructors

		#region Properties

		public int Code {
			get { return code; }
		}

		public override string Message {
			get { return message; }
		}

		#endregion // Properties
	}
}
