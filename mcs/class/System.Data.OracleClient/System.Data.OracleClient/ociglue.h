// 
// ociglue.c -  provides glue between 
//              managed C#/.NET System.Data.OracleClient.dll and 
//              unmanaged native c library oci.dll
//              to be used in Mono System.Data.OracleClient as
//              the Oracle 8i data provider.
//  
// Part of unmanaged C library System.Data.OracleClient.ociglue.dll
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
// Licensed under the MIT/X11 License.
//

#ifndef __MONO_OCIGLUE_LIB_H__
#define __MONO_OCIGLUE_LIB_H__

#include <glib.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <oci.h>

struct _oci_glue_connection_t
{
	ub4 connection_handle;

	OCIEnv *envhp;
	OCIError *errhp;
	OCISession *authp;
	OCIServer *srvhp;
	OCISvcCtx *svchp;
	OCIStmt *stmthp;
};

typedef struct _oci_glue_connection_t oci_glue_connection_t;

/*
dvoid *OciGlue_OCIEnvCreate(sword *status);
dvoid *OciGlue_OCIHandleAlloc(sword *status, CONST dvoid *parenth, ub4 type);
void OciGlue_OCIServerAttach(sword *status, dvoid *srvhp, dvoid *errhp, char *dblink, ub4 mode);
void OciGlue_OCIAttrSet(sword *status, dvoid *trgthndlp, ub4 trghndltyp, dvoid *attributep, ub4 size, ub4 attrtype, dvoid *errhp);
void OciGlue_OCISessionBegin(sword *status, dvoid *svchp, dvoid *errhp, dvoid *usrhp, ub4 credt, ub4 mode );
*/

sword OciGlue_Connect (ub4 *connection_handle, char *database, char *username, char *password);
sword OciGlue_PrepareAndExecuteNonQuerySimple (ub4 connection_handle, char *sqlstmt, int *found);
sword OciGlue_Disconnect (ub4 connection_handle);
guint OciGlue_ConnectionCount ();
CONST text *OciGlue_CheckError (sword status, ub4 connection_handle);
void Free (void *obj);

#endif /* __MONO_OCIGLUE_H__ */

