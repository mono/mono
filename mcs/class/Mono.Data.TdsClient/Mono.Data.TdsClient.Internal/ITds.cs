//
// Mono.Data.TdsClient.Internal.ITds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Data.Common;
using System.Collections;

namespace Mono.Data.TdsClient.Internal {
        internal interface ITds 
	{
		#region Properties

		string Database {
			get;
		}

		bool DoneProc {
			get;
		}

		bool IsConnected {
			get;
		}

		string ServerVersion {
			get;
		}

		TdsPacketColumnInfoResult Schema {
			get;
		}

		TdsPacketRowResult ColumnValues {
			get;
		}

		ArrayList OutputParameters {
			get;	
			set;
		}

		#endregion // Properties

		#region Methods

		void Cancel ();
		bool Connect (TdsConnectionParameters connectionParameters);
		void Disconnect ();

		int ExecuteNonQuery (string sql);
		int ExecuteNonQuery (string sql, int timeout);

		void ExecuteQuery (string sql);
		void ExecuteQuery (string sql, int timeout);

		bool NextResult ();
		bool NextRow ();
		void SkipToEnd ();

		#endregion

		#region Events

		event TdsInternalErrorMessageEventHandler TdsErrorMessage;
		event TdsInternalInfoMessageEventHandler TdsInfoMessage;

		#endregion // Events
	}
}
