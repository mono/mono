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
	internal sealed class OciLobLocator : OciDescriptorHandle, IOciDescriptorHandle, IDisposable
	{
		#region Fields

		OciErrorHandle errorHandle;
		OciServiceHandle service;
		OciDataType type;

		#endregion // Fields

		#region Constructors

		public OciLobLocator (OciEnvironmentHandle environment, IntPtr handle)
			: base (OciDescriptorType.LobLocator, environment, handle)
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
			status = OCILobOpen (service.Handle, 
						errorHandle.Handle,
						Handle,
						(byte) mode);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public void EndBatch ()
		{
			int status = 0;
			status = OCILobClose (service.Handle, 
						errorHandle.Handle,
						Handle);

			if (status != 0) {
				OciErrorInfo info = ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}
		}

		public uint Erase (uint offset, uint amount)
		{
			int status = 0;
			uint output = amount;
			status = OCILobErase (service.Handle,
						errorHandle.Handle,
						Handle,
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
			status = OCILobGetChunkSize (service.Handle, 
							errorHandle.Handle,
							Handle,
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
			status = OCILobGetLength (service.Handle, 
							errorHandle.Handle,
							Handle,
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

			status = OCILobRead (service.Handle,
						errorHandle.Handle,
						Handle,
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
			status = OCILobTrim (service.Handle,
						errorHandle.Handle,
						Handle,
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

			status = OCILobWrite (service.Handle,
						errorHandle.Handle,
						Handle,
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

		public void Dispose ()
		{
			Environment.FreeDescriptor (this);
		}

		#endregion // Methods
	}
}
