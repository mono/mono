//
// System.Data.OleDb.OleDbError
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbError
	{
		private string errorMessage;
		private int nativeError;
		private string errorSource;
		private string sqlState;

		#region Constructors

		internal OleDbError (string msg, int code, string source, string sql)
		{
			errorMessage = msg;
			nativeError = code;
			errorSource = source;
			sqlState = sql;
		}
		
		#endregion // Constructors
		
		#region Properties

		public string Message {
			get {
				return errorMessage;
			}
		}

		public int NativeError {
			get {
				return nativeError;
			}
		}

		public string Source {
			get {
				return errorSource;
			}
		}

		public string SQLState {
			get {
				return sqlState;
			}
		}

		#endregion

		#region Methods
		
		[MonoTODO]
		public override string ToString ()
		{
			string toStr;
                        String stackTrace;
                        stackTrace = " <Stack Trace>";
                        // FIXME: generate the correct SQL error string
                        toStr = "OleDbError:" + errorMessage + stackTrace;
                        return toStr;

		}

		#endregion
	}
}
