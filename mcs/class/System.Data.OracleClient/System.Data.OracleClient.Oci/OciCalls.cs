//
// OciCalls.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
//
// Authors: Joerg Rosenkranz <joergr@voelcker.com>
//          Daniel Morgan <monodanmorg@yahoo.com>
//
// Copyright (C) Joerg Rosenkranz, 2004
// Copyright (C) Daniel Morgan, 2005, 2009
//
// Licensed under the MIT/X11 License.
//

//#define ORACLE_DATA_ACCESS

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient.Oci
{
	internal sealed class OciCalls
	{
#if TRACE
		private static bool traceOci;

		static OciCalls()
		{
			string env = Environment.GetEnvironmentVariable("OCI_TRACE");

			traceOci = (env != null && env.Length > 0);
		}
#endif

		private OciCalls ()
		{}

		#region OCI native calls

		private sealed class OciNativeCalls
		{
			private OciNativeCalls ()
			{}

			[DllImport ("oci", EntryPoint = "OCIAttrSet")]
			internal static extern int OCIAttrSet (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				IntPtr attributep,
				uint size,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrSet")]
			internal static extern int OCIAttrSetString (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				string attributep,
				uint size,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

#if ORACLE_DATA_ACCESS
			[DllImport ("oci", EntryPoint = "OCIPasswordChange")]
			internal static extern int OCIPasswordChange (IntPtr svchp, 
				IntPtr errhp,
				byte [] user_name, 
				[MarshalAs (UnmanagedType.U4)] int usernm_len,
				byte [] opasswd,
				[MarshalAs (UnmanagedType.U4)] int opasswd_len,
				byte [] npasswd,
				[MarshalAs (UnmanagedType.U4)] int npasswd_len,
				[MarshalAs (UnmanagedType.U4)] uint mode);
#endif

			[DllImport ("oci")]
			internal static extern int OCIErrorGet (IntPtr hndlp,
				uint recordno,
				IntPtr sqlstate,
				out int errcodep,
				IntPtr bufp,
				uint bufsize,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type);

			[DllImport ("oci", EntryPoint = "OCIBindByName")]
			internal static extern int OCIBindByName (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				string placeholder,
				int placeh_len,
				IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci", EntryPoint = "OCIBindByName")]
			internal static extern int OCIBindByNameRef (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				string placeholder,
				int placeh_len,
				ref IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci", EntryPoint = "OCIBindByName")]
			internal static extern int OCIBindByNameBytes (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				string placeholder,
				int placeh_len,
				byte[] valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci", EntryPoint = "OCIBindByPos")]
			internal static extern int OCIBindByPos (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				uint position,
				IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci", EntryPoint = "OCIBindByPos")]
			internal static extern int OCIBindByPosBytes (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				uint position,
				byte[] valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci", EntryPoint = "OCIBindByPos")]
			internal static extern int OCIBindByPosRef (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				uint position,
				ref IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIDateTimeFromText (IntPtr hndl,
				IntPtr errhp, [In][Out] byte[] date_str, uint dstr_length,
				[In][Out] byte[] fmt, uint fmt_length,
				[In][Out] byte[] lang_name, uint lang_length, IntPtr datetime);

			[DllImport ("oci")]
			internal static extern int OCIDefineByPos (IntPtr stmtp,
				out IntPtr defnpp,
				IntPtr errhp,
				[MarshalAs (UnmanagedType.U4)] int position,
				IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U4)] OciDataType dty,
				ref short indp,
				ref short rlenp,
				IntPtr rcodep,
				uint mode);

			[DllImport ("oci", EntryPoint="OCIDefineByPos")]
			internal static extern int OCIDefineByPosPtr (IntPtr stmtp,
				out IntPtr defnpp,
				IntPtr errhp,
				[MarshalAs (UnmanagedType.U4)] int position,
				ref IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U4)] OciDataType dty,
				ref short indp,
				ref short rlenp,
				IntPtr rcodep,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIDescriptorFree (IntPtr hndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type);

			[DllImport ("oci")]
			internal static extern int OCIEnvCreate (out IntPtr envhpp,
				[MarshalAs (UnmanagedType.U4)] OciEnvironmentMode mode,
				IntPtr ctxp,
				IntPtr malocfp,
				IntPtr ralocfp,
				IntPtr mfreep,
				int xtramem_sz,
				IntPtr usrmempp);

			[DllImport ("oci")]
			internal static extern int OCIAttrGet (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out IntPtr attributep,
				out int sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrGet")]
			internal static extern int OCIAttrGetSByte (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out sbyte attributep,
				IntPtr sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrGet")]
			internal static extern int OCIAttrGetByte (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out byte attributep,
				IntPtr sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrGet")]
			internal static extern int OCIAttrGetUInt16 (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out ushort attributep,
				IntPtr sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrGet")]
			internal static extern int OCIAttrGetInt32 (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out int attributep,
				IntPtr sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci", EntryPoint = "OCIAttrGet")]
			internal static extern int OCIAttrGetIntPtr (IntPtr trgthndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType trghndltyp,
				out IntPtr attributep,
				IntPtr sizep,
				[MarshalAs (UnmanagedType.U4)] OciAttributeType attrtype,
				IntPtr errhp);

			[DllImport ("oci")]
			internal static extern int OCIDescriptorAlloc (IntPtr parenth,
				out IntPtr hndlpp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type,
				int xtramem_sz,
				IntPtr usrmempp);

			[DllImport ("oci")]
			internal static extern int OCIHandleAlloc (IntPtr parenth,
				out IntPtr descpp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type,
				int xtramem_sz,
				IntPtr usrmempp);

			[DllImport ("oci")]
			internal static extern int OCIHandleFree (IntPtr hndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type);

			[DllImport ("oci")]
			internal static extern int OCILobClose (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp);

			[DllImport ("oci")]
			internal static extern int OCILobCopy (IntPtr svchp,
				IntPtr errhp,
				IntPtr dst_locp,
				IntPtr src_locp,
				uint amount,
				uint dst_offset,
				uint src_offset);

			[DllImport ("oci")]
			internal static extern int OCILobErase (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				ref uint amount,
				uint offset);

			[DllImport ("oci")]
			internal static extern int OCILobGetChunkSize (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				out uint chunk_size);

			[DllImport ("oci")]
			internal static extern int OCILobGetLength (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				out uint lenp);

			[DllImport ("oci")]
			internal static extern int OCILobOpen (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				byte mode);

			[DllImport ("oci")]
			internal static extern int OCILobRead (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				ref uint amtp,
				uint offset,
				byte[] bufp,
				uint bufl,
				IntPtr ctxp,
				IntPtr cbfp,
				ushort csid,
				byte csfrm);

			[DllImport ("oci")]
			internal static extern int OCILobTrim (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				uint newlen);

			[DllImport ("oci")]
			internal static extern int OCILobWrite (IntPtr svchp,
				IntPtr errhp,
				IntPtr locp,
				ref uint amtp,
				uint offset,
				byte[] bufp,
				uint bufl,
				byte piece,
				IntPtr ctxp,
				IntPtr cbfp,
				ushort csid,
				byte csfrm);

			[DllImport ("oci")]
			internal static extern int OCILobCharSetForm (IntPtr svchp, 
				IntPtr errhp,
				IntPtr locp,
				out byte csfrm);
			
			[DllImport ("oci")]
			internal static extern int OCINlsGetInfo (IntPtr hndl,
				IntPtr errhp,
				[In][Out] byte[] bufp,
				uint buflen,
				ushort item);

			[DllImport ("oci")]
			internal static extern int OCIServerAttach (IntPtr srvhp,
				IntPtr errhp,
				string dblink,
				[MarshalAs (UnmanagedType.U4)] int dblink_len,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIServerDetach (IntPtr srvhp,
				IntPtr errhp,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIServerVersion (IntPtr hndlp,
				IntPtr errhp,
				[In][Out] byte[] bufp,
				uint bufsz,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type);

			[DllImport ("oci")]
			internal static extern int OCISessionBegin (IntPtr svchp,
				IntPtr errhp,
				IntPtr usrhp,
				[MarshalAs (UnmanagedType.U4)] OciCredentialType credt,
				[MarshalAs (UnmanagedType.U4)] OciSessionMode mode);

			[DllImport ("oci")]
			internal static extern int OCISessionEnd (IntPtr svchp,
				IntPtr errhp,
				IntPtr usrhp,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIParamGet (IntPtr hndlp,
				[MarshalAs (UnmanagedType.U4)] OciHandleType htype,
				IntPtr errhp,
				out IntPtr parmdpp,
				[MarshalAs (UnmanagedType.U4)] int pos);

			[DllImport ("oci")]
			internal static extern int OCIStmtExecute (IntPtr svchp,
				IntPtr stmthp,
				IntPtr errhp,
				[MarshalAs (UnmanagedType.U4)] uint iters,
				uint rowoff,
				IntPtr snap_in,
				IntPtr snap_out,
				[MarshalAs (UnmanagedType.U4)] OciExecuteMode mode);

			[DllImport ("oci")]
			internal static extern int OCIStmtFetch (IntPtr stmtp,
				IntPtr errhp,
				uint nrows,
				ushort orientation,
				uint mode);


			[DllImport ("oci")]
			internal static extern int OCIStmtPrepare (IntPtr stmthp,
				IntPtr errhp,
				byte [] stmt,
				[MarshalAs (UnmanagedType.U4)] int stmt_length,
				[MarshalAs (UnmanagedType.U4)] OciStatementLanguage language,
				[MarshalAs (UnmanagedType.U4)] OciStatementMode mode);

			[DllImport ("oci")]
			internal static extern int OCITransCommit (IntPtr svchp,
				IntPtr errhp,
				uint flags);

			[DllImport ("oci")]
			internal static extern int OCITransRollback (IntPtr svchp,
				IntPtr errhp,
				uint flags);

			[DllImport ("oci")]
			internal static extern int OCITransStart (IntPtr svchp,
				IntPtr errhp,
				uint timeout,
				[MarshalAs (UnmanagedType.U4)] OciTransactionFlags flags);

			[DllImport ("oci")]
			internal static extern int OCICharSetToUnicode (
				IntPtr svchp,
				[MarshalAs (UnmanagedType.LPWStr)] StringBuilder dst,
				[MarshalAs (UnmanagedType.SysUInt)] int dstlen,
				byte [] src,
				[MarshalAs (UnmanagedType.SysUInt)] int srclen,
				[MarshalAs (UnmanagedType.SysUInt)] out int rsize);

			[DllImport ("oci")]
			internal static extern int OCIUnicodeToCharSet (
				IntPtr svchp,
				byte [] dst,
				[MarshalAs (UnmanagedType.SysUInt)] int dstlen,
				[MarshalAs (UnmanagedType.LPWStr)] string src,
				[MarshalAs (UnmanagedType.SysUInt)] int srclen,
				[MarshalAs (UnmanagedType.SysUInt)] out int rsize);
		}

		#endregion

		#region OCI call wrappers

		internal static int OCIAttrSet (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			IntPtr attributep,
			uint size,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIAttrSet ({0}, {1})", trghndltyp, attrtype), "OCI");
			#endif
			return OciNativeCalls.OCIAttrSet (trgthndlp, trghndltyp, attributep, size, attrtype, errhp);
		}

		internal static int OCIAttrSetString (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			string attributep,
			uint size,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIAttrSetString ({0}, {1})", trghndltyp, attrtype), "OCI");
			#endif
			return OciNativeCalls.OCIAttrSetString (trgthndlp, trghndltyp, attributep, size, attrtype, errhp);
		}
#if ORACLE_DATA_ACCESS
		internal static int OCIPasswordChange (IntPtr svchp, IntPtr errhp,
				int usernm_len,
				byte [] opasswd,
				int opasswd_len,
				byte [] npasswd,
				int npasswd_len,
				uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIPasswordChange"), "OCI");
			#endif
			return OciNativeCalls.OCIPasswordChange (svchp, errhp, user_name, (uint) usernm_len, opasswd, (uint) opasswd_len, npasswd, (uint) npasswd_len, mode);
		}
#endif
		internal static int OCIErrorGet (IntPtr hndlp,
			uint recordno,
			IntPtr sqlstate,
			out int errcodep,
			IntPtr bufp,
			uint bufsize,
			OciHandleType type)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIErrorGet", "OCI");
			#endif
			return OciNativeCalls.OCIErrorGet (hndlp, recordno, sqlstate, out errcodep, bufp, bufsize, type);
		}

		internal static int OCIBindByName (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			string placeholder,
			int placeh_len,
			IntPtr valuep,
			int value_sz,
			OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByName", "OCI");
			#endif
			return OciNativeCalls.OCIBindByName (stmtp, out bindpp, errhp, placeholder, placeh_len, valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIBindByNameRef (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			string placeholder,
			int placeh_len,
			ref IntPtr valuep,
			int value_sz,
			OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByName", "OCI");
			#endif
			return OciNativeCalls.OCIBindByNameRef (stmtp, out bindpp, errhp, placeholder, placeh_len, ref valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIBindByNameBytes (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			string placeholder,
			int placeh_len,
			byte[] valuep,
			int value_sz,
			[MarshalAs (UnmanagedType.U2)] OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByName", "OCI");
			#endif
			return OciNativeCalls.OCIBindByNameBytes (stmtp, out bindpp, errhp, placeholder, placeh_len, valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIBindByPos (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			uint position,
			IntPtr valuep,
			int value_sz,
			[MarshalAs (UnmanagedType.U2)] OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByPos", "OCI");
			#endif
			return OciNativeCalls.OCIBindByPos (stmtp, out bindpp, errhp, position, valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIBindByPosRef (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			uint position,
			ref IntPtr valuep,
			int value_sz,
			[MarshalAs (UnmanagedType.U2)] OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByPos", "OCI");
			#endif
			return OciNativeCalls.OCIBindByPosRef (stmtp, out bindpp, errhp, position, ref valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIBindByPosBytes (IntPtr stmtp,
			out IntPtr bindpp,
			IntPtr errhp,
			uint position,
			byte[] valuep,
			int value_sz,
			[MarshalAs (UnmanagedType.U2)] OciDataType dty,
			ref short indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIBindByPos", "OCI");
			#endif
			return OciNativeCalls.OCIBindByPosBytes (stmtp, out bindpp, errhp, position, valuep,
				value_sz, dty, ref indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		[DllImport ("oci")]
		internal static extern void OCIDateTimeConstruct (IntPtr hndl,
			IntPtr err,
			IntPtr datetime,
			short year,
			byte month,
			byte day,
			byte hour,
			byte min,
			byte sec,
			uint fsec,
			byte[] timezone,
			uint timezone_length);

		[DllImport ("oci")]
		internal static extern void OCIDateTimeGetDate (IntPtr hndl,
			IntPtr err,
			IntPtr datetime,
			out short year,
			out byte month,
			out byte day);

		[DllImport ("oci")]
		internal static extern void OCIDateTimeGetTime (IntPtr hndl,
			IntPtr err,
			IntPtr datetime,
			out byte hour,
			out byte min,
			out byte sec,
			out uint fsec);
				
		[DllImport ("oci")]
		internal static extern int OCIIntervalGetDaySecond (IntPtr hndl,
			IntPtr err,
			out int days,
			out int hours,
			out int mins,
			out int secs,
			out int fsec,
			IntPtr interval);

		[DllImport ("oci")]
		internal static extern int OCIIntervalGetYearMonth (IntPtr hndl,
			IntPtr err,
			out int years,
			out int months,
			IntPtr interval);

		internal static int OCIDefineByPos (IntPtr stmtp,
			out IntPtr defnpp,
			IntPtr errhp,
			int position,
			IntPtr valuep,
			int value_sz,
			OciDataType dty,
			ref short indp,
			ref short rlenp,
			IntPtr rcodep,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIDefineByPos", "OCI");
			#endif
			return OciNativeCalls.OCIDefineByPos (stmtp, out defnpp, errhp, position, valuep,
				value_sz, dty, ref indp, ref rlenp, rcodep, mode);
		}

		internal static int OCIDefineByPosPtr (IntPtr stmtp,
			out IntPtr defnpp,
			IntPtr errhp,
			int position,
			ref IntPtr valuep,
			int value_sz,
			OciDataType dty,
			ref short indp,
			ref short rlenp,
			IntPtr rcodep,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIDefineByPosPtr", "OCI");
			#endif
			return OciNativeCalls.OCIDefineByPosPtr (stmtp, out defnpp, errhp, position, ref valuep,
				value_sz, dty, ref indp, ref rlenp, rcodep, mode);
		}

		internal static int OCIDescriptorFree (IntPtr hndlp,
			OciHandleType type)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIDescriptorFree ({0})", type), "OCI");
			#endif
			return OciNativeCalls.OCIDescriptorFree (hndlp, type);
		}

		internal static int OCIEnvCreate (out IntPtr envhpp,
			OciEnvironmentMode mode,
			IntPtr ctxp,
			IntPtr malocfp,
			IntPtr ralocfp,
			IntPtr mfreep,
			int xtramem_sz,
			IntPtr usrmempp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIEnvCreate", "OCI");
			#endif
			return OciNativeCalls.OCIEnvCreate (out envhpp, mode, ctxp, malocfp, ralocfp, mfreep,
				xtramem_sz, usrmempp);
		}

		internal static int OCIAttrGet (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out IntPtr attributep,
			out int sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGet", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGet (trgthndlp, trghndltyp, out attributep, out sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetSByte (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out sbyte attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGetSByte", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGetSByte (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetByte (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out byte attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGetByte", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGetByte (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetUInt16 (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out ushort attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGetUInt16", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGetUInt16 (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetInt32 (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out int attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGetInt32", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGetInt32 (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetIntPtr (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out IntPtr attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIAttrGetIntPtr", "OCI");
			#endif
			return OciNativeCalls.OCIAttrGetIntPtr (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIDescriptorAlloc (IntPtr parenth,
			out IntPtr hndlpp,
			OciHandleType type,
			int xtramem_sz,
			IntPtr usrmempp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIDescriptorAlloc", "OCI");
			#endif
			return OciNativeCalls.OCIDescriptorAlloc (parenth, out hndlpp, type, xtramem_sz, usrmempp);
		}

		internal static int OCIHandleAlloc (IntPtr parenth,
			out IntPtr descpp,
			OciHandleType type,
			int xtramem_sz,
			IntPtr usrmempp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIHandleAlloc ({0})", type), "OCI");
			#endif
			return OciNativeCalls.OCIHandleAlloc (parenth, out descpp, type, xtramem_sz, usrmempp);
		}

		internal static int OCIHandleFree (IntPtr hndlp,
			OciHandleType type)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIHandleFree ({0})", type), "OCI");
			#endif
			return OciNativeCalls.OCIHandleFree (hndlp, type);
		}

		internal static int OCILobClose (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobClose", "OCI");
			#endif
			return OciNativeCalls.OCILobClose (svchp, errhp, locp);
		}

		internal static int OCILobCopy (IntPtr svchp,
			IntPtr errhp,
			IntPtr dst_locp,
			IntPtr src_locp,
			uint amount,
			uint dst_offset,
			uint src_offset)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobCopy", "OCI");
			#endif
			return OciNativeCalls.OCILobCopy (svchp, errhp, dst_locp, src_locp, amount, dst_offset, src_offset);
		}

		internal static int OCILobErase (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			ref uint amount,
			uint offset)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobErase", "OCI");
			#endif
			return OciNativeCalls.OCILobErase (svchp, errhp, locp, ref amount, offset);
		}

		internal static int OCILobGetChunkSize (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			out uint chunk_size)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobGetChunkSize", "OCI");
			#endif
			return OciNativeCalls.OCILobGetChunkSize (svchp, errhp, locp, out chunk_size);
		}

		internal static int OCILobGetLength (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			out uint lenp)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobGetLength", "OCI");
			#endif
			return OciNativeCalls.OCILobGetLength (svchp, errhp, locp, out lenp);
		}

		internal static int OCILobOpen (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			byte mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobOpen", "OCI");
			#endif
			return OciNativeCalls.OCILobOpen (svchp, errhp, locp, mode);
		}

		internal static int OCILobRead (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			ref uint amtp,
			uint offset,
			byte[] bufp,
			uint bufl,
			IntPtr ctxp,
			IntPtr cbfp,
			ushort csid,
			byte csfrm)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobRead", "OCI");
			#endif
			return OciNativeCalls.OCILobRead (svchp, errhp, locp, ref amtp, offset, bufp, bufl,
				ctxp, cbfp, csid, csfrm);
		}

		internal static int OCILobTrim (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			uint newlen)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobTrim", "OCI");
			#endif
			return OciNativeCalls.OCILobTrim (svchp, errhp, locp, newlen);
		}

		internal static int OCILobWrite (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			ref uint amtp,
			uint offset,
			byte[] bufp,
			uint bufl,
			byte piece,
			IntPtr ctxp,
			IntPtr cbfp,
			ushort csid,
			byte csfrm)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobWrite", "OCI");
			#endif
			return OciNativeCalls.OCILobWrite (svchp, errhp, locp, ref amtp, offset, bufp, bufl,
				piece, ctxp, cbfp, csid, csfrm);
		}

		internal static int OCILobCharSetForm (IntPtr svchp, 
			IntPtr errhp,
			IntPtr locp,
			out byte csfrm)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCILobCharSetForm", "OCI");
			#endif
			return OciNativeCalls.OCILobCharSetForm (svchp, errhp, locp, out csfrm);			
		}
		
		internal static int OCINlsGetInfo (IntPtr hndl,
			IntPtr errhp,
			ref byte[] bufp,
			uint buflen,
			ushort item)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCINlsGetInfo", "OCI");
			#endif
			return OciNativeCalls.OCINlsGetInfo (hndl, errhp, bufp, buflen, item);
		}

		internal static int OCIServerAttach (IntPtr srvhp,
			IntPtr errhp,
			string dblink,
			int dblink_len,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIServerAttach", "OCI");
			#endif
			return OciNativeCalls.OCIServerAttach (srvhp, errhp, dblink, dblink_len, mode);
		}

		internal static int OCIServerDetach (IntPtr srvhp,
			IntPtr errhp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIServerDetach", "OCI");
			#endif
			return OciNativeCalls.OCIServerDetach (srvhp, errhp, mode);
		}

		internal static int OCIServerVersion (IntPtr hndlp,
			IntPtr errhp,
			ref byte[] bufp,
			uint bufsz,
			OciHandleType hndltype)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIServerVersion", "OCI");
			#endif
			return OciNativeCalls.OCIServerVersion (hndlp,
				errhp,
				bufp,
				bufsz,
				hndltype);
		}

		internal static int OCISessionBegin (IntPtr svchp,
			IntPtr errhp,
			IntPtr usrhp,
			OciCredentialType credt,
			OciSessionMode mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCISessionBegin", "OCI");
			#endif
			return OciNativeCalls.OCISessionBegin (svchp, errhp, usrhp, credt, mode);
		}

		internal static int OCISessionEnd (IntPtr svchp,
			IntPtr errhp,
			IntPtr usrhp,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCISessionEnd", "OCI");
			#endif
			return OciNativeCalls.OCISessionEnd (svchp, errhp, usrhp, mode);
		}

		internal static int OCIParamGet (IntPtr hndlp,
			OciHandleType htype,
			IntPtr errhp,
			out IntPtr parmdpp,
			int pos)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIParamGet", "OCI");
			#endif
			return OciNativeCalls.OCIParamGet (hndlp, htype, errhp, out parmdpp, pos);
		}

		internal static int OCIStmtExecute (IntPtr svchp,
			IntPtr stmthp,
			IntPtr errhp,
			bool iters,
			uint rowoff,
			IntPtr snap_in,
			IntPtr snap_out,
			OciExecuteMode mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIStmtExecute", "OCI");
			#endif

			uint it = 0;
			if (iters == true)
				it = 1;

			return OciNativeCalls.OCIStmtExecute (svchp, stmthp, errhp, it, rowoff,
				snap_in, snap_out, mode);
		}

		internal static int OCIStmtFetch (IntPtr stmtp,
			IntPtr errhp,
			uint nrows,
			ushort orientation,
			uint mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIStmtFetch", "OCI");
			#endif
			return OciNativeCalls.OCIStmtFetch (stmtp, errhp, nrows, orientation, mode);
		}


		internal static int OCIStmtPrepare (IntPtr stmthp,
			IntPtr errhp,
			byte [] stmt,
			int stmt_length,
			OciStatementLanguage language,
			OciStatementMode mode)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, string.Format("OCIStmtPrepare ({0})", System.Text.Encoding.UTF8.GetString(stmt)), "OCI");
			#endif

			return OciNativeCalls.OCIStmtPrepare (stmthp, errhp, stmt, stmt_length, language, mode);
		}

		internal static int OCITransCommit (IntPtr svchp,
			IntPtr errhp,
			uint flags)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCITransCommit", "OCI");
			#endif
			return OciNativeCalls.OCITransCommit (svchp, errhp, flags);
		}

		internal static int OCITransRollback (IntPtr svchp,
			IntPtr errhp,
			uint flags)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCITransRollback", "OCI");
			#endif
			return OciNativeCalls.OCITransRollback (svchp, errhp, flags);
		}

		internal static int OCITransStart (IntPtr svchp,
			IntPtr errhp,
			uint timeout,
			OciTransactionFlags flags)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCITransStart", "OCI");
			#endif
			return OciNativeCalls.OCITransStart (svchp, errhp, timeout, flags);
		}

		internal static int OCICharSetToUnicode (
			IntPtr svchp,
			StringBuilder dst,
			byte [] src,
			out int rsize)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCICharSetToUnicode", "OCI");
			#endif

			return OciNativeCalls.OCICharSetToUnicode (svchp, dst, dst!=null ? dst.Capacity : 0, src, src.Length, out rsize);
		}

		internal static int OCIUnicodeToCharSet (
			IntPtr svchp,
			byte [] dst,
			[MarshalAs (UnmanagedType.LPWStr)] string src,
			[MarshalAs (UnmanagedType.SysUInt)] out int rsize)
		{
			#if TRACE
			Trace.WriteLineIf(traceOci, "OCIUnicodeToCharSet", "OCI");
			#endif

			return OciNativeCalls.OCIUnicodeToCharSet (svchp, dst, dst!=null ? dst.Length : 0, src, src.Length, out rsize);
		}

		[DllImport ("oci")]
		internal static extern int OCIDateTimeCheck (IntPtr hndl,
			IntPtr err, IntPtr date, out uint valid);

		#endregion

		#region AllocateClear

		private static bool IsUnix =
			(int) Environment.OSVersion.Platform == 4 || (int) Environment.OSVersion.Platform == 128 || (int) Environment.OSVersion.Platform == 6;

		[DllImport("libc")]
		private static extern IntPtr calloc (int nmemb, int size);

		private const int GMEM_ZEROINIT = 0x40;

		[DllImport("kernel32")]
		private static extern IntPtr GlobalAlloc (int flags, int bytes);

		//http://download-uk.oracle.com/docs/cd/B14117_01/appdev.101/b10779/oci05bnd.htm#423147
		internal static IntPtr AllocateClear (int cb)
		{
			if (IsUnix) {
				return calloc (1, cb);
			} else {
				return GlobalAlloc (GMEM_ZEROINIT, cb);
			}
		}

		#endregion AllocateClear
	}
}

