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

		bool InUse {
			get;
			set;
		}

		bool IsConnected {
			get;
		}

		string ServerVersion {
			get;
		}

		#endregion // Properties

		#region Methods

		bool Connect (TdsConnectionParameters connectionParameters);

		void Disconnect ();

		int ExecuteNonQuery (string sql);

		void ExecuteQuery (string sql);

		#endregion
	}
}
