//
// Mono.Data.Tds.Protocol.ITds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.Tds;
using System;
using System.Collections;

namespace Mono.Data.Tds.Protocol {
        public interface ITds 
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

		int RecordsAffected {
			get;
			set;
		}

		#endregion // Properties

		#region Methods

		void Cancel ();
		bool Connect (TdsConnectionParameters connectionParameters);
		void Disconnect ();

		void Execute (string commandText);
		void ExecProc (string procedure);

		void Execute (string commandText, TdsMetaParameterCollection parameters, int timeout, bool wantResults);
		void ExecPrepared (string statementId, TdsMetaParameterCollection parameters, int timeout, bool wantResults);
		void ExecProc (string procedure, TdsMetaParameterCollection parameters, int timeout, bool wantResults);

		bool NextResult ();
		bool NextRow ();

		string Prepare (string commandText, TdsMetaParameterCollection parameters);

		void SkipToEnd ();

		void Unprepare (string statementId);

		#endregion

		#region Events

		event TdsInternalErrorMessageEventHandler TdsErrorMessage;
		event TdsInternalInfoMessageEventHandler TdsInfoMessage;

		#endregion // Events
	}
}
