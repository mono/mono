// 
// OciEnvironmentHandle.cs 
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
using System.Runtime.InteropServices;

namespace System.Data.OracleClient.Oci {
	internal class OciEnvironmentHandle : IOciHandle, IDisposable
	{
		#region Fields

		IntPtr handle;

		#endregion // Fields

		#region Constructors

		public OciEnvironmentHandle ()
			: this (OciEnvironmentMode.Default)
		{ 
		}

		public OciEnvironmentHandle (OciEnvironmentMode mode)
		{
			int status = 0;
			this.handle = IntPtr.Zero;

			status = OCIEnvCreate (out handle, mode, 
					IntPtr.Zero, 
					IntPtr.Zero, 
					IntPtr.Zero, 
			 		IntPtr.Zero, 
					0, 
					IntPtr.Zero);
		}

		#endregion // Constructors

		#region Properties

		public IntPtr Handle { 
			get { return handle; }
			set { handle = value; }
		}

		public OciHandleType HandleType {
			get { return OciHandleType.Environment; }
		}

		#endregion // Properties

		#region Methods

		[DllImport ("oci")]
		public static extern int OCIEnvCreate (out IntPtr envhpp,
							[MarshalAs (UnmanagedType.U4)] OciEnvironmentMode mode,
							IntPtr ctxp,
							IntPtr malocfp,
							IntPtr ralocfp,
							IntPtr mfreep,
							int xtramem_sz,
							IntPtr usrmempp);

		[DllImport ("oci")]
		public static extern int OCIHandleAlloc (IntPtr parenth,
							out IntPtr hndlpp,
							[MarshalAs (UnmanagedType.U4)] OciHandleType type,
							int xtramem_sz,
							IntPtr usrmempp);

		[DllImport ("oci")]
		public static extern int OCIHandleFree (IntPtr hndlp, 
							[MarshalAs (UnmanagedType.U4)] OciHandleType type);


		public IOciHandle Allocate (OciHandleType type)
		{
			IntPtr newHandle = IntPtr.Zero;
			int status = 0;

			status = OCIHandleAlloc (Handle,
						out newHandle,
						type,
						0,
						IntPtr.Zero);
			if (status != 0 && status != 1) 
				return null;

			switch (type) {
				case OciHandleType.Service:
					return new OciServiceHandle (this, newHandle);
				case OciHandleType.Error:
					return new OciErrorHandle (this, newHandle);
				case OciHandleType.Server:
					return new OciServerHandle (this, newHandle);
				case OciHandleType.Session:
					return new OciSessionHandle (this, newHandle);
				case OciHandleType.Statement:
					return new OciStatementHandle (this, newHandle);
				case OciHandleType.Transaction:
					return new OciTransactionHandle (this, newHandle);
				default:
					OCIHandleFree (newHandle, type);
					newHandle = IntPtr.Zero;
					throw new ArgumentException ("Invalid handle type.");
			}
		}

		public void Dispose ()
		{
			FreeHandle (this);
		}

		public void FreeHandle (IOciHandle toBeDisposed)
		{
			OCIHandleFree (toBeDisposed.Handle, toBeDisposed.HandleType);
			toBeDisposed.Handle = IntPtr.Zero;
		}

		public OciErrorInfo HandleError ()
		{
			int errbufSize = 512;
			IntPtr errbuf = Marshal.AllocHGlobal (errbufSize);

			OciErrorInfo info;
			info.ErrorCode = 0;
			info.ErrorMessage = String.Empty;

			OciGlue.OCIErrorGet (handle,
					1,
					IntPtr.Zero,
					out info.ErrorCode,
					errbuf,
					(uint) errbufSize,
					OciHandleType.Environment);

			object err = Marshal.PtrToStringAnsi (errbuf);
			if (err != null) {
				string errmsg = (string) err;
				info.ErrorMessage = String.Copy (errmsg);
				Marshal.FreeHGlobal (errbuf);
			}

			return info;
		}

		#endregion // Methods
	}
}
