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

#include "ociglue.h"

GSList *conlist = NULL;

oci_glue_connection_t *find_connection (ub4 connection_handle);
GSList *find_connection_node (ub4 connection_handle);

sword OciGlue_Connect (ub4 *connection_handle, 
					   char *database, char *username, char *password)
{
	sword status;
	oci_glue_connection_t *oci_glue_handle;

	*connection_handle = 0;

	oci_glue_handle = g_new(oci_glue_connection_t, 1);

	*connection_handle = (ub4) oci_glue_handle;
	
	oci_glue_handle->connection_handle = *connection_handle;
	oci_glue_handle->envhp	= (OCIEnv *) 0;
	oci_glue_handle->errhp	= (OCIError *) 0;
	oci_glue_handle->authp	= (OCISession *) 0;
	oci_glue_handle->srvhp	= (OCIServer *) 0;
	oci_glue_handle->svchp	= (OCISvcCtx *) 0;
	oci_glue_handle->stmthp	= (OCIStmt *) 0;

	conlist = g_slist_append (conlist, oci_glue_handle);

	status = OCIEnvCreate(&(oci_glue_handle->envhp), OCI_DEFAULT, (dvoid *)0, 
                               0, 0, 0, (size_t) 0, (dvoid **)0);
    
	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

	status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->errhp), 
					OCI_HTYPE_ERROR, 
                   (size_t) 0, (dvoid **) 0);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  /* server contexts */
  status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->srvhp), OCI_HTYPE_SERVER,
                   (size_t) 0, (dvoid **) 0);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIHandleAlloc( (dvoid *) (oci_glue_handle->envhp), 
					(dvoid **) &(oci_glue_handle->svchp), OCI_HTYPE_SVCCTX,
                   (size_t) 0, (dvoid **) 0);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIServerAttach(oci_glue_handle->srvhp, 
					oci_glue_handle->errhp, (text *)"", strlen(""), 0);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIAttrSet( (dvoid *) (oci_glue_handle->svchp), OCI_HTYPE_SVCCTX, 
					(dvoid *) (oci_glue_handle->srvhp), 
                    (ub4) 0, OCI_ATTR_SERVER, 
					(OCIError *) (oci_glue_handle->errhp));

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIHandleAlloc((dvoid *) (oci_glue_handle->envhp), 
						(dvoid **)&(oci_glue_handle->authp), 
                        (ub4) OCI_HTYPE_SESSION, (size_t) 0, (dvoid **) 0);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIAttrSet((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION,
                 (dvoid *) username, (ub4) strlen((char *)username),
                 (ub4) OCI_ATTR_USERNAME, oci_glue_handle->errhp);
  
	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIAttrSet((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION,
                 (dvoid *) password, (ub4) strlen((char *)password),
                 (ub4) OCI_ATTR_PASSWORD, oci_glue_handle->errhp);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCISessionBegin ( oci_glue_handle->svchp,  
						oci_glue_handle->errhp, 
						oci_glue_handle->authp, 
						OCI_CRED_RDBMS, 
						(ub4) OCI_DEFAULT);

	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

  status = OCIAttrSet((dvoid *) (oci_glue_handle->svchp), 
					(ub4) OCI_HTYPE_SVCCTX,
                   (dvoid *) (oci_glue_handle->authp), (ub4) 0,
                   (ub4) OCI_ATTR_SESSION, 
				   oci_glue_handle->errhp); 

  	if(status != 0) {
		OciGlue_Disconnect (*connection_handle);
		return status;
	}

	return status;
}

sword OciGlue_PrepareAndExecuteNonQuerySimple (ub4 connection_handle,
									char *sqlstmt, int *found)
{
	sword status = 0;
	oci_glue_connection_t *oci_glue_handle;
	void *node;
	GSList *con_node;
	
		
	if(!conlist)
		return -1;

	if(connection_handle == 0)
		return -2;

	if(!found)
		return -1;

	*found = 0;

	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return -1;
	else
		*found = oci_glue_handle->connection_handle;

	if(!(oci_glue_handle->stmthp)) {
		status = OCIHandleAlloc((dvoid *) (oci_glue_handle->envhp), 
						(dvoid **) &(oci_glue_handle->stmthp),
			            (ub4)OCI_HTYPE_STMT, (CONST size_t) 0, (dvoid **) 0);

		if(status != 0)
			return status;
	}
	
	status = OCIStmtPrepare((oci_glue_handle->stmthp), 
						oci_glue_handle->errhp, 
						(CONST OraText *)sqlstmt, (ub4)strlen(sqlstmt),
                    (ub4) OCI_NTV_SYNTAX, (ub4) OCI_DEFAULT);

	if(status != 0)
		return status;

	status = OCIStmtExecute(oci_glue_handle->svchp, 
						oci_glue_handle->stmthp, 
						oci_glue_handle->errhp, 
						(ub4) 1, (ub4) 0, 
               (CONST OCISnapshot *) NULL, (OCISnapshot *) NULL, OCI_DEFAULT);

	return status;
}

sword OciGlue_Disconnect (ub4 connection_handle)
{
	sword status = -1;
	
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	if(connection_handle == 0)
		return -2;

	if(conlist == NULL)
		return -2;

	oci_glue_handle = find_connection (connection_handle);

	if(oci_glue_handle) {
		
		status = OCISessionEnd(oci_glue_handle->svchp, 
			oci_glue_handle->errhp, oci_glue_handle->authp, (ub4) 0);
  
		status = OCIServerDetach(oci_glue_handle->srvhp, oci_glue_handle->errhp, 
			(ub4) OCI_DEFAULT);

		if (oci_glue_handle->srvhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->srvhp, (ub4) OCI_HTYPE_SERVER);

		if (oci_glue_handle->svchp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->svchp, (ub4) OCI_HTYPE_SVCCTX);

		if (oci_glue_handle->errhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->errhp, (ub4) OCI_HTYPE_ERROR);

		if (oci_glue_handle->authp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->authp, (ub4) OCI_HTYPE_SESSION);

		if (oci_glue_handle->stmthp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->stmthp, (ub4) OCI_HTYPE_STMT);

		if (oci_glue_handle->envhp)
			status = OCIHandleFree((dvoid *) oci_glue_handle->envhp, (ub4) OCI_HTYPE_ENV);
	}
	node = find_connection_node (connection_handle);
	if(node) {
			conlist = g_slist_remove_link (conlist, node);
			g_slist_free_1 (node);
			node = NULL;
	}
	if(oci_glue_handle) {
		g_free (oci_glue_handle);
		oci_glue_handle = NULL;
	}

	status = 0;
	
  return status;
}

guint OciGlue_ConnectionCount ()
{
	return g_slist_length (conlist);
}

CONST text *OciGlue_CheckError (sword status, ub4 connection_handle)
{
	oci_glue_connection_t *oci_glue_handle = NULL;
	text *errbuf;
	sb4 errcode = 0;
	size_t errbuf_size;
	
	if(!conlist)
		return NULL;

	if(connection_handle == 0)
		return NULL;

	oci_glue_handle = find_connection (connection_handle);

	if(!oci_glue_handle)
		return NULL;

	errbuf_size = sizeof(text) * 512;
	errbuf = (text *) malloc(errbuf_size);

	OCIErrorGet((dvoid *)(oci_glue_handle->errhp), 
		(ub4) 1, (text *) NULL, &errcode,
		errbuf, (ub4) errbuf_size, OCI_HTYPE_ERROR);
	
	return errbuf;
}

void Free (void *obj)
{
	free(obj);
}

GSList *find_connection_node (ub4 connection_handle)
{
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	for(node = conlist;
		node;
		node = node->next) {

		oci_glue_handle = (oci_glue_connection_t *) node->data;
		if(oci_glue_handle->connection_handle == connection_handle)
			return node;
	}
	return NULL;
}

oci_glue_connection_t *find_connection (ub4 connection_handle)
{	
	GSList *node = NULL;
	oci_glue_connection_t *oci_glue_handle = NULL;

	for(node = conlist;
		node;
		node = node->next) {

		oci_glue_handle = (oci_glue_connection_t *) node->data;	
		if(oci_glue_handle->connection_handle == connection_handle)
			return oci_glue_handle;		
	}
	return NULL;
}

/* For some reason, I was unable to get these to work. */
/*
dvoid *OciGlue_OCIEnvCreate(sword *status)
{
	OCIEnv *myenvhp = NULL;

	*status = OCIEnvCreate(&myenvhp, OCI_THREADED|OCI_OBJECT, (dvoid *)0, 
                               0, 0, 0, (size_t) 0, (dvoid **)0);

	return myenvhp;
}

dvoid *OciGlue_OCIHandleAlloc(sword *status, CONST dvoid *parenth, ub4 type)
{
	dvoid *hndlpp = NULL;
	 
	*status = OCIHandleAlloc ((dvoid *)parenth, (dvoid **)&hndlpp,
							type, 0, (dvoid **) 0);

	return hndlpp;
}

void OciGlue_OCIServerAttach(sword *status, dvoid *srvhp, dvoid *errhp, 
							 char *dblink, ub4 mode)
{
	*status = OCIServerAttach(srvhp, errhp, 
				dblink, (sb4) strlen(dblink), (ub4) mode);
}

void OciGlue_OCIAttrSet(sword *status, 
						dvoid *trgthndlp, ub4 trghndltyp, 
						dvoid *attributep, ub4 size, ub4 attrtype, 
						dvoid *errhp)
{
	*status = OCIAttrSet (trgthndlp, trghndltyp, 
						  attributep, size, attrtype, 
						  errhp);
}

void OciGlue_OCISessionBegin(sword *status, dvoid *svchp, dvoid *errhp, dvoid *usrhp,
							ub4 credt, ub4 mode )
{
	*status = OCISessionBegin (svchp, errhp, usrhp, credt, mode);
}
*/

