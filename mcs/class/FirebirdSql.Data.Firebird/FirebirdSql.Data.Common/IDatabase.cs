/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

namespace FirebirdSql.Data.Common
{
	#region Delegates

	internal delegate void WarningMessageCallback(IscException warning);

	#endregion

	internal interface IDatabase : IDisposable
	{
		#region Callback

		WarningMessageCallback WarningMessage
		{
			get;
			set;
		}

		#endregion

		#region Properties

		int Handle
		{
			get;
		}

		int TransactionCount
		{
			get;
			set;
		}

		string ServerVersion
		{
			get;
		}

		Charset Charset
		{
			get;
			set;
		}

		short PacketSize
		{
			get;
			set;
		}

		short Dialect
		{
			get;
			set;
		}

		bool HasRemoteEventSupport
		{
			get;
		}

		#endregion

		#region Methods

		void Attach(DatabaseParameterBuffer dpb, string dataSource, int port, string database);
		void Detach();

		void CreateDatabase(DatabaseParameterBuffer dpb, string dataSource, int port, string database);
		void DropDatabase();

		ITransaction BeginTransaction(TransactionParameterBuffer tpb);

		StatementBase CreateStatement();
		StatementBase CreateStatement(ITransaction transaction);

		BlobParameterBuffer CreateBlobParameterBuffer();
		DatabaseParameterBuffer CreateDatabaseParameterBuffer();
		EventParameterBuffer CreateEventParameterBuffer();
		TransactionParameterBuffer CreateTransactionParameterBuffer();

		ArrayList GetDatabaseInfo(byte[] items);
		ArrayList GetDatabaseInfo(byte[] items, int bufferLength);

		void CloseEventManager();
		RemoteEvent CreateEvent();
		void QueueEvents(RemoteEvent events);
		void CancelEvents(RemoteEvent events);

		#endregion
	}
}
