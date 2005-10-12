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
using System.Runtime.InteropServices;
using System.Security;

namespace FirebirdSql.Data.Embedded
{
	[SuppressUnmanagedCodeSecurity]
	internal sealed class FbClient
	{
		#region Conditional	directives

#if	(LINUX)
	#if	(FBCLIENT)
			public const string	DllPath = "libfbclient";
	#elif (GDS32)
			public const string	DllPath = "libgds";
	#elif (VULCAN)
			public const string	DllPath = "libfirebird32";
	#elif (FYRACLE)
			public const string	DllPath = "fyracle";
	#else
			public const string	DllPath = "libfbembed";
	#endif
#else
	#if	(FBCLIENT)
			public const string	DllPath = "fbclient";
	#elif (GDS32)
			public const string	DllPath = "gds32";
	#elif (VULCAN)
			public const string	DllPath = "firebird32";
	#elif (FYRACLE)
			public const string	DllPath = "fyracle";
	#else
			public const string DllPath = "fbembed";
	#endif
#endif

		#endregion

		#region Constructors

		private FbClient()
		{
		}

		#endregion

		#region Array Functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_array_get_slice(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] destArray,
			ref	int sliceLength);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_array_put_slice(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	long arrayId,
			IntPtr desc,
			byte[] sourceArray,
			ref	int sliceLength);

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_array_get_slice(
			[In, Out] int[] statusVector,
			string tableName,
			string columnName,
			ref	short sqlDtype,
			ref	short sqlLength,
			ref	short dimensions,
			IntPtr desc);
		  
		[DllImport(FbClient.DllPath)]
		public static extern int isc_array_lookup_bounds(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			string tableName,
			string columnName,
			IntPtr desc);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_array_lookup_desc(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			string tableName,
			string columnName,
			IntPtr desc);
		*/
		#endregion

		#region Blob functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_create_blob2(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_open_blob2(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			ref	int blobHandle,
			ref	long blobId,
			short bpbLength,
			byte[] bpbAddress);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_get_segment(
			[In, Out] int[] statusVector,
			ref	int blobHandle,
			ref	short actualSegLength,
			short segBufferLength,
			byte[] segBuffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_put_segment(
			[In, Out] int[] statusVector,
			ref	int blobHandle,
			short segBufferLength,
			byte[] segBuffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_cancel_blob(
			[In, Out] int[] statusVector,
			ref	int blobHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_close_blob(
			[In, Out] int[] statusVector,
			ref	int blobHandle);

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_blob_info(
			[In, Out] int[] statusVector,
			ref	int blobHandle,
			short itemListBufferLength,
			byte[] itemListBuffer,
			short resultBufferLength,
			byte[] resultBuffer);
		*/

		#endregion

		#region Database functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_attach_database(
			[In, Out] int[] statusVector,
			short dbNameLength,
			string dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_detach_database(
			[In, Out] int[] statusVector,
			ref	int dbHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_database_info(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			short itemListBufferLength,
			byte[] itemListBuffer,
			short resultBufferLength,
			byte[] resultBuffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_create_database(
			[In, Out] int[] statusVector,
			short dbNameLength,
			string dbName,
			ref	int dbHandle,
			short parmBufferLength,
			byte[] parmBuffer,
			short dbType);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_drop_database(
			[In, Out] int[] statusVector,
			ref	int dbHandle);

		#endregion

		#region Events functions

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_event_block(
				ref	byte[] event_buffer,
				ref	byte[] result_buffer,
				short id_count,
				string eventName);

		[DllImport(FbClient.DllPath)]
		public static extern void isc_event_counts(
			[In, Out] int[] statusVector,
			short bufferLength,
			byte[] eventBuffer,
			byte[] resultBuffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_que_events(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int eventId,
			short length,
			byte[] eventBuffer,
			IntPtr eventFunction,
			IntPtr eventFunctionArg);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_cancel_events(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int eventId);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_wait_for_event(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			short length,
			byte[] eventBuffer,
			byte[] resultBuffer);
		*/

		#endregion

		#region Transaction	functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_start_multiple(
			[In, Out]	int[] statusVector,
			ref	int trHandle,
			short dbHandleCount,
			IntPtr tebVectorAddress);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_commit_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_commit_retaining(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_rollback_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_rollback_retaining(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_start_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			short dbHandleCount,
			ref	int dbHandle,
			short tpbLength,
			byte[] tpbAddress);
		  
		[DllImport(FbClient.DllPath)]
		public static extern int isc_prepare_transaction(
			[In, Out] int[] statusVector,
			ref	int trHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_prepare_transaction2(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			short msgLength,
			byte[] message);
		*/

		#endregion

		#region DSQL functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_allocate_statement(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int stmtHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_describe(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_describe_bind(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_prepare(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short length,
			byte[] statement,
			short dialect,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_execute(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_execute2(
			[In, Out] int[] statusVector,
			ref	int trHandle,
			ref	int stmtHandle,
			short da_version,
			IntPtr inXsqlda,
			IntPtr outXsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_fetch(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short daVersion,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_free_statement(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short option);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_sql_info(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			short itemsLength,
			byte[] items,
			short bufferLength,
			byte[] buffer);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_vax_integer(
			byte[] buffer,
			short length);

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_execute_immediate(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			short length,
			string statement,
			short dialect,
			IntPtr xsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_exec_immed2(
			[In, Out] int[] statusVector,
			ref	int dbHandle,
			ref	int trHandle,
			short length,
			string statement,
			short dialect,
			IntPtr inXsqlda,
			IntPtr outXsqlda);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_dsql_set_cursor_name(
			[In, Out] int[] statusVector,
			ref	int stmtHandle,
			string cursorName,
			short type);
		*/

		#endregion

		#region Services functions

		[DllImport(FbClient.DllPath)]
		public static extern int isc_service_attach(
			[In, Out] int[] statusVector,
			short serviceLength,
			string service,
			ref	int svcHandle,
			short spbLength,
			byte[] spb);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_service_start(
			[In, Out] int[] statusVector,
			ref	int svcHandle,
			ref	int reserved,
			short spbLength,
			byte[] spb);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_service_detach(
			[In, Out] int[] statusVector,
			ref	int svcHandle);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_service_query(
			[In, Out] int[] statusVector,
			ref	int svcHandle,
			ref	int reserved,
			short sendSpbLength,
			byte[] sendSpb,
			short requestSpbLength,
			byte[] requestSpb,
			short bufferLength,
			byte[] buffer);

		#endregion

		#region Error handling

		/*
		[DllImport(FbClient.DllPath)]
		public static extern int isc_interprete(
			byte[] buffer,
			ref	int[] statusVector);

		[DllImport(FbClient.DllPath)]
		public static extern int isc_sqlcode(ref int[] statusVector);
		*/

		#endregion
	}
}