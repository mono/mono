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

namespace FirebirdSql.Data.Common
{
	internal enum DbStatementType : int
	{
		None		= 0,
		Select		= IscCodes.isc_info_sql_stmt_select,
		Insert		= IscCodes.isc_info_sql_stmt_insert,
		Update		= IscCodes.isc_info_sql_stmt_update,
		Delete		= IscCodes.isc_info_sql_stmt_delete,
		DDL			= IscCodes.isc_info_sql_stmt_ddl,
		GetSegment	= IscCodes.isc_info_sql_stmt_get_segment,
		PutSegment	= IscCodes.isc_info_sql_stmt_put_segment,
		StoredProcedure = IscCodes.isc_info_sql_stmt_exec_procedure,
		StartTrans	= IscCodes.isc_info_sql_stmt_start_trans,
		Commit		= IscCodes.isc_info_sql_stmt_commit,
		Rollback	= IscCodes.isc_info_sql_stmt_rollback,
		SelectForUpdate = IscCodes.isc_info_sql_stmt_select_for_upd,
		SetGenerator = IscCodes.isc_info_sql_stmt_set_generator,
		SavePoint	= IscCodes.isc_info_sql_stmt_savepoint
	}
}
