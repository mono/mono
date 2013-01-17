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
using System.Runtime.Serialization;

namespace System.Data.OracleClient {
	[Serializable]
	public sealed class OracleException : System.Data.Common.DbException
	{
		int code;

		internal OracleException (int code, string message) : base (message)
		{
			this.code = code;
		}

		private OracleException (SerializationInfo si, StreamingContext sc) : base(si, sc)
		{
			code = si.GetInt32 ("code");
		}
		public int Code {
			get { return code; }
		}
		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			si.AddValue ("code", code, typeof(int));
			base.GetObjectData (si, context);
		}
	}
}
