// 
// OciLobLocator.cs 
//  
// Part of managed C#/.NET library System.Data.OracleClient.dll
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient.Oci
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient.Oci
// 
// Author: 
//     Tim Coleman <tim@timcoleman.com>
//         
// Copyright (C) Tim Coleman, 2003
// 

using System;
using System.Data.OracleClient;
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal sealed class OciLobLocator : OciDescriptorHandle, IDisposable
	{
		#region Fields

		bool disposed = false;
		OciErrorHandle errorHandle;
		OciServiceHandle service;
		OciDataType type;

		#endregion // Fields

		#region Constructors

		public OciLobLocator (OciHandle parent, IntPtr handle)
			: base (OciHandleType.LobLocator, parent, handle)
		{
		}

		#endregion // Constructors

		#region Properties 

		public OciErrorHandle ErrorHandle {
			get { return errorHandle; }
			set { errorHandle = value; }
		}

		public OciServiceHandle Service {
			get { return service; }
			set { service = value; }
		}

		public OciDataType LobType {	
			get { return type; }
			set { type = value; }
		}
		
		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		static extern int OCILobClose (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp);

		[DllImport ("oci")]
		static extern int OCILobCopy (IntPtr svchp,
						IntPtr errhp,
						IntPtr dst_locp,
						IntPtr src_locp,
						uint amount,
						uint dst_offset,
						uint src_offset);

		[DllImport ("oci")]
		static extern int OCILobErase (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp,
						ref uint amount,
						uint offset);

		[DllImport ("oci")]
		static extern int OCILobGetChunkSize (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp,
						out uint chunk_size);

		[DllImport ("oci")]
		static extern int OCILobGetLength (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp,
						out uint lenp);

		[DllImport ("oci")]
		static extern int OCILobOpen (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp,
						byte mode);

		[DllImport ("oci")]
		static extern int OCILobRead (IntPtr svchp,
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
		static extern int OCILobTrim (IntPtr svchp,
						IntPtr errhp,
						IntPtr locp,
						uint newlen);

		[DllImport ("oci")]
		static extern int OCILobWrite (IntPtr svchp,
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


		public void BeginBatch (OracleLobOpenMode mode)
		{
			int status = 0;
			status = OCILobOpen (Service, 
						ErrorHandle,
						Handle,
						(byte) mode);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public uint Copy (OciLobLocator destination, uint amount, uint destinationOffset, uint sourceOffset)
		{
			OCILobCopy (Service,
					ErrorHandle,
					destination,
					Handle,
					amount,
					destinationOffset,
					sourceOffset);
			return amount;
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				disposed = true;
				base.Dispose ();
			}
		}

		public void EndBatch ()
		{
			int status = 0;
			status = OCILobClose (Service, ErrorHandle, this);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public uint Erase (uint offset, uint amount)
		{
			int status = 0;
			uint output = amount;
			status = OCILobErase (Service,
						ErrorHandle,
						this,
						ref output,
						(uint) offset);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return output;
		}

		public int GetChunkSize ()
		{
			int status = 0;
			uint output;
			status = OCILobGetChunkSize (Service,
							ErrorHandle,
							this,
							out output);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return (int) output;
		}

		public long GetLength (bool binary)
		{
			int status = 0;
			uint output;
			status = OCILobGetLength (Service, 
						ErrorHandle,
						this,
						out output);
			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			if (!binary)
				output *= 2;

			return (long) output;
		}

		public int Read (byte[] buffer, uint offset, uint count, bool binary)
		{
			int status = 0;
			uint amount = count;

			// Character types are UTF-16, so amount of characters is 1/2
			// the amount of bytes
			if (!binary) 
				amount /= 2;

			status = OCILobRead (Service,
						ErrorHandle,
						this,
						ref amount,
						offset,
						buffer,
						count,
						IntPtr.Zero,
						IntPtr.Zero,
						1000,  // OCI_UCS2ID
						0);    // Ignored if csid is specified as above

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return (int) amount;
		}

		public void Trim (uint newlen)
		{
			int status = 0;
			status = OCILobTrim (Service,
						ErrorHandle,
						this,
						newlen);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

		}

		public int Write (byte[] buffer, uint offset, uint count, OracleType type)
		{
			int status = 0;
			uint amount = count;

			if (type == OracleType.Clob)
				amount /= 2;

			status = OCILobWrite (Service,
						ErrorHandle,
						this,
						ref amount,
						offset,
						buffer,
						count,
						0,    // OCI_ONE_PIECE
						IntPtr.Zero,
						IntPtr.Zero,
						1000, // OCI_UCS2ID
						0);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			return (int) amount;
		}

		#endregion // Methods
	}
}
