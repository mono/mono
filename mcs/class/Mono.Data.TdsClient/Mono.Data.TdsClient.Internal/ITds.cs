//
// Mono.Data.TdsClient.Internal.ITds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;

namespace Mono.Data.TdsClient.Internal {
        internal interface ITds 
	{
		#region Properties

		string Database {
			get;
		}

		bool IsConnected {
			get;
		}

		string ServerVersion {
			get;
		}

		TdsPacketColumnInfoResult ColumnInfo {
			get;
		}

		TdsPacketRowResult ColumnValues {
			get;
		}

		TdsPacketErrorResultCollection Errors {
			get;
		}

		#endregion // Properties

		#region Methods

		void Cancel ();
		bool Connect (TdsConnectionParameters connectionParameters);
		void Disconnect ();
		int ExecuteNonQuery (string sql);
		void ExecuteQuery (string sql);
		bool NextResult ();
		bool NextRow ();

		#endregion
	}
}
