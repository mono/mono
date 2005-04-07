//
// Mono.Data.Tds.Protocol.ITds.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

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

		TdsDataColumnCollection Columns {
			get;
		}

		TdsDataRow ColumnValues {
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
		bool Reset ();

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

#if NET_2_0
                #region Asynchronous Methods
                IAsyncResult BeginExecuteNonQuery (string sql,
                                                   TdsMetaParameterCollection parameters,
                                                   AsyncCallback callback,
                                                   object state);
                void EndExecuteNonQuery (IAsyncResult ar);
                IAsyncResult BeginExecuteQuery (string sql,
                                                   TdsMetaParameterCollection parameters,
                                                   AsyncCallback callback,
                                                   object state);
                void EndExecuteQuery (IAsyncResult ar);

                IAsyncResult BeginExecuteProcedure (string prolog,
                                                                    string epilog,
                                                                    string cmdText,
                                                                    bool IsNonQuery,
                                                                    TdsMetaParameterCollection parameters,
                                                                    AsyncCallback callback,
                                                                    object state);
                void EndExecuteProcedure (IAsyncResult ar);

                void WaitFor (IAsyncResult ar);
                void CheckAndThrowException (IAsyncResult ar);

                #endregion //Asynchronous Methods
#endif // NET_2_0

	}
}
