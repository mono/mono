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
//
// Copyright (C) Joerg Rosenkranz, 2004
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci
{
	internal sealed class OciCalls
	{

		private static bool traceOci;
#if TRACE

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

			[DllImport ("oci")]
			internal static extern int OCIErrorGet (IntPtr hndlp,
				uint recordno,
				IntPtr sqlstate,
				out int errcodep,
				IntPtr bufp,
				uint bufsize,
				[MarshalAs (UnmanagedType.U4)] OciHandleType type);

			[DllImport ("oci")]
			internal static extern int OCIBindByName (IntPtr stmtp,
				out IntPtr bindpp,
				IntPtr errhp,
				string placeholder,
				int placeh_len,
				IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				int indp,
				IntPtr alenp,
				IntPtr rcodep,
				uint maxarr_len,
				IntPtr curelp,
				uint mode);

			[DllImport ("oci")]
			internal static extern int OCIDefineByPos (IntPtr stmtp,
				out IntPtr defnpp,
				IntPtr errhp,
				[MarshalAs (UnmanagedType.U4)] int position,
				IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				ref int rlenp,
				IntPtr rcodep,
				uint mode);

			[DllImport ("oci", EntryPoint="OCIDefineByPos")]
			internal static extern int OCIDefineByPosPtr (IntPtr stmtp,
				out IntPtr defnpp,
				IntPtr errhp,
				[MarshalAs (UnmanagedType.U4)] int position,
				ref IntPtr valuep,
				int value_sz,
				[MarshalAs (UnmanagedType.U2)] OciDataType dty,
				ref short indp,
				ref int rlenp,
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
				[MarshalAs (UnmanagedType.U4)] bool iters,
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
			Trace.WriteLineIf(traceOci, string.Format("OCIAttrSet ({0}, {1})", trghndltyp, attrtype), "OCI");
			return OciNativeCalls.OCIAttrSet (trgthndlp, trghndltyp, attributep, size, attrtype, errhp);
		}

		internal static int OCIAttrSetString (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			string attributep,
			uint size,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, string.Format("OCIAttrSetString ({0}, {1})", trghndltyp, attrtype), "OCI");
			return OciNativeCalls.OCIAttrSetString (trgthndlp, trghndltyp, attributep, size, attrtype, errhp);
		}

		internal static int OCIErrorGet (IntPtr hndlp,
			uint recordno,
			IntPtr sqlstate,
			out int errcodep,
			IntPtr bufp,
			uint bufsize,
			OciHandleType type)
		{
			Trace.WriteLineIf(traceOci, "OCIErrorGet", "OCI");
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
			int indp,
			IntPtr alenp,
			IntPtr rcodep,
			uint maxarr_len,
			IntPtr curelp,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIBindByName", "OCI");
			return OciNativeCalls.OCIBindByName (stmtp, out bindpp, errhp, placeholder, placeh_len, valuep, 
				value_sz, dty, indp, alenp, rcodep, maxarr_len, curelp, mode);
		}

		internal static int OCIDefineByPos (IntPtr stmtp,
			out IntPtr defnpp,
			IntPtr errhp,
			int position,
			IntPtr valuep,
			int value_sz,
			OciDataType dty,
			ref short indp,
			ref int rlenp,
			IntPtr rcodep,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIDefineByPos", "OCI");
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
			ref int rlenp,
			IntPtr rcodep,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIDefineByPosPtr", "OCI");
			return OciNativeCalls.OCIDefineByPosPtr (stmtp, out defnpp, errhp, position, ref valuep, 
				value_sz, dty, ref indp, ref rlenp, rcodep, mode);
		}

		internal static int OCIDescriptorFree (IntPtr hndlp,
			OciHandleType type)
		{
			Trace.WriteLineIf(traceOci, string.Format("OCIDescriptorFree ({0})", type), "OCI");
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
			Trace.WriteLineIf(traceOci, "OCIEnvCreate", "OCI");
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
			Trace.WriteLineIf(traceOci, "OCIAttrGet", "OCI");
			return OciNativeCalls.OCIAttrGet (trgthndlp, trghndltyp, out attributep, out sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetSByte (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out sbyte attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, "OCIAttrGetSByte", "OCI");
			return OciNativeCalls.OCIAttrGetSByte (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetByte (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out byte attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, "OCIAttrGetByte", "OCI");
			return OciNativeCalls.OCIAttrGetByte (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetUInt16 (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out ushort attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, "OCIAttrGetUInt16", "OCI");
			return OciNativeCalls.OCIAttrGetUInt16 (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetInt32 (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out int attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, "OCIAttrGetInt32", "OCI");
			return OciNativeCalls.OCIAttrGetInt32 (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIAttrGetIntPtr (IntPtr trgthndlp,
			OciHandleType trghndltyp,
			out IntPtr attributep,
			IntPtr sizep,
			OciAttributeType attrtype,
			IntPtr errhp)
		{
			Trace.WriteLineIf(traceOci, "OCIAttrGetIntPtr", "OCI");
			return OciNativeCalls.OCIAttrGetIntPtr (trgthndlp, trghndltyp, out attributep, sizep, attrtype, errhp);
		}

		internal static int OCIDescriptorAlloc (IntPtr parenth,
			out IntPtr hndlpp,
			OciHandleType type,
			int xtramem_sz,
			IntPtr usrmempp)
		{
			Trace.WriteLineIf(traceOci, "OCIDescriptorAlloc", "OCI");
			return OciNativeCalls.OCIDescriptorAlloc (parenth, out hndlpp, type, xtramem_sz, usrmempp);
		}

		internal static int OCIHandleAlloc (IntPtr parenth,
			out IntPtr descpp,
			OciHandleType type,
			int xtramem_sz,
			IntPtr usrmempp)
		{
			Trace.WriteLineIf(traceOci, string.Format("OCIHandleAlloc ({0})", type), "OCI");
			return OciNativeCalls.OCIHandleAlloc (parenth, out descpp, type, xtramem_sz, usrmempp);
		}

		internal static int OCIHandleFree (IntPtr hndlp,
			OciHandleType type)
		{
			Trace.WriteLineIf(traceOci, string.Format("OCIHandleFree ({0})", type), "OCI");
			return OciNativeCalls.OCIHandleFree (hndlp, type);
		}

		internal static int OCILobClose (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp)
		{
			Trace.WriteLineIf(traceOci, "OCILobClose", "OCI");
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
			Trace.WriteLineIf(traceOci, "OCILobCopy", "OCI");
			return OciNativeCalls.OCILobCopy (svchp, errhp, dst_locp, src_locp, amount, dst_offset, src_offset);
		}

		internal static int OCILobErase (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			ref uint amount,
			uint offset)
		{
			Trace.WriteLineIf(traceOci, "OCILobErase", "OCI");
			return OciNativeCalls.OCILobErase (svchp, errhp, locp, ref amount, offset);
		}

		internal static int OCILobGetChunkSize (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			out uint chunk_size)
		{
			Trace.WriteLineIf(traceOci, "OCILobGetChunkSize", "OCI");
			return OciNativeCalls.OCILobGetChunkSize (svchp, errhp, locp, out chunk_size);
		}

		internal static int OCILobGetLength (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			out uint lenp)
		{
			Trace.WriteLineIf(traceOci, "OCILobGetLength", "OCI");
			return OciNativeCalls.OCILobGetLength (svchp, errhp, locp, out lenp);
		}

		internal static int OCILobOpen (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			byte mode)
		{
			Trace.WriteLineIf(traceOci, "OCILobOpen", "OCI");
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
			Trace.WriteLineIf(traceOci, "OCILobRead", "OCI");
			return OciNativeCalls.OCILobRead (svchp, errhp, locp, ref amtp, offset, bufp, bufl, 
				ctxp, cbfp, csid, csfrm);
		}

		internal static int OCILobTrim (IntPtr svchp,
			IntPtr errhp,
			IntPtr locp,
			uint newlen)
		{
			Trace.WriteLineIf(traceOci, "OCILobTrim", "OCI");
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
			Trace.WriteLineIf(traceOci, "OCILobWrite", "OCI");
			return OciNativeCalls.OCILobWrite (svchp, errhp, locp, ref amtp, offset, bufp, bufl,
				piece, ctxp, cbfp, csid, csfrm);
		}

		internal static int OCIServerAttach (IntPtr srvhp,
			IntPtr errhp,
			string dblink,
			int dblink_len,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIServerAttach", "OCI");
			return OciNativeCalls.OCIServerAttach (srvhp, errhp, dblink, dblink_len, mode);
		}

		internal static int OCIServerDetach (IntPtr srvhp,
			IntPtr errhp,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIServerDetach", "OCI");
			return OciNativeCalls.OCIServerDetach (srvhp, errhp, mode);
		}

		internal static int OCISessionBegin (IntPtr svchp,
			IntPtr errhp,
			IntPtr usrhp,
			OciCredentialType credt,
			OciSessionMode mode)
		{
			Trace.WriteLineIf(traceOci, "OCISessionBegin", "OCI");
			return OciNativeCalls.OCISessionBegin (svchp, errhp, usrhp, credt, mode);
		}

		internal static int OCISessionEnd (IntPtr svchp,
			IntPtr errhp,
			IntPtr usrhp,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCISessionEnd", "OCI");
			return OciNativeCalls.OCISessionEnd (svchp, errhp, usrhp, mode);
		}

		internal static int OCIParamGet (IntPtr hndlp,
			OciHandleType htype,
			IntPtr errhp,
			out IntPtr parmdpp,
			int pos)
		{
			Trace.WriteLineIf(traceOci, "OCIParamGet", "OCI");
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
			Trace.WriteLineIf(traceOci, "OCIStmtExecute", "OCI");
			return OciNativeCalls.OCIStmtExecute (svchp, stmthp, errhp, iters, rowoff,
				snap_in, snap_out, mode);
		}

		internal static int OCIStmtFetch (IntPtr stmtp,
			IntPtr errhp,
			uint nrows,
			ushort orientation,
			uint mode)
		{
			Trace.WriteLineIf(traceOci, "OCIStmtFetch", "OCI");
			return OciNativeCalls.OCIStmtFetch (stmtp, errhp, nrows, orientation, mode);
		}
							

		internal static int OCIStmtPrepare (IntPtr stmthp,
			IntPtr errhp,
			byte [] stmt,
			int stmt_length,
			OciStatementLanguage language,
			OciStatementMode mode)
		{
			Trace.WriteLineIf(traceOci, string.Format("OCIStmtPrepare ({0})", System.Text.Encoding.UTF8.GetString(stmt)), "OCI");
			
			return OciNativeCalls.OCIStmtPrepare (stmthp, errhp, stmt, stmt_length, language, mode);
		}

		internal static int OCITransCommit (IntPtr svchp,
			IntPtr errhp,
			uint flags)
		{
			Trace.WriteLineIf(traceOci, "OCITransCommit", "OCI");
			return OciNativeCalls.OCITransCommit (svchp, errhp, flags);
		}

		internal static int OCITransRollback (IntPtr svchp,
			IntPtr errhp,
			uint flags)
		{
			Trace.WriteLineIf(traceOci, "OCITransRollback", "OCI");
			return OciNativeCalls.OCITransRollback (svchp, errhp, flags);
		}

		internal static int OCITransStart (IntPtr svchp,
			IntPtr errhp,
			uint timeout,
			OciTransactionFlags flags)
		{
			Trace.WriteLineIf(traceOci, "OCITransStart", "OCI");
			return OciNativeCalls.OCITransStart (svchp, errhp, timeout, flags);
		}

		#endregion
	}
}
