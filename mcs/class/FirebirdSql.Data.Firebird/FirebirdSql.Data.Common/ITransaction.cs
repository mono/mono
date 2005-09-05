/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;

namespace FirebirdSql.Data.Common
{
	#region Enumerations

	internal enum TransactionState
	{
		NoTransaction,
		TrasactionStarting,
		TransactionStarted,
		TransactionPreparing,
		TransactionPrepared,
		TransactionCommiting,
		TransactionRollbacking
	}

	#endregion

	#region Delegates

	internal delegate void TransactionUpdateEventHandler(object sender, EventArgs e);

	#endregion

	internal interface ITransaction : IDisposable
	{
		#region Events

		event TransactionUpdateEventHandler Update;

		#endregion

		#region Properties

		int Handle
		{
			get;
		}

		TransactionState State
		{
			get;
		}

		#endregion

		#region Methods

		void BeginTransaction(TransactionParameterBuffer tpb);
		void Commit();
		void CommitRetaining();
		void Rollback();
		void RollbackRetaining();
		// void Prepare();
		// void Prepare(byte[] buffer);

		#endregion
	}
}
