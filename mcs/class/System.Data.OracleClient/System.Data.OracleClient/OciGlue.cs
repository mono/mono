// 
// ociglue.cs - provides glue between 
//              managed C#/.NET System.Data.OracleClient.dll and 
//              unmanaged native c library oci.dll
//              to be used in Mono System.Data.OracleClient as
//              the Oracle 8i data provider.
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.OCI
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
// 
// Author: 
//     Daniel Morgan <danmorg@sc.rr.com>
//         
// Copyright (C) Daniel Morgan, 2002
// 

using System;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.OCI {
	internal sealed class OciGlue {

		// TODO: need to clean up, dispose, close, etc...
		
		// connection parameters
		string database = "";
		string username = "";
		string password = "";

		private UInt32 ociGlueConnectionHandle = 0;

		// http://download-west.oracle.com/docs/cd/A87861_01/NT817EE/index.htm
		// from oracle/ora81/oci/include/oci.h

		// Currently, these are not being used, nor are the
		// OCI_* constants nor the DllImports for oci.dll
		IntPtr myenvhp = IntPtr.Zero; // OCIEnv - the environment handle
		IntPtr mysrvhp = IntPtr.Zero; // OCIServer - the server handle
		IntPtr myerrhp = IntPtr.Zero; // OCIError - the error handle
		IntPtr myusrhp = IntPtr.Zero; // OCISession- user session handle
		IntPtr mysvchp = IntPtr.Zero; // OCISvcCtx- the  service handle

		public const UInt32 OCI_DEFAULT = (UInt32) 0x00;

		public const UInt32 OCI_THREADED  = (UInt32) 0x01;
		public const UInt32 OCI_OBJECT    = (UInt32) 0x02;
		public const UInt32 OCI_EVENTS    = (UInt32) 0x04;
		public const UInt32 OCI_RESERVED1 = (UInt32) 0x08;
		public const UInt32 OCI_SHARED    = (UInt32) 0x10;
		public const UInt32 OCI_RESERVED2 = (UInt32) 0x20;
		
		public const UInt32 OCI_NO_UCB    = (UInt32) 0x40;
		public const UInt32  OCI_NO_MUTEX = (UInt32) 0x80;
		
		public const UInt32  OCI_SHARED_EXT = (UInt32) 0x100;
		public const UInt32  OCI_CACHE      = (UInt32) 0x200;
		public const UInt32  OCI_NO_CACHE   = (UInt32) 0x400;

		public const UInt32 OCI_HTYPE_FIRST         = 1;
		public const UInt32 OCI_HTYPE_ENV           = 1;
		public const UInt32 OCI_HTYPE_ERROR         = 2;
		public const UInt32 OCI_HTYPE_SVCCTX        = 3;
		public const UInt32 OCI_HTYPE_STMT          = 4;
		public const UInt32 OCI_HTYPE_BIND          = 5;
		public const UInt32 OCI_HTYPE_DEFINE        = 6;
		public const UInt32 OCI_HTYPE_DESCRIBE      = 7;
		public const UInt32 OCI_HTYPE_SERVER        = 8;
		public const UInt32 OCI_HTYPE_SESSION       = 9;
		public const UInt32 OCI_HTYPE_TRANS         =10;
		public const UInt32 OCI_HTYPE_COMPLEXOBJECT =11;
		public const UInt32 OCI_HTYPE_SECURITY      =12;
		public const UInt32 OCI_HTYPE_SUBSCRIPTION  =13;
		public const UInt32 OCI_HTYPE_DIRPATH_CTX   =14;
		public const UInt32 OCI_HTYPE_DIRPATH_COLUMN_ARRAY =15;
		public const UInt32 OCI_HTYPE_DIRPATH_STREAM       =16;
		public const UInt32 OCI_HTYPE_PROC          =17;
		public const UInt32 OCI_HTYPE_LAST          =17;

		public const UInt32 OCI_ATTR_FNCODE  =1;
		public const UInt32 OCI_ATTR_OBJECT   =2;
		public const UInt32 OCI_ATTR_NONBLOCKING_MODE  =3;
		public const UInt32 OCI_ATTR_SQLCODE  =4;
		public const UInt32 OCI_ATTR_ENV  =5;
		public const UInt32 OCI_ATTR_SERVER =6;
		public const UInt32 OCI_ATTR_SESSION =7;
		public const UInt32 OCI_ATTR_TRANS   =8;
		public const UInt32 OCI_ATTR_ROW_COUNT =  9;
		public const UInt32 OCI_ATTR_SQLFNCODE =10;
		public const UInt32 OCI_ATTR_PREFETCH_ROWS = 11;
		public const UInt32 OCI_ATTR_NESTED_PREFETCH_ROWS =12;
		public const UInt32 OCI_ATTR_PREFETCH_MEMORY =13;
		public const UInt32 OCI_ATTR_NESTED_PREFETCH_MEMORY =14;
		public const UInt32 OCI_ATTR_CHAR_COUNT  =15; 
		
		public const UInt32 OCI_ATTR_PDSCL   =16;
		public const UInt32 OCI_ATTR_FSPRECISION =OCI_ATTR_PDSCL;
		
		public const UInt32 OCI_ATTR_PDPRC   =17;
		public const UInt32 OCI_ATTR_LFPRECISION =OCI_ATTR_PDPRC; 
		
		public const UInt32 OCI_ATTR_PARAM_COUNT =18;
		public const UInt32 OCI_ATTR_ROWID   =19;
		public const UInt32 OCI_ATTR_CHARSET  =20;
		public const UInt32 OCI_ATTR_NCHAR   =21;
		public const UInt32 OCI_ATTR_USERNAME =22;
		public const UInt32 OCI_ATTR_PASSWORD =23;
		public const UInt32 OCI_ATTR_STMT_TYPE  = 24;
		public const UInt32 OCI_ATTR_INTERNAL_NAME =  25;
		public const UInt32 OCI_ATTR_EXTERNAL_NAME =  26;
		public const UInt32 OCI_ATTR_XID  =   27;
		public const UInt32 OCI_ATTR_TRANS_LOCK =28;
		public const UInt32 OCI_ATTR_TRANS_NAME= 29;
		public const UInt32 OCI_ATTR_HEAPALLOC =30;
		public const UInt32 OCI_ATTR_CHARSET_ID =31;
		public const UInt32 OCI_ATTR_CHARSET_FORM =32;
		public const UInt32 OCI_ATTR_MAXDATA_SIZE =33;
		public const UInt32 OCI_ATTR_CACHE_OPT_SIZE =34;
		public const UInt32 OCI_ATTR_CACHE_MAX_SIZE =35;
		public const UInt32 OCI_ATTR_PINOPTION =36;
		public const UInt32 OCI_ATTR_ALLOC_DURATION =37;
		
		public const UInt32 OCI_ATTR_PIN_DURATION =38;
		public const UInt32 OCI_ATTR_FDO          =39;
		public const UInt32 OCI_ATTR_POSTPROCESSING_CALLBACK =40;
		
		public const UInt32 OCI_ATTR_POSTPROCESSING_CONTEXT =41;
		
		public const UInt32 OCI_ATTR_ROWS_RETURNED =42;
		
		public const UInt32 OCI_ATTR_FOCBK        =43;
		public const UInt32 OCI_ATTR_IN_V8_MODE   =44;
		public const UInt32 OCI_ATTR_LOBEMPTY     =45;
		public const UInt32 OCI_ATTR_SESSLANG     =46;

		public const UInt32 OCI_ATTR_VISIBILITY	=	47;
		public const UInt32 OCI_ATTR_RELATIVE_MSGID	=	48;
		public const UInt32 OCI_ATTR_SEQUENCE_DEVIATION=	49;

		public const UInt32 OCI_ATTR_CONSUMER_NAME	=	50;
		public const UInt32 OCI_ATTR_DEQ_MODE		=51;
		public const UInt32 OCI_ATTR_NAVIGATION	=	52;
		public const UInt32 OCI_ATTR_WAIT		=	53;
		public const UInt32 OCI_ATTR_DEQ_MSGID		=54;

		public const UInt32 OCI_ATTR_PRIORITY		=55;
		public const UInt32 OCI_ATTR_DELAY		=	56;
		public const UInt32 OCI_ATTR_EXPIRATION	=	57;	
		public const UInt32 OCI_ATTR_CORRELATION	=	58;
		public const UInt32 OCI_ATTR_ATTEMPTS		=59;	
		public const UInt32 OCI_ATTR_RECIPIENT_LIST	=	60;
		public const UInt32 OCI_ATTR_EXCEPTION_QUEUE	=61;	   
		public const UInt32 OCI_ATTR_ENQ_TIME		=62; 
		public const UInt32 OCI_ATTR_MSG_STATE		=63;
		
		public const UInt32 OCI_ATTR_AGENT_NAME	=	64;
		public const UInt32 OCI_ATTR_AGENT_ADDRESS	=	65;
		public const UInt32 OCI_ATTR_AGENT_PROTOCOL	=	66;

		public const UInt32 OCI_ATTR_SENDER_ID		=68;
		public const UInt32 OCI_ATTR_ORIGINAL_MSGID	=     69;

		public const UInt32 OCI_ATTR_QUEUE_NAME	=     70;
		public const UInt32 OCI_ATTR_NFY_MSGID         =     71;
		public const UInt32 OCI_ATTR_MSG_PROP          =     72;

		public const UInt32 OCI_ATTR_NUM_DML_ERRORS    =     73;
		public const UInt32 OCI_ATTR_DML_ROW_OFFSET    =     74;

		public const UInt32 OCI_ATTR_DATEFORMAT        =     75;
		public const UInt32 OCI_ATTR_BUF_ADDR          =     76;
		public const UInt32 OCI_ATTR_BUF_SIZE          =     77;
		public const UInt32 OCI_ATTR_DIRPATH_MODE      =     78;
		public const UInt32 OCI_ATTR_DIRPATH_NOLOG     =     79;
		public const UInt32 OCI_ATTR_DIRPATH_PARALLEL  =     80;
		public const UInt32 OCI_ATTR_NUM_ROWS          =     81;

		public const UInt32 OCI_ATTR_COL_COUNT        =      82;
		
		public const UInt32 OCI_ATTR_STREAM_OFFSET    =      83;
		public const UInt32 OCI_ATTR_SHARED_HEAPALLOC  =     84;

		public const UInt32 OCI_ATTR_SERVER_GROUP      =     85;

		public const UInt32 OCI_ATTR_MIGSESSION       =      86;

		public const UInt32 OCI_ATTR_NOCACHE           =     87;

		public const UInt32 OCI_ATTR_MEMPOOL_SIZE       =    88;
		public const UInt32 OCI_ATTR_MEMPOOL_INSTNAME    =   89;
		public const UInt32 OCI_ATTR_MEMPOOL_APPNAME      =  90;
		public const UInt32 OCI_ATTR_MEMPOOL_HOMENAME   =    91;
		public const UInt32 OCI_ATTR_MEMPOOL_MODEL      =    92;
		public const UInt32 OCI_ATTR_MODES              =    93;

		public const UInt32 OCI_ATTR_SUBSCR_NAME        =    94;
		public const UInt32 OCI_ATTR_SUBSCR_CALLBACK    =    95;
		public const UInt32 OCI_ATTR_SUBSCR_CTX         =    96;
		public const UInt32 OCI_ATTR_SUBSCR_PAYLOAD     =    97;
		public const UInt32 OCI_ATTR_SUBSCR_NAMESPACE   =    98;

		public const UInt32 OCI_ATTR_PROXY_CREDENTIALS   =   99;
		public const UInt32 OCI_ATTR_INITIAL_CLIENT_ROLES = 100;

		public const UInt32 OCI_ATTR_UNK           =   101;
		public const UInt32 OCI_ATTR_NUM_COLS      =   102;
		public const UInt32 OCI_ATTR_LIST_COLUMNS  =   103;
		public const UInt32 OCI_ATTR_RDBA          =   104;
		public const UInt32 OCI_ATTR_CLUSTERED     =   105;
		public const UInt32  OCI_ATTR_PARTITIONED   =   106;
		public const UInt32 OCI_ATTR_INDEX_ONLY    =   107; 
		public const UInt32 OCI_ATTR_LIST_ARGUMENTS =  108; 
		public const UInt32 OCI_ATTR_LIST_SUBPROGRAMS =109; 
		public const UInt32 OCI_ATTR_REF_TDO        =  110; 
		public const UInt32 OCI_ATTR_LINK           =  111; 
		public const UInt32 OCI_ATTR_MIN            =  112; 
		public const UInt32 OCI_ATTR_MAX            =  113; 
		public const UInt32 OCI_ATTR_INCR           =  114; 
		public const UInt32 OCI_ATTR_CACHE          =  115; 
		public const UInt32 OCI_ATTR_ORDER          =  116; 
		public const UInt32 OCI_ATTR_HW_MARK        =  117; 
		public const UInt32 OCI_ATTR_TYPE_SCHEMA    =  118; 
		public const UInt32 OCI_ATTR_TIMESTAMP      =  119; 
		public const UInt32 OCI_ATTR_NUM_ATTRS      =  120; 
		public const UInt32 OCI_ATTR_NUM_PARAMS     =  121; 
		public const UInt32 OCI_ATTR_OBJID          =  122; 
		public const UInt32 OCI_ATTR_PTYPE          =  123; 
		public const UInt32 OCI_ATTR_PARAM          =  124; 
		public const UInt32 OCI_ATTR_OVERLOAD_ID    =  125; 
		public const UInt32 OCI_ATTR_TABLESPACE     =  126; 
		public const UInt32 OCI_ATTR_TDO            =  127; 
		public const UInt32 OCI_ATTR_LTYPE          =  128;   
		public const UInt32 OCI_ATTR_PARSE_ERROR_OFFSET =129; 
		public const UInt32 OCI_ATTR_IS_TEMPORARY   =  130;   
		public const UInt32 OCI_ATTR_IS_TYPED       =  131;   
		public const UInt32 OCI_ATTR_DURATION       =  132;   
		public const UInt32 OCI_ATTR_IS_INVOKER_RIGHTS= 133;  
		public const UInt32 OCI_ATTR_OBJ_NAME      =   134;
		public const UInt32 OCI_ATTR_OBJ_SCHEMA    =   135;
		public const UInt32 OCI_ATTR_OBJ_ID        =   136;

		public const UInt32 OCI_ATTR_DIRPATH_SORTED_INDEX =   137; 

		public const UInt32 OCI_ATTR_DIRPATH_INDEX_MAINT_METHOD= 138;

		public const UInt32 OCI_ATTR_DIRPATH_FILE            =139;
		public const UInt32 OCI_ATTR_DIRPATH_STORAGE_INITIAL =140;
		public const UInt32 OCI_ATTR_DIRPATH_STORAGE_NEXT    =141;

		public const UInt32 OCI_ATTR_TRANS_TIMEOUT     =       142; 
		public const UInt32 OCI_ATTR_SERVER_STATUS	=	143;
		public const UInt32 OCI_ATTR_STATEMENT         =       144; 
		
		public const UInt32 OCI_ATTR_NO_CACHE          =      145;
		public const UInt32 OCI_ATTR_RESERVED_1        =      146;
		public const UInt32 OCI_ATTR_SERVER_BUSY       =      147;
		
		public const UInt32 OCI_UCS2ID = 1000;

		public const UInt32 OCI_SERVER_NOT_CONNECTED = 0x0; 
		public const UInt32 OCI_SERVER_NORMAL        = 0x1; 

		public const UInt32 OCI_SUBSCR_NAMESPACE_ANONYMOUS =  0;
		public const UInt32 OCI_SUBSCR_NAMESPACE_AQ        =  1;
		public const UInt32 OCI_SUBSCR_NAMESPACE_MAX       =  2;

		public const UInt32 OCI_CRED_RDBMS  =  1;
		public const UInt32 OCI_CRED_EXT    =  2;
		public const UInt32 OCI_CRED_PROXY  =  3;

		public const Int32 OCI_SUCCESS = 0;
		public const Int32 OCI_SUCCESS_WITH_INFO = 1;
		public const Int32 OCI_RESERVED_FOR_INT_USE = 200;
		public const Int32 OCI_NO_DATA = 100;
		public const Int32 OCI_ERROR = -1;
		public const Int32 OCI_INVALID_HANDLE = -2;
		public const Int32 OCI_NEED_DATA = 99;
		public const Int32 OCI_STILL_EXECUTING = -3123;
		public const Int32 OCI_CONTINUE = -24200;
		
		// from oci/include/ociap.h

		//------ Platform Invoke to native c library oci.dll ----------
/*
		[DllImport("oci.dll")]
		public static extern Int32 OCIEnvCreate (
			out IntPtr envhpp, UInt32 mode, IntPtr ctxp, 
			ref IntPtr malocfp, ref IntPtr ralocfp, ref IntPtr mfreefp,
			uint xtramemsz, out IntPtr usrmempp);
		//sword OCIEnvCreate   ( 
		//	OCIEnv **envhpp, ub4 mode, CONST dvoid *ctxp,
		//	CONST dvoid *(*malocfp) (dvoid *ctxp, size_t size),
		//	CONST dvoid *(*ralocfp) (dvoid *ctxp, dvoid *memptr, size_t newsize),
		//      CONST void (*mfreefp) (dvoid *ctxp, dvoid *memptr)),
		//      size_t    xtramemsz, dvoid **usrmempp);
		// [OUT],[IN],[IN],[IN],[IN],[IN],[IN],[OUT]

		[DllImport("oci.dll")]
		public static extern Int32 OCIHandleAlloc ( 
			IntPtr parenth, ref IntPtr hndlpp, UInt32 type, 
			int xtramem_sz, ref IntPtr usrmempp);
		//sword OCIHandleAlloc(
		//	CONST dvoid *parenth, dvoid **hndlpp, ub4 type, 
		//	size_t xtramem_sz, dvoid **usrmempp );
		// [IN],OUT],[OUT],[IN],[OUT]

		[DllImport("oci.dll")]
		public static extern Int32 OCIServerAttach ( 
			ref IntPtr srvhp, ref IntPtr errhp, string dblink,
			Int32 dblink_len, UInt32 mode);
		//sword OCIServerAttach (
		//	OCIServer *srvhp, OCIError *errhp, CONST text *dblink,
		//      sb4 dblink_len, ub4 mode);
		// [IN/OUT],[IN/OUT],[IN],[IN],[IN]

		[DllImport("oci.dll")]
		public static extern Int32 OCIAttrSet ( 
			ref IntPtr trgthndlp, UInt32 trghndltyp, IntPtr attributep,
			UInt32 size, UInt32 attrtype,
			ref IntPtr errhp);
		//sword OCIAttrSet ( 
		//	dvoid *trgthndlp, ub4 trghndltyp, dvoid *attributep,
		//	ub4 size, ub4 attrtype, OCIError *errhp );
		// [IN/OUT], [IN/OUT], [IN], [IN], [IN], [IN/OUT]

		[DllImport("oci.dll")]
		public static extern Int32 OCISessionBegin ( 
			IntPtr svchp, ref IntPtr errhp, ref IntPtr usrhp,
			UInt32 credt, UInt32 mode);
		//sword OCISessionBegin (
		//	OCISvcCtx *svchp, OCIError *errhp,
		//	OCISession *usrhp, ub4 credt, ub4 mode );
		// [IN],[IN],[IN/OUT],[IN],[IN]
*/
		[DllImport("oci.dll")]
		public static extern UInt32 OCIErrorGet ( 
			ref IntPtr hndlp, UInt32 recordno, ref IntPtr sqlstate,
			out Int32 errcodep, out string bufp, UInt32 bufsiz,
			UInt32 type);
		//sword OCIErrorGet(
		//	dvoid *hndlp, ub4 recordno, text *sqlstate,
		//	sb4 *errcodep, text *bufp, ub4 bufsiz,
		//	ub4 type);
		// [IN],[IN],[OUT],[OUT],[OUT],[IN],[IN]

		//------ Platform Invoke to native c library System.Data.OracleClient.ociglue.dll ----------
		// System.Data.OracleClient.ociglue.dll is the glue between C# and oci.dll

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern IntPtr OciGlue_OCIEnvCreate(ref Int32 status);
		// void *OciGlue_OCIEnvCreate(sword *status);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern IntPtr OciGlue_OCIHandleAlloc(ref Int32 status,
						IntPtr parenth, UInt32 type);
		// dvoid *OciGlue_OCIHandleAlloc(sword *status, 
		//		CONST dvoid *parenth, ub4 type);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern void OciGlue_OCIServerAttach(ref Int32 status,
				IntPtr srvhp,
				IntPtr errhp, string dblink, UInt32 mode);
		// void OciGlue_OCIServerAttach(sword *status, dvoid *srvhp, 
		//		dvoid *errhp, char *dblink, ub4 mode);
		
		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern void OciGlue_OCIAttrSet(ref Int32 status, 
			IntPtr trgthndlp, UInt32 trghndltyp, 
			IntPtr attributep, UInt32 size, UInt32 attrtype, 
			IntPtr errhp);
		//void OciGlue_OCIAttrSet(sword *status, 
		//	dvoid *trgthndlp, ub4 trghndltyp, 
		//	dvoid *attributep, ub4 size, ub4 attrtype, 
		//	dvoid *errhp)

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern IntPtr OciGlue_OCISessionBegin(ref Int32 status,
			IntPtr svchp, IntPtr errhp, IntPtr usrhp,
			UInt32 credt, UInt32 mode);
		// void OciGlue_OCISessionBegin(sword *status, 
		//	dvoid *svchp, dvoid *errhp, dvoid *usrhp,
		//	ub4 credt, ub4 mode )	

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern Int32 OciGlue_Connect (
			out UInt32 ociGlueConnectionHandle, 
			string database, 
			string username, string password);
		//sword OciGlue_Connect (
		//	uint4 oci_glue_handle, 
		//	char *database, 
		//	char *username, char *password);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern Int32 OciGlue_Disconnect (UInt32 connection_handle);
		// sword OciGlue_Disconnect (ub4 connection_handle);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern Int32 OciGlue_PrepareAndExecuteNonQuerySimple (
			UInt32 ociGlueConnectionHandle,
			string sqlstmt, out int found);
		//sword OciGlue_PrepareAndExecuteNonQuerySimple (
		//	guint4 oci_glue_handle, 
		//	char *sqlstmt, int *found);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern UInt32 OciGlue_ConnectionCount();
		//guint OciGlue_ConnectionCount();

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern IntPtr OciGlue_CheckError (Int32 status, UInt32 connection_handle);
		// CONST text *OciGlue_CheckError (sword status, ub4 connection_handle);

		[DllImport("System.Data.OracleClient.ociglue.dll")]
		public static extern void Free (IntPtr obj);
		// void Free (void *obj);

		// --------- Methods --------------------------------

		public string OciGlueCheckError(Int32 status) {
			IntPtr intptrMsg = IntPtr.Zero;
			string strMsg = "";
			string msg = "";
			
			intptrMsg = OciGlue_CheckError(status, ociGlueConnectionHandle);
			strMsg = Marshal.PtrToStringAnsi(intptrMsg);
			if(strMsg != null) {
				msg = String.Copy(strMsg);
				Free(intptrMsg);
			}

			return msg;
		}

		// for debug
		public static void IsObjectNull(string usedfor, string name, object obj) {
			string exists = "";
			if(obj == null) {
				exists = "is null";
			}
			else {
				exists = "is not null";
				if(obj is IntPtr) {
					IntPtr intptr = (IntPtr) obj;
					if(intptr == IntPtr.Zero)
						exists = exists + " - IntPtr is IntPtr.Zero";
					else
						exists = exists + " - IntPtr is not IntPtr.Zero";				     
				}
			}
			string outstring = 
				"UsedFor: " + usedfor + 
				" Object name: " + name + 
				" exists: " + exists; 
		}

		public Int32 OciGlueConnect(OracleConnectionInfo conInfo) {

			Int32 status = 0;
			
			database = conInfo.Database;
			username = conInfo.Username;
			password = conInfo.Password;

			Console.WriteLine("OciGlue_Connect");
			status = OciGlue.OciGlue_Connect (out ociGlueConnectionHandle,
				database, username, password);
			Console.WriteLine("  Handle: " + ociGlueConnectionHandle);

			if(status != 0) {
				CheckStatus(status);
			}		
						
			return status;
		}

		public Int32 OciGlueDisconnect() {

			Int32 status = 0;
			string msg = "";
			
			Console.WriteLine("OciGlue_Disconnect");
			Console.WriteLine("  Handle: " + ociGlueConnectionHandle);
			status = OciGlue.OciGlue_Disconnect (ociGlueConnectionHandle);
			ociGlueConnectionHandle = 0;
			
			if(status != 0) {
				msg = CheckStatus(status);
				throw new Exception(msg);
			}
						
			return status;
		}


		// Helper methods
		public Int32 PrepareAndExecuteNonQuerySimple(string sql) 
		{
			Int32 status = 0;
			int found = 0;

			Console.WriteLine("PrepareAndExecuteNonQuerySimple");
			status = OciGlue_PrepareAndExecuteNonQuerySimple (
				ociGlueConnectionHandle, sql, out found);

			Console.WriteLine("  Handle: " + ociGlueConnectionHandle +
				" Found: " + found.ToString());

			CheckStatus(status);
			return status;
		}

		// currently not used
		public void Connect() 
		{	
			Int32 status = 0;
			IntPtr nothing = IntPtr.Zero;
			// initialize the mode to be the threaded and object environment
			Console.WriteLine();
			// OCI_THREADED|OCI_OBJECT
			// OCI_DEFAULT
			// OCI_THREADED|OCI_OBJECT|OCI_NO_UCB
			//status = OCIEnvCreate(out myenvhp, OCI_DEFAULT|OCI_NO_UCB, 
			//	IntPtr.Zero, 
			//	ref nothing, ref nothing, ref nothing, 0, 
			//	out nothing);
			myenvhp = OciGlue_OCIEnvCreate(ref status);
			IsObjectNull("Result from OCIEnvCreate for initialize the mode for environment","myenvhp",myenvhp);
			CheckStatus(status);

			// allocate a server handle
			Console.WriteLine();
			//status = OCIHandleAlloc (myenvhp, ref mysrvhp,
			//	OCI_HTYPE_SERVER, 0, ref nothing);
			mysrvhp = OciGlue_OCIHandleAlloc(ref status,
						myenvhp, OCI_HTYPE_SERVER);
			IsObjectNull("Result from OCIHandleAlloc for server handle","mysrvhp",mysrvhp);
			CheckStatus(status);

			// allocate an error handle
			Console.WriteLine();
			//status = OCIHandleAlloc (myenvhp, ref myerrhp,
			//	OCI_HTYPE_ERROR, 0, ref nothing);
			myerrhp = OciGlue_OCIHandleAlloc(ref status,
						myenvhp, OCI_HTYPE_ERROR);
			IsObjectNull("Result from OCIHandleAlloc for error handle","myerrhp",myerrhp);
			CheckError(status);

			// create a server context
			Console.WriteLine();
			string dblink = "DANSDB";
			//mysrvhp = OciGlueMallocType (OciGlueType_OCIServer);
			//status = OCIServerAttach (ref mysrvhp, ref myerrhp, dblink, 
			//	dblink.Length, OCI_DEFAULT);
			OciGlue_OCIServerAttach(ref status,
				mysrvhp, myerrhp, dblink, OCI_DEFAULT);
			IsObjectNull("Result from OCIServerAttach for server context","mysrvhp",mysrvhp);
			CheckError(status);

			// allocate a service handle
			Console.WriteLine();
			//status = OCIHandleAlloc (myenvhp, ref mysvchp,
			//	OCI_HTYPE_SVCCTX, 0, ref nothing);
			mysvchp = OciGlue_OCIHandleAlloc(ref status,
				myenvhp, OCI_HTYPE_SVCCTX);
			IsObjectNull("Result from OCIHandleAlloc for service handle","myenvhp",mysvchp);
			CheckError(status);
			Console.Out.Flush();

			// set the server attribute in the service context handle
			Console.WriteLine();
			Console.Out.Flush();
			//status = OCIAttrSet (
			//	ref mysvchp, OCI_HTYPE_SVCCTX, 
			//	mysrvhp, 0, OCI_ATTR_SERVER, ref myerrhp);
			OciGlue_OCIAttrSet(ref status,
				mysvchp, OCI_HTYPE_SVCCTX, 
				mysrvhp, (UInt32) 0, OCI_ATTR_SERVER, myerrhp);

			IsObjectNull("Result from OCIAttrSet for server attribute","mysvchp",mysvchp);
			Console.WriteLine("status is: " + status.ToString());
			CheckError(status);

			// allocate a user session handle
			Console.WriteLine(" blah ");
			Console.Out.Flush();
			//status = OCIHandleAlloc (myenvhp, ref myusrhp,
			//	OCI_HTYPE_SESSION, 0, ref nothing);
			myusrhp = OciGlue_OCIHandleAlloc(ref status,
				myenvhp, OCI_HTYPE_SESSION);
			IsObjectNull("Result from OCIHandleAlloc for user session","myusrhp",myusrhp);
			CheckError(status);

			// set username attribute in user session handle
			Console.WriteLine();
			string username = "scott";
			IntPtr intptrUsername = Marshal.StringToHGlobalAnsi( username );
			//status = OCIAttrSet (
			//	ref myusrhp, OCI_HTYPE_SESSION,
			//	intptrUsername, (UInt32) username.Length,
			//	OCI_ATTR_USERNAME, ref myerrhp);
			OciGlue_OCIAttrSet(ref status,
				myusrhp, OCI_HTYPE_SESSION,
				intptrUsername, (UInt32) username.Length,
				OCI_ATTR_USERNAME, myerrhp);
			IsObjectNull("Result from OCIAttrSet for username","myusrhp",myusrhp);
			CheckError(status);

			// set password attribute in user session handle
			Console.WriteLine();
			string password = "tiger";
			IntPtr intptrPassword = Marshal.StringToHGlobalAnsi( password );
			//status = OCIAttrSet (ref myusrhp, 
			//	OCI_HTYPE_SESSION,
			//	intptrPassword, (UInt32) password.Length,
			//	OCI_ATTR_PASSWORD, ref myerrhp);
			OciGlue_OCIAttrSet(ref status,
				myusrhp,
				OCI_HTYPE_SESSION,
				intptrPassword, (UInt32) password.Length,
				OCI_ATTR_PASSWORD, myerrhp);
			IsObjectNull("Result from OCIAttrSet for password","myusrhp",myusrhp);
			CheckError(status);
			
			// begin session
			Console.WriteLine();
			//status = OCISessionBegin (mysvchp, ref myerrhp, ref myusrhp,
			//	OCI_CRED_RDBMS, OCI_DEFAULT);
			OciGlue_OCISessionBegin(ref status,
				mysvchp, myerrhp, myusrhp,
				OCI_CRED_RDBMS, OCI_DEFAULT);
			IsObjectNull("Result from OCISessionBegin","myusrhp",myusrhp);
			CheckError(status);

			// FIXME: check for errors to make sure
			//        authentication was successful

			// set the user session attribute in the service context handle
			Console.WriteLine();
			//status = OCIAttrSet (ref mysvchp, 
			//	OCI_HTYPE_SVCCTX, 
			//	myusrhp, 0, OCI_ATTR_SESSION, ref myerrhp);
			OciGlue_OCIAttrSet(ref status,
				mysvchp, 
				OCI_HTYPE_SVCCTX, 
				myusrhp, 0, OCI_ATTR_SESSION, myerrhp);
			IsObjectNull("Result from OCIAttrSet for user session attribute in service context handle","myusrhp",myusrhp);
			CheckError(status);

			Console.WriteLine();
			// FIXME: check for errors after each call

			// FIXME: need to properly free resources, such as,
			//        intptrUsername, intptrPassword, etc...
			
			// Wait
			Console.WriteLine("Waiting... Press Enter to continue.");
			string s = Console.ReadLine();
			Console.WriteLine();
			Console.WriteLine("Exiting...");
		}

		public string CheckStatus(Int32 status) {
			return CheckError(status);
		}

		// OCIError errhp, Int32 status
		public string CheckError(Int32 status) {		
			string msg = "";
			Int32 errcode = 0;

			switch (status) {
			case OCI_SUCCESS:
				msg = "Succsss";
				break;
			case OCI_SUCCESS_WITH_INFO:
				msg = "Error - OCI_SUCCESS_WITH_INFO";
				break;
			case OCI_NEED_DATA:
				msg = "Error - OCI_NEED_DATA";
				break;
			case OCI_NO_DATA:
				msg = "Error - OCI_NODATA";
				break;
			case OCI_ERROR:
				if(ociGlueConnectionHandle != 0) {
					string errmsg = OciGlueCheckError(status);
					//OCIErrorGet(ref errhp, 1, ref nullString, out errcode,
					//	out errbuf, (UInt32) errbuf.Length, OCI_HTYPE_ERROR);
					msg = "OCI Error - errcode: " + errcode.ToString() + " errbuf: " + errmsg;
				}
				else
					msg = "OCI Error!";
				break;
			case OCI_INVALID_HANDLE:
				msg = "Error - OCI_INVALID_HANDLE";
				break;
			case OCI_STILL_EXECUTING:
				msg = "Error - OCI_STILL_EXECUTE";
				break;
			case OCI_CONTINUE:
				msg = "Error - OCI_CONTINUE";
				break;
			default:
				msg = "Default";
				break;
			}
			return msg;
		}
	}
}
