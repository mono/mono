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
 * 
 *	This file was originally ported	from Jaybird
 */

using System;

namespace FirebirdSql.Data.Common
{
	internal sealed class IscCodes
	{
		#region Constructors

		private IscCodes()
		{
		}

		#endregion

		#region General

		public const int SQLDA_VERSION1 = 1;
		public const int SQL_DIALECT_V5 = 1;
		public const int SQL_DIALECT_V6_TRANSITION = 2;
		public const int SQL_DIALECT_V6 = 3;
		public const int SQL_DIALECT_CURRENT = SQL_DIALECT_V6;

		public const int DSQL_close = 1;
		public const int DSQL_drop = 2;

		public const int ARRAY_DESC_COLUMN_MAJOR = 1;	/* Set for FORTRAN */

		public const int ISC_STATUS_LENGTH = 20;

		#endregion

		#region Buffer sizes

		public const int BUFFER_SIZE_128 = 128;
		public const int BUFFER_SIZE_256 = 256;
		public const int MAX_BUFFER_SIZE = 1024;

		public const int ROWS_AFFECTED_BUFFER_SIZE = 34;
		public const int STATEMENT_TYPE_BUFFER_SIZE = 8;

		#endregion

		#region Protocol Codes

		/* The protocol	is defined blocks, rather than messages, to
		 * separate	the	protocol from the transport	layer.	
		 */
		public const int CONNECT_VERSION2 = 2;

		/* Protocol	4 is protocol 3	plus server	management functions */
		public const int PROTOCOL_VERSION3 = 3;
		public const int PROTOCOL_VERSION4 = 4;

		/* Protocol	5 includes support for a d_float data type */
		public const int PROTOCOL_VERSION5 = 5;

		/* Protocol	6 includes support for cancel remote events, blob seek,
		 * and unknown message type	
		 */
		public const int PROTOCOL_VERSION6 = 6;

		/* Protocol	7 includes DSQL	support	*/
		public const int PROTOCOL_VERSION7 = 7;

		/* Protocol	8 includes collapsing first	receive	into a send, drop database,
		 * DSQL	execute	2, DSQL	execute	immediate 2, DSQL insert, services,	and
		 * transact	request.
		 */
		public const int PROTOCOL_VERSION8 = 8;

		/* Protocol	9 includes support for SPX32
		 * SPX32 uses WINSOCK instead of Novell	SDK
		 * In order	to differentiate between the old implementation
		 * of SPX and this one,	different PROTOCOL VERSIONS	are	used 
		 */
		public const int PROTOCOL_VERSION9 = 9;

		/* Protocol	10 includes	support	for	warnings and removes the requirement for
		 * encoding	and	decoding status	codes.
		 */
		public const int PROTOCOL_VERSION10 = 10;

		#endregion

		#region Server Class

		public const int isc_info_db_class_classic_access = 13;
		public const int isc_info_db_class_server_access = 14;

		#endregion

		#region Operation Codes

		// Operation (packet) types
		public const int op_void = 0;	// Packet has been voided
		public const int op_connect = 1;	// Connect to remote server
		public const int op_exit = 2;	// Remote end has exitted
		public const int op_accept = 3;	// Server accepts connection
		public const int op_reject = 4;	// Server rejects connection
		public const int op_protocol = 5;	// Protocol	selection
		public const int op_disconnect = 6;	// Connect is going	away
		public const int op_credit = 7;	// Grant (buffer) credits
		public const int op_continuation = 8;	// Continuation	packet
		public const int op_response = 9;	// Generic response	block

		// Page	server operations

		public const int op_open_file = 10;	// Open	file for page service
		public const int op_create_file = 11;	// Create file for page	service
		public const int op_close_file = 12;	// Close file for page service
		public const int op_read_page = 13;	// optionally lock and read	page
		public const int op_write_page = 14;	// write page and optionally release lock
		public const int op_lock = 15;	// sieze lock
		public const int op_convert_lock = 16;	// convert existing	lock
		public const int op_release_lock = 17;	// release existing	lock
		public const int op_blocking = 18;	// blocking	lock message

		// Full	context	server operations

		public const int op_attach = 19;	// Attach database
		public const int op_create = 20;	// Create database
		public const int op_detach = 21;	// Detach database
		public const int op_compile = 22;	// Request based operations
		public const int op_start = 23;
		public const int op_start_and_send = 24;
		public const int op_send = 25;
		public const int op_receive = 26;
		public const int op_unwind = 27;
		public const int op_release = 28;

		public const int op_transaction = 29;	// Transaction operations
		public const int op_commit = 30;
		public const int op_rollback = 31;
		public const int op_prepare = 32;
		public const int op_reconnect = 33;

		public const int op_create_blob = 34;	// Blob	operations //
		public const int op_open_blob = 35;
		public const int op_get_segment = 36;
		public const int op_put_segment = 37;
		public const int op_cancel_blob = 38;
		public const int op_close_blob = 39;

		public const int op_info_database = 40;	// Information services
		public const int op_info_request = 41;
		public const int op_info_transaction = 42;
		public const int op_info_blob = 43;

		public const int op_batch_segments = 44;	// Put a bunch of blob segments

		public const int op_mgr_set_affinity = 45;	// Establish server	affinity
		public const int op_mgr_clear_affinity = 46;	// Break server	affinity
		public const int op_mgr_report = 47;	// Report on server

		public const int op_que_events = 48;	// Que event notification request
		public const int op_cancel_events = 49;	// Cancel event	notification request
		public const int op_commit_retaining = 50;	// Commit retaining	(what else)
		public const int op_prepare2 = 51;	// Message form	of prepare
		public const int op_event = 52;	// Completed event request (asynchronous)
		public const int op_connect_request = 53;	// Request to establish	connection
		public const int op_aux_connect = 54;	// Establish auxiliary connection
		public const int op_ddl = 55;	// DDL call
		public const int op_open_blob2 = 56;
		public const int op_create_blob2 = 57;
		public const int op_get_slice = 58;
		public const int op_put_slice = 59;
		public const int op_slice = 60;	// Successful response to public const int op_get_slice
		public const int op_seek_blob = 61;	// Blob	seek operation

		// DSQL	operations //

		public const int op_allocate_statement = 62;	// allocate	a statment handle
		public const int op_execute = 63;	// execute a prepared statement
		public const int op_exec_immediate = 64;	// execute a statement
		public const int op_fetch = 65;	// fetch a record
		public const int op_fetch_response = 66;	// response	for	record fetch
		public const int op_free_statement = 67;	// free	a statement
		public const int op_prepare_statement = 68;	// prepare a statement
		public const int op_set_cursor = 69;	// set a cursor	name
		public const int op_info_sql = 70;

		public const int op_dummy = 71;	// dummy packet	to detect loss of client

		public const int op_response_piggyback = 72;	// response	block for piggybacked messages
		public const int op_start_and_receive = 73;
		public const int op_start_send_and_receive = 74;

		public const int op_exec_immediate2 = 75;	// execute an immediate	statement with msgs
		public const int op_execute2 = 76;	// execute a statement with	msgs
		public const int op_insert = 77;
		public const int op_sql_response = 78;	// response	from execute; exec immed; insert

		public const int op_transact = 79;
		public const int op_transact_response = 80;
		public const int op_drop_database = 81;

		public const int op_service_attach = 82;
		public const int op_service_detach = 83;
		public const int op_service_info = 84;
		public const int op_service_start = 85;

		public const int op_rollback_retaining = 86;

		#endregion

		#region Database Parameter Block

		public const int isc_dpb_version1 = 1;
		public const int isc_dpb_cdd_pathname = 1;
		public const int isc_dpb_allocation = 2;
		public const int isc_dpb_journal = 3;
		public const int isc_dpb_page_size = 4;
		public const int isc_dpb_num_buffers = 5;
		public const int isc_dpb_buffer_length = 6;
		public const int isc_dpb_debug = 7;
		public const int isc_dpb_garbage_collect = 8;
		public const int isc_dpb_verify = 9;
		public const int isc_dpb_sweep = 10;
		public const int isc_dpb_enable_journal = 11;
		public const int isc_dpb_disable_journal = 12;
		public const int isc_dpb_dbkey_scope = 13;
		public const int isc_dpb_number_of_users = 14;
		public const int isc_dpb_trace = 15;
		public const int isc_dpb_no_garbage_collect = 16;
		public const int isc_dpb_damaged = 17;
		public const int isc_dpb_license = 18;
		public const int isc_dpb_sys_user_name = 19;
		public const int isc_dpb_encrypt_key = 20;
		public const int isc_dpb_activate_shadow = 21;
		public const int isc_dpb_sweep_interval = 22;
		public const int isc_dpb_delete_shadow = 23;
		public const int isc_dpb_force_write = 24;
		public const int isc_dpb_begin_log = 25;
		public const int isc_dpb_quit_log = 26;
		public const int isc_dpb_no_reserve = 27;
		public const int isc_dpb_user_name = 28;
		public const int isc_dpb_password = 29;
		public const int isc_dpb_password_enc = 30;
		public const int isc_dpb_sys_user_name_enc = 31;
		public const int isc_dpb_interp = 32;
		public const int isc_dpb_online_dump = 33;
		public const int isc_dpb_old_file_size = 34;
		public const int isc_dpb_old_num_files = 35;
		public const int isc_dpb_old_file = 36;
		public const int isc_dpb_old_start_page = 37;
		public const int isc_dpb_old_start_seqno = 38;
		public const int isc_dpb_old_start_file = 39;
		public const int isc_dpb_drop_walfile = 40;
		public const int isc_dpb_old_dump_id = 41;
		/*
		public const int isc_dpb_wal_backup_dir			 = 42;
		public const int isc_dpb_wal_chkptlen			 = 43;
		public const int isc_dpb_wal_numbufs			 = 44;
		public const int isc_dpb_wal_bufsize			 = 45;
		public const int isc_dpb_wal_grp_cmt_wait		 = 46;
		*/
		public const int isc_dpb_lc_messages = 47;
		public const int isc_dpb_lc_ctype = 48;
		public const int isc_dpb_cache_manager = 49;
		public const int isc_dpb_shutdown = 50;
		public const int isc_dpb_online = 51;
		public const int isc_dpb_shutdown_delay = 52;
		public const int isc_dpb_reserved = 53;
		public const int isc_dpb_overwrite = 54;
		public const int isc_dpb_sec_attach = 55;
		/*
		public const int isc_dpb_disable_wal			 = 56;
		*/
		public const int isc_dpb_connect_timeout = 57;
		public const int isc_dpb_dummy_packet_interval = 58;
		public const int isc_dpb_gbak_attach = 59;
		public const int isc_dpb_sql_role_name = 60;
		public const int isc_dpb_set_page_buffers = 61;
		public const int isc_dpb_working_directory = 62;
		public const int isc_dpb_sql_dialect = 63;
		public const int isc_dpb_set_db_readonly = 64;
		public const int isc_dpb_set_db_sql_dialect = 65;
		public const int isc_dpb_gfix_attach = 66;
		public const int isc_dpb_gstat_attach = 67;
		public const int isc_dpb_set_db_charset = 68;

		#endregion

		#region Transaction	Parameter Block

		public const int isc_tpb_version1 = 1;
		public const int isc_tpb_version3 = 3;
		public const int isc_tpb_consistency = 1;
		public const int isc_tpb_concurrency = 2;
		public const int isc_tpb_shared = 3;
		public const int isc_tpb_protected = 4;
		public const int isc_tpb_exclusive = 5;
		public const int isc_tpb_wait = 6;
		public const int isc_tpb_nowait = 7;
		public const int isc_tpb_read = 8;
		public const int isc_tpb_write = 9;
		public const int isc_tpb_lock_read = 10;
		public const int isc_tpb_lock_write = 11;
		public const int isc_tpb_verb_time = 12;
		public const int isc_tpb_commit_time = 13;
		public const int isc_tpb_ignore_limbo = 14;
		public const int isc_tpb_read_committed = 15;
		public const int isc_tpb_autocommit = 16;
		public const int isc_tpb_rec_version = 17;
		public const int isc_tpb_no_rec_version = 18;
		public const int isc_tpb_restart_requests = 19;
		public const int isc_tpb_no_auto_undo = 20;

		#endregion

		#region Services Parameter Block

		public const int isc_spb_version1 = 1;
		public const int isc_spb_current_version = 2;
		public const int isc_spb_version = isc_spb_current_version;
		public const int isc_spb_user_name = isc_dpb_user_name;
		public const int isc_spb_sys_user_name = isc_dpb_sys_user_name;
		public const int isc_spb_sys_user_name_enc = isc_dpb_sys_user_name_enc;
		public const int isc_spb_password = isc_dpb_password;
		public const int isc_spb_password_enc = isc_dpb_password_enc;
		public const int isc_spb_command_line = 105;
		public const int isc_spb_dbname = 106;
		public const int isc_spb_verbose = 107;
		public const int isc_spb_options = 108;

		public const int isc_spb_connect_timeout = isc_dpb_connect_timeout;
		public const int isc_spb_dummy_packet_interval = isc_dpb_dummy_packet_interval;
		public const int isc_spb_sql_role_name = isc_dpb_sql_role_name;

		public const int isc_spb_num_att = 5;
		public const int isc_spb_num_db = 6;

		#endregion

		#region Services Actions

		public const int isc_action_svc_backup = 1;	/* Starts database backup process on the server	*/
		public const int isc_action_svc_restore = 2;	/* Starts database restore process on the server */
		public const int isc_action_svc_repair = 3;	/* Starts database repair process on the server	*/
		public const int isc_action_svc_add_user = 4;	/* Adds	a new user to the security database	*/
		public const int isc_action_svc_delete_user = 5;	/* Deletes a user record from the security database	*/
		public const int isc_action_svc_modify_user = 6;	/* Modifies	a user record in the security database */
		public const int isc_action_svc_display_user = 7;	/* Displays	a user record from the security	database */
		public const int isc_action_svc_properties = 8;	/* Sets	database properties	*/
		public const int isc_action_svc_add_license = 9;	/* Adds	a license to the license file */
		public const int isc_action_svc_remove_license = 10;	/* Removes a license from the license file */
		public const int isc_action_svc_db_stats = 11;	/* Retrieves database statistics */
		public const int isc_action_svc_get_ib_log = 12;	/* Retrieves the InterBase log file	from the server	*/

		#endregion

		#region Services Information

		public const int isc_info_svc_svr_db_info = 50;	/* Retrieves the number	of attachments and databases */
		public const int isc_info_svc_get_license = 51;	/* Retrieves all license keys and IDs from the license file	*/
		public const int isc_info_svc_get_license_mask = 52;	/* Retrieves a bitmask representing	licensed options on	the	server */
		public const int isc_info_svc_get_config = 53;	/* Retrieves the parameters	and	values for IB_CONFIG */
		public const int isc_info_svc_version = 54;	/* Retrieves the version of	the	services manager */
		public const int isc_info_svc_server_version = 55;	/* Retrieves the version of	the	InterBase server */
		public const int isc_info_svc_implementation = 56;	/* Retrieves the implementation	of the InterBase server	*/
		public const int isc_info_svc_capabilities = 57;	/* Retrieves a bitmask representing	the	server's capabilities */
		public const int isc_info_svc_user_dbpath = 58;	/* Retrieves the path to the security database in use by the server	*/
		public const int isc_info_svc_get_env = 59;	/* Retrieves the setting of	$INTERBASE */
		public const int isc_info_svc_get_env_lock = 60;	/* Retrieves the setting of	$INTERBASE_LCK */
		public const int isc_info_svc_get_env_msg = 61;	/* Retrieves the setting of	$INTERBASE_MSG */
		public const int isc_info_svc_line = 62;	/* Retrieves 1 line	of service output per call */
		public const int isc_info_svc_to_eof = 63;	/* Retrieves as much of	the	server output as will fit in the supplied buffer */
		public const int isc_info_svc_timeout = 64;	/* Sets	/ signifies	a timeout value	for	reading	service	information	*/
		public const int isc_info_svc_get_licensed_users = 65;	/* Retrieves the number	of users licensed for accessing	the	server */
		public const int isc_info_svc_limbo_trans = 66;	/* Retrieve	the	limbo transactions */
		public const int isc_info_svc_running = 67;	/* Checks to see if	a service is running on	an attachment */
		public const int isc_info_svc_get_users = 68;	/* Returns the user	information	from isc_action_svc_display_users */

		#endregion

		#region Services Properties

		public const int isc_spb_prp_page_buffers = 5;
		public const int isc_spb_prp_sweep_interval = 6;
		public const int isc_spb_prp_shutdown_db = 7;
		public const int isc_spb_prp_deny_new_attachments = 9;
		public const int isc_spb_prp_deny_new_transactions = 10;
		public const int isc_spb_prp_reserve_space = 11;
		public const int isc_spb_prp_write_mode = 12;
		public const int isc_spb_prp_access_mode = 13;
		public const int isc_spb_prp_set_sql_dialect = 14;

		// WRITE_MODE_PARAMETERS
		public const int isc_spb_prp_wm_async = 37;
		public const int isc_spb_prp_wm_sync = 38;

		// ACCESS_MODE_PARAMETERS
		public const int isc_spb_prp_am_readonly = 39;
		public const int isc_spb_prp_am_readwrite = 40;

		// RESERVE_SPACE_PARAMETERS
		public const int isc_spb_prp_res_use_full = 35;
		public const int isc_spb_prp_res = 36;

		// Option Flags		
		public const int isc_spb_prp_activate = 0x0100;
		public const int isc_spb_prp_db_online = 0x0200;

		#endregion

		#region Backup Service

		public const int isc_spb_bkp_file = 5;
		public const int isc_spb_bkp_factor = 6;
		public const int isc_spb_bkp_length = 7;

		#endregion

		#region Restore	Service

		public const int isc_spb_res_buffers = 9;
		public const int isc_spb_res_page_size = 10;
		public const int isc_spb_res_length = 11;
		public const int isc_spb_res_access_mode = 12;

		public const int isc_spb_res_am_readonly = isc_spb_prp_am_readonly;
		public const int isc_spb_res_am_readwrite = isc_spb_prp_am_readwrite;

		#endregion

		#region Repair Service

		public const int isc_spb_rpr_commit_trans = 15;
		public const int isc_spb_rpr_rollback_trans = 34;
		public const int isc_spb_rpr_recover_two_phase = 17;
		public const int isc_spb_tra_id = 18;
		public const int isc_spb_single_tra_id = 19;
		public const int isc_spb_multi_tra_id = 20;
		public const int isc_spb_tra_state = 21;
		public const int isc_spb_tra_state_limbo = 22;
		public const int isc_spb_tra_state_commit = 23;
		public const int isc_spb_tra_state_rollback = 24;
		public const int isc_spb_tra_state_unknown = 25;
		public const int isc_spb_tra_host_site = 26;
		public const int isc_spb_tra_remote_site = 27;
		public const int isc_spb_tra_db_path = 28;
		public const int isc_spb_tra_advise = 29;
		public const int isc_spb_tra_advise_commit = 30;
		public const int isc_spb_tra_advise_rollback = 31;
		public const int isc_spb_tra_advise_unknown = 33;

		#endregion

		#region Security Service

		public const int isc_spb_sec_userid = 5;
		public const int isc_spb_sec_groupid = 6;
		public const int isc_spb_sec_username = 7;
		public const int isc_spb_sec_password = 8;
		public const int isc_spb_sec_groupname = 9;
		public const int isc_spb_sec_firstname = 10;
		public const int isc_spb_sec_middlename = 11;
		public const int isc_spb_sec_lastname = 12;

		#endregion

		#region Configuration Keys

		public const int ISCCFG_LOCKMEM_KEY = 0;
		public const int ISCCFG_LOCKSEM_KEY = 1;
		public const int ISCCFG_LOCKSIG_KEY = 2;
		public const int ISCCFG_EVNTMEM_KEY = 3;
		public const int ISCCFG_DBCACHE_KEY = 4;
		public const int ISCCFG_PRIORITY_KEY = 5;
		public const int ISCCFG_IPCMAP_KEY = 6;
		public const int ISCCFG_MEMMIN_KEY = 7;
		public const int ISCCFG_MEMMAX_KEY = 8;
		public const int ISCCFG_LOCKORDER_KEY = 9;
		public const int ISCCFG_ANYLOCKMEM_KEY = 10;
		public const int ISCCFG_ANYLOCKSEM_KEY = 11;
		public const int ISCCFG_ANYLOCKSIG_KEY = 12;
		public const int ISCCFG_ANYEVNTMEM_KEY = 13;
		public const int ISCCFG_LOCKHASH_KEY = 14;
		public const int ISCCFG_DEADLOCK_KEY = 15;
		public const int ISCCFG_LOCKSPIN_KEY = 16;
		public const int ISCCFG_CONN_TIMEOUT_KEY = 17;
		public const int ISCCFG_DUMMY_INTRVL_KEY = 18;
		public const int ISCCFG_TRACE_POOLS_KEY = 19; /* Internal Use only	*/
		public const int ISCCFG_REMOTE_BUFFER_KEY = 20;

		#endregion

		#region Common Structural Codes

		public const int isc_info_end = 1;
		public const int isc_info_truncated = 2;
		public const int isc_info_error = 3;
		public const int isc_info_data_not_ready = 4;
		public const int isc_info_flag_end = 127;

		#endregion

		#region SQL	Information

		public const int isc_info_sql_select = 4;
		public const int isc_info_sql_bind = 5;
		public const int isc_info_sql_num_variables = 6;
		public const int isc_info_sql_describe_vars = 7;
		public const int isc_info_sql_describe_end = 8;
		public const int isc_info_sql_sqlda_seq = 9;
		public const int isc_info_sql_message_seq = 10;
		public const int isc_info_sql_type = 11;
		public const int isc_info_sql_sub_type = 12;
		public const int isc_info_sql_scale = 13;
		public const int isc_info_sql_length = 14;
		public const int isc_info_sql_null_ind = 15;
		public const int isc_info_sql_field = 16;
		public const int isc_info_sql_relation = 17;
		public const int isc_info_sql_owner = 18;
		public const int isc_info_sql_alias = 19;
		public const int isc_info_sql_sqlda_start = 20;
		public const int isc_info_sql_stmt_type = 21;
		public const int isc_info_sql_get_plan = 22;
		public const int isc_info_sql_records = 23;
		public const int isc_info_sql_batch_fetch = 24;
		public const int isc_info_sql_relation_alias = 25;

		#endregion

		#region SQL	Information	Return Values

		public const int isc_info_sql_stmt_select = 1;
		public const int isc_info_sql_stmt_insert = 2;
		public const int isc_info_sql_stmt_update = 3;
		public const int isc_info_sql_stmt_delete = 4;
		public const int isc_info_sql_stmt_ddl = 5;
		public const int isc_info_sql_stmt_get_segment = 6;
		public const int isc_info_sql_stmt_put_segment = 7;
		public const int isc_info_sql_stmt_exec_procedure = 8;
		public const int isc_info_sql_stmt_start_trans = 9;
		public const int isc_info_sql_stmt_commit = 10;
		public const int isc_info_sql_stmt_rollback = 11;
		public const int isc_info_sql_stmt_select_for_upd = 12;
		public const int isc_info_sql_stmt_set_generator = 13;
		public const int isc_info_sql_stmt_savepoint = 14;

		#endregion

		#region Database Information

		public const int isc_info_db_id = 4;
		public const int isc_info_reads = 5;
		public const int isc_info_writes = 6;
		public const int isc_info_fetches = 7;
		public const int isc_info_marks = 8;

		public const int isc_info_implementation = 11;
		public const int isc_info_isc_version = 12;
		public const int isc_info_base_level = 13;
		public const int isc_info_page_size = 14;
		public const int isc_info_num_buffers = 15;
		public const int isc_info_limbo = 16;
		public const int isc_info_current_memory = 17;
		public const int isc_info_max_memory = 18;
		public const int isc_info_window_turns = 19;
		public const int isc_info_license = 20;

		public const int isc_info_allocation = 21;
		public const int isc_info_attachment_id = 22;
		public const int isc_info_read_seq_count = 23;
		public const int isc_info_read_idx_count = 24;
		public const int isc_info_insert_count = 25;
		public const int isc_info_update_count = 26;
		public const int isc_info_delete_count = 27;
		public const int isc_info_backout_count = 28;
		public const int isc_info_purge_count = 29;
		public const int isc_info_expunge_count = 30;

		public const int isc_info_sweep_interval = 31;
		public const int isc_info_ods_version = 32;
		public const int isc_info_ods_minor_version = 33;
		public const int isc_info_no_reserve = 34;
		public const int isc_info_logfile = 35;
		public const int isc_info_cur_logfile_name = 36;
		public const int isc_info_cur_log_part_offset = 37;
		public const int isc_info_num_wal_buffers = 38;
		public const int isc_info_wal_buffer_size = 39;
		public const int isc_info_wal_ckpt_length = 40;

		public const int isc_info_wal_cur_ckpt_interval = 41;
		public const int isc_info_wal_prv_ckpt_fname = 42;
		public const int isc_info_wal_prv_ckpt_poffset = 43;
		public const int isc_info_wal_recv_ckpt_fname = 44;
		public const int isc_info_wal_recv_ckpt_poffset = 45;
		public const int isc_info_wal_grpc_wait_usecs = 47;
		public const int isc_info_wal_num_io = 48;
		public const int isc_info_wal_avg_io_size = 49;
		public const int isc_info_wal_num_commits = 50;

		public const int isc_info_wal_avg_grpc_size = 51;
		public const int isc_info_forced_writes = 52;
		public const int isc_info_user_names = 53;
		public const int isc_info_page_errors = 54;
		public const int isc_info_record_errors = 55;
		public const int isc_info_bpage_errors = 56;
		public const int isc_info_dpage_errors = 57;
		public const int isc_info_ipage_errors = 58;
		public const int isc_info_ppage_errors = 59;
		public const int isc_info_tpage_errors = 60;

		public const int isc_info_set_page_buffers = 61;
		public const int isc_info_db_sql_dialect = 62;
		public const int isc_info_db_read_only = 63;
		public const int isc_info_db_size_in_pages = 64;

		/* Values 65 -100 unused to	avoid conflict with	InterBase */

		public const int frb_info_att_charset = 101;
		public const int isc_info_db_class = 102;
		public const int isc_info_firebird_version = 103;
		public const int isc_info_oldest_transaction = 104;
		public const int isc_info_oldest_active = 105;
		public const int isc_info_oldest_snapshot = 106;
		public const int isc_info_next_transaction = 107;
		public const int isc_info_db_provider = 108;
		public const int isc_info_active_transactions = 109;

		#endregion

		#region Information	Request

		public const int isc_info_number_messages = 4;
		public const int isc_info_max_message = 5;
		public const int isc_info_max_send = 6;
		public const int isc_info_max_receive = 7;
		public const int isc_info_state = 8;
		public const int isc_info_message_number = 9;
		public const int isc_info_message_size = 10;
		public const int isc_info_request_cost = 11;
		public const int isc_info_access_path = 12;
		public const int isc_info_req_select_count = 13;
		public const int isc_info_req_insert_count = 14;
		public const int isc_info_req_update_count = 15;
		public const int isc_info_req_delete_count = 16;

		#endregion

		#region Array Slice	Description	Language

		public const int isc_sdl_version1 = 1;
		public const int isc_sdl_eoc = 255;
		public const int isc_sdl_relation = 2;
		public const int isc_sdl_rid = 3;
		public const int isc_sdl_field = 4;
		public const int isc_sdl_fid = 5;
		public const int isc_sdl_struct = 6;
		public const int isc_sdl_variable = 7;
		public const int isc_sdl_scalar = 8;
		public const int isc_sdl_tiny_integer = 9;
		public const int isc_sdl_short_integer = 10;
		public const int isc_sdl_long_integer = 11;
		public const int isc_sdl_literal = 12;
		public const int isc_sdl_add = 13;
		public const int isc_sdl_subtract = 14;
		public const int isc_sdl_multiply = 15;
		public const int isc_sdl_divide = 16;
		public const int isc_sdl_negate = 17;
		public const int isc_sdl_eql = 18;
		public const int isc_sdl_neq = 19;
		public const int isc_sdl_gtr = 20;
		public const int isc_sdl_geq = 21;
		public const int isc_sdl_lss = 22;
		public const int isc_sdl_leq = 23;
		public const int isc_sdl_and = 24;
		public const int isc_sdl_or = 25;
		public const int isc_sdl_not = 26;
		public const int isc_sdl_while = 27;
		public const int isc_sdl_assignment = 28;
		public const int isc_sdl_label = 29;
		public const int isc_sdl_leave = 30;
		public const int isc_sdl_begin = 31;
		public const int isc_sdl_end = 32;
		public const int isc_sdl_do3 = 33;
		public const int isc_sdl_do2 = 34;
		public const int isc_sdl_do1 = 35;
		public const int isc_sdl_element = 36;

		#endregion

		#region Blob Parametr Block

		public const int isc_bpb_version1 = 1;
		public const int isc_bpb_source_type = 1;
		public const int isc_bpb_target_type = 2;
		public const int isc_bpb_type = 3;
		public const int isc_bpb_source_interp = 4;
		public const int isc_bpb_target_interp = 5;
		public const int isc_bpb_filter_parameter = 6;

		public const int isc_bpb_type_segmented = 0;
		public const int isc_bpb_type_stream = 1;

		public const int RBL_eof = 1;
		public const int RBL_segment = 2;
		public const int RBL_eof_pending = 4;
		public const int RBL_create = 8;

		#endregion

		#region Blob Information

		public const int isc_info_blob_num_segments = 4;
		public const int isc_info_blob_max_segment = 5;
		public const int isc_info_blob_total_length = 6;
		public const int isc_info_blob_type = 7;

		#endregion

		#region Event Codes

		public const int P_REQ_async = 1;	// Auxiliary asynchronous port
		public const int EPB_version1 = 1;

		#endregion

		#region Facilities

		public const int JRD = 0;
		public const int GFIX = 3;
		public const int DSQL = 7;
		public const int DYN = 8;
		public const int GBAK = 12;
		public const int GDEC = 18;
		public const int LICENSE = 19;
		public const int GSTAT = 21;

		#endregion

		#region Error code generation

		public const int ISC_MASK = 0x14000000;	// Defines the code	as a valid ISC code

		#endregion

		#region ISC	Error codes

		public const int isc_facility = 20;
		public const int isc_err_base = 335544320;
		public const int isc_err_factor = 1;
		public const int isc_arg_end = 0;	// end of argument list
		public const int isc_arg_gds = 1;	// generic DSRI	status value
		public const int isc_arg_string = 2;	// string argument
		public const int isc_arg_cstring = 3;	// count & string argument
		public const int isc_arg_number = 4;	// numeric argument	(long)
		public const int isc_arg_interpreted = 5;	// interpreted status code (string)
		public const int isc_arg_vms = 6;	// VAX/VMS status code (long)
		public const int isc_arg_unix = 7;	// UNIX	error code
		public const int isc_arg_domain = 8;	// Apollo/Domain error code
		public const int isc_arg_dos = 9;	// MSDOS/OS2 error code
		public const int isc_arg_mpexl = 10;	// HP MPE/XL error code
		public const int isc_arg_mpexl_ipc = 11;	// HP MPE/XL IPC error code
		public const int isc_arg_next_mach = 15;	// NeXT/Mach error code
		public const int isc_arg_netware = 16;	// NetWare error code
		public const int isc_arg_win32 = 17;	// Win32 error code
		public const int isc_arg_warning = 18;	// warning argument

		public const int isc_arith_except = 335544321;
		public const int isc_bad_dbkey = 335544322;
		public const int isc_bad_db_format = 335544323;
		public const int isc_bad_db_handle = 335544324;
		public const int isc_bad_dpb_content = 335544325;
		public const int isc_bad_dpb_form = 335544326;
		public const int isc_bad_req_handle = 335544327;
		public const int isc_bad_segstr_handle = 335544328;
		public const int isc_bad_segstr_id = 335544329;
		public const int isc_bad_tpb_content = 335544330;
		public const int isc_bad_tpb_form = 335544331;
		public const int isc_bad_trans_handle = 335544332;
		public const int isc_bug_check = 335544333;
		public const int isc_convert_error = 335544334;
		public const int isc_db_corrupt = 335544335;
		public const int isc_deadlock = 335544336;
		public const int isc_excess_trans = 335544337;
		public const int isc_from_no_match = 335544338;
		public const int isc_infinap = 335544339;
		public const int isc_infona = 335544340;
		public const int isc_infunk = 335544341;
		public const int isc_integ_fail = 335544342;
		public const int isc_invalid_blr = 335544343;
		public const int isc_io_error = 335544344;
		public const int isc_lock_conflict = 335544345;
		public const int isc_metadata_corrupt = 335544346;
		public const int isc_not_valid = 335544347;
		public const int isc_no_cur_rec = 335544348;
		public const int isc_no_dup = 335544349;
		public const int isc_no_finish = 335544350;
		public const int isc_no_meta_update = 335544351;
		public const int isc_no_priv = 335544352;
		public const int isc_no_recon = 335544353;
		public const int isc_no_record = 335544354;
		public const int isc_no_segstr_close = 335544355;
		public const int isc_obsolete_metadata = 335544356;
		public const int isc_open_trans = 335544357;
		public const int isc_port_len = 335544358;
		public const int isc_read_only_field = 335544359;
		public const int isc_read_only_rel = 335544360;
		public const int isc_read_only_trans = 335544361;
		public const int isc_read_only_view = 335544362;
		public const int isc_req_no_trans = 335544363;
		public const int isc_req_sync = 335544364;
		public const int isc_req_wrong_db = 335544365;
		public const int isc_segment = 335544366;
		public const int isc_segstr_eof = 335544367;
		public const int isc_segstr_no_op = 335544368;
		public const int isc_segstr_no_read = 335544369;
		public const int isc_segstr_no_trans = 335544370;
		public const int isc_segstr_no_write = 335544371;
		public const int isc_segstr_wrong_db = 335544372;
		public const int isc_sys_request = 335544373;
		public const int isc_stream_eof = 335544374;
		public const int isc_unavailable = 335544375;
		public const int isc_unres_rel = 335544376;
		public const int isc_uns_ext = 335544377;
		public const int isc_wish_list = 335544378;
		public const int isc_wrong_ods = 335544379;
		public const int isc_wronumarg = 335544380;
		public const int isc_imp_exc = 335544381;
		public const int isc_random = 335544382;
		public const int isc_fatal_conflict = 335544383;
		public const int isc_badblk = 335544384;
		public const int isc_invpoolcl = 335544385;
		public const int isc_nopoolids = 335544386;
		public const int isc_relbadblk = 335544387;
		public const int isc_blktoobig = 335544388;
		public const int isc_bufexh = 335544389;
		public const int isc_syntaxerr = 335544390;
		public const int isc_bufinuse = 335544391;
		public const int isc_bdbincon = 335544392;
		public const int isc_reqinuse = 335544393;
		public const int isc_badodsver = 335544394;
		public const int isc_relnotdef = 335544395;
		public const int isc_fldnotdef = 335544396;
		public const int isc_dirtypage = 335544397;
		public const int isc_waifortra = 335544398;
		public const int isc_doubleloc = 335544399;
		public const int isc_nodnotfnd = 335544400;
		public const int isc_dupnodfnd = 335544401;
		public const int isc_locnotmar = 335544402;
		public const int isc_badpagtyp = 335544403;
		public const int isc_corrupt = 335544404;
		public const int isc_badpage = 335544405;
		public const int isc_badindex = 335544406;
		public const int isc_dbbnotzer = 335544407;
		public const int isc_tranotzer = 335544408;
		public const int isc_trareqmis = 335544409;
		public const int isc_badhndcnt = 335544410;
		public const int isc_wrotpbver = 335544411;
		public const int isc_wroblrver = 335544412;
		public const int isc_wrodpbver = 335544413;
		public const int isc_blobnotsup = 335544414;
		public const int isc_badrelation = 335544415;
		public const int isc_nodetach = 335544416;
		public const int isc_notremote = 335544417;
		public const int isc_trainlim = 335544418;
		public const int isc_notinlim = 335544419;
		public const int isc_traoutsta = 335544420;
		public const int isc_connect_reject = 335544421;
		public const int isc_dbfile = 335544422;
		public const int isc_orphan = 335544423;
		public const int isc_no_lock_mgr = 335544424;
		public const int isc_ctxinuse = 335544425;
		public const int isc_ctxnotdef = 335544426;
		public const int isc_datnotsup = 335544427;
		public const int isc_badmsgnum = 335544428;
		public const int isc_badparnum = 335544429;
		public const int isc_virmemexh = 335544430;
		public const int isc_blocking_signal = 335544431;
		public const int isc_lockmanerr = 335544432;
		public const int isc_journerr = 335544433;
		public const int isc_keytoobig = 335544434;
		public const int isc_nullsegkey = 335544435;
		public const int isc_sqlerr = 335544436;
		public const int isc_wrodynver = 335544437;
		public const int isc_funnotdef = 335544438;
		public const int isc_funmismat = 335544439;
		public const int isc_bad_msg_vec = 335544440;
		public const int isc_bad_detach = 335544441;
		public const int isc_noargacc_read = 335544442;
		public const int isc_noargacc_write = 335544443;
		public const int isc_read_only = 335544444;
		public const int isc_ext_err = 335544445;
		public const int isc_non_updatable = 335544446;
		public const int isc_no_rollback = 335544447;
		public const int isc_bad_sec_info = 335544448;
		public const int isc_invalid_sec_info = 335544449;
		public const int isc_misc_interpreted = 335544450;
		public const int isc_update_conflict = 335544451;
		public const int isc_unlicensed = 335544452;
		public const int isc_obj_in_use = 335544453;
		public const int isc_nofilter = 335544454;
		public const int isc_shadow_accessed = 335544455;
		public const int isc_invalid_sdl = 335544456;
		public const int isc_out_of_bounds = 335544457;
		public const int isc_invalid_dimension = 335544458;
		public const int isc_rec_in_limbo = 335544459;
		public const int isc_shadow_missing = 335544460;
		public const int isc_cant_validate = 335544461;
		public const int isc_cant_start_journal = 335544462;
		public const int isc_gennotdef = 335544463;
		public const int isc_cant_start_logging = 335544464;
		public const int isc_bad_segstr_type = 335544465;
		public const int isc_foreign_key = 335544466;
		public const int isc_high_minor = 335544467;
		public const int isc_tra_state = 335544468;
		public const int isc_trans_invalid = 335544469;
		public const int isc_buf_invalid = 335544470;
		public const int isc_indexnotdefined = 335544471;
		public const int isc_login = 335544472;
		public const int isc_invalid_bookmark = 335544473;
		public const int isc_bad_lock_level = 335544474;
		public const int isc_relation_lock = 335544475;
		public const int isc_record_lock = 335544476;
		public const int isc_max_idx = 335544477;
		public const int isc_jrn_enable = 335544478;
		public const int isc_old_failure = 335544479;
		public const int isc_old_in_progress = 335544480;
		public const int isc_old_no_space = 335544481;
		public const int isc_no_wal_no_jrn = 335544482;
		public const int isc_num_old_files = 335544483;
		public const int isc_wal_file_open = 335544484;
		public const int isc_bad_stmt_handle = 335544485;
		public const int isc_wal_failure = 335544486;
		public const int isc_walw_err = 335544487;
		public const int isc_logh_small = 335544488;
		public const int isc_logh_inv_version = 335544489;
		public const int isc_logh_open_flag = 335544490;
		public const int isc_logh_open_flag2 = 335544491;
		public const int isc_logh_diff_dbname = 335544492;
		public const int isc_logf_unexpected_eof = 335544493;
		public const int isc_logr_incomplete = 335544494;
		public const int isc_logr_header_small = 335544495;
		public const int isc_logb_small = 335544496;
		public const int isc_wal_illegal_attach = 335544497;
		public const int isc_wal_invalid_wpb = 335544498;
		public const int isc_wal_err_rollover = 335544499;
		public const int isc_no_wal = 335544500;
		public const int isc_drop_wal = 335544501;
		public const int isc_stream_not_defined = 335544502;
		public const int isc_wal_subsys_error = 335544503;
		public const int isc_wal_subsys_corrupt = 335544504;
		public const int isc_no_archive = 335544505;
		public const int isc_shutinprog = 335544506;
		public const int isc_range_in_use = 335544507;
		public const int isc_range_not_found = 335544508;
		public const int isc_charset_not_found = 335544509;
		public const int isc_lock_timeout = 335544510;
		public const int isc_prcnotdef = 335544511;
		public const int isc_prcmismat = 335544512;
		public const int isc_wal_bugcheck = 335544513;
		public const int isc_wal_cant_expand = 335544514;
		public const int isc_codnotdef = 335544515;
		public const int isc_xcpnotdef = 335544516;
		public const int isc_except = 335544517;
		public const int isc_cache_restart = 335544518;
		public const int isc_bad_lock_handle = 335544519;
		public const int isc_jrn_present = 335544520;
		public const int isc_wal_err_rollover2 = 335544521;
		public const int isc_wal_err_logwrite = 335544522;
		public const int isc_wal_err_jrn_comm = 335544523;
		public const int isc_wal_err_expansion = 335544524;
		public const int isc_wal_err_setup = 335544525;
		public const int isc_wal_err_ww_sync = 335544526;
		public const int isc_wal_err_ww_start = 335544527;
		public const int isc_shutdown = 335544528;
		public const int isc_existing_priv_mod = 335544529;
		public const int isc_primary_key_ref = 335544530;
		public const int isc_primary_key_notnull = 335544531;
		public const int isc_ref_cnstrnt_notfound = 335544532;
		public const int isc_foreign_key_notfound = 335544533;
		public const int isc_ref_cnstrnt_update = 335544534;
		public const int isc_check_cnstrnt_update = 335544535;
		public const int isc_check_cnstrnt_del = 335544536;
		public const int isc_integ_index_seg_del = 335544537;
		public const int isc_integ_index_seg_mod = 335544538;
		public const int isc_integ_index_del = 335544539;
		public const int isc_integ_index_mod = 335544540;
		public const int isc_check_trig_del = 335544541;
		public const int isc_check_trig_update = 335544542;
		public const int isc_cnstrnt_fld_del = 335544543;
		public const int isc_cnstrnt_fld_rename = 335544544;
		public const int isc_rel_cnstrnt_update = 335544545;
		public const int isc_constaint_on_view = 335544546;
		public const int isc_invld_cnstrnt_type = 335544547;
		public const int isc_primary_key_exists = 335544548;
		public const int isc_systrig_update = 335544549;
		public const int isc_not_rel_owner = 335544550;
		public const int isc_grant_obj_notfound = 335544551;
		public const int isc_grant_fld_notfound = 335544552;
		public const int isc_grant_nopriv = 335544553;
		public const int isc_nonsql_security_rel = 335544554;
		public const int isc_nonsql_security_fld = 335544555;
		public const int isc_wal_cache_err = 335544556;
		public const int isc_shutfail = 335544557;
		public const int isc_check_constraint = 335544558;
		public const int isc_bad_svc_handle = 335544559;
		public const int isc_shutwarn = 335544560;
		public const int isc_wrospbver = 335544561;
		public const int isc_bad_spb_form = 335544562;
		public const int isc_svcnotdef = 335544563;
		public const int isc_no_jrn = 335544564;
		public const int isc_transliteration_failed = 335544565;
		public const int isc_start_cm_for_wal = 335544566;
		public const int isc_wal_ovflow_log_required = 335544567;
		public const int isc_text_subtype = 335544568;
		public const int isc_dsql_error = 335544569;
		public const int isc_dsql_command_err = 335544570;
		public const int isc_dsql_constant_err = 335544571;
		public const int isc_dsql_cursor_err = 335544572;
		public const int isc_dsql_datatype_err = 335544573;
		public const int isc_dsql_decl_err = 335544574;
		public const int isc_dsql_cursor_update_err = 335544575;
		public const int isc_dsql_cursor_open_err = 335544576;
		public const int isc_dsql_cursor_close_err = 335544577;
		public const int isc_dsql_field_err = 335544578;
		public const int isc_dsql_internal_err = 335544579;
		public const int isc_dsql_relation_err = 335544580;
		public const int isc_dsql_procedure_err = 335544581;
		public const int isc_dsql_request_err = 335544582;
		public const int isc_dsql_sqlda_err = 335544583;
		public const int isc_dsql_var_count_err = 335544584;
		public const int isc_dsql_stmt_handle = 335544585;
		public const int isc_dsql_function_err = 335544586;
		public const int isc_dsql_blob_err = 335544587;
		public const int isc_collation_not_found = 335544588;
		public const int isc_collation_not_for_charset = 335544589;
		public const int isc_dsql_dup_option = 335544590;
		public const int isc_dsql_tran_err = 335544591;
		public const int isc_dsql_invalid_array = 335544592;
		public const int isc_dsql_max_arr_dim_exceeded = 335544593;
		public const int isc_dsql_arr_range_error = 335544594;
		public const int isc_dsql_trigger_err = 335544595;
		public const int isc_dsql_subselect_err = 335544596;
		public const int isc_dsql_crdb_prepare_err = 335544597;
		public const int isc_specify_field_err = 335544598;
		public const int isc_num_field_err = 335544599;
		public const int isc_col_name_err = 335544600;
		public const int isc_where_err = 335544601;
		public const int isc_table_view_err = 335544602;
		public const int isc_distinct_err = 335544603;
		public const int isc_key_field_count_err = 335544604;
		public const int isc_subquery_err = 335544605;
		public const int isc_expression_eval_err = 335544606;
		public const int isc_node_err = 335544607;
		public const int isc_command_end_err = 335544608;
		public const int isc_index_name = 335544609;
		public const int isc_exception_name = 335544610;
		public const int isc_field_name = 335544611;
		public const int isc_token_err = 335544612;
		public const int isc_union_err = 335544613;
		public const int isc_dsql_construct_err = 335544614;
		public const int isc_field_aggregate_err = 335544615;
		public const int isc_field_ref_err = 335544616;
		public const int isc_order_by_err = 335544617;
		public const int isc_return_mode_err = 335544618;
		public const int isc_extern_func_err = 335544619;
		public const int isc_alias_conflict_err = 335544620;
		public const int isc_procedure_conflict_error = 335544621;
		public const int isc_relation_conflict_err = 335544622;
		public const int isc_dsql_domain_err = 335544623;
		public const int isc_idx_seg_err = 335544624;
		public const int isc_node_name_err = 335544625;
		public const int isc_table_name = 335544626;
		public const int isc_proc_name = 335544627;
		public const int isc_idx_create_err = 335544628;
		public const int isc_wal_shadow_err = 335544629;
		public const int isc_dependency = 335544630;
		public const int isc_idx_key_err = 335544631;
		public const int isc_dsql_file_length_err = 335544632;
		public const int isc_dsql_shadow_number_err = 335544633;
		public const int isc_dsql_token_unk_err = 335544634;
		public const int isc_dsql_no_relation_alias = 335544635;
		public const int isc_indexname = 335544636;
		public const int isc_no_stream_plan = 335544637;
		public const int isc_stream_twice = 335544638;
		public const int isc_stream_not_found = 335544639;
		public const int isc_collation_requires_text = 335544640;
		public const int isc_dsql_domain_not_found = 335544641;
		public const int isc_index_unused = 335544642;
		public const int isc_dsql_self_join = 335544643;
		public const int isc_stream_bof = 335544644;
		public const int isc_stream_crack = 335544645;
		public const int isc_db_or_file_exists = 335544646;
		public const int isc_invalid_operator = 335544647;
		public const int isc_conn_lost = 335544648;
		public const int isc_bad_checksum = 335544649;
		public const int isc_page_type_err = 335544650;
		public const int isc_ext_readonly_err = 335544651;
		public const int isc_sing_select_err = 335544652;
		public const int isc_psw_attach = 335544653;
		public const int isc_psw_start_trans = 335544654;
		public const int isc_invalid_direction = 335544655;
		public const int isc_dsql_var_conflict = 335544656;
		public const int isc_dsql_no_blob_array = 335544657;
		public const int isc_dsql_base_table = 335544658;
		public const int isc_duplicate_base_table = 335544659;
		public const int isc_view_alias = 335544660;
		public const int isc_index_root_page_full = 335544661;
		public const int isc_dsql_blob_type_unknown = 335544662;
		public const int isc_req_max_clones_exceeded = 335544663;
		public const int isc_dsql_duplicate_spec = 335544664;
		public const int isc_unique_key_violation = 335544665;
		public const int isc_srvr_version_too_old = 335544666;
		public const int isc_drdb_completed_with_errs = 335544667;
		public const int isc_dsql_procedure_use_err = 335544668;
		public const int isc_dsql_count_mismatch = 335544669;
		public const int isc_blob_idx_err = 335544670;
		public const int isc_array_idx_err = 335544671;
		public const int isc_key_field_err = 335544672;
		public const int isc_no_delete = 335544673;
		public const int isc_del_last_field = 335544674;
		public const int isc_sort_err = 335544675;
		public const int isc_sort_mem_err = 335544676;
		public const int isc_version_err = 335544677;
		public const int isc_inval_key_posn = 335544678;
		public const int isc_no_segments_err = 335544679;
		public const int isc_crrp_data_err = 335544680;
		public const int isc_rec_size_err = 335544681;
		public const int isc_dsql_field_ref = 335544682;
		public const int isc_req_depth_exceeded = 335544683;
		public const int isc_no_field_access = 335544684;
		public const int isc_no_dbkey = 335544685;
		public const int isc_jrn_format_err = 335544686;
		public const int isc_jrn_file_full = 335544687;
		public const int isc_dsql_open_cursor_request = 335544688;
		public const int isc_ib_error = 335544689;
		public const int isc_cache_redef = 335544690;
		public const int isc_cache_too_small = 335544691;
		public const int isc_log_redef = 335544692;
		public const int isc_log_too_small = 335544693;
		public const int isc_partition_too_small = 335544694;
		public const int isc_partition_not_supp = 335544695;
		public const int isc_log_length_spec = 335544696;
		public const int isc_precision_err = 335544697;
		public const int isc_scale_nogt = 335544698;
		public const int isc_expec_int = 335544699;
		public const int isc_expec_long = 335544700;
		public const int isc_expec_uint = 335544701;
		public const int isc_like_escape_invalid = 335544702;
		public const int isc_svcnoexe = 335544703;
		public const int isc_net_lookup_err = 335544704;
		public const int isc_service_unknown = 335544705;
		public const int isc_host_unknown = 335544706;
		public const int isc_grant_nopriv_on_base = 335544707;
		public const int isc_dyn_fld_ambiguous = 335544708;
		public const int isc_dsql_agg_ref_err = 335544709;
		public const int isc_complex_view = 335544710;
		public const int isc_unprepared_stmt = 335544711;
		public const int isc_expec_positive = 335544712;
		public const int isc_dsql_sqlda_value_err = 335544713;
		public const int isc_invalid_array_id = 335544714;
		public const int isc_extfile_uns_op = 335544715;
		public const int isc_svc_in_use = 335544716;
		public const int isc_err_stack_limit = 335544717;
		public const int isc_invalid_key = 335544718;
		public const int isc_net_init_error = 335544719;
		public const int isc_loadlib_failure = 335544720;
		public const int isc_network_error = 335544721;
		public const int isc_net_connect_err = 335544722;
		public const int isc_net_connect_listen_err = 335544723;
		public const int isc_net_event_connect_err = 335544724;
		public const int isc_net_event_listen_err = 335544725;
		public const int isc_net_read_err = 335544726;
		public const int isc_net_write_err = 335544727;
		public const int isc_integ_index_deactivate = 335544728;
		public const int isc_integ_deactivate_primary = 335544729;
		public const int isc_cse_not_supported = 335544730;
		public const int isc_tra_must_sweep = 335544731;
		public const int isc_unsupported_network_drive = 335544732;
		public const int isc_io_create_err = 335544733;
		public const int isc_io_open_err = 335544734;
		public const int isc_io_close_err = 335544735;
		public const int isc_io_read_err = 335544736;
		public const int isc_io_write_err = 335544737;
		public const int isc_io_delete_err = 335544738;
		public const int isc_io_access_err = 335544739;
		public const int isc_udf_exception = 335544740;
		public const int isc_lost_db_connection = 335544741;
		public const int isc_no_write_user_priv = 335544742;
		public const int isc_token_too_long = 335544743;
		public const int isc_max_att_exceeded = 335544744;
		public const int isc_login_same_as_role_name = 335544745;
		public const int isc_reftable_requires_pk = 335544746;
		public const int isc_usrname_too_long = 335544747;
		public const int isc_password_too_long = 335544748;
		public const int isc_usrname_required = 335544749;
		public const int isc_password_required = 335544750;
		public const int isc_bad_protocol = 335544751;
		public const int isc_dup_usrname_found = 335544752;
		public const int isc_usrname_not_found = 335544753;
		public const int isc_error_adding_sec_record = 335544754;
		public const int isc_error_modifying_sec_record = 335544755;
		public const int isc_error_deleting_sec_record = 335544756;
		public const int isc_error_updating_sec_db = 335544757;
		public const int isc_sort_rec_size_err = 335544758;
		public const int isc_bad_default_value = 335544759;
		public const int isc_invalid_clause = 335544760;
		public const int isc_too_many_handles = 335544761;
		public const int isc_optimizer_blk_exc = 335544762;
		public const int isc_invalid_string_constant = 335544763;
		public const int isc_transitional_date = 335544764;
		public const int isc_read_only_database = 335544765;
		public const int isc_must_be_dialect_2_and_up = 335544766;
		public const int isc_blob_filter_exception = 335544767;
		public const int isc_exception_access_violation = 335544768;
		public const int isc_exception_datatype_missalignment = 335544769;
		public const int isc_exception_array_bounds_exceeded = 335544770;
		public const int isc_exception_float_denormal_operand = 335544771;
		public const int isc_exception_float_divide_by_zero = 335544772;
		public const int isc_exception_float_inexact_result = 335544773;
		public const int isc_exception_float_invalid_operand = 335544774;
		public const int isc_exception_float_overflow = 335544775;
		public const int isc_exception_float_stack_check = 335544776;
		public const int isc_exception_float_underflow = 335544777;
		public const int isc_exception_int_divide_by_zero = 335544778;
		public const int isc_exception_int_overflow = 335544779;
		public const int isc_exception_unknown = 335544780;
		public const int isc_exception_stack_overflow = 335544781;
		public const int isc_exception_sigsegv = 335544782;
		public const int isc_exception_sigill = 335544783;
		public const int isc_exception_sigbus = 335544784;
		public const int isc_exception_sigfpe = 335544785;
		public const int isc_ext_file_delete = 335544786;
		public const int isc_ext_file_modify = 335544787;
		public const int isc_adm_task_denied = 335544788;
		public const int isc_extract_input_mismatch = 335544789;
		public const int isc_insufficient_svc_privileges = 335544790;
		public const int isc_file_in_use = 335544791;
		public const int isc_service_att_err = 335544792;
		public const int isc_ddl_not_allowed_by_db_sql_dial = 335544793;
		public const int isc_cancelled = 335544794;
		public const int isc_unexp_spb_form = 335544795;
		public const int isc_sql_dialect_datatype_unsupport = 335544796;
		public const int isc_svcnouser = 335544797;
		public const int isc_depend_on_uncommitted_rel = 335544798;
		public const int isc_svc_name_missing = 335544799;
		public const int isc_too_many_contexts = 335544800;
		public const int isc_datype_notsup = 335544801;
		public const int isc_dialect_reset_warning = 335544802;
		public const int isc_dialect_not_changed = 335544803;
		public const int isc_database_create_failed = 335544804;
		public const int isc_inv_dialect_specified = 335544805;
		public const int isc_valid_db_dialects = 335544806;
		public const int isc_sqlwarn = 335544807;
		public const int isc_dtype_renamed = 335544808;
		public const int isc_extern_func_dir_error = 335544809;
		public const int isc_date_range_exceeded = 335544810;
		public const int isc_inv_client_dialect_specified = 335544811;
		public const int isc_valid_client_dialects = 335544812;
		public const int isc_optimizer_between_err = 335544813;
		public const int isc_service_not_supported = 335544814;
		public const int isc_generator_name = 335544815;
		public const int isc_udf_name = 335544816;
		public const int isc_bad_limit_param = 335544817;
		public const int isc_bad_skip_param = 335544818;
		public const int isc_io_32bit_exceeded_err = 335544819;
		public const int isc_invalid_savepoint = 335544820;
		public const int isc_dsql_column_pos_err = 335544821;
		public const int isc_dsql_agg_where_err = 335544822;
		public const int isc_dsql_agg_group_err = 335544823;
		public const int isc_dsql_agg_column_err = 335544824;
		public const int isc_dsql_agg_having_err = 335544825;
		public const int isc_dsql_agg_nested_err = 335544826;
		public const int isc_exec_sql_invalid_arg = 335544827;
		public const int isc_exec_sql_invalid_req = 335544828;
		public const int isc_exec_sql_invalid_var = 335544829;
		public const int isc_exec_sql_max_call_exceeded = 335544830;
		public const int isc_conf_access_denied = 335544831;
		public const int isc_gfix_db_name = 335740929;
		public const int isc_gfix_invalid_sw = 335740930;
		public const int isc_gfix_incmp_sw = 335740932;
		public const int isc_gfix_replay_req = 335740933;
		public const int isc_gfix_pgbuf_req = 335740934;
		public const int isc_gfix_val_req = 335740935;
		public const int isc_gfix_pval_req = 335740936;
		public const int isc_gfix_trn_req = 335740937;
		public const int isc_gfix_full_req = 335740940;
		public const int isc_gfix_usrname_req = 335740941;
		public const int isc_gfix_pass_req = 335740942;
		public const int isc_gfix_subs_name = 335740943;
		public const int isc_gfix_wal_req = 335740944;
		public const int isc_gfix_sec_req = 335740945;
		public const int isc_gfix_nval_req = 335740946;
		public const int isc_gfix_type_shut = 335740947;
		public const int isc_gfix_retry = 335740948;
		public const int isc_gfix_retry_db = 335740951;
		public const int isc_gfix_exceed_max = 335740991;
		public const int isc_gfix_corrupt_pool = 335740992;
		public const int isc_gfix_mem_exhausted = 335740993;
		public const int isc_gfix_bad_pool = 335740994;
		public const int isc_gfix_trn_not_valid = 335740995;
		public const int isc_gfix_unexp_eoi = 335741012;
		public const int isc_gfix_recon_fail = 335741018;
		public const int isc_gfix_trn_unknown = 335741036;
		public const int isc_gfix_mode_req = 335741038;
		public const int isc_gfix_opt_SQL_dialect = 335741039;
		public const int isc_dsql_dbkey_from_non_table = 336003074;
		public const int isc_dsql_transitional_numeric = 336003075;
		public const int isc_dsql_dialect_warning_expr = 336003076;
		public const int isc_sql_db_dialect_dtype_unsupport = 336003077;
		public const int isc_isc_sql_dialect_conflict_num = 336003079;
		public const int isc_dsql_warning_number_ambiguous = 336003080;
		public const int isc_dsql_warning_number_ambiguous1 = 336003081;
		public const int isc_dsql_warn_precision_ambiguous = 336003082;
		public const int isc_dsql_warn_precision_ambiguous1 = 336003083;
		public const int isc_dsql_warn_precision_ambiguous2 = 336003084;
		public const int isc_dyn_role_does_not_exist = 336068796;
		public const int isc_dyn_no_grant_admin_opt = 336068797;
		public const int isc_dyn_user_not_role_member = 336068798;
		public const int isc_dyn_delete_role_failed = 336068799;
		public const int isc_dyn_grant_role_to_user = 336068800;
		public const int isc_dyn_inv_sql_role_name = 336068801;
		public const int isc_dyn_dup_sql_role = 336068802;
		public const int isc_dyn_kywd_spec_for_role = 336068803;
		public const int isc_dyn_roles_not_supported = 336068804;
		public const int isc_dyn_domain_name_exists = 336068812;
		public const int isc_dyn_field_name_exists = 336068813;
		public const int isc_dyn_dependency_exists = 336068814;
		public const int isc_dyn_dtype_invalid = 336068815;
		public const int isc_dyn_char_fld_too_small = 336068816;
		public const int isc_dyn_invalid_dtype_conversion = 336068817;
		public const int isc_dyn_dtype_conv_invalid = 336068818;
		public const int isc_gbak_unknown_switch = 336330753;
		public const int isc_gbak_page_size_missing = 336330754;
		public const int isc_gbak_page_size_toobig = 336330755;
		public const int isc_gbak_redir_ouput_missing = 336330756;
		public const int isc_gbak_switches_conflict = 336330757;
		public const int isc_gbak_unknown_device = 336330758;
		public const int isc_gbak_no_protection = 336330759;
		public const int isc_gbak_page_size_not_allowed = 336330760;
		public const int isc_gbak_multi_source_dest = 336330761;
		public const int isc_gbak_filename_missing = 336330762;
		public const int isc_gbak_dup_inout_names = 336330763;
		public const int isc_gbak_inv_page_size = 336330764;
		public const int isc_gbak_db_specified = 336330765;
		public const int isc_gbak_db_exists = 336330766;
		public const int isc_gbak_unk_device = 336330767;
		public const int isc_gbak_blob_info_failed = 336330772;
		public const int isc_gbak_unk_blob_item = 336330773;
		public const int isc_gbak_get_seg_failed = 336330774;
		public const int isc_gbak_close_blob_failed = 336330775;
		public const int isc_gbak_open_blob_failed = 336330776;
		public const int isc_gbak_put_blr_gen_id_failed = 336330777;
		public const int isc_gbak_unk_type = 336330778;
		public const int isc_gbak_comp_req_failed = 336330779;
		public const int isc_gbak_start_req_failed = 336330780;
		public const int isc_gbak_rec_failed = 336330781;
		public const int isc_gbak_rel_req_failed = 336330782;
		public const int isc_gbak_db_info_failed = 336330783;
		public const int isc_gbak_no_db_desc = 336330784;
		public const int isc_gbak_db_create_failed = 336330785;
		public const int isc_gbak_decomp_len_error = 336330786;
		public const int isc_gbak_tbl_missing = 336330787;
		public const int isc_gbak_blob_col_missing = 336330788;
		public const int isc_gbak_create_blob_failed = 336330789;
		public const int isc_gbak_put_seg_failed = 336330790;
		public const int isc_gbak_rec_len_exp = 336330791;
		public const int isc_gbak_inv_rec_len = 336330792;
		public const int isc_gbak_exp_data_type = 336330793;
		public const int isc_gbak_gen_id_failed = 336330794;
		public const int isc_gbak_unk_rec_type = 336330795;
		public const int isc_gbak_inv_bkup_ver = 336330796;
		public const int isc_gbak_missing_bkup_desc = 336330797;
		public const int isc_gbak_string_trunc = 336330798;
		public const int isc_gbak_cant_rest_record = 336330799;
		public const int isc_gbak_send_failed = 336330800;
		public const int isc_gbak_no_tbl_name = 336330801;
		public const int isc_gbak_unexp_eof = 336330802;
		public const int isc_gbak_db_format_too_old = 336330803;
		public const int isc_gbak_inv_array_dim = 336330804;
		public const int isc_gbak_xdr_len_expected = 336330807;
		public const int isc_gbak_open_bkup_error = 336330817;
		public const int isc_gbak_open_error = 336330818;
		public const int isc_gbak_missing_block_fac = 336330934;
		public const int isc_gbak_inv_block_fac = 336330935;
		public const int isc_gbak_block_fac_specified = 336330936;
		public const int isc_gbak_missing_username = 336330940;
		public const int isc_gbak_missing_password = 336330941;
		public const int isc_gbak_missing_skipped_bytes = 336330952;
		public const int isc_gbak_inv_skipped_bytes = 336330953;
		public const int isc_gbak_err_restore_charset = 336330965;
		public const int isc_gbak_err_restore_collation = 336330967;
		public const int isc_gbak_read_error = 336330972;
		public const int isc_gbak_write_error = 336330973;
		public const int isc_gbak_db_in_use = 336330985;
		public const int isc_gbak_sysmemex = 336330990;
		public const int isc_gbak_restore_role_failed = 336331002;
		public const int isc_gbak_role_op_missing = 336331005;
		public const int isc_gbak_page_buffers_missing = 336331010;
		public const int isc_gbak_page_buffers_wrong_param = 336331011;
		public const int isc_gbak_page_buffers_restore = 336331012;
		public const int isc_gbak_inv_size = 336331014;
		public const int isc_gbak_file_outof_sequence = 336331015;
		public const int isc_gbak_join_file_missing = 336331016;
		public const int isc_gbak_stdin_not_supptd = 336331017;
		public const int isc_gbak_stdout_not_supptd = 336331018;
		public const int isc_gbak_bkup_corrupt = 336331019;
		public const int isc_gbak_unk_db_file_spec = 336331020;
		public const int isc_gbak_hdr_write_failed = 336331021;
		public const int isc_gbak_disk_space_ex = 336331022;
		public const int isc_gbak_size_lt_min = 336331023;
		public const int isc_gbak_svc_name_missing = 336331025;
		public const int isc_gbak_not_ownr = 336331026;
		public const int isc_gbak_mode_req = 336331031;
		public const int isc_gsec_cant_open_db = 336723983;
		public const int isc_gsec_switches_error = 336723984;
		public const int isc_gsec_no_op_spec = 336723985;
		public const int isc_gsec_no_usr_name = 336723986;
		public const int isc_gsec_err_add = 336723987;
		public const int isc_gsec_err_modify = 336723988;
		public const int isc_gsec_err_find_mod = 336723989;
		public const int isc_gsec_err_rec_not_found = 336723990;
		public const int isc_gsec_err_delete = 336723991;
		public const int isc_gsec_err_find_del = 336723992;
		public const int isc_gsec_err_find_disp = 336723996;
		public const int isc_gsec_inv_param = 336723997;
		public const int isc_gsec_op_specified = 336723998;
		public const int isc_gsec_pw_specified = 336723999;
		public const int isc_gsec_uid_specified = 336724000;
		public const int isc_gsec_gid_specified = 336724001;
		public const int isc_gsec_proj_specified = 336724002;
		public const int isc_gsec_org_specified = 336724003;
		public const int isc_gsec_fname_specified = 336724004;
		public const int isc_gsec_mname_specified = 336724005;
		public const int isc_gsec_lname_specified = 336724006;
		public const int isc_gsec_inv_switch = 336724008;
		public const int isc_gsec_amb_switch = 336724009;
		public const int isc_gsec_no_op_specified = 336724010;
		public const int isc_gsec_params_not_allowed = 336724011;
		public const int isc_gsec_incompat_switch = 336724012;
		public const int isc_gsec_inv_username = 336724044;
		public const int isc_gsec_inv_pw_length = 336724045;
		public const int isc_gsec_db_specified = 336724046;
		public const int isc_gsec_db_admin_specified = 336724047;
		public const int isc_gsec_db_admin_pw_specified = 336724048;
		public const int isc_gsec_sql_role_specified = 336724049;
		public const int isc_license_no_file = 336789504;
		public const int isc_license_op_specified = 336789523;
		public const int isc_license_op_missing = 336789524;
		public const int isc_license_inv_switch = 336789525;
		public const int isc_license_inv_switch_combo = 336789526;
		public const int isc_license_inv_op_combo = 336789527;
		public const int isc_license_amb_switch = 336789528;
		public const int isc_license_inv_parameter = 336789529;
		public const int isc_license_param_specified = 336789530;
		public const int isc_license_param_req = 336789531;
		public const int isc_license_syntx_error = 336789532;
		public const int isc_license_dup_id = 336789534;
		public const int isc_license_inv_id_key = 336789535;
		public const int isc_license_err_remove = 336789536;
		public const int isc_license_err_update = 336789537;
		public const int isc_license_err_convert = 336789538;
		public const int isc_license_err_unk = 336789539;
		public const int isc_license_svc_err_add = 336789540;
		public const int isc_license_svc_err_remove = 336789541;
		public const int isc_license_eval_exists = 336789563;
		public const int isc_gstat_unknown_switch = 336920577;
		public const int isc_gstat_retry = 336920578;
		public const int isc_gstat_wrong_ods = 336920579;
		public const int isc_gstat_unexpected_eof = 336920580;
		public const int isc_gstat_open_err = 336920605;
		public const int isc_gstat_read_err = 336920606;
		public const int isc_gstat_sysmemex = 336920607;
		public const int isc_err_max = 689;

		#endregion

		#region BLR	Codes

		public const int blr_version5 = 5;
		public const int blr_begin = 2;
		public const int blr_message = 4;
		public const int blr_eoc = 76;
		public const int blr_end = 255;	/* note: defined as -1 in gds.h	*/

		public const int blr_text = 14;
		public const int blr_text2 = 15;
		public const int blr_short = 7;
		public const int blr_long = 8;
		public const int blr_quad = 9;
		public const int blr_int64 = 16;
		public const int blr_float = 10;
		public const int blr_double = 27;
		public const int blr_d_float = 11;
		public const int blr_timestamp = 35;
		public const int blr_varying = 37;
		public const int blr_varying2 = 38;
		public const int blr_blob = 261;
		public const int blr_cstring = 40;
		public const int blr_cstring2 = 41;
		public const int blr_blob_id = 45;
		public const int blr_sql_date = 12;
		public const int blr_sql_time = 13;

		#endregion

		#region DataType Definitions

		public const int SQL_TEXT = 452;
		public const int SQL_VARYING = 448;
		public const int SQL_SHORT = 500;
		public const int SQL_LONG = 496;
		public const int SQL_FLOAT = 482;
		public const int SQL_DOUBLE = 480;
		public const int SQL_D_FLOAT = 530;
		public const int SQL_TIMESTAMP = 510;
		public const int SQL_BLOB = 520;
		public const int SQL_ARRAY = 540;
		public const int SQL_QUAD = 550;
		public const int SQL_TYPE_TIME = 560;
		public const int SQL_TYPE_DATE = 570;
		public const int SQL_INT64 = 580;

		// Historical alias	for	pre	V6 applications
		public const int SQL_DATE = SQL_TIMESTAMP;

		#endregion
	}
}
